using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;

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
            
            if (File.Exists(pathConfigFile))
            {
                try
                {
                    LoadedConfig = (ModsMergerConfig?)JsonSerializer.Deserialize(File.ReadAllText(pathConfigFile), typeof(ModsMergerConfig));

                    LoadedConfig.ConfigPath = pathConfigFile;

                    CheckAndAddEnvVars();
                    CheckVersionAndEmbeddedExtraction();
                    return LoadedConfig;
                }
                catch (Exception e)
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
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (LoadedConfig.ToolVersion != version)
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

        /// <summary>
        /// Save the current loaded config to the specified path
        /// </summary>
        /// <returns></returns>
        public static bool SaveConfig(string pathConfigFile = "ERModsMergerConfig\\config.json")
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                File.WriteAllText(pathConfigFile, JsonSerializer.Serialize(LoadedConfig, typeof(ModsMergerConfig), options));

                LoadedConfig.SaveModEngineConfig();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SaveModEngineConfig()
        {
            // there is dll mods
            if (Mods.Count(x => x.IsDllMod && x.Enabled) > 0)
            {
                string modEngineConfig = File.ReadAllText(AppDataFolderPath + "\\ModEngine2\\config_eldenring_prog.toml");
                var dllModsPaths = Mods.FindAll(x => x.IsDllMod && x.Enabled).Select(x => x.FilePath).ToList();

                string confDlls = "\"" + string.Join("\",\"", dllModsPaths) + "\"";

                modEngineConfig = modEngineConfig.Replace("$DLL_MODS_PATHS", confDlls).Replace("$MOD_DIR", MergedModsFolderPath).Replace("\\", "\\\\");
                File.WriteAllText(AppDataFolderPath + "\\ModEngine2\\config_eldenring.toml", modEngineConfig);
            }
            else //set normal modengine config
            {
                string modEngineConfig = File.ReadAllText(AppDataFolderPath + "\\ModEngine2\\config_eldenring_prog.toml");

                modEngineConfig = modEngineConfig.Replace("$DLL_MODS_PATHS", "").Replace("$MOD_DIR", MergedModsFolderPath).Replace("\\", "\\\\");
                File.WriteAllText(AppDataFolderPath + "\\ModEngine2\\config_eldenring.toml", modEngineConfig);
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

        public string ConfigPath { get; set; }

        int _consolePrintDelay = 15;
        public int ConsolePrintDelay { get { return _consolePrintDelay; } set { _consolePrintDelay = value; } }

        string _toolVersion = "";
        public string ToolVersion { get { return _toolVersion; } set { _toolVersion = value; } }



        public List<ModConfig> Mods { get; set; }

        public ModsMergerConfig()
        {
            Mods = new List<ModConfig>();
        }

        public bool Save(string newConfigPath = "")
        {
            try
            {
                if(ConfigPath != "")
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;

                    if(newConfigPath == "")
                        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(LoadedConfig, typeof(ModsMergerConfig), options));
                    else
                        File.WriteAllText(newConfigPath, JsonSerializer.Serialize(LoadedConfig, typeof(ModsMergerConfig), options));


                    SaveModEngineConfig();

                    return true;
                }
               
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }

    public class ModConfig
    {
        public string Name { get; set; }
        public string DirPath { get; set; }
        public bool Enabled { get; set; }
        public string Note { get; set; }

        public bool IsDllMod { get; set; }
        public string FilePath { get; set; }

        public List<ModFileConfig> ModFiles { get; set; }

        public ModConfig(string name, string dirPath, bool enabled)
        {
            Name = name;
            DirPath = dirPath;
            Enabled = enabled;
            Note = "";
            ModFiles = new List<ModFileConfig>();
            IsDllMod = false;
        }
    }

    public class ModFileConfig
    {
        public string Path { get; set; }
        public bool Enabled { get; set; }

        public ModFileConfig(string path, bool enabled = true)
        {
            Path = path;
            Enabled = enabled;
        }
    }
}
