using SocialTalents.MongoSync.Console.Model;
using System;

namespace SocialTalents.MongoSync.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Command cmd = ParseCommand(args);
            cmd.Execute();

        }

        public static Action<string> Console = (s) => System.Console.WriteLine(s);

        public static Command ParseCommand(params string[] args)
        {
            Command result = new HelpCommand();
            // try parse arguments only when they exist
            if (args != null && args.Length > 0)
            {
                try
                {
                    Command candidate = new HelpCommand();
                    CommandType cmdType = (CommandType)Enum.Parse(typeof(CommandType), args[0], true);
                    switch (cmdType)
                    {
                        case CommandType.Help: candidate = new HelpCommand(); break;
                        case CommandType.Insert:
                        case CommandType.Merge:
                        case CommandType.Upsert:
                            candidate = new ImportCommand(); break;
                        case CommandType.Export:
                            candidate = new ExportCommand(); break;
                        default: throw new NotImplementedException();
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
