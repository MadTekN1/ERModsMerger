using DotNext.Collections.Generic;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Diagnostics;
using ERModsMerger.Core.Utility.LuaUtils;

namespace ERModsMerger.Core.Formats
{
    internal class LUA_HKS
    {

        public LUA_HKS(string path)
        {
            var luaFileName = Path.GetFileName(path);

            var vanillaParser = new LuaParser(ModsMergerConfig.LoadedConfig.AppDataFolderPath + "\\EldenRingHKS\\" + luaFileName);
            var moddedParser = new LuaParser(path);

            var comparison = LuaCompare.Compare(vanillaParser.RootNode, moddedParser.RootNode);

        }


        public static void MergeFiles(List<FileToMerge> files)
        {
            foreach (var file in files)
            {
                var luaFile = new LUA_HKS(file.Path);
            }
        }

    }
}
