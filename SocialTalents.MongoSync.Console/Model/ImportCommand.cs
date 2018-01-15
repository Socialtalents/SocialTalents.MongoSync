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

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(Connection) && !string.IsNullOrEmpty(File); 
        }
    }
}
