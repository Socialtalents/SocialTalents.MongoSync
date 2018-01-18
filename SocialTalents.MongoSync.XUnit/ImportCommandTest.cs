using MongoDB.Driver;
using Moq;
using SocialTalents.MongoSync.Console;
using SocialTalents.MongoSync.Console.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;

namespace SocialTalents.MongoSync.XUnit
{
    public class ImportCommandTest
    {
        ImportCommand _command;
        StringBuilder _execLog = new StringBuilder();
        StringBuilder _consoleLog = new StringBuilder();
        Mock<IMongoDatabase> _mongoDatabaseMock = new Mock<IMongoDatabase>();
        Mock<IMongoCollection<SyncEntity>> _syncEntityCollection = new Mock<IMongoCollection<SyncEntity>>();

        public ImportCommandTest()
        {
            _command = new ImportCommand();
            _command.Connection = "mongodb://localhost/db";
            _command.ConnectToMongoDb = (conn, db) => _mongoDatabaseMock.Object;

            _mongoDatabaseMock.Setup(db => db.GetCollection<SyncEntity>(ImportCommand.SyncCollectionName, null)).Returns(_syncEntityCollection.Object);
            _mongoDatabaseMock.Setup(db => db.DropCollection("Collection", It.IsAny<CancellationToken>())).Verifiable();

            Program.Exec = (cmd, arg) => { _execLog.AppendLine($"{cmd} {arg}"); return arg.Contains("err") ? 1 : 0; };
            Program.Console = (line) => _consoleLog.AppendLine(line);
        }

        [Fact]
        public void FilesSetProperly()
        {
            Assert.Equal("*.js*", _command.FilesFilter);
        }

        [Fact]
        public void HappyIntegration()
        {
            _command.ReadCompletedImports = () => new Dictionary<string, SyncEntity>() { { "123.Collection.Insert.json", new SyncEntity() { Imported = DateTime.Now } } };
            _command.ReadFiles = (filter) => new FileInfo[] { new FileInfo("123.Collection.Insert.json"), new FileInfo("234.Collection.Insert.json"), new FileInfo("123.Collection.drop.json"),
                    new FileInfo("345.Collection.eval.js")};
            _command.Validate();
            _command.Execute();

            _mongoDatabaseMock.Verify();
            string fullLog = _consoleLog.ToString();
            // Error message not found
            Assert.True(fullLog.IndexOf("Error") < 0);

            /*
Found 2 files to import
Importing 123.Collection.drop.json to collection Collection, mode Drop
Dropping collection Collection
Collection Collection dropped
Importing 234.Collection.Insert.json to collection Collection, mode Insert
Import completed successfully
             */

            Assert.Contains("Found 4 files to import", fullLog);
            Assert.Contains("Skipping 123.Collection.Insert.json", fullLog);
            Assert.Contains("Dropping collection Collection", fullLog);
            Assert.Contains("Importing 234.Collection.Insert.json to collection Collection, mode Insert", fullLog);
            Assert.Contains("Importing 345.Collection.eval.js to collection Collection, mode Eval", fullLog);
            Assert.Contains("successfully", fullLog);
        }


    }
}
