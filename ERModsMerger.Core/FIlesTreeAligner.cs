using DotNext;
using ERModsMerger.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERModsMerger.Core
{
    public static class FIlesTreeAligner
    {
        public static bool TryAlignAndCopyToModsToMerge(string path, string modName)
        {
            string pathTemp = ModsMergerConfig.LoadedConfig.AppDataFolderPath + "\\temp";
            if(Directory.Exists(pathTemp))
                Directory.Delete(pathTemp, true);

            Directory.CreateDirectory(pathTemp);

            List<string> files = new List<string>();

            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))//is directory
                Utils.FindAllFiles(path, ref files, true);
            else if (Path.GetExtension(path) == ".zip")
            {
                ZipFile.ExtractToDirectory(path, pathTemp);
                Utils.FindAllFiles(pathTemp, ref files, true);
            }

            if (files.Count > 0)
            {
                var dictionary = File.ReadAllLines(ModsMergerConfig.LoadedConfig.AppDataFolderPath + "\\Dictionaries\\EldenRingDictionary.txt").ToList();
                //reformat
                for (int i = 0; i< dictionary.Count(); i++)
                {
                    dictionary[i] = dictionary[i].Replace("/", "\\").Replace("\r", "").Replace("\n", "");
                    if(dictionary[i].StartsWith("\\"))
                        dictionary[i] = dictionary[i].Substring(1);
                }
                    
                //magic is below
                foreach (string file in files)
                {
                    if(file.Contains("regulation.bin"))
                    {
                        string parent = Directory.GetParent(file).FullName;
                        Utils.CopyDirectory(parent, ModsMergerConfig.LoadedConfig.ModsToMergeFolderPath+ "\\"+ modName, true);
                        return true;
                    }
                    else
                    {
                        var foundIndex = dictionary.FindIndex(x=> x != "" && file.Contains(x));
                        if(foundIndex != -1)
                        {
                            int toSkip = dictionary[foundIndex].Split("\\").Length;

                            string parent = file;
                            for (int i = 0; i < toSkip; i++)
                                parent = Directory.GetParent(parent).FullName;

                            Utils.CopyDirectory(parent, ModsMergerConfig.LoadedConfig.ModsToMergeFolderPath + "\\" + modName, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }


    }
}
