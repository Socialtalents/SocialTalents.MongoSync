using SocialTalents.MongoSync.Console.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SocialTalents.MongoSync.XUnit
{
    public class ConnecitonStringTest
    {
        [Theory]
        [InlineData("mongodb://masterDev:BfkvYB@127.0.0.1:28017/master", "127.0.0.1:28017", "masterDev", "BfkvYB", "master",
            "--host 127.0.0.1:28017 --db master --username masterDev --password BfkvYB")]
        [InlineData("mongodb://masterDev:BfkvYB@127.0.0.1/master", "127.0.0.1", "masterDev", "BfkvYB", "master",
            "--host 127.0.0.1 --db master --username masterDev --password BfkvYB")]
        [InlineData("mongodb://masterDev@127.0.0.1:28017/master", "127.0.0.1:28017", "masterDev", null, "master",
            "--host 127.0.0.1:28017 --db master --username masterDev")]
        [InlineData("mongodb://127.0.0.1:28017/master", "127.0.0.1:28017", null, null, "master"
            , "--host 127.0.0.1:28017 --db master")]
        [InlineData("masterDev:BfkvYB@127.0.0.1:28017/master", "127.0.0.1:28017", "masterDev", "BfkvYB", "master"
            , "--host 127.0.0.1:28017 --db master --username masterDev --password BfkvYB")]
        public void TestParsing(string input, string host, string user, string password, string database, string expectedCommandLine)
        {
            var c = new ConnectionString(input);

            Assert.Equal(host, c.Host);
            Assert.Equal(user, c.UserName);
            Assert.Equal(password, c.Password);
            Assert.Equal(database, c.Database);

            Assert.Equal(expectedCommandLine, c.ToCommandLine());
        }

        [Fact]
        public void DbNameRequired()
        {
            string connectionWithoutDbName = "masterDev:BfkvYB@127.0.0.1:28017";
            Assert.Throws<ArgumentException>(() => new ConnectionString(connectionWithoutDbName));
        }
    }
}
