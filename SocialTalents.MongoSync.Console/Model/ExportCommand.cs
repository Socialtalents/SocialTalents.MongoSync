using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public class ExportCommand : Command
    {
        public string CollectionName { get; set; }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(Connection) && !string.IsNullOrEmpty(File) && !string.IsNullOrEmpty(SearchQueryForExport);
        }

        protected override Dictionary<string, Action<Command, string>> ParsingRules()
        {
            var r = base.ParsingRules();
            r.Add("--collection", (c, a) => (c as ExportCommand).CollectionName = a);
            return r;
        }
    }
}
