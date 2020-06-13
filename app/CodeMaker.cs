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
        private static Regex _functionRegex = new Regex(
            @"[A-Za-z_]+[A-Za-z0-9_]*(\*|\s)+[A-Za-z_]+[A-Za-z0-9_]*\s*\(\s*([A-Za-z_]+[A-Za-z0-9_]*(\*|\s)+[A-Za-z_]+[A-Za-z0-9_]*(\s*,\s*[A-Za-z_]+[A-Za-z0-9_]*(\*|\s)+[A-Za-z_]+[A-Za-z0-9_]*\s*)*)?\s*\)\s*{",
            RegexOptions.Compiled
        );
        private static Regex _structRegex = new Regex(
            @"struct\s*[A-Za-z_]+[A-Za-z0-9_]*\s*{\s*(\s*.*;\s*)*\s*}\s*;",
            RegexOptions.Compiled
        );
        private static Regex _typedefRegex = new Regex(
            @"typedef\s*(\s*.\s*)*\s*[A-Za-z_]+[A-Za-z_0-9]*\s*;",
            RegexOptions.Compiled
        );
        private static Regex _variableRegex = new Regex(
            @"[A-Za-z_]+[A-Za-z0-9_]*\s*[A-Za-z_]+[A-Za-z0-9_]*(\s*=\s*.*)?(\s*,\s*[A-Za-z_]+[A-Za-z0-9_]*(\s*=\s*.*)?\s*)*;",
            RegexOptions.Compiled
        );
        private static Regex _defineRegex = new Regex(
            @"#define\s*.+\n",
            RegexOptions.Compiled
        );
        private static Regex _importRegex = new Regex(
            @"import\s*\(\s*[A-Za-z0-9_]+\s*\)",
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

                // Have to remove others so functions can be parsed
                FunctionDefinition[] funcDefs = parseFunctions(code, module.Name);

                saveNewHeader(module, funcDefs);
                saveNewCCode(module, code);
            }
        }

        private static void saveNewCCode(Module module, string sourceCode)
        {
            // Recreate source code
            StringBuilder newCCode = new StringBuilder();
            string code = sourceCode;
            
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
            
            newCCode.Append(code);

            File.WriteAllText("obj/" + module.Name + ".c", newCCode.ToString());
        }

        private static void saveNewHeader(Module module, FunctionDefinition[] funcDefs)
        {
            if(module.Exports.Length < 1)
                return;

            // Start header with include guard
            StringBuilder newHeaderCode = new StringBuilder("#ifndef _");
            newHeaderCode.Append(Path.GetFileNameWithoutExtension(module.Source).ToUpper());
            newHeaderCode.Append("_H_\n#define _");
            newHeaderCode.Append(Path.GetFileNameWithoutExtension(module.Source).ToUpper());
            newHeaderCode.Append("_H_\n#endif\n\n");

            // Save the function headers to header
            foreach(FunctionDefinition func in funcDefs)
            {
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

            File.WriteAllText("headers/" + module.Name + ".h", newHeaderCode.ToString());
        }

        private static FunctionDefinition[] parseFunctions(string code, string name)
        {
            MatchCollection functions = _functionRegex.Matches(code);
            List<FunctionDefinition> funcDefs = new List<FunctionDefinition>();

            Console.WriteLine(functions.Count + " functions in " + name + ":");
            foreach(Match func in functions)
            {
                // Parse the functions
                Console.WriteLine(func.Value);
                funcDefs.Add(new FunctionDefinition(func.Value));
                Console.WriteLine(funcDefs[funcDefs.Count - 1].ToString());
            }

            return funcDefs.ToArray();
        }
    }
}