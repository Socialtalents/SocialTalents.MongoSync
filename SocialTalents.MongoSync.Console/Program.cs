using SocialTalents.MongoSync.Console.Model;
using System;
using System.Linq;
using System.Diagnostics;

namespace SocialTalents.MongoSync.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Command cmd = ParseCommand(args);
            cmd.Validate();

            cmd.Execute();
        }

        public static Action<string> Console = (s) => System.Console.WriteLine(s);
        public static Func<string, string, int> Exec = (cmd, arg) =>
        {
            Console("executing mongo command...");
            Console($"{cmd} {arg}");

            ProcessStartInfo startInfo = new ProcessStartInfo(cmd, arg);
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process proc = new Process();
            proc.StartInfo = startInfo;
            proc.Start();

            var output = proc.StandardOutput.ReadToEnd();
            var error = proc.StandardError.ReadToEnd();

            Console($"Execution completed with state {proc.ExitCode}");
            Console(output);
            Console(error);
            return proc.ExitCode;
        };

        public static Command ParseCommand(params string[] args)
        {
            Command result = new HelpCommand();
            // try parse arguments only when they exist
            if (args != null && args.Length > 0)
            {
                try
                {
                    Command candidate = new HelpCommand();
                    CommandType cmdType = Enum.Parse<CommandType>(args[0], true);
                    switch (cmdType)
                    {
                        case CommandType.Help:
                            candidate = new HelpCommand();
                            break;
                        case CommandType.Import:
                            candidate = new ImportCommand();
                            break;
                        case CommandType.Export:
                            candidate = new ExportCommand();
                            break;
                        default: throw new NotImplementedException($"Command {cmdType} not implemented yet");
                    }
                    candidate.CommandType = cmdType;
                    candidate.Parse(args);
                    result = candidate;
                }
                catch (Exception ex)
                {
                    Console(ex.Message);
#if DEBUG
                    Console(ex.StackTrace);
#endif
                }
            }
            return result;
        }

        
    }
}
