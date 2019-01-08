using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public enum ImportMode
    {
        Unknown = 0,
        Insert,
        Upsert,
        Merge,
        Delete,
        Drop,
        Eval,
        CreateIndex
    }
}
