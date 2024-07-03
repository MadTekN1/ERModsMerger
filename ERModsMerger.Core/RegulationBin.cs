using SoulsFormats;
using System.Collections;

namespace ERModsMerger.Core
{
    class RegulationBin : IDisposable
    {
        BND4 bnd;
        Dictionary<string, PARAMDEF> _paramdefs;
        public Dictionary<string, PARAM> Params { get; set; }

        public string Version { get; set; }

        public RegulationBin(string path) 
        { 
            LoadParamDefs();
            Params = new Dictionary<string, PARAM>();
            bnd = SFUtil.DecryptERRegulation(path);
            Load();
        }

        private void Load()
        {
            Version = bnd.Version; // should be 11220021
            for (int i = 0; i < bnd.Files.Count; i++)
            {
                double progress = ((double)i / (double)bnd.Files.Count) * 100;
                Console.Write($"\rProgess: {Math.Round(progress, 0)}%");

                var paramName = Path.GetFileNameWithoutExtension(bnd.Files[i].Name);

                if (!bnd.Files[i].Name.ToUpper().EndsWith(".PARAM"))
                {
                    continue;
                }

                PARAM p;

                p = PARAM.ReadIgnoreCompression(bnd.Files[i].Bytes);

                

                if (!_paramdefs.ContainsKey(p.ParamType ?? ""))
                {
                    continue;
                }

                if (p.ParamType == null)
                {
                    throw new Exception("Param type is unexpectedly null");
                }

                PARAMDEF def = _paramdefs[p.ParamType];
                try
                {
                    p.ApplyParamdef(def);
                }
                catch (Exception e)
                {
                    var name = bnd.Files[i].Name.Split("\\").Last();
                    var message = $"Could not apply ParamDef for {name}";
                }

                Params.Add(paramName, p);
            }

            Console.WriteLine($"\rProgess: 100% - Loaded ✓");
        }


        public void MergeFrom(Dictionary<string, PARAM> fromParams, Dictionary<string, PARAM> vanillaParams, bool manualConflictResolving = false)
        {
            int counter = 0;
            int maxCounter = fromParams.Count;
            foreach (var fromParam in fromParams)
            {
                string mergingProgressConsole = $"\rMerging regulation.bin - Progress {Math.Round(((double)counter / (double)maxCounter) * 100, 0)}%";

                Console.Write(mergingProgressConsole);

                bool modifiedParam = false;

                for (int r = 0; r < fromParam.Value.Rows.Count; r++)
                {
                    int rowId = fromParam.Value.Rows[r].ID;
                    int rowIndexFound = Params[fromParam.Key].Rows.FindIndex(x => x.ID == rowId);

                    // if row to merge already exist AND have not ID duplicates
                    if (rowIndexFound != -1 && Params[fromParam.Key].Rows.Count(x => x.ID == rowId) == 1)
                    {
                        try
                        {
                            for (int c = 0; c < fromParam.Value.Rows[r].Cells.Count; c++)
                            {
                                if (Params[fromParam.Key].Rows[rowIndexFound].Cells[c] != null && fromParam.Value.Rows[r].Cells[c] != null)
                                {
                                    var valCurrent = Params[fromParam.Key].Rows[rowIndexFound].Cells[c].Value;
                                    var valFromParam = fromParam.Value.Rows[r].Cells[c].Value;
                                    

                                    // verify if row exist in vanilla
                                    if (vanillaParams[fromParam.Key].Rows.Count > rowIndexFound)
                                    {
                                        var valFromVanilla = vanillaParams[fromParam.Key].Rows[rowIndexFound].Cells[c].Value;

                                        //modded param->row->field is different from vanilla
                                        if (!Compare(valFromVanilla, valFromParam))
                                        {
                                            //detect an attempt to re-edit a value already edited by another mod
                                            //manual resolving
                                            if(manualConflictResolving && !Compare(valCurrent, valFromVanilla))
                                            {
                                                Console.Write($"\r- Detected conflict in {fromParam.Key}->[{rowIndexFound.ToString()}] {Params[fromParam.Key].Rows[rowIndexFound].Name}->{Params[fromParam.Key].Rows[rowIndexFound].Cells[c].Def.ToString()}\n");
                                                Console.Write($"   From value: {valCurrent.ToString()}\n");
                                                Console.Write($"   To value: {valFromParam.ToString()}\n\n");

                                                Console.ForegroundColor = ConsoleColor.Cyan;
                                                Console.Write("\r<<APPLY NEW VALUE (Press 'A')>> || <<PRESS ANY OTHER KEY TO IGNORE>>");
                                                Console.ResetColor();

                                                char keyPressed = Console.ReadKey(true).KeyChar;

                                                if(keyPressed == 'A' || keyPressed == 'a')
                                                {
                                                    Params[fromParam.Key].Rows[rowIndexFound].Cells[c].Value = valFromParam;
                                                    modifiedParam = true;
                                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                                    Console.Write("\rNew value applied!                                                         \n\n");
                                                    Console.ResetColor();
                                                }
                                                else
                                                {
                                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                                    Console.Write("\rNew value ignored!                                                         \n\n");
                                                    Console.ResetColor();
                                                }

                                            }
                                            else
                                            {
                                                Params[fromParam.Key].Rows[rowIndexFound].Cells[c].Value = valFromParam;
                                                modifiedParam = true;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        Params[fromParam.Key].Rows[rowIndexFound].Cells[c].Value = valFromParam;
                                        modifiedParam = true;
                                    }

                                }

                            }

                        }
                        catch (Exception e)
                        {
                            Console.Write($"\r⚠ Error during merging Row {r.ToString()} in Param {fromParam.Key}\n");
                        }

                        
                    }
                    else if(rowIndexFound == -1) // if the row to merge is a new one
                    {
                        Params[fromParam.Key].Rows.Add(new PARAM.Row(fromParam.Value.Rows[r]));
                        modifiedParam = true;
                    }

                }


                if (modifiedParam)
                {
                    int bndFileIndex = bnd.Files.FindIndex(x => x.Name.Contains(fromParam.Key + ".param"));
                    try
                    {
                        if (bndFileIndex != -1)
                        {
                            bnd.Files[bndFileIndex].Bytes = Params[fromParam.Key].Write();
                        }
                        else
                        {
                            Console.WriteLine("bnd file not found");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Write($"\r⚠ Unable to merge Param {fromParam.Key}\n");
                    }
                }
                counter++;
            }

            Console.Write($"\rMerging regulation.bin - Progess: 100% - Done ✓\n\n");
        }

        public void Save(string path)
        {
            SFUtil.EncryptERRegulation(path, bnd);
        }


       

        public void Dispose()
        {
            bnd.Dispose();
            Params.Clear();
            _paramdefs.Clear();

            // Notify the garbage collector 
            // about the cleaning event 
            GC.SuppressFinalize(this);
        }

        private void LoadParamDefs()
        {
            _paramdefs = new Dictionary<string, PARAMDEF>();
            //Load paramdefs
            var files = Directory.GetFiles("ERModsMergerConfig\\ParamDefs", "*.xml");

            foreach (var f in files)
            {
                var pdef = PARAMDEF.XmlDeserialize(f, false);

                _paramdefs.Add(pdef.ParamType, pdef);
            }

        }

        public static bool Compare(object? x, object? y)
        {
            if (x is null || y is null)
                return false;

            if (x is IEnumerable a && y is IEnumerable b)
                return a.Cast<object>().SequenceEqual(b.Cast<object>());

            return x.Equals(y);
        }
    }
}
