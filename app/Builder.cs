using System;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace dcw
{
    class Builder
    {
        public static void BuildObjects(WrapRequest request)
        {
            string[] sourceFiles = Directory.GetFiles("obj/", "*.c");
            
            try
            {
                foreach(string fileName in sourceFiles)
                {
                    Process buildProcess = new Process();
                    buildProcess.StartInfo.FileName = "/bin/bash";

                    StringBuilder args = new StringBuilder();
                    args.Append("-c \"cd ");
                    args.Append(Path.GetFullPath(Directory.GetCurrentDirectory()).Replace(" ", "\\ "));
                    args.Append("; ");
                    args.Append(request.Compiler);
                    args.Append(" -Iheaders -c ");
                    args.Append(fileName).Append(' ');
                    args.Append(request.PassThroughOptions);
                    args.Append('"');

                    Console.WriteLine("/bin/bash " + args.ToString());

                    buildProcess.StartInfo.Arguments = args.ToString();
                    buildProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    buildProcess.Start();
                    buildProcess.WaitForExit();
                }
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