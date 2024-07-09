using System.IO;
using System.Text.Json;

namespace ERModsMerger.Core
{
    public class ModsMergerConfig
    {
        public static ModsMergerConfig? LoadedConfig { get; set; }

        /// <summary>
        /// Load the config from the specified path
        /// </summary>
        /// <param name="pathConfigFile">Where the config.json file is located</param>
        /// <returns></returns>
        public static ModsMergerConfig? LoadConfig(string pathConfigFile = "ERModsMergerConfig\\config.json")
        {
            if(File.Exists(pathConfigFile))
            {
                try
                {
                    LoadedConfig = (ModsMergerConfig?)JsonSerializer.Deserialize(File.ReadAllText(pathConfigFile), typeof(ModsMergerConfig));

                    CheckAndAddEnvVars();
                    CheckVersionAndEmbeddedExtraction();
                    return LoadedConfig;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }


        /// <summary>
        /// Check the current version of assembly, if different (updated) extract (and overwrite) embedded resource to appData path
        /// </summary>
        private static void CheckVersionAndEmbeddedExtraction()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            if(LoadedConfig.ToolVersion != version)
            {
                EmbeddedResourcesExtractor.ExtractAssets();

                LoadedConfig.ToolVersion = version;
            }
        }

        /// <summary>
        /// If valid, add the game path to environment variable "PATH" so DLL IMPORTS works for soulsformats // Oodle
        /// </summary>
        private static void CheckAndAddEnvVars()
        {
            var env = Environment.GetEnvironmentVariable("PATH");
            if (env != null &&
                ModsMergerConfig.LoadedConfig != null &&
                File.Exists(ModsMergerConfig.LoadedConfig.GamePath + "\\regulation.bin") &&
                !env.Split(';').Contains(ModsMergerConfig.LoadedConfig.GamePath))
            {
                Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + ModsMergerConfig.LoadedConfig.GamePath);
            }
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


        string _appDataFolderPath = "ERModsMergerConfig";
        public string AppDataFolderPath { get { return _appDataFolderPath; } set { _appDataFolderPath = value; } }

        string _modsToMergeFolderPath = "ModsToMerge";
        public string ModsToMergeFolderPath { get { return _modsToMergeFolderPath; } set { _modsToMergeFolderPath = value; } }

        string _mergedModsFolderPath = "MergedMods";
        public string MergedModsFolderPath { get { return _mergedModsFolderPath; } set { _mergedModsFolderPath = value; } }

        int _consolePrintDelay = 15;
        public int ConsolePrintDelay { get { return _consolePrintDelay; } set { _consolePrintDelay = value; } }

        string _toolVersion = "";
        public string ToolVersion { get { return _toolVersion; } set { _toolVersion = value; } }

        public List<ModConfig> Mods { get; set; }

        public ModsMergerConfig()
        {
            Mods = new List<ModConfig>();
        }
    }

    public class ModConfig
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Enabled { get; set; }

        public ModConfig(string name, string path, bool enabled)
        {
            Name = name;
            Path = path;
            Enabled = enabled;
        }
    }
}
