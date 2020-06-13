using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

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
                
                // Recreate source code
                StringBuilder newCCode = new StringBuilder();

                // Start header with include guard
                StringBuilder newHeaderCode = new StringBuilder("#ifndef _");
                newHeaderCode.Append(Path.GetFileNameWithoutExtension(module.Source).ToUpper());
                newHeaderCode.Append("_H_\n#define _");
                newHeaderCode.Append(Path.GetFileNameWithoutExtension(module.Source).ToUpper());
                newHeaderCode.Append("_H_\n#endif\n\n");
                
                // Replace all import statements
                // If it's not in our list of sources, then simply convert to #include <...>
                // If it is in our list of sources, then convert it to #include <headers/...>
                foreach(string import in module.Imports)
                {
                    newCCode.Append("#include <");
                    newCCode.Append(import);
                    newCCode.Append(".h>\n");
                }

                // Get all the different "things" in the code
                string code = File.ReadAllText(module.Source);

                MatchCollection functions = _functionRegex.Matches(code);
                List<FunctionDefinition> funcDefs = new List<FunctionDefinition>();

                Console.WriteLine(functions.Count + " functions in " + module.Name + ":");
                foreach(Match func in functions)
                {
                    Console.WriteLine(func.Value);
                    funcDefs.Add(new FunctionDefinition(func.Value));
                    Console.WriteLine(funcDefs[funcDefs.Count - 1].ToString());
                }

                // Save the new files
                if(module.Exports.Length > 0)
                    File.WriteAllText("headers/" + module.Name + ".h", newHeaderCode.ToString());

                File.WriteAllText("obj/" + module.Name + ".c", newCCode.ToString());
            }
        }
    }
}