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
            Program.Console("SocialTalents.MongoSync.Console <command> --uri Connection [--file file] [--collection collection] [--query 'query'] [--authenticationDatabase admin]");
            Program.Console("command        help    Display help");
            Program.Console("               Import  process file(s) specified");
            Program.Console("               Export  Export collection to file (using optional query)");
            Program.Console("connection     mongodb Connection String (mongo+srv not supported)");
            Program.Console("file           file or files to use, e.g. countries.json or *.json");
            Program.Console("               File name format: [Order].[Collection].[ImportMode].json");
            Program.Console("query          Query to use to export data, default is '{}'");
            Program.Console("authenticationDatabase          Database to use for authentication");
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
