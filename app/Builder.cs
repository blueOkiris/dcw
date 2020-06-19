using System;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace dcw
{
    class Builder
    {
        public static void BuildObjects(WrapRequest request, ((string, string), (string, string))[] code)
        {            
            try
            {
                if(Directory.Exists("temp"))
                    Directory.Delete("temp", true);

                if(Directory.Exists("obj"))
                    Directory.Delete("obj", true);

                Directory.CreateDirectory("temp");
                Directory.CreateDirectory("temp/src");
                Directory.CreateDirectory("temp/headers");
                Directory.CreateDirectory("obj");

                // Copy files
                foreach(((string, string), (string, string)) headerSource in code)
                {
                    File.WriteAllText("temp/headers/" + headerSource.Item1.Item1, headerSource.Item1.Item2);
                    File.WriteAllText("temp/src/" + headerSource.Item2.Item1, headerSource.Item2.Item2);
                }

                string[] sourceFiles = Directory.GetFiles("temp/src/", "*.c");

                foreach(string fileName in sourceFiles)
                {
                    Process buildProcess = new Process();
                    buildProcess.StartInfo.FileName = "/bin/bash";

                    StringBuilder args = new StringBuilder();
                    args.Append("-c \"cd ");
                    args.Append(Path.GetFullPath(Directory.GetCurrentDirectory()).Replace(" ", "\\ "));
                    args.Append("; ");
                    args.Append(request.Compiler);
                    args.Append(" -Itemp/headers -c ");
                    args.Append(fileName).Append(' ');
                    args.Append(" -o obj/");
                    args.Append(Path.GetFileNameWithoutExtension(fileName)).Append(".o ");
                    args.Append(request.PassThroughOptions);
                    args.Append('"');

                    Console.WriteLine("/bin/bash " + args.ToString());

                    buildProcess.StartInfo.Arguments = args.ToString();
                    buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    buildProcess.Start();
                    buildProcess.WaitForExit();
                }

                Directory.Delete("temp", true);
            }
            catch(PlatformNotSupportedException pnse)
            {
                Console.WriteLine(ErrorCodes.Strings[ErrorCodes.Codes.LINUX_ERROR]);
                Console.WriteLine(pnse.Message);
                Environment.Exit((int) ErrorCodes.Codes.LINUX_ERROR);
            }
        }
    }
}