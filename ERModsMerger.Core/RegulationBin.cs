using SoulsFormats;
using System.Collections;

namespace ERModsMerger.Core
{
    class RegulationBin : IDisposable
    {
        BND4 bnd;
        Dictionary<string, PARAMDEF> _paramdefs;
        public Dictionary<string, PARAM> Params { get; set; }

        public RegulationBin(string path) 
        { 
            LoadParamDefs();
            Params = new Dictionary<string, PARAM>();
            bnd = SFUtil.DecryptERRegulation(path);
            Load();
        }

        private void Load()
        {
            ulong version = Convert.ToUInt64(bnd.Version);
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

       
        public void MergeFrom(Dictionary<string, PARAM> fromParams, Dictionary<string, PARAM> vanillaParams)
        {
            int counter = 0;
            int maxCounter = fromParams.Count;
            foreach (var fromParam in fromParams)
            {
                Console.Write($"\rMerging regulation.bin - Progress {Math.Round(((double)counter / (double)maxCounter) * 100, 0)}%");

                bool modifiedParam = false;

                for (int r = 0; r < fromParam.Value.Rows.Count; r++)
                {
                    int rowId = fromParam.Value.Rows[r].ID;
                    int rowIndexFound = Params[fromParam.Key].Rows.FindIndex(x => x.ID == rowId);

                    if (rowIndexFound != -1 && Params[fromParam.Key].Rows.Count(x => x.ID == rowId) == 1) // if row to merge already exist AND have not ID duplicates
                    {
                        for (int c = 0; c < fromParam.Value.Rows[r].Cells.Count; c++)
                        {
                            if (Params[fromParam.Key].Rows[rowIndexFound].Cells[c] != null && fromParam.Value.Rows[r].Cells[c] != null)
                            {
                                var valParam = Params[fromParam.Key].Rows[rowIndexFound].Cells[c].Value;

                                string nameFromParam = fromParam.Value.Rows[r].Cells[c].Def.ToString();
                                var valFromParam = fromParam.Value.Rows[r].Cells[c].Value;

                                //TODO: control existing value
                                var valFromVanilla = vanillaParams[fromParam.Key].Rows[rowIndexFound].Cells[c].Value;

                                if (!Compare(valFromVanilla, valFromParam))
                                {

                                    Params[fromParam.Key].Rows[rowIndexFound].Cells[c].Value = valFromParam;
                                    modifiedParam = true;
                                }

                            }

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
            var files = Directory.GetFiles("Config\\ParamDefs", "*.xml");

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
