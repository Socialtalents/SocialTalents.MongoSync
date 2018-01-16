using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public class HelpCommand : Command
    {
        public override void Execute()
        {
            Program.Console("Usage:");
            Program.Console("SocialTalents.MongoSync.Console <command> --conn Connection [--file file] [--collection collection] [--query 'query']");
            Program.Console("command        help    Display help");
            Program.Console("               Insert  insert file(s) specified");
            Program.Console("               Upsert  Upsert file(s) specified");
            Program.Console("               Merge   Merge file(s) specified");
            Program.Console("               Export  Export collection to file (using optional query)");
            Program.Console("connection     mongodb Connection String ");
            Program.Console("file           file or files to use, e.g. countries.json or *.json ");
            Program.Console("query          Query to use to select data, default is '{}'");
        }

        public override void Parse(string[] args)
        {
            // no need to parse anything for help command
        }

        public override void Validate()
        {
            
        }
    }
}
