using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Markup;
using static SoulsFormats.MSB.Shape.Composite;

namespace ERModsMerger.Core.Utility.LuaUtils
{
    internal class LuaParser
    {
        public LuaNode RootNode { get; set; }

        public LuaParser(string luaFilePath)
        {
            var rawLua = File.ReadAllLines(luaFilePath);
            var rawCleaned = CleanLuaLines(rawLua);
            RootNode = BuildNodesTree(new LuaNode(Path.GetFileName(luaFilePath)), rawCleaned);
        }

        private LuaNode BuildNodesTree(LuaNode mainNode, List<string> lines)
        {
            var currentParentNode = mainNode;
            for (int i = 0; i < lines.Count; i++)
                currentParentNode = currentParentNode.AddChildAndReturnParentForTheFollowingNode(lines[i]);

            return currentParentNode.GetRoot();
        }

        private List<string> CleanLuaLines(string[] lines)
        {
            var luaLines = lines.ToList();
            var regexCleanComments = new Regex("^((-{2,})|(( +)(-{2,})))"); //match all strings staring with "--" or strings staring with space(s) and followed by "--"
            var regexCleanWhiteSpaces = new Regex("^( {0,})$");
            

            for (int i = 0; i < luaLines.Count; i++)
            {
                //delete white spaces at start / end of string
                luaLines[i] = luaLines[i].Trim();

                //delete comments at end of line
                var indexOfComment = luaLines[i].IndexOf("--");
                if(indexOfComment != -1)
                    luaLines[i] = luaLines[i].Substring(0, indexOfComment);

            }
            //remove all comments and blank lines
            luaLines.RemoveAll(x => regexCleanComments.IsMatch(x) || regexCleanWhiteSpaces.IsMatch(x)); //remove all comments and blank lines
            return luaLines;
        }

        
    }

    internal class LuaNode
    {


        public LuaNode? ParentNode { get; private set; }
        public List<LuaNode>? ChildNodes { get; private set; }
        public string Name { get; set; }


        public LuaNode(string name)
        {
            Name = name;
        }

        public LuaNode()
        {
            Name = "";
        }

        public void AddChild(LuaNode node)
        {
            node.ParentNode = this;
            if( ChildNodes == null )
                ChildNodes = new List<LuaNode>();
            ChildNodes.Add(node);
        }

        public virtual LuaNode? AddChildAndReturnParentForTheFollowingNode(string node)
        {
            LuaNode? nextParent = this;

            /*
            * regexs
            * 
            * func     (^(function ))|(= function)
            * if       (^(if ))|( if )
            * for      (^(for ))|( for )
            * while    (^(while ))|( while )
            * 
            */

            Regex regexFunction = new Regex("(^(function ))|(= function)");
            Regex regexCondition = new Regex("(^(if ))|( if )");
            Regex regexLoop = new Regex("(^(for ))|( for )|(^(while ))|( while )");
            Regex regexVariable = new Regex(@"((local ){0,})(\S+) = (-{0,})((\d+|\w+)|({(.{0,})}$))");
            Regex regexVariableArray = new Regex(@"((local ){0,})(\S+) = {");

            LuaNode? luaNode = null;
            switch (node)
            {
                case string a when regexFunction.IsMatch(a): luaNode = new LuaFunctionDeclaration(a); nextParent = luaNode; break;
                case string a when regexCondition.IsMatch(a): luaNode = new LuaCondition(a); nextParent = luaNode; break;
                case string a when regexLoop.IsMatch(a): luaNode = new LuaLoop(a); nextParent = luaNode; break;
                case string a when regexVariable.IsMatch(a): luaNode = new LuaVar(a.Split('=')[0].Trim(), a.Split('=')[1].Trim()); break;
                case string a when regexVariableArray.IsMatch(a): luaNode = new LuaVarArray(a.Split('=')[0].Trim()); nextParent = luaNode; break;
            }

            if(luaNode == null)
                luaNode = new LuaNode(node);

            AddChild(luaNode);

            if (node == "end")
            {
                if(luaNode != null && luaNode.ParentNode != null)
                    luaNode.ParentNode.RefineNode();

                nextParent = ParentNode;
            }
                

            return nextParent;
        }

