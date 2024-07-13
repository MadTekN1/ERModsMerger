using DotNext.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using ERModsMerger.Core.Utility;

namespace ERModsMerger.Core
{
    public class ModsMerger
    {
        public static void StartMerge(bool manualConflictResolving = false, bool requestUserConfirmations = true)
        {

            LOG.Log("Retrieve config");

            ModsMergerConfig config = ModsMergerConfig.LoadedConfig;

            if (config == null)
            {
                LOG.Log("Could not load Config at ERModsMergerConfig\\config.json\n   If you have made modifications, please verify if everything is correct.",
                        LOGTYPE.ERROR);

                OnMergeFinish(true);
                return;
            }

            LOG.Log("Config loaded\n");

            LOG.Log("-- START MERGING --\n");


            string[] dirs = Directory.GetDirectories(config.ModsToMergeFolderPath);
            if(dirs != null && dirs.Length > 0)
            {
                List<string> modsDirectories = dirs.OrderByDescending(q => q).ToList();

                //if mods are present in config with special order, change modsDirectories
                if (ModsMergerConfig.LoadedConfig.Mods.Count > 0)
                {
                    modsDirectories = new List<string>();
                    ModsMergerConfig.LoadedConfig.Mods.FindAll(x => x.Enabled).ForEach((x) => { modsDirectories.Insert(0,x.Path); });
                }


                //Search all files in directories, add them to the dispatcher and then search which files are conflicting
                var dispatcher = new MergeableFilesDispatcher();
                var allFiles = new List<string>();
                foreach (string modsDirectory in modsDirectories)
                    Utils.FindAllFiles(modsDirectory, ref allFiles, true);

                foreach (string file in allFiles)
                    dispatcher.AddFile(file);

                dispatcher.SearchForConflicts();

                
                var unsuportedFilesConflicts = dispatcher.Conflicts.FindAll(x => !x.SupportedFormat);
                if (unsuportedFilesConflicts.Count > 0)
                {
                    LOG.Log($"{unsuportedFilesConflicts.Count} unsupported conflict(s) found:\n",
                        LOGTYPE.WARNING);

                    foreach (var conflict in unsuportedFilesConflicts)
                    {
                        LOG.Log("- " + conflict.FilesToMerge[0].ModRelativePath + ":",
                            LOGTYPE.WARNING);
                        conflict.FilesToMerge.ForEach((x) => { Console.WriteLine("      - " + x.Path); });
                        Console.WriteLine();
                    }
                        

                    if(requestUserConfirmations)
                    {
                        var answer = LOG.QueryUserYesNoQuestion("Due to unsuported conflicts, some file(s) will be completely overwriten and this will potentially cause in-game issues. Are you sure you want to continue?");

                        if (!answer)
                        {
                            OnMergeFinish(true);
                            return;
                        } 
                    }
                }

                Console.WriteLine();
                
                LOG.Log("Initial directories merge");
                
                if(Directory.Exists(config.MergedModsFolderPath))
                    Directory.Delete(config.MergedModsFolderPath, true);

                Directory.CreateDirectory(config.MergedModsFolderPath);

                foreach (string modsDirectory in modsDirectories)
                    Utils.CopyDirectory(modsDirectory, config.MergedModsFolderPath);
                


                dispatcher.MergeAllConflicts(manualConflictResolving);

                
            }
            else
            {
                LOG.Log($"No mod folder(s) could be found in {config.ModsToMergeFolderPath}\n⚠ Verify if everything is placed well like in example.\n⚠ Relaunch and look at example to see expected folders placement.",
                    LOGTYPE.ERROR);
            }

            

            OnMergeFinish(true);
        }

        public delegate void MergeFinishEventHandler(bool finished);

        public static event MergeFinishEventHandler MergeFinish;

        protected static void OnMergeFinish(bool finished)
        {
            if (MergeFinish != null)
            {
                MergeFinish(true);
            }
        }


    }
}
