using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public class ExportCommand : Command
    {
        public string CollectionName { get; set; }
        public string SearchQueryForExport { get; set; } = "{}";
        public string TimePrefix { get; set; } = buildPrefix();

        private static string buildPrefix()
        {
            // assuming 1 export for same collection every 100 s is enough to sort imports later
            long ticksPerSecond = 10000000;
            return (DateTime.UtcNow.Ticks / ticksPerSecond / 100).ToString();
        }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(Connection))
            {
                throw new ArgumentException("Connection parameter required for export command");
            }
            if (string.IsNullOrEmpty(CollectionName))
            {
                throw new ArgumentException("CollectionName parameter required for export command");
            }
        }

        protected override Dictionary<string, Action<Command, string>> ParsingRules()
        {
            var r = base.ParsingRules();
            r.Add("--collection", (c, a) => (c as ExportCommand).CollectionName = a);
            r.Add("--query", (c, arg) => (c as ExportCommand).SearchQueryForExport = arg);
            return r;
        }

        public static string COMMAND = "mongoexport";

        public override void Execute()
        {
            ConnectionString cs = new ConnectionString(Connection);
            string argument = $"{cs.ToCommandLine()} --collection {CollectionName} --query {SearchQueryForExport} --type json " +
                // assuming no chance to generate more than 1 file per 10 seconds
                $"--out {TimePrefix}.{CollectionName}.Insert.json";

            Program.Exec(COMMAND, argument);
        }
    }
}
