using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.MongoSync.Console.Model
{
    public class ConnectionString
    {
        public ConnectionString(string connection)
        {
            connection = connection.Replace("mongodb://", "");
            string[] splitByAt = connection.Split('@');
            // has username / password
            if (splitByAt.Length > 1)
            {
                var usernameAndPassword = splitByAt[0].Split(':');
                UserName = usernameAndPassword[0];
                if (usernameAndPassword.Length > 1)
                {
                    Password = usernameAndPassword[1];
                }
                fillHostAndDb(splitByAt[1]);
            }
            else
            {
                fillHostAndDb(splitByAt[0]);
            }
        }

        private void fillHostAndDb(string connectionStringAfterAt)
        {
            var splitBySlash = connectionStringAfterAt.Split('/');
            if (splitBySlash.Length != 2)
            {
                throw new ArgumentException("Connection string should have a database name in it, e.g. localhost/mydatabase");
            }

            Host = splitBySlash[0];
            Database = splitBySlash[1];
        }

        public string Database { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }

        public string ToCommandLine()
        {
            StringBuilder sb = new StringBuilder();
            // mongo.exe requires databasename as first paramater, in this way it is easier to fix parameters
            sb.Append($"--db {Database} --host {Host}");
            if (!string.IsNullOrEmpty(UserName))
            {
                sb.Append($" --username {UserName}");
                if (!string.IsNullOrEmpty(Password))
                {
                    sb.Append($" --password {Password}");
                }
            }
            return sb.ToString();
        }
    }
}
