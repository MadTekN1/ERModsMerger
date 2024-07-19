using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERModsMerger.Core.Utility.LuaUtils
{
    internal class LuaCompare
    {
        public List<LuaNode> Additions {  get; set; }
        public List<LuaNode> Deletions {  get; set; }
        public List<LuaNode> Modifications {  get; set; }



        public LuaCompare() 
        {
            Additions = new List<LuaNode>();
            Deletions = new List<LuaNode>();
            Modifications = new List<LuaNode>();
        }

        public static LuaCompare Compare(LuaNode node1, LuaNode node2)
        {
            LuaCompare compare = new LuaCompare();

            var additions = new List<LuaNode>();
            GetAdditions(node1, node2, ref additions);
            compare.Additions = additions;

            var deletions = new List<LuaNode>();
            GetDeletions(node1, node2, ref deletions);
            compare.Deletions = deletions;

            return compare;
        }

        private static void GetAdditions(LuaNode node1, LuaNode node2, ref List<LuaNode> result)
        {
            if (node2.ChildNodes != null)
                foreach (var child in node2.ChildNodes)
                {
                    int indexFound = -1;
                    if(node1.ChildNodes != null)
                        indexFound = node1.ChildNodes.FindIndex(x => x.Name == child.Name);

                    if (indexFound != -1)//found
                    {
                        // found // search in sub nodes
                        GetAdditions(node1.ChildNodes[indexFound], child, ref result);
                    }
                    else
                    {
                        // not found // we add it to the addition list
                        result.Add(child);
                    }
                }
        }

        private static void GetDeletions(LuaNode node1, LuaNode node2, ref List<LuaNode> result)
        {

            if (node1.ChildNodes != null)
                foreach (var child in node1.ChildNodes)
                {
                    int indexFound = -1;
                    if (node2.ChildNodes != null)
                        indexFound = node2.ChildNodes.FindIndex(x => x.Name == child.Name);

                    if (indexFound != -1)//found
                    {
                        // found // search in sub nodes
                        GetDeletions(child, node2.ChildNodes[indexFound], ref result);
                    }
                    else
                    {
                        // not found // we add it to the deletion list
                        result.Add(child);
                    }
                }
        }
    }
}
