using SocialTalents.MongoSync.Console;
using SocialTalents.MongoSync.Console.Model;
using System;
using System.Text;
using Xunit;

namespace SocialTalents.MongoSync.XUnit
{
    public class ProgramTests
    {
        StringBuilder _output = new StringBuilder();
        public ProgramTests()
        {
            Program.Console = (s) => _output.AppendLine(s);
        }

        [Fact]
        public void NoParams_NoError()
        {
            Program.Main(new string[0]);
            Assert.True(_output.ToString().IndexOf("Usage") == 0);
        }

        [Fact]
        public void HelpCommand_Lowercase()
        {
            Program.Main(new string[] { "help"});
            Assert.True(_output.ToString().IndexOf("Usage") == 0);
        }

        [Theory]
        [InlineData("help", typeof(HelpCommand), CommandType.Help)]
        [InlineData("insert", typeof(ImportCommand), CommandType.Insert)]
        //[InlineData("Merge", typeof(ImportCommand), CommandType.Merge)]
        //[InlineData("UPSERT", typeof(ImportCommand), CommandType.Upsert)]
        [InlineData("eXport", typeof(ExportCommand), CommandType.Export)]
        public void ParseCommand_Type(string commandName, Type t, CommandType type)
        {
            Command c = Program.ParseCommand(commandName);
            Assert.Equal(type, c.CommandType);
            Assert.IsType(t, c);
        }

        [Fact]
        // Local mongodb must be accessble for this test to succeed
        public void ValidImport()
        {
            Command c = new ImportCommand();
            c.Parse("--conn mongodb://localhost:27017/test --file *.json".Split(' '));
            Assert.Equal("mongodb://localhost:27017/test", c.Connection);
            Assert.Equal("*.json", c.File);
            c.Validate();
        }

        [Fact]
        public void ValidExport()
        {
            var c = new ExportCommand();
            c.Parse("--conn mongocon --query {} --collection countries".Split(' '));
            Assert.Equal("mongocon", c.Connection);
            Assert.Equal("{}", c.SearchQueryForExport);
            Assert.Equal("countries", (c as ExportCommand).CollectionName);
            c.Validate();
        }

        [Fact]
        public void Export_Execute()
        {
            string executable = null;
            string arguments = null;
            Program.Exec = (cmd, args) => { executable = cmd; arguments = args; return 127; };
            var c = new ExportCommand();
            c.Parse("--conn host/database --collection Countries --query {a:1}".Split(' '));
            c.Execute();

            Assert.Equal(ExportCommand.COMMAND, executable);
            Assert.Equal($"--host host --db database --collection Countries --query {{a:1}} --type json --out {c.TimePrefix}.Countries.json", arguments);
        }

        [Fact]
        public void NotValidExport_CollectionMissing()
        {
            var c = new ExportCommand();
            c.Parse("--conn mongocon --query {}".Split(' '));
            Assert.Equal("mongocon", c.Connection);
            Assert.Equal("{}", c.SearchQueryForExport);
            Assert.Throws<ArgumentException>(() => c.Validate());
        }

        [Fact]
        public void DefaultCommand_IsValid_RequiresImplementation()
        {
            Assert.Throws<NotImplementedException>(() => new Command().Validate());
        }
    }
}
