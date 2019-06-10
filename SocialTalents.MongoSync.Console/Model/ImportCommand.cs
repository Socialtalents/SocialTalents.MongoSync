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
            // to cover .js and .json
            FilesFilter = "*.js*";
            ReadCompletedImports = ReadCompletedImportsDefault;
            ConnectToMongoDb = ConnectToMongoDbDefault;
        }

        public ConnectionString ConnectionString { get; set; }
        public string FilesFilter { get; set; }

        public override void Execute()
        {
            try
            {
                Dictionary<string, SyncEntity> completedImports = ReadCompletedImports();

                var allFiles = ReadFiles(FilesFilter);
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
                                var resultCode = Program.Exec(IMPORT_COMMAND, $"{ConnectionString.ToCommandLine()}{AuthenticationDatabaseToCommandLine()} --collection {collectionName} --type json --mode {importMode.ToString().ToLower()} --stopOnError --file {f.Name}");
                                if (resultCode != 0)
                                {
                                    throw new InvalidOperationException($"mongoimport result code {resultCode}, interrupting");
                                }
                                break;
                            case ImportMode.Eval:
                                var evalResultCode = Program.Exec(MONGO_COMMAND, $"{Connection} {f.Name}");
                                if (evalResultCode != 0)
                                {
                                    throw new InvalidOperationException($"mongo result code {evalResultCode}, interrupting");
                                }
                                break;
                            case ImportMode.CreateIndex:
                                var text = File.ReadAllText(f.FullName);

                                Program.Console("Index params:");
                                Program.Console(text);

                                var command = string.Format(CREATE_INDEX, collectionName, text);
                                var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".js");
                                File.WriteAllText(fileName, command);

                                try
                                {
                                    var createIndexResultCode = Program.Exec(MONGO_COMMAND, $"{Connection} {fileName}");
                                    switch (createIndexResultCode)
                                    {
                                        // no error
                                        case 0:
                                            break;
                                        case 11000:
                                            throw new InvalidOperationException($"CreateIndex failed with error 'duplicate key error', interrupting");
                                        default:
                                            // all error codes with explanation
                                            // https://github.com/mongodb/mongo/blob/master/src/mongo/base/error_codes.err
                                            throw new InvalidOperationException($"CreateIndex result code {createIndexResultCode}, interrupting");
                                    }
                                }
                                finally
                                {
                                    File.Delete(fileName);
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
                Environment.Exit(1);
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

        public const string IMPORT_COMMAND = "mongoimport";
        public const string MONGO_COMMAND = "mongo";
        public const string SyncCollectionName = "_mongoSync";

        private const string CREATE_INDEX = "var r = db.{0}.createIndex({1}); if(r.ok!=1) {{quit(r.code)}}";

        public Func<Dictionary<string, SyncEntity>> ReadCompletedImports { get; set; }
        public Func<string, FileInfo[]> ReadFiles { get; set; } = (fileFilter) => new DirectoryInfo(".").GetFiles(fileFilter);
        public Func<string, string, IMongoDatabase> ConnectToMongoDb { get; set; }

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
            result.Add("--file", (cmd, arg) => (cmd as ImportCommand).FilesFilter = arg);
            return result;
        }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(Connection))
            {
                throw new ArgumentException("Connection parameter required for import command");
            }
            if (string.IsNullOrEmpty(FilesFilter))
            {
                throw new ArgumentException("Connection parameter required for import command");
            }
            ConnectionString = new ConnectionString(Connection);

            // try to connect
            MongoDatabase = ConnectToMongoDb(Connection, ConnectionString.Database);
            SyncCollection = MongoDatabase.GetCollection<SyncEntity>(SyncCollectionName);
        }

        private IMongoDatabase ConnectToMongoDbDefault(string connection, string databaseName)
        {
            var mongoClient = new MongoClient(Connection);
            return mongoClient.GetDatabase(ConnectionString.Database);
        }
    }
}
