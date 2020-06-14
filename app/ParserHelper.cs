using System;
using System.Collections.Generic;
using System.Text;

namespace dcw
{
    partial class Parser
    {
        private static (string, int) parseCodeBlock(string code, ref int i)
        {
            if(!passWhiteSpace(code, ref i))
                return ("", -1);

            (string, int) lBrace = parsePhrase(code, "{", ref i);
            if(lBrace.Item2 == -1)
                return ("", -1);

            StringBuilder block = new StringBuilder();
            block.Append('{');
            int braceLevel = 0;
            while(code[i] != '}' && braceLevel == 0)
            {
                block.Append(code[i]);
                
                if(code[i] == '{')
                    braceLevel++;
                if(code[i] == '}')
                    braceLevel--;

                i++;
                if(i >= code.Length)
                    return ("", -1);
            }
            block.Append('}');

            return (block.ToString(), lBrace.Item2);
        }

        private static ((string, int), (string, int)) parseTypeIdent(string code, ref int i)
        {
            (string, int) paramType = parseType(code, ref i);
            if(paramType.Item2 == -1)
                return (("", -1), ("", -1));

            (string, int) paramIdent = parseIdent(code, ref i);
            if(paramIdent.Item2 == -1)
                return (paramType, ("", -1));

            return (paramType, paramIdent);
        }

        private static (string, int) parsePhrase(string code, string phrase, ref int i)
        {
            if(!passWhiteSpace(code, ref i))
                return ("", -1);

            if(code.Substring(i).StartsWith(phrase))
            {
                i += phrase.Length;
                return (phrase, i - phrase.Length);
            }
            else
                return ("", -1);
        }

        private static (string, int) parseIdent(string code, ref int i)
        {
            if(!passWhiteSpace(code, ref i))
                return ("", -1);

            if(!char.IsLetter(code[i]) && code[i] != '_')
                return ("", -1);

            StringBuilder typeDef = new StringBuilder();
            int identStart = i;
            while(char.IsLetterOrDigit(code[i]) || code[i] == '_')
            {
                typeDef.Append(code[i]);
                
                i++;
                if(i >= code.Length)
                    break;
            }

            return (typeDef.ToString(), identStart);
        }

        private static (string, int) parseType(string code, ref int i)
        {
            if(!passWhiteSpace(code, ref i))
                return ("", -1);

            if(!char.IsLetter(code[i]) && code[i] != '_')
                return ("", -1);

            StringBuilder typeDef = new StringBuilder();
            int typeStart = i;
            while(char.IsLetterOrDigit(code[i]) || code[i] == '_')
            {
                typeDef.Append(code[i]);
                
                i++;
                if(i >= code.Length)
                    break;
            }

            while(!char.IsLetterOrDigit(code[i]) && code[i] != '_')
            {
                if(code[i] == '*')
                    typeDef.Append('*');
                else if(!char.IsWhiteSpace(code[i]))
                    return ("", -1);
                
                i++;
                if(i >= code.Length)
                    break;
            }

            return (typeDef.ToString(), typeStart);
        }

        private static bool passWhiteSpace(string code, ref int i)
        {
            if(i >= code.Length)
                return false;

            // Parse whitespace
            while(char.IsWhiteSpace(code[i]))
            {
                i++;
                if(i >= code.Length)
                    return false;
            }

            return true;
        }
    }
}