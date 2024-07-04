using DotNext.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ERModsMerger.Core
{
    internal class MergeableFilesDispatcher
    {
        List<FileToMerge> FilesToMerge {  get; set; }
        List<FileConflict> Conflicts { get; set; }

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
            string[] supportedFormats = { "regulation.bin", ".emevd.dcx" };

            foreach (var conflict in Conflicts)
            {
                switch(conflict.FilesToMerge[0].ModRelativePath)
                {
                    case string a when a.Contains("regulation.bin"): RegulationBin.MergeRegulations(conflict.FilesToMerge, manualConflictsResolving); break;
                    //case string a when a.Contains(".emevd.dcx"): Formats.EMEVD_DCX.MergeFiles(conflict.FilesToMerge); break; //WIP
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
            ModRelativePath = splittedPath.Skip(2).ToString("\\");
        }
    }

    internal class FileConflict
    {
        public List<FileToMerge> FilesToMerge { get; set; }

        public FileConflict(List<FileToMerge> filesToMerge)
        {
            FilesToMerge = filesToMerge;
        }

    }
}
