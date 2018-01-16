using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public class SyncEntity
    {
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public string FileName { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Imported { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; } = false;
    }
}
