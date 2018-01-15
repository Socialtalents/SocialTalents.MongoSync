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
        public string SearchQueryForExport { get; set; }

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public virtual void Parse(string[] args)
        {
            var parsingRules = ParsingRules();
            string lastSegment = null;
            foreach(var parameter in args)
            {
                if (lastSegment == null)
                {
                    string key = parameter.ToLower();
                    if (parsingRules.ContainsKey(key))
                    {
                        lastSegment = key;
                    }
                }
                else
                {
                    try
                    {
                        parsingRules[lastSegment](this, parameter);
                    }
                    catch (Exception ex)
                    {
                        Program.Console($"Cannot parse {lastSegment} parameter: {ex.Message}");
                        throw ex;
                    }
                    lastSegment = null;
                }
            }
        }

        public virtual bool IsValid()
        {
            throw new NotImplementedException("Need to be overriden");
        }

        protected virtual Dictionary<string, Action<Command, string>> ParsingRules()
        {
            var result = new Dictionary<string, Action<Command, string>>();
            result.Add("--conn", (cmd, arg) => cmd.Connection = arg);
            result.Add("--file", (cmd, arg) => cmd.File = arg);
            result.Add("--query", (cmd, arg) => cmd.SearchQueryForExport = arg);
            return result;
        }
    }
}
