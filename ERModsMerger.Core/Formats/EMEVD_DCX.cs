using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNext.Collections.Generic;
using SoulsFormats;

namespace ERModsMerger.Core.Formats
{
    internal class EMEVD_DCX
    {
        public EMEVD Emevd {  get; set; }

        public EMEVD_DCX(string path) 
        {
            Emevd =  EMEVD.Read(path);
        }


        public static void MergeFiles(List<FileToMerge> files)
        {
            if(!File.Exists(ModsMergerConfig.LoadedConfig.GamePath + "\\" + files[0].ModRelativePath))
            {
                Console.WriteLine("⚠ Could not locate vanilla .emevd file at: " + ModsMergerConfig.LoadedConfig.GamePath + "\\" + files[0].ModRelativePath + "\nPlease unpack the game using UXM (https://github.com/Nordgaren/UXM-Selective-Unpack)");
                return;
            }


            var vanilla_emevd = new EMEVD_DCX(ModsMergerConfig.LoadedConfig.GamePath+ "\\" + files[0].ModRelativePath).Emevd;
            var base_emevd  = new EMEVD_DCX(files[0].Path).Emevd;

            for (var f = 1; f < files.Count; f++)
            {
                var emevd_to_merge = new EMEVD_DCX(files[f].Path).Emevd;

                //foreach events
                for(var e = 0; e < emevd_to_merge.Events.Count; e++)
                {
                    var event_id_to_merge = emevd_to_merge.Events[e].ID;
                    var event_found_in_vanilla = vanilla_emevd.Events.Find(x => x.ID == event_id_to_merge);
                    var event_found_in_base = base_emevd.Events.Find(x => x.ID == event_id_to_merge);

                    //event dont exist in vanilla_emevd, we add it
                    if (event_found_in_vanilla == null || event_found_in_base == null)
                    {
                        base_emevd.Events.Add(emevd_to_merge.Events[e]);
                    }
                    else
                    {
                        //foreach instruction
                        for(var i = 0; i < emevd_to_merge.Events[e].Instructions.Count; i++)
                        {
                            var instruction_to_merge = emevd_to_merge.Events[e].Instructions[i];

                            EMEVD.Instruction? vanilla_instruction = null;
                            if(event_found_in_vanilla.Instructions.Count>i)
                                vanilla_instruction = event_found_in_vanilla.Instructions[i];

                            EMEVD.Instruction? base_instruction = null;
                            if(event_found_in_base.Instructions.Count>i)
                                base_instruction = event_found_in_base.Instructions[i];

                            
                            //instruction to merge is different from vanilla
                            if (vanilla_instruction == null
                                || !Utils.AdvancedEquals(instruction_to_merge.ArgData, vanilla_instruction.ArgData)
                                || instruction_to_merge.ID != vanilla_instruction.ID
                                || instruction_to_merge.Layer != vanilla_instruction.Layer
                                || instruction_to_merge.Bank != vanilla_instruction.Bank)
                            {
                                
                            }

                        }
                    }

                //END foreach events
                }
            }
        }
    }
}
