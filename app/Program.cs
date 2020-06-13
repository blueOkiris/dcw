using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace dcw
{
    public class Program
    {
        static void Main(string[] args)
        {
            ArgumentProcessor argCompiler = ArgumentProcessor.GetInstance();
            argCompiler.SetArguments(args);
            Console.WriteLine(argCompiler.ToString());

            WrapRequest wrapRequest = argCompiler.GetRequest();
            Console.WriteLine("Application Wrap Request: {0}", wrapRequest.ToString());
            
            NewCodeMaker.GenerateNewCode(wrapRequest);
        }
    }

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
        private static ArgumentProcessor instance = null;
        private string[] arguments;

        private ArgumentProcessor() {}

        public static ArgumentProcessor GetInstance()
        {
            if(instance == null) 
                instance = new ArgumentProcessor();

            return instance;
        }

        public void SetArguments(string[] arguments)
        {
            this.arguments = arguments;
        }

        public WrapRequest GetRequest()
        {
            if(arguments.Length < 3)
            {
                Console.WriteLine(ErrorCodes.Strings[ErrorCodes.Codes.NOT_ENOUGH_ARGS]);
                Environment.Exit((int) ErrorCodes.Codes.NOT_ENOUGH_ARGS);
            }

            // The three parts of the request
            string compiler = arguments[1];
            List<string> sourceFiles = new List<string>();
            StringBuilder passThroughOptions = new StringBuilder();

            // Get source files
            int i = 1;
            while(arguments[i] != "-cf")
            {
                //Console.WriteLine(arguments[i]);
                sourceFiles.Add(arguments[i++]);

                if(i >= arguments.Length)
                    break;
            }

            i++;
            while(i < arguments.Length)
            {
                passThroughOptions.Append(arguments[i++]);
                passThroughOptions.Append(' ');
            }

            return new WrapRequest(compiler, sourceFiles.ToArray(), passThroughOptions.ToString());
        }

        public override string ToString()
        {
            StringBuilder argProcessorStr = new StringBuilder();

            argProcessorStr.Append("Arguments: { ");
            arguments.ToList().ForEach(arg => argProcessorStr.Append(arg).Append(", "));
            argProcessorStr.Remove(argProcessorStr.Length - 2, 1);
            argProcessorStr.Append("}");

            return argProcessorStr.ToString();
        }
    }
}
