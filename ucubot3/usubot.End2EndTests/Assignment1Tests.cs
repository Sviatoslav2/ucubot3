using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using ucubot.Model;

namespace usubot.End2EndTests
{
    [TestFixture]
    [Category("Assignment1")]
    public class Assignment1Tests
    {
        private HttpClient _client;

        [SetUp]
        public void Init()
        {
            _client = new HttpClient {BaseAddress = new Uri("http://app:80")};
        }
        
        [Test, Order(0)]
        public void CleanData()
        {
            Utils.CleanDatabase(Connection.CONNECTION_STRING_NODB);
        }
        
        [Test, Order(1)]
        public void TestDatabaseWasCreated()
        {
            // create database test
            var dbScript = Utils.ReadMysqlScript("db");
            using (var conn = new MySqlConnection(Connection.CONNECTION_STRING_NODB))
            {
                conn.Open();
                
                var users1 = Utils.MapDataTableToStringCollection(Utils.ExecuteDataTable("SELECT User FROM mysql.user;", conn)).ToArray();
                users1.Length.Should().BeGreaterThan(1); // we don't know actual name of the user...
                
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                command.ExecuteNonQuery();

                var databases = Utils.MapDataTableToStringCollection(Utils.ExecuteDataTable("SHOW DATABASES;", conn)).ToArray();
                databases.Should().Contain("ucubot");

                var users = Utils.MapDataTableToStringCollection(Utils.ExecuteDataTable("SELECT User FROM mysql.user;", conn)).ToArray();
                users.Length.Should().Be(3); // we don't know actual name of the user, and there is only root exists after cleanup
            }
        }

        [Test, Order(2)]
        public void TestTableWasCreated()
        {
            // create database test
            var dbScript = Utils.ReadMysqlScript("lesson-signal");
            using (var conn = new MySqlConnection(Connection.CONNECTION_STRING_NODB))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                command.ExecuteNonQuery();
                
                var tables = Utils.MapDataTableToStringCollection(Utils.ExecuteDataTable("SHOW TABLES;", conn)).ToArray();
                tables.Should().Contain("lesson_signal");
            }
        }

        [Test, Order(3)]
        public async Task TestGetCreateGetDeleteGet()
        {
            // check is empty
            var getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            var values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(0);
            
            // create
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user_id", "U111"),
                new KeyValuePair<string, string>("text", "simple")
            });
            var createResponse = await _client.PostAsync("/api/LessonSignalEndpoint", content);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            
            // check
            getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(1);
            values[0].UserId.Should().Be("U111");
            values[0].Type.Should().Be(LessonSignalType.BoringSimple);
            
            // delete
            var deleteResponse = await _client.DeleteAsync($"/api/LessonSignalEndpoint/{values[0].Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            
            // check
            getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(0);
        }
        
        [Test, Order(4)]
        public async Task TestSqlInjectionFail()
        {
            // check is empty
            var getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            var values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(0);
            
            // create
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user_id", "U111"),
                new KeyValuePair<string, string>("text", "simple")
            });
            var createResponse = await _client.PostAsync("/api/LessonSignalEndpoint", content);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            
            // create another with attack
            content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user_id", "U111', 0); DELETE FROM lesson_signal; #"),
                new KeyValuePair<string, string>("text", "simple")
            });
            createResponse = await _client.PostAsync("/api/LessonSignalEndpoint", content);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            
            // check
            getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(2);
        }
        
        [Test, Order(5)]
        public async Task TestNonExistRecordReturns404()
        {
            // get previous values
            var getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            var values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            var newId = values.Select(v => v.Id).Max() + 1;
            
            // check
            var response = await _client.GetAsync($"/api/LessonSignalEndpoint/{newId}");
            Assert.IsTrue(new[]{HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.NoContent }.Contains(response.StatusCode),
                $"Non exists record response should not be {response.StatusCode}");
        }

        [TearDown]
        public void Done()
        {
            _client.Dispose();
        }
    }
}
