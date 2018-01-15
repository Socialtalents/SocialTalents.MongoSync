using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public class Command
    {
        public string Connection { get; set; }
        public CommandType CommandType { get; set; }

        public string File { get; set; }
        public string Collection { get; set; }
        public string SearchQueryForExport { get; set; }

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public virtual void Parse(string[] args)
        {
            string lastSegment = null;
            foreach(var parameter in args)
            {
                if (lastSegment == null)
                {
                    string key = parameter.ToLower();
                    if (Parsing.ContainsKey(key))
                    {
                        lastSegment = key;
                    }
                }
                else
                {
                    try
                    {
                        Parsing[lastSegment](this, parameter);
                    }
                    catch (Exception ex)
                    {
                        Program.Console($"Cannot parse {lastSegment} parameter: {ex.Message}");
                    }
                }
            }
        }

        public virtual bool IsValid()
        {
            return true;
        }

        private static Dictionary<string, Action<Command, string>> Parsing = new Dictionary<string, Action<Command, string>>();

        static Command()
        {
            Parsing.Add("-c", (cmd, arg) => cmd.Connection = arg);
            Parsing.Add("-f", (cmd, arg) => cmd.File = arg);
            Parsing.Add("-q", (cmd, arg) => cmd.SearchQueryForExport = arg);
        }
    }
}
