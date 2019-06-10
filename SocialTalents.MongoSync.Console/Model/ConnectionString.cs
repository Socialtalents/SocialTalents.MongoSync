using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using MongoDB.Driver;
using MongoConnectionString = MongoDB.Driver.Core.Configuration.ConnectionString;

namespace SocialTalents.MongoSync.Console.Model
{
    public class ConnectionString
    {
        private readonly MongoConnectionString _connectionString;

        public ConnectionString(string connection)
        {
            try { _connectionString = new MongoConnectionString(connection); }
            catch (MongoConfigurationException exception) { throw new ArgumentException(exception.Message); }

            if(Database == null)
                throw new ArgumentException("Connection string should have a database name in it, e.g. localhost/mydatabase");
        }

        public string UserName => _connectionString.Username;
        public string Password => _connectionString.Password;

        public string Database => _connectionString.DatabaseName;
        public IEnumerable<string> Hosts => _connectionString.Hosts.Select(endPoint =>
            endPoint is DnsEndPoint dnsEndPoint
                ? $"{dnsEndPoint.Host}:{dnsEndPoint.Port}"
                : $"{endPoint}"
        );

        public string ToCommandLine()
        {
            StringBuilder sb = new StringBuilder();
            // mongo.exe requires database name as first parameter, in this way it is easier to fix parameters
            sb.Append($"--db {Database}");

            // https://docs.mongodb.com/manual/reference/program/mongo/#cmdoption-mongo-host
            var hostOrReplica = string.IsNullOrEmpty(_connectionString.ReplicaSet)
                ? Hosts.First()
                : $"{_connectionString.ReplicaSet}/{string.Join(",", Hosts)}";
            sb.Append($" --host {hostOrReplica}");

            if (!string.IsNullOrEmpty(UserName))
            {
                sb.Append($" --username {UserName}");
                if (!string.IsNullOrEmpty(Password))
                {
                    sb.Append($" --password {Password}");
                }
            }

            if (_connectionString.Ssl.HasValue && _connectionString.Ssl.Value)
            {
                sb.Append(" --ssl");
            }
            return sb.ToString();
        }
    }
}
