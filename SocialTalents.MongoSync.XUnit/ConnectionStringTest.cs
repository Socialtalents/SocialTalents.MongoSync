using SocialTalents.MongoSync.Console.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SocialTalents.MongoSync.XUnit
{
    public class ConnectionStringTest
    {
        // mongo.exe requires database name as first parameter, in this way it is easier to fix parameters
        [Theory]
        [InlineData("mongodb://masterDev:BfkvYB@127.0.0.1/master", new []{"127.0.0.1:27017"}, "masterDev", "BfkvYB", "master",
            "--db master --host 127.0.0.1:27017 --username masterDev --password BfkvYB")]
        [InlineData("mongodb://masterDev@127.0.0.1:28017/master", new []{"127.0.0.1:28017"}, "masterDev", null, "master",
            "--db master --host 127.0.0.1:28017 --username masterDev")]
        [InlineData("mongodb://127.0.0.1:28017/master", new []{"127.0.0.1:28017"}, null, null, "master"
            , "--db master --host 127.0.0.1:28017")]
        [InlineData("mongodb://masterDev:BfkvYB@127.0.0.1:28017/master", new []{"127.0.0.1:28017"}, "masterDev", "BfkvYB", "master"
            , "--db master --host 127.0.0.1:28017 --username masterDev --password BfkvYB")]
        // takes first host from the list if not replica set specified:
        [InlineData("mongodb://127.0.0.1:28018,127.0.0.1:28019/master", new []{"127.0.0.1:28018", "127.0.0.1:28019"}, null, null, "master"
            , "--db master --host 127.0.0.1:28018")]
        // parses replica set:
        [InlineData("mongodb://127.0.0.1:28018,127.0.0.1:28019/master?replicaSet=rs0", new []{"127.0.0.1:28018", "127.0.0.1:28019"}, null, null, "master"
            , "--db master --host rs0/127.0.0.1:28018,127.0.0.1:28019")]
        public void TestParsing(string input, string[] hosts, string user, string password, string database, string expectedCommandLine)
        {
            var c = new ConnectionString(input);

            Assert.Equal(hosts, c.Hosts);
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
