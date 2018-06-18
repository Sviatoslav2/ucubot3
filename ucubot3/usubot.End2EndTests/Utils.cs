using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace usubot.End2EndTests
{
    public class Utils
    {
        public static void CleanDatabase(string connection)
        {
            // HACK: waits few seconds to give a time for mysql container to start
            Thread.Sleep(3000);
            Assert.That(true);
            
            // clean data
            using (var conn = new MySqlConnection(connection))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = "DROP DATABASE IF EXISTS ucubot;";
                command.ExecuteNonQuery();
                
                var users = ExecuteDataTable("SELECT User, Host FROM mysql.user;", conn);
                foreach (DataRow row in users.Rows)
                {
                    var name = (string) row["User"];
                    if (name == "root") continue;
                    var host = (string) row["Host"];

                    var cmd = $"DROP USER '{name}'@'{host}';";
                    var command2 = conn.CreateCommand();
                    command2.CommandText = cmd;
                    command2.ExecuteNonQuery();
                }
                
                var users2 = MapDataTableToStringCollection(ExecuteDataTable("SELECT User, Host FROM mysql.user;", conn)).ToArray();
                users2.Length.Should().Be(2);
            }
        }
        
        public static string ReadMysqlScript(string scriptName)
        {
            using (var reader = new StreamReader(File.OpenRead($"/app/ucubot/Scripts/{scriptName}.sql")))
            {
                return reader.ReadToEnd();
            }
        }

        public static DataTable ExecuteDataTable(string sqlCommand, MySqlConnection conn)
        {
            var adapter = new MySqlDataAdapter(sqlCommand, conn);

            var dataset = new DataSet();

            adapter.Fill(dataset);

            return dataset.Tables[0];
        }

        public static IEnumerable<string> MapDataTableToStringCollection(DataTable table)
        {
            return table.Rows.Cast<DataRow>().Select(r => r[0].ToString());
        }

        public static T ParseJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
        }
    }
}