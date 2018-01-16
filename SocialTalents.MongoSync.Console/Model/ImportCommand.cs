using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public class ImportCommand : Command
    {
        public ImportCommand()
        {
            File = "*.json";
        }

        public override void Execute()
        {
            base.Execute();
        }

        protected override Dictionary<string, Action<Command, string>> ParsingRules()
        {
            var result = base.ParsingRules();
            result.Add("--file", (cmd, arg) => cmd.File = arg);
            return result;
        }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(Connection))
            {
                throw new ArgumentException("Connection parameter required for import command");
            }
            if (string.IsNullOrEmpty(File))
            {
                throw new ArgumentException("Connection parameter required for import command");
            }
        }
    }
}
