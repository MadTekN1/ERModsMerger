using System.Reflection;

namespace ERModsMerger.Core
{
    public static class ModsMerger
    {
        public static void StartMerge()
        {

            Console.WriteLine("LOG: Retrieve config");
            ModsMergerConfig config = ModsMergerConfig.LoadConfig();
            Console.WriteLine("LOG: Config loaded\n");

            Console.WriteLine("-- START MERGING --\n");


            Console.WriteLine("LOG: Loading vanilla regulation.bin");

            //load vanilla regulation.bin
            RegulationBin vanillaRegulationBin = new RegulationBin(config.GamePath + "\\regulation.bin");

            

            List<string> modsDirectories = Directory.GetDirectories(config.ModsToMergeFolderPath).OrderByDescending(q => q).ToList();

            Console.Write("\nLOG: Initial directories merge");
            
            Directory.Delete(config.MergedModsFolderPath, true);
            Directory.CreateDirectory(config.MergedModsFolderPath);

            foreach (string modsDirectory in modsDirectories)
                CopyDirectory(modsDirectory, config.MergedModsFolderPath);
            Console.Write(" - Done\n\n");

            //load modded regulation.bin files and merge them into mainRegulationBin
            if (File.Exists(modsDirectories[0] + "\\regulation.bin"))
            {
                //load modded regulation.bin
                Console.WriteLine($"LOG: Loading initial modded regulation: {modsDirectories[0]}\\regulation.bin");
                RegulationBin mainRegulationBin = new RegulationBin(modsDirectories[0] + "\\regulation.bin");
                Console.WriteLine();

                for (int i = 1; i < modsDirectories.Count; i++)
                {
                    if (File.Exists(modsDirectories[i] + "\\regulation.bin"))
                    {
                        //load modded regulation.bin
                        Console.WriteLine($"LOG: Loading {modsDirectories[i]}\\regulation.bin");

                        RegulationBin moddedRegulationBin = new RegulationBin(modsDirectories[i] + "\\regulation.bin");
                        mainRegulationBin.MergeFrom(moddedRegulationBin.Params, vanillaRegulationBin.Params);
                    }
                }

                Console.WriteLine("LOG: Saving merged regulation.bin");
                mainRegulationBin.Save(config.MergedModsFolderPath + "\\regulation.bin");
                Console.WriteLine("Saved in: " + config.MergedModsFolderPath + "\\regulation.bin");
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
