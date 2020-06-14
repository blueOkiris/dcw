using System;
using System.Collections.Generic;
using System.Text;

namespace dcw
{
    partial class Parser
    {
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

        public static StructDefinition[] parseStructDefinitions(string code, string moduleName)
        {
            List<StructDefinition> structDefs = new List<StructDefinition>();

            return structDefs.ToArray();
        }
    }
}