        public virtual void RefineNode()
        {

        }

        public void CreateChildsNodes(string luaCode)
        {
            //testing
            Regex regexFunc = new Regex("(^(function ))|(= function)|( function )");
            Regex regexNodeStartNames = new Regex("(^(if ))|( if )|(^(for ))|( for )|(^(while ))|( while )");
            Regex regexVariable = new Regex(@"((local ){0,})(\S+) = (-{0,})((\d+|\w+)|({(.{0,})}$))");
            Regex regexVariableArray = new Regex(@"((local ){0,})(\S+) = {");
            Regex regexEndNode = new Regex(@"( end )");

            int currentCharIndex = 0;
            int maxCharIndex = luaCode.Length;

            var currentChildNode = new LuaNode();
            while (currentCharIndex < maxCharIndex)
            {
                Dictionary<string, int> regexCharIndexes = new Dictionary<string, int>();
                regexCharIndexes.Add("RegexFunc", regexFunc.Match(luaCode.Substring(currentCharIndex)).Index);
                regexCharIndexes.Add("regexNodeStartNames", regexNodeStartNames.Match(luaCode.Substring(currentCharIndex)).Index);
                regexCharIndexes.Add("regexVariable", regexVariable.Match(luaCode.Substring(currentCharIndex)).Index);
                regexCharIndexes.Add("regexVariableArray", regexVariableArray.Match(luaCode.Substring(currentCharIndex)).Index);
                regexCharIndexes.Add("regexEndNode", regexEndNode.Match(luaCode.Substring(currentCharIndex)).Index);

                
                int charsToSkip = regexCharIndexes.Min().Value;
                currentChildNode.Name += luaCode.Substring(currentCharIndex, charsToSkip);

                switch (regexCharIndexes.Min())
                {
                    case KeyValuePair<string, int> a when a.Key == "RegexFunc": break;
                }
            }
        }


        public LuaNode GetRoot()
        {
            if (ParentNode == null)
                return this;

            var currentParent = ParentNode;
            while( currentParent.ParentNode != null ) 
                currentParent = currentParent.ParentNode;

            return currentParent;
        }
    }

    internal class LuaFunctionDeclaration : LuaNode
    {
        public string FunctionName { get; set; }
        public LuaFunctionDeclaration(string name) : base(name)
        {
            Name = name;
            FunctionName = name.Substring(0, name.IndexOf('(')).Replace("function", "").Replace(" ", "");
        }

        public LuaFunctionDeclaration()
        {
            FunctionName = "";
        }
    }

    internal class LuaCondition : LuaNode
    {

        public LuaCondition(string name) : base(name)
        {
            Name = name;
        }

        public override void RefineNode()
        {
            base.RefineNode();

            if(!Name.Contains(" then"))
            {
                int rangeToDel = 0;
                for(int i = 0; i <ChildNodes.Count; i++)
                {
                    Name += " " + ChildNodes[i].Name;
                    rangeToDel++;
                    if (ChildNodes[i].Name.Contains(" then"))
                        break;
                }
                ChildNodes.RemoveRange(0, rangeToDel);
            }

        }
    }

    internal class LuaLoop : LuaNode
    {

        public LuaLoop(string name) : base(name)
        {
            Name = name;
        }
    }


    internal class LuaVar : LuaNode
    {
        public string Value { get; set; }

        public LuaVar(string name, string value) : base(name)
        {
            Name = name;
            Value = value;
        }
    }

    internal class LuaVarArray : LuaNode
    {
        public List<string> Values { get; set; }

        public LuaVarArray(string name, List<string>? values = null) : base(name)
        {
            Name = name;

            if(values != null)
                Values = values;
            else
                Values = new List<string>();
        }

        public override LuaNode? AddChildAndReturnParentForTheFollowingNode(string node)
        {
            if (node == "}")
                return ParentNode;

            Values.Add(node);

            return this;
        }
    }
}
