using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace dcw
{
    struct WrapRequest
    {
        public string Compiler;
        public string[] SourceFiles;
        public string PassThroughOptions;

        public WrapRequest(string compiler, string[] sourceFiles, string passThroughOptions)
        {
            Compiler = compiler;
            SourceFiles = sourceFiles;
            PassThroughOptions = passThroughOptions;
        }

        public override string ToString()
        {
            StringBuilder wrapRequestStr = new StringBuilder();
            
            wrapRequestStr.Append("{ Compiler: ");
            wrapRequestStr.Append(Compiler);
            wrapRequestStr.Append(", Source Files: { ");
            SourceFiles.ToList().ForEach(sourceFile => wrapRequestStr.Append(sourceFile).Append(", "));
            wrapRequestStr.Remove(wrapRequestStr.Length - 2, 1);
            wrapRequestStr.Append("}, Pass Through: '");
            wrapRequestStr.Append(PassThroughOptions);
            wrapRequestStr.Append("' }");

            return wrapRequestStr.ToString();
        }
    }

    // This is a class that sets up the arguments to be processed
    // It's a singleton. There should only be one.
    // Doesn't need to be, but I thought it'd be fun
    class ArgumentProcessor
    {
        private static ArgumentProcessor _instance = null;
        private string[] _arguments;

        private ArgumentProcessor() {}

        public static ArgumentProcessor GetInstance()
        {
            if(_instance == null) 
                _instance = new ArgumentProcessor();

            return _instance;
        }

        public void SetArguments(string[] arguments)
        {
            _arguments = arguments;
        }

        public WrapRequest GetRequest()
        {
            if(_arguments.Length < 3)
            {
                Console.WriteLine(ErrorCodes.Strings[ErrorCodes.Codes.NOT_ENOUGH_ARGS]);
                Environment.Exit((int) ErrorCodes.Codes.NOT_ENOUGH_ARGS);
            }

            // The three parts of the request
            string compiler = _arguments[1];
            List<string> sourceFiles = new List<string>();
            StringBuilder passThroughOptions = new StringBuilder();

            // Get source files
            int i = 1;
            while(_arguments[i] != "-cf")
            {
                //Console.WriteLine(arguments[i]);
                sourceFiles.Add(_arguments[i++]);

                if(i >= _arguments.Length)
                    break;
            }

            i++;
            while(i < _arguments.Length)
            {
                passThroughOptions.Append(_arguments[i++]);
                passThroughOptions.Append(' ');
            }

            return new WrapRequest(compiler, sourceFiles.ToArray(), passThroughOptions.ToString());
        }

        public override string ToString()
        {
            StringBuilder argProcessorStr = new StringBuilder();

            argProcessorStr.Append("Arguments: { ");
            _arguments.ToList().ForEach(arg => argProcessorStr.Append(arg).Append(", "));
            argProcessorStr.Remove(argProcessorStr.Length - 2, 1);
            argProcessorStr.Append("}");

            return argProcessorStr.ToString();
        }
    }
}