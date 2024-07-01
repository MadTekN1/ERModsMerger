using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace ERModsMerger.Core
{
    public class ModsMergerConfig
    {
        public static ModsMergerConfig? LoadedConfig { get; set; }

        public static ModsMergerConfig? LoadConfig(string pathConfigFile = "ERModsMergerConfig\\config.json")
        {
            if(File.Exists(pathConfigFile))
            {
                try
                {
                    LoadedConfig = (ModsMergerConfig?)JsonSerializer.Deserialize(File.ReadAllText(pathConfigFile), typeof(ModsMergerConfig));
                    return LoadedConfig;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        public static bool SaveConfig(string pathConfigFile = "ERModsMergerConfig\\config.json")
        {
            try
            {

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                File.WriteAllText(pathConfigFile, JsonSerializer.Serialize(LoadedConfig, typeof(ModsMergerConfig), options));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        string _gamePath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ELDEN RING\\Game";
        public string GamePath { get { return _gamePath; } set { _gamePath = value; } }

        string _modsToMergeFolderPath = "ModsToMerge";
        public string ModsToMergeFolderPath { get { return _modsToMergeFolderPath; } set { _modsToMergeFolderPath = value; } }

        string _mergedModsFolderPath = "MergedMods";
        public string MergedModsFolderPath { get { return _mergedModsFolderPath; } set { _mergedModsFolderPath = value; } }



        public ModsMergerConfig()
        {

        }
    }
}
