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
        public string Source;

        public Module(string fileName)
        {
            string name = "", code = "", moduleString = "";
            List<string> exports = new List<string>();
            List<string> imports = new List<string>();

            Regex moduleDef = new Regex(
                @"module\s*\(\s*[A-Za-z0-9_]+\s*(,\s*[A-Za-z0-9_]+\s*)*\s*\)\s*;",
                RegexOptions.Compiled
            );

            Regex importDef = new Regex(
                @"import\s*\(\s*[A-Za-z0-9_]+\s*\)\s*;",
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
            imports.Add(name);
            Imports = imports.ToArray();
            Exports = exports.ToArray();
            Source = fileName;
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
            moduleStr.Append("}, Source File: ");
            moduleStr.Append(Source);
            moduleStr.Append(" }");

            return moduleStr.ToString();
        }
    }

    struct FunctionDefinition
    {
        private static Regex _paramRegex = new Regex(
            @"[A-Za-z_]+[A-Za-z0-9_]*(\*|\s)+[A-Za-z_]+[A-Za-z0-9_]",
            RegexOptions.Compiled
        );

        public string ReturnType;
        public string Name;
        public (string, string)[] Parameters;
        public string SourceCode;

        // Parse a function definition
        public FunctionDefinition(string sourceCode)
        {
            SourceCode = sourceCode;

            // Match the first "type name" thing
            Match typeAndName = _paramRegex.Match(sourceCode);
            StringBuilder fName = new StringBuilder();
            StringBuilder fType = new StringBuilder();

            string fStr = typeAndName.Value;
            int i = 0;
            for(i = fStr.Length - 1; char.IsLetterOrDigit(fStr[i]) || fStr[i] == '_'; i--)
                fName.Append(fStr[i]);

            for(; i >= 0; i--)
                fType.Append(fStr[i]);

            Name = new string(fName.ToString().Reverse().ToArray());
            ReturnType = new string(fType.ToString().Reverse().ToArray());

            List<(string, string)> parameters = new List<(string, string)>();
            int end = sourceCode.IndexOf('{');
            int begin = sourceCode.IndexOf('(');
            MatchCollection parameterMatches = _paramRegex.Matches(sourceCode.Substring(begin, end - begin));
            foreach(Match param in parameterMatches)
            {
                StringBuilder parName = new StringBuilder();
                StringBuilder typeName = new StringBuilder();

                string parStr = param.Value;
                for(i = parStr.Length - 1; char.IsLetterOrDigit(parStr[i]) || parStr[i] == '_'; i--)
                    parName.Append(parStr[i]);

                for(; i >= 0; i--)
                    typeName.Append(parStr[i]);

                parameters.Add((new string(typeName.ToString().Reverse().ToArray()), new string(parName.ToString().Reverse().ToArray())));
            }

            Parameters = parameters.ToArray();
        }

        public override string ToString()
        {
            StringBuilder funcStr = new StringBuilder();

            funcStr.Append("Function: { Name: ");
            funcStr.Append(Name);
            funcStr.Append(", Returns: ");
            funcStr.Append(ReturnType);
            funcStr.Append(", Params: { ");
            Parameters.ToList().ForEach(tup => funcStr.Append("{ ").Append(tup.Item2).Append(" : ").Append(tup.Item1).Append(" }, "));
            funcStr.Remove(funcStr.Length - 2, 1);
            funcStr.Append("} }");

            return funcStr.ToString();
        }
    }

    // These two share code, but it's small, so don't alter them
    struct StructDefinition
    {
        public string Name;
        public string Source;

        public StructDefinition(string source, string name)
        {
            Source = source;
            Name = name;
        }

        public override string ToString()
        {
            return "Struct { Name: " + Name + ", Source: " + Source + " }";
        }
    }

    struct DefinitionDefinition
    {
        public string Name;
        public string Source;

        public DefinitionDefinition(string source, string name)
        {
            Source = source;
            Name = name;
        }

        public override string ToString()
        {
            return "Definition { Name: " + Name + ", Source: " + Source + " }";
        }
    }
}