using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace dcw
{
    class NewCodeMaker
    {
        private static Regex _moduleDef = new Regex(
            @"module\s*\(\s*[A-Za-z0-9_]+\s*(,\s*[A-Za-z0-9_]+\s*)*\s*\)\s*;",
            RegexOptions.Compiled
        );
        private static Regex _importRegex = new Regex(
            @"import\s*\(\s*[A-Za-z0-9_]+\s*\)\s*;",
            RegexOptions.Compiled
        );
        private static Regex _lineCommentRegex = new Regex(
            @"\/\/.*\n",
            RegexOptions.Compiled
        );

        private static Module[] getModules(string[] fileNames)
        {
            List<Module> moduleList = new List<Module>();

            Console.WriteLine("Loading modules:");
            foreach(string fileName in fileNames)
                moduleList.Add(new Module(fileName));

            return moduleList.ToArray();
        }

        public static void GenerateNewCode(WrapRequest request)
        {
            Module[] modules = getModules(request.SourceFiles);

            // Files to output
            if(!Directory.Exists("headers"))
                Directory.CreateDirectory("headers");
            if(!Directory.Exists("obj"))
                Directory.CreateDirectory("obj");

            foreach(Module module in modules)
            {
                Console.WriteLine(module.ToString());

                // Get all the different "things" in the code
                string code = File.ReadAllText(module.Source);
                
                // Remove comments
                MatchCollection comments = _lineCommentRegex.Matches(code);
                foreach(Match comment in comments)
                    code = code.Replace(comment.Value, "");

                FunctionDefinition[] funcDefs = Parser.parseFunctions(code, module.Name);
                StructDefinition[] structDefs = Parser.parseStructs(code, module.Name);
                MacroDefinition[] defs = Parser.parseMacros(code, module.Name);

                saveNewHeader(module, funcDefs, structDefs, defs);
                saveNewCCode(module, code, structDefs, defs);
            }
        }

        private static void saveNewCCode(Module module, string sourceCode, StructDefinition[] structDefs, MacroDefinition[] defs)
        {
            // Recreate source code
            StringBuilder newCCode = new StringBuilder();
            string code = sourceCode;

            // Remove module lines
            Match moduleLine = _moduleDef.Match(code);
            code = code.Replace(moduleLine.Value, "");
            
            // Replace all import statements
            // If it's not in our list of sources, then simply convert to #include <...>
            // If it is in our list of sources, then convert it to #include <headers/...>
            MatchCollection importStmts = _importRegex.Matches(code);
            foreach(Match importStmt in importStmts)
                code = code.Replace(importStmt.Value, "");
            foreach(string import in module.Imports)
            {
                newCCode.Append("#include <");
                newCCode.Append(import);
                newCCode.Append(".h>\n\n");
            }

            foreach(StructDefinition structDef in structDefs)
            {
                if(module.Exports.Contains(structDef.Name))
                    code = code.Replace(structDef.Source, "");
            }

            foreach(MacroDefinition def in defs)
            {
                if(module.Exports.Contains(def.Name))
                    code = code.Replace(def.Source, "");
            }
            
            newCCode.Append(code);

            File.WriteAllText("obj/" + module.Name + ".c", newCCode.ToString());
        }

        private static void saveNewHeader(Module module, FunctionDefinition[] funcDefs, StructDefinition[] structDefs, MacroDefinition[] defs)
        {
            //if(module.Exports.Length < 1)
            //    return;

            // Start header with include guard
            StringBuilder newHeaderCode = new StringBuilder("#ifndef _");
            newHeaderCode.Append(Path.GetFileNameWithoutExtension(module.Source).ToUpper());
            newHeaderCode.Append("_H_\n#define _");
            newHeaderCode.Append(Path.GetFileNameWithoutExtension(module.Source).ToUpper());
            newHeaderCode.Append("_H_\n#endif\n\n");

            // Save the function headers to header
            foreach(FunctionDefinition func in funcDefs)
            {
                if(!module.Exports.Contains(func.Name))
                    continue;

                StringBuilder funcHeader = new StringBuilder();

                funcHeader.Append(func.ReturnType);
                funcHeader.Append(' ');
                funcHeader.Append(func.Name);
                funcHeader.Append("(");
                func.Parameters.ToList().ForEach(tup => funcHeader.Append(tup.Item1).Append(' ').Append(tup.Item2).Append(", "));
                funcHeader.Remove(funcHeader.Length - 2, 2);
                funcHeader.Append(");");

                newHeaderCode.Append(funcHeader);
                newHeaderCode.Append("\n");
            }
            newHeaderCode.Append("\n");

            foreach(StructDefinition structDef in structDefs)
            {
                if(module.Exports.Contains(structDef.Name))
                    newHeaderCode.Append(structDef.Source);
            }

            foreach(MacroDefinition def in defs)
            {
                if(module.Exports.Contains(def.Name))
                    newHeaderCode.Append(def.Source);
            }

            File.WriteAllText("headers/" + module.Name + ".h", newHeaderCode.ToString());
        }
    }
}