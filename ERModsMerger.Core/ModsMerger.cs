using DotNext.Collections.Generic;
using System.Reflection;

namespace ERModsMerger.Core
{
    public static class ModsMerger
    {
        public static void StartMerge(bool manualConflictResolving = false)
        {

            Console.WriteLine("LOG: Retrieve config");

            ModsMergerConfig config = ModsMergerConfig.LoadConfig();

            if (config == null)
            {
                Console.WriteLine("⚠ Could not load Config at ERModsMergerConfig\\config.json\n⚠ If you have made modifications, please verify if everything is correct.");
                return;
            }

            Console.WriteLine("LOG: Config loaded\n");

            Console.WriteLine("-- START MERGING --\n");


            string[] dirs = Directory.GetDirectories(config.ModsToMergeFolderPath);
            if(dirs != null && dirs.Length > 0)
            {
                List<string> modsDirectories = dirs.OrderByDescending(q => q).ToList();

                Console.Write("\nLOG: Initial directories merge");

                if(Directory.Exists(config.MergedModsFolderPath))
                    Directory.Delete(config.MergedModsFolderPath, true);

                Directory.CreateDirectory(config.MergedModsFolderPath);

                foreach (string modsDirectory in modsDirectories)
                    CopyDirectory(modsDirectory, config.MergedModsFolderPath);

                Console.Write(" - Done\n\n");


                //Search all files in directories, add them to the dispatcher and then search which files are conflicting
                var dispatcher = new MergeableFilesDispatcher();
                var allFiles = new List<string>();
                foreach (string modsDirectory in modsDirectories)
                    FindAllFiles(modsDirectory, ref allFiles, true);

                foreach (string file in allFiles)
                    dispatcher.AddFile(file);

                dispatcher.SearchForConflicts();

                
                dispatcher.MergeAllConflicts(manualConflictResolving);

                
            }
            else
            {
                Console.WriteLine($"⚠ No mod folder(s) could be found in {config.ModsToMergeFolderPath}\n⚠ Verify if everything is placed well like in example.\n⚠ Relaunch and look at example to see expected folders placement.");
            }

            

        }

        static void FindAllFiles(string path, ref List<string> files, bool searchInSubDirectories)
        {
            files.AddAll(Directory.GetFiles(path).ToList());
            if (searchInSubDirectories && Directory.GetDirectories(path).Length > 0)
            {
                foreach(var directory in Directory.GetDirectories(path))
                    FindAllFiles(directory, ref files, searchInSubDirectories);
            }
        }


        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
