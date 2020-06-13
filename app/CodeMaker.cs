using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace dcw
{
    struct Module
    {
        public string Name;
        public string[] Exports;
        public string[] Imports;

        public Module(string fileName)
        {
            string name = "", code = "", moduleString = "";
            List<string> exports = new List<string>();
            List<string> imports = new List<string>();

            Regex moduleDef = new Regex(
                @"module\s*\(\s*[A-Za-z0-9_]+\s*(,\s*[A-Za-z0-9_]+\s*)*\s*\)",
                RegexOptions.Compiled
            );

            Regex importDef = new Regex(
                @"import\s*\(\s*[A-Za-z0-9_]+\s*\)",
                RegexOptions.Compiled
            );

            if(!File.Exists(fileName))
            {
                Console.WriteLine(ErrorCodes.Strings[ErrorCodes.Codes.SOURCE_NOT_FOUND]);
                Console.WriteLine("Source: " + fileName);
                Environment.Exit((int) ErrorCodes.Codes.SOURCE_NOT_FOUND);
            }

            try
            {
                code = File.ReadAllText(fileName);
            }
            catch(IOException ioe)
            {
                Console.WriteLine(ErrorCodes.Strings[ErrorCodes.Codes.FILE_IO_ERR]);
                Console.WriteLine("Source: " + fileName);
                Console.WriteLine("Error: " + ioe.Message);
                Environment.Exit((int) ErrorCodes.Codes.FILE_IO_ERR);
            }

            // Get the module definition
            MatchCollection moduleDefs = moduleDef.Matches(code);

            if(moduleDefs.Count != 1)
            {
                Console.WriteLine(ErrorCodes.Strings[ErrorCodes.Codes.NOT_ONE_MODULE]);
                Console.WriteLine("Source: " + fileName);
                Environment.Exit((int) ErrorCodes.Codes.NOT_ONE_MODULE);
            }

            // Will just be one
            foreach(Match match in moduleDefs)
                moduleString = match.Value;
            
            // Get the name
            int parenInd = moduleString.IndexOf('(');
            int firstComma = moduleString.IndexOf(',');
            
            if(firstComma != -1)
            {
                //Console.WriteLine("First Comma: " + firstComma + ", First parenth: " + parenInd + ", Length: " + (firstComma - parenInd));
                name = moduleString.Substring(parenInd + 1, firstComma - parenInd - 1).Trim();

                // Get the export names
                string[] names = moduleString.Substring(firstComma + 1).Replace(")", " ").Trim().Split(",");
                foreach(string n in names)
                    exports.Add(n.Trim());
            }
            else        // No export (probably main )
            {
                int par2 = moduleString.IndexOf(')');
                name = moduleString.Substring(parenInd + 1, par2 - parenInd - 1).Trim();
            }

            // Find all import definitions
            MatchCollection importMatches = importDef.Matches(code);
            foreach(Match importMatch in importMatches)
            {
                string importStrRaw = importMatch.Value;

                int par1 = importStrRaw.IndexOf('(');
                int par2 = importStrRaw.IndexOf(')');

                imports.Add(importStrRaw.Substring(par1 + 1, par2 - par1 - 1).Trim());
            }

            Name = name;
            Imports = imports.ToArray();
            Exports = exports.ToArray();
        }

        public override string ToString() 
        {
            StringBuilder moduleStr = new StringBuilder();

            moduleStr.Append("Module: { Name: ");
            moduleStr.Append(Name);
            moduleStr.Append(", Imports: { ");
            Imports.ToList().ForEach(importStr => moduleStr.Append(importStr).Append(", "));
            moduleStr.Remove(moduleStr.Length - 2, 1);
            moduleStr.Append("}, Exports: { ");
            Exports.ToList().ForEach(exportStr => moduleStr.Append(exportStr).Append(", "));
            moduleStr.Remove(moduleStr.Length - 2, 1);
            moduleStr.Append("} }");

            return moduleStr.ToString();
        }
    }

    class NewCodeMaker
    {
        private static Module[] getModules(string[] fileNames)
        {
            List<Module> moduleList = new List<Module>();

            foreach(string fileName in fileNames)
                moduleList.Add(new Module(fileName));

            return moduleList.ToArray();
        }

        public static void GenerateNewCode(WrapRequest request)
        {
            Module[] modules = getModules(request.SourceFiles);

            foreach(Module module in modules)
                Console.WriteLine(module.ToString());
        }
    }
}