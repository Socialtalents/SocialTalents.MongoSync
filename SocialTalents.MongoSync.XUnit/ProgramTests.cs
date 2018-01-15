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
        [InlineData("Merge", typeof(ImportCommand), CommandType.Merge)]
        [InlineData("UPSERT", typeof(ImportCommand), CommandType.Upsert)]
        [InlineData("eXport", typeof(ExportCommand), CommandType.Export)]
        public void ParseCommand_Type(string commandName, Type t, CommandType type)
        {
            Command c = Program.ParseCommand(commandName);
            Assert.Equal(type, c.CommandType);
            Assert.IsType(t, c);
        }

        [Fact]
        public void ValidImport()
        {
            Command c = new ImportCommand();
            c.Parse("--conn mongocon --file *.json".Split(' '));
            Assert.Equal("mongocon", c.Connection);
            Assert.Equal("*.json", c.File);
            Assert.True(c.IsValid());
        }

        [Fact]
        public void ValidExport()
        {
            Command c = new ExportCommand();
            c.Parse("--conn mongocon --file 3.Countries.json --query {}".Split(' '));
            Assert.Equal("mongocon", c.Connection);
            Assert.Equal("3.Countries.json", c.File);
            Assert.Equal("{}", c.SearchQueryForExport);
            Assert.True(c.IsValid());
        }

        [Fact]
        public void DefaultCommand_IsValid_RequiresImplementation()
        {
            Assert.Throws<NotImplementedException>(() => new Command().IsValid());
        }
    }
}
