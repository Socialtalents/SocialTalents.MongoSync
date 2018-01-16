using MongoDB.Bson;
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
                        Program.Console($"Skipping {f.Name}, already imported on {completedImports[f.Name].Imported.ToString("u")}");
                        continue;
                    }

                    var fileNameSplit = f.Name.Split('.');
                    if (fileNameSplit.Length != 4)
                    {
                        throw new InvalidOperationException($"File {f.Name} do not have 4 components: [Order].[Collection].[Operation].json");
                    }
                    string collectionName = fileNameSplit[1];
                    ImportMode importMode = Enum.Parse<ImportMode>(fileNameSplit[2], true);
                    
                    Program.Console($"Importing {f.Name} to collection {collectionName}, mode {importMode}");


                    SyncEntity importResult = new SyncEntity();
                    importResult.FileName = f.Name;

                    try
                    {
                        switch (importMode)
                        {
                            case ImportMode.Drop:
                                dropCollection(collectionName);
                                break;
                            case ImportMode.Delete:
                                deleteFromCollection(collectionName, f);
                                break;
                            case ImportMode.Insert:
                            case ImportMode.Upsert:
                            case ImportMode.Merge:
                                var resultCode = Program.Exec(COMMAND, $"{ConnectionString.ToCommandLine()} --collection {collectionName} --type json --mode {importMode.ToString().ToLower()} --stopOnError --file {f.Name}");
                                if (resultCode != 0)
                                {
                                    throw new InvalidOperationException($"mongoimport result code {resultCode}, interrupting");
                                }
                                break;
                            default: throw new InvalidOperationException($"Import mode {importMode} not implemented yet");
                        }
                        importResult.Success = true;
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

        private void deleteFromCollection(string collectionName, FileInfo f)
        {
            var deleteCommand = System.IO.File.ReadAllLines(f.FullName);
            foreach(string delete in deleteCommand)
            {
                if (!string.IsNullOrEmpty(delete))
                {
                    Program.Console($"Deleting from  {collectionName}: {delete}");
                    var deleteResult = MongoDatabase.GetCollection<BsonDocument>(collectionName).DeleteMany(delete);
                    Program.Console($"Deleted from  {collectionName}: {deleteResult.DeletedCount} documents deleted");
                }
            }
            
        }

        private void dropCollection(string collectionName)
        {
            Program.Console($"Dropping collection {collectionName}");
            MongoDatabase.DropCollection(collectionName);
            Program.Console($"Collection {collectionName} dropped");

        }

        public const string COMMAND = "mongoimport";
        public const string SyncCollectionName = "_mongoSync";

        public Func<Dictionary<string, SyncEntity>> ReadCompletedImports { get; set; }
        public IMongoDatabase MongoDatabase { get; private set; }
        public IMongoCollection<SyncEntity> SyncCollection { get; private set; }

        // Wrapping intp function to override for testing, no plan to unit test this
        public Dictionary<string, SyncEntity> ReadCompletedImportsDefault()
        {
            Program.Console($"Reading information about completed imports from {SyncCollectionName}");
            var completedImports = SyncCollection.Find((f) => f.Success).ToEnumerable();
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
            MongoDatabase = mongoClient.GetDatabase(ConnectionString.Database);
            SyncCollection = MongoDatabase.GetCollection<SyncEntity>(SyncCollectionName);
        }
    }
}
