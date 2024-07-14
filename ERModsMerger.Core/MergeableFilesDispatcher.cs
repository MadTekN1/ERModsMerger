﻿using DotNext.Collections.Generic;
using ERModsMerger.Core.Formats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ERModsMerger.Core
{
    internal class MergeableFilesDispatcher
    {
        List<FileToMerge> FilesToMerge {  get; set; }
        public List<FileConflict> Conflicts { get; set; }

        public MergeableFilesDispatcher()
        {
            FilesToMerge = new List<FileToMerge>();
            Conflicts = new List<FileConflict>();
        }

        public void AddFile(string path)
        {
            FilesToMerge.Add(new FileToMerge(path));
        }

        public void SearchForConflicts()
        {
            var groups = FilesToMerge.GroupBy(x => x.ModRelativePath);

            foreach (var group in groups)
            {
                if(group.Count() > 1)
                {
                    Conflicts.Add(new FileConflict(group.ToList()));
                }
            }
        }

        public void MergeAllConflicts(bool manualConflictsResolving)
        {
            foreach (var conflict in Conflicts)
            {
                switch(conflict.FilesToMerge[0].ModRelativePath)
                {
                    case string a when a.Contains("regulation.bin"): RegulationBin.MergeRegulationsV2(conflict.FilesToMerge, manualConflictsResolving); break;
                    case string a when a.Contains(".emevd.dcx"): EMEVD_DCX.MergeFiles(conflict.FilesToMerge); break; //WIP //to be tested
                    case string a when a.Contains(".msgbnd.dcx"): MSGBND_DCX.MergeFiles(conflict.FilesToMerge); break; //WIP //to be tested
                }
            }
        }
    }

    internal class FileToMerge
    {
        public string Path { get; set; }
        public string ModRelativePath { get; set; }

        public FileToMerge(string path)
        {
            Path = path;
            var splittedPath = Path.Split('\\');

            var toSkip = ModsMergerConfig.LoadedConfig.CurrentProfile.ModsToMergeFolderPath.Split("\\").Count() +1;

            ModRelativePath = splittedPath.Skip(toSkip).ToString("\\");
        }
    }

    internal class FileConflict
    {
        public List<FileToMerge> FilesToMerge { get; set; }

        public bool SupportedFormat { get; set; }

        public FileConflict(List<FileToMerge> filesToMerge)
        {
            FilesToMerge = filesToMerge;

            switch(FilesToMerge[0].ModRelativePath)
            {
                case string a when a.Contains("regulation.bin"): SupportedFormat = true; break;
                case string a when a.Contains(".msgbnd.dcx"): SupportedFormat = true; break;
                //case string a when a.Contains(".emevd.dcx"): SupportedFormat = true; break;
                default: SupportedFormat = false; break;
            }

        }

    }
}
