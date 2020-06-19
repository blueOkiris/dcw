using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dcw
{
    partial class Parser
    {
        public static string[] parseGlobals(
            string code, Module module, 
            FunctionDefinition[] funcDefs, 
            StructDefinition[] structDefs, 
            MacroDefinition[] defs, 
            TypedefDefinition[] typedefs)
        {
            List<string> globals = new List<string>();

            for(int i = 0; i < code.Length; i++)
            {
                // We only want global scope!
                (string, int) lbrace = parsePhrase(code, "{", ref i);
                if(lbrace.Item2 != -1)
                {
                    int braceLevel = 0;
                    while(code[i] != '}' && braceLevel == 0)
                    {
                        if(code[i] == '{')
                            braceLevel++;
                        if(code[i] == '}')
                            braceLevel--;

                        i++;
                        if(i >= code.Length)
                        {
                            Console.WriteLine(ErrorCodes.Strings[ErrorCodes.Codes.MISSING_END_BRACKET]);
                            Environment.Exit((int) ErrorCodes.Codes.MISSING_END_BRACKET);
                        }
                    }
                    i++;
                }

                (string, int) typeStr = parseType(code, ref i);
                if(typeStr.Item2 == -1)
                    continue;
                
                (string, int) name = parseIdent(code, ref i);
                if(name.Item2 == -1)
                    continue;

                if(module.Exports.Contains(name.Item1)
                        && !funcDefs.ToList().Select(
                            (funcDef) => funcDef.Name).ToArray().Contains(name.Item1)
                        && !structDefs.ToList().Select(
                            (structDef) => structDef.Name).ToArray().Contains(name.Item1)
                        && !defs.ToList().Select(
                            (def) => def.Name).ToArray().Contains(name.Item1)
                        && !typedefs.ToList().Select(
                            (def) => def.Name).ToArray().Contains(name.Item1))
                    globals.Add(typeStr.Item1 + " " + name.Item1);
            }

            Console.WriteLine("There are " + globals.Count + " exported global variables in " + module.Name);

            return globals.ToArray();
        }

        public static FunctionDefinition[] parseFunctions(string code, string moduleName)
        {
            List<FunctionDefinition> funcDefs = new List<FunctionDefinition>();

            // type ident ( type ident, type ident, type ident ) { ... }
            for(int i = 0; i < code.Length; i++)
            {
                (string, int) type = parseType(code, ref i);
                if(type.Item2 == -1)                                // Found identifier
                    continue;

                (string, int) name = parseIdent(code, ref i);
                if(name.Item2 == -1)                                // Didn't find identifier
                    continue;

                (string, int) lParenth = parsePhrase(code, "(", ref i);
                if(lParenth.Item2 == -1)
                    continue;

                List<((string, int), (string, int))> parameters = new List<((string, int), (string, int))>();
                (string, int) rParenth = parsePhrase(code, ")", ref i);
                while(rParenth.Item2 == -1)
                {
                    if(i >= code.Length)
                    {
                        Console.WriteLine(ErrorCodes.Strings[ErrorCodes.Codes.MISSING_END_PARENTH]);
                        Console.WriteLine("Function: " + name);
                        Environment.Exit((int) ErrorCodes.Codes.MISSING_END_PARENTH);
                    }

                    parameters.Add(parseTypeIdent(code, ref i));
                    parsePhrase(code, ",", ref i);                  // We don't care if it fails, gcc will get this

                    rParenth = parsePhrase(code, ")", ref i);
                }

                // Finally, parse the code block
                (string, int) sourceBlock = parseCodeBlock(code, ref i);
                if(sourceBlock.Item2 == -1)
                    continue;

                // And that makes an entire function definition
                // Let's build it
                StringBuilder func = new StringBuilder();
                func.Append(type.Item1).Append(' ');
                func.Append(name.Item1).Append('(');
                parameters.ForEach(par => func.Append(par.Item1.Item1).Append(' ').Append(par.Item2.Item1).Append(", "));
                func.Remove(func.Length - 2, 1);
                func.Append(") ");
                func.Append(sourceBlock);

                funcDefs.Add(new FunctionDefinition(func.ToString()));
            }

            Console.WriteLine("There are " + funcDefs.Count + " functions in module " + moduleName);

            return funcDefs.ToArray();
        }

        public static StructDefinition[] parseStructs(string code, string moduleName)
        {
            List<StructDefinition> structDefs = new List<StructDefinition>();
            
            // struct <ident> { <body> } ;
            for(int i = 0; i < code.Length; i++)
            {
                int j = i;
                (string, int) keyword = parsePhrase(code, "struct", ref i);
                if(keyword.Item2 == -1)
                    continue;
                int structStart = j;

                (string, int) name = parseIdent(code, ref i);
                if(name.Item2 == -1)
                    continue;

                (string, int) body = parseCodeBlock(code, ref i);
                if(body.Item2 == -1)
                    continue;

                (string, int) semi = parsePhrase(code, ";", ref i);
                if(semi.Item2 == -1)
                    continue;

                structDefs.Add(new StructDefinition(code.Substring(structStart, i - structStart), name.Item1));
            }

            Console.WriteLine("There are " + structDefs.Count + " struct definitions in " + moduleName);

            return structDefs.ToArray();
        }

        public static MacroDefinition[] parseMacros(string code, string moduleName)
        {
            // TODO: parse preprocessor code
            List<MacroDefinition> defs = new List<MacroDefinition>();

            /*
             * macro ( <macro-code> , <ident> ) ;
             * Here, we'll do macro ( <body> ) ;
             * and throw an error if it fails
             */
            
            for(int i = 0; i < code.Length; i++)
            {
                int macroStart = i;

                (string, int) keyword = parsePhrase(code, "macro", ref i);
                if(keyword.Item2 == -1)
                    continue;

                (string, int) lParenth = parsePhrase(code, "(", ref i);
                if(lParenth.Item2 == -1)
                    continue;

                (string, int) name = parseIdent(code, ref i);
                if(name.Item2 == -1)
                    continue;

                (string, int) comma = parsePhrase(code, ",", ref i);
                if(lParenth.Item2 == -1)
                    continue;

                (string, int) body = parseMacroBody(code, ref i);
                if(body.Item2 == -1)
                    continue;

                (string, int) rParenth = parsePhrase(code, ")", ref i);
                if(rParenth.Item2 == -1)
                    continue;

                (string, int) semi = parsePhrase(code, ";", ref i);
                if(semi.Item2 == -1)
                    continue;
                
                defs.Add(new MacroDefinition(code.Substring(macroStart, i - macroStart), body.Item1, name.Item1));
            }

            Console.WriteLine("There are " + defs.Count + " macro definitions in " + moduleName);

            return defs.ToArray();
        }

        // Basically the same as parseMacros
        public static TypedefDefinition[] parseTypedefs(string code, string moduleName)
        {
            // TODO: parse preprocessor code
            List<TypedefDefinition> defs = new List<TypedefDefinition>();

            /*
             * macro ( <macro-code> , <ident> ) ;
             * Here, we'll do macro ( <body> ) ;
             * and throw an error if it fails
             */
            
            for(int i = 0; i < code.Length; i++)
            {
                int typedefStart = i;

                (string, int) keyword = parsePhrase(code, "typedef", ref i);
                if(keyword.Item2 == -1)
                    continue;

                (string, int) lParenth = parsePhrase(code, "(", ref i);
                if(lParenth.Item2 == -1)
                    continue;

                (string, int) name = parseIdent(code, ref i);
                if(name.Item2 == -1)
                    continue;

                (string, int) comma = parsePhrase(code, ",", ref i);
                if(lParenth.Item2 == -1)
                    continue;

                (string, int) body = parseMacroBody(code, ref i);
                if(body.Item2 == -1)
                    continue;

                (string, int) rParenth = parsePhrase(code, ")", ref i);
                if(rParenth.Item2 == -1)
                    continue;

                (string, int) semi = parsePhrase(code, ";", ref i);
                if(semi.Item2 == -1)
                    continue;
                
                defs.Add(new TypedefDefinition(code.Substring(typedefStart, i - typedefStart).Trim(), body.Item1, name.Item1));
            }

            Console.WriteLine("There are " + defs.Count + " macro definitions in " + moduleName);

            return defs.ToArray();
        }
    }
}