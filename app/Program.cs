﻿using System;

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
            Builder.BuildObjects(wrapRequest);
        }
    }
}
