using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public class ImportCommand : Command
    {
        public ImportCommand()
        {
            File = "*.json";
            ReadCompletedImports = ReadCompletedImportsDefault;
        }

        public ConnectionString ConnectionString { get; set; }

        public override void Execute()
        {
            try
            {
                Dictionary<string, SyncEntity> completedImports = ReadCompletedImports();

                DirectoryInfo di = new DirectoryInfo(".");
                var allFiles = di.GetFiles(File);
                Program.Console($"Found {allFiles.Length} files to import");
                foreach (var f in allFiles.OrderBy(f => f.Name))
                {
                    if (completedImports.ContainsKey(f.Name))
                    {
                        Program.Console($"Skipping {f.Name}, already imported on {completedImports[f.Name].Imported.ToFileTimeUtc()}");
                        continue;
                    }

                    string collectionName = f.Name.Split('.')[1];
                    Program.Console($"Importing {f.Name} to collection {collectionName}");


                    SyncEntity importResult = new SyncEntity();
                    importResult.FileName = f.Name;

                    try
                    {
                        var resultCode = Program.Exec(COMMAND, $"{ConnectionString.ToCommandLine()} --collection {collectionName} --type json --mode insert --stopOnError --file {f.Name}");
                        if (resultCode != 0)
                        {
                            throw new InvalidOperationException($"mongoimport result code {resultCode}, interrupting");
                        }
                    }
                    finally
                    {
                        SyncCollection.InsertOne(importResult);
                    }
                }
                Program.Console($"Import completed successfully");
            }
            catch (Exception ex)
            {
                Program.Console($"Error during import: {ex.Message}");
            }
        }

        public const string COMMAND = "mongoimport";
        public const string SyncCollectionName = "_mongoSync";

        public Func<Dictionary<string, SyncEntity>> ReadCompletedImports { get; set; }
        public IMongoCollection<SyncEntity> SyncCollection { get; private set; }

        // Wrapping intp function to override for testing, no plan to unit test this
        public Dictionary<string, SyncEntity> ReadCompletedImportsDefault()
        {
            Program.Console($"Reading information about completed imports from {SyncCollectionName}");
            var completedImports = SyncCollection.Find((f) => true).ToEnumerable();
            var result = completedImports.ToDictionary(e => e.FileName, e => e);
            Program.Console($"{result.Count} imported items found");
            return result;
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
            ConnectionString = new ConnectionString(Connection);

            // try to connect
            var mongoClient = new MongoClient(Connection);
            var database = mongoClient.GetDatabase(ConnectionString.Database);
            SyncCollection = database.GetCollection<SyncEntity>(SyncCollectionName);
        }
    }
}
