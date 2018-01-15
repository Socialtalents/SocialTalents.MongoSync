using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public enum CommandType
    {
        None = 0,
        Insert,
        Upsert,
        Merge,
        Export,
        Help    
    }
}
