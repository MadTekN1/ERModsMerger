using System.Reflection;

namespace ERModsMerger.Core
{
    public static class ModsMerger
    {
        public static void StartMerge()
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


            Console.WriteLine("LOG: Loading vanilla regulation.bin");

            if(!File.Exists(config.GamePath + "\\regulation.bin"))
            {
                Console.WriteLine($"⚠ Could not locate vanilla regualtion bin at {config.GamePath}\n⚠ Please verify GamePath in ERModsMergerConfig\\config.json");
                return;
            }

            //load vanilla regulation.bin
            RegulationBin vanillaRegulationBin;
            try
            {
                vanillaRegulationBin = new RegulationBin(config.GamePath + "\\regulation.bin");
            }
            catch (Exception e)
            {
                Console.WriteLine("⚠ Could not load vanilla regulation.bin\n⚠ Your game regulation version might be incompatible");
                return;
            }
            

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

                //load modded regulation.bin files and merge them into mainRegulationBin
                if (File.Exists(modsDirectories[0] + "\\regulation.bin"))
                {
                    //load modded regulation.bin
                    Console.WriteLine($"LOG: Loading initial modded regulation: {modsDirectories[0]}\\regulation.bin");

                    RegulationBin mainRegulationBin;
                    try
                    {
                        mainRegulationBin = new RegulationBin(modsDirectories[0] + "\\regulation.bin");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"⚠ Could not load {modsDirectories[0]}\\regulation.bin\n⚠ Regulation version might be incompatible");
                        return;
                    }

                    Console.WriteLine();

                    for (int i = 1; i < modsDirectories.Count; i++)
                    {
                        if (File.Exists(modsDirectories[i] + "\\regulation.bin"))
                        {
                            //load modded regulation.bin
                            Console.WriteLine($"LOG: Loading {modsDirectories[i]}\\regulation.bin");

                            try
                            {
                                RegulationBin moddedRegulationBin = new RegulationBin(modsDirectories[i] + "\\regulation.bin");

                                mainRegulationBin.MergeFrom(moddedRegulationBin.Params, vanillaRegulationBin.Params);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"⚠ Could not load {modsDirectories[i]}\\regulation.bin\n⚠ Regulation version might be incompatible");
                            }
                        }
                    }

                    Console.WriteLine("LOG: Saving merged regulation.bin");
                    mainRegulationBin.Save(config.MergedModsFolderPath + "\\regulation.bin");
                    Console.WriteLine("Saved in: " + config.MergedModsFolderPath + "\\regulation.bin");
                }
            }
            else
            {
                Console.WriteLine($"⚠ No mod folder(s) could be found in {config.ModsToMergeFolderPath}\n⚠ Verify if everything is placed well like in example.\n⚠ Relaunch and look at example to see expected folders placement.");
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
