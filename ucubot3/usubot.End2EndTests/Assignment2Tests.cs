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
    [Category("Assignment2")]
    public class Assignment2Tests
    {
        protected HttpClient _client;

        [SetUp]
        public void Init()
        {
            _client = new HttpClient {BaseAddress = new Uri("http://app:80")};
        }
        
        [Test, Order(-10)]
        public void Preparation()
        {
            Utils.CleanDatabase(Connection.CONNECTION_STRING_NODB);
        }
        
        [Test, Order(10)]
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

        [Test, Order(20)]
        public void Test_Student_TableWasCreated()
        {
            // create database test
            var dbScript = Utils.ReadMysqlScript("student");
            using (var conn = new MySqlConnection(Connection.CONNECTION_STRING))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                command.ExecuteNonQuery();
                
                var tables = Utils.MapDataTableToStringCollection(Utils.ExecuteDataTable("SHOW TABLES;", conn)).ToArray();
                tables.Should().Contain("student");
            }
        }
        
        [Test, Order(30)]
        public void Test_LessonSignal_TableWasCreated()
        {
            // create database test
            var dbScript = Utils.ReadMysqlScript("lesson-signal");
            var dbScript2 = Utils.ReadMysqlScript("lesson-signal2");
            using (var conn = new MySqlConnection(Connection.CONNECTION_STRING))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                command.ExecuteNonQuery();
                
                var tables = Utils.MapDataTableToStringCollection(Utils.ExecuteDataTable("SHOW TABLES;", conn)).ToArray();
                tables.Should().Contain("lesson_signal");
                
                var command2 = conn.CreateCommand();
                command2.CommandText = dbScript2;
                command2.ExecuteNonQuery();
                
                var constranints = Utils.MapDataTableToStringCollection(Utils.ExecuteDataTable(@"SELECT REFERENCED_TABLE_NAME 
FROM information_schema.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_SCHEMA = 'ucubot'
 AND TABLE_NAME = 'lesson_signal'", conn)).ToArray();
                constranints.Should().Contain("student");
            }
        }

        [Test, Order(40)]
        public async Task Test_Student_GetCreateGetUpdateGetDeleteGet()
        {
            // check is empty
            var getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            var values = Utils.ParseJson<Student[]>(getResponse);
            values.Length.Should().Be(0);
            
            // create
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserId", "U111"),
                new KeyValuePair<string, string>("FirstName", "vasya"),
                new KeyValuePair<string, string>("LastName", "popov")
            });
            var createResponse = await _client.PostAsync("/api/StudentEndpoint", content);
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(createResponse.StatusCode));
            
            // check
            getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            values = Utils.ParseJson<Student[]>(getResponse);
            values.Length.Should().Be(1);
            values[0].UserId.Should().Be("U111");
            values[0].FirstName.Should().Be("vasya");
            values[0].LastName.Should().Be("popov");
            
            // update
            content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Id", values[0].Id.ToString()),
                new KeyValuePair<string, string>("UserId", "U111"),
                new KeyValuePair<string, string>("FirstName", "vasya"),
                new KeyValuePair<string, string>("LastName", "petrov")
            });
            var updateResponse = await _client.PutAsync("/api/StudentEndpoint", content);
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(updateResponse.StatusCode));
            
            // check
            getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            values = Utils.ParseJson<Student[]>(getResponse);
            values.Length.Should().Be(1);
            values[0].UserId.Should().Be("U111");
            values[0].FirstName.Should().Be("vasya");
            values[0].LastName.Should().Be("petrov");
            
            // check by id
            getResponse = await _client.GetStringAsync($"/api/StudentEndpoint/{values[0].Id}");
            var value = Utils.ParseJson<Student>(getResponse);
            value.UserId.Should().Be("U111");
            value.FirstName.Should().Be("vasya");
            value.LastName.Should().Be("petrov");
            
            // delete
            var deleteResponse = await _client.DeleteAsync($"/api/StudentEndpoint/{values[0].Id}");
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(deleteResponse.StatusCode));
            
            // check
            getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            values = Utils.ParseJson<Student[]>(getResponse);
            values.Length.Should().Be(0);
        }
        
        [Test, Order(50)]
        public async Task Test_Student_SqlInjectionFail()
        {
            // check is empty
            var getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            var values = Utils.ParseJson<Student[]>(getResponse);
            values.Length.Should().Be(0);
            
            // create another with attack
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserId", "U111', 0); DELETE FROM lesson_signal; #"),
                new KeyValuePair<string, string>("FirstName", "1"),
                new KeyValuePair<string, string>("LastName", "2")
            });
            var createResponse = await _client.PostAsync("/api/StudentEndpoint", content);
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(createResponse.StatusCode));
            
            // check
            getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            values = Utils.ParseJson<Student[]>(getResponse);
            values.Length.Should().Be(1);
        }
        
        [Test, Order(60)]
        public async Task Test_Student_NonExistRecordReturns404()
        {
            // get previous values
            var getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            var values = Utils.ParseJson<Student[]>(getResponse);
            var newId = values.Select(v => v.Id).Max() + 1;
            
            // check
            var response = await _client.GetAsync($"/api/StudentEndpoint/{newId}");
            Assert.IsTrue(new[]{HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.NoContent }.Contains(response.StatusCode),
                $"Non exists record response should not be {response.StatusCode}");
        }
        
        [Test, Order(70)]
        public async Task Test_Student_UserIdUnique()
        {
            // check is empty
            var getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            var values = Utils.ParseJson<Student[]>(getResponse);
            values.Length.Should().Be(1);
            
            // create new
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserId", "U111"),
                new KeyValuePair<string, string>("FirstName", "1"),
                new KeyValuePair<string, string>("LastName", "2")
            });
            var createResponse = await _client.PostAsync("/api/StudentEndpoint", content);
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(createResponse.StatusCode));
            
            // create second with the same user_id
            content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserId", "U111"),
                new KeyValuePair<string, string>("FirstName", "1"),
                new KeyValuePair<string, string>("LastName", "2")
            });
            createResponse = await _client.PostAsync("/api/StudentEndpoint", content);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
            
            // check
            getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            values = Utils.ParseJson<Student[]>(getResponse);
            values.Length.Should().Be(2);
        }
        
        [Test, Order(80)]
        public async Task Test_LessonSignal_GetCreateGetDeleteGet()
        {
            // check is empty
            var getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            var values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(0);
            
            // create with user_id already exists
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user_id", "U111"),
                new KeyValuePair<string, string>("text", "simple")
            });
            var createResponse = await _client.PostAsync("/api/LessonSignalEndpoint", content);
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(createResponse.StatusCode));
            
            // check
            getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(1);
            values[0].UserId.Should().Be("U111");
            values[0].Type.Should().Be(LessonSignalType.BoringSimple);
            
            // delete
            var deleteResponse = await _client.DeleteAsync($"/api/LessonSignalEndpoint/{values[0].Id}");
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(deleteResponse.StatusCode));
            
            // check
            getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(0);
        }
        
        [Test, Order(90)]
        public async Task Test_LessonSignal_NonExistRecordReturns404()
        {
            // get previous values
            var getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            var values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            var newId = values.Length > 0 ? values.Select(v => v.Id).Max() + 1 : 1;
            
            // check
            var response = await _client.GetAsync($"/api/LessonSignalEndpoint/{newId}");
            Assert.IsTrue(new[]{HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.NoContent }.Contains(response.StatusCode),
                $"Non exists record response should not be {response.StatusCode}");
        }
        
        [Test, Order(100)]
        public async Task Test_LessonSignal_CanNotCreateForNonExistsStudent()
        {
            // create with user_id non exists
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user_id", "U112"),
                new KeyValuePair<string, string>("text", "simple")
            });
            var createResponse = await _client.PostAsync("/api/LessonSignalEndpoint", content);
            createResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            // check
            var getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            var values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(0);
        }
        
        [Test, Order(110)]
        public async Task Test_Student_LessonSignal_CanNotDeleteWithChildRecords()
        {
            // check students non empty
            var getResponseStudents = await _client.GetStringAsync("/api/StudentEndpoint");
            var valuesStudents = Utils.ParseJson<Student[]>(getResponseStudents);
            valuesStudents.Length.Should().Be(2);
            
            // check lesson signal is empty
            var getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            var values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(0);
            
            // create lesson signal with user_id already exists
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user_id", valuesStudents[0].UserId),
                new KeyValuePair<string, string>("text", "simple")
            });
            var createResponse = await _client.PostAsync("/api/LessonSignalEndpoint", content);
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(createResponse.StatusCode));
            
            // check lesson signal is non empty
            getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            values.Length.Should().Be(1);
            
            // try delete student
            var deleteResponse1 = await _client.DeleteAsync($"/api/StudentEndpoint/{valuesStudents[0].Id}");
            deleteResponse1.StatusCode.Should().Be(HttpStatusCode.Conflict);
            
            // check ls
            getResponse = await _client.GetStringAsync("/api/StudentEndpoint");
            valuesStudents = Utils.ParseJson<Student[]>(getResponse);
            valuesStudents.Length.Should().Be(2);
            
            // delete lesson signal
            var deleteResponse = await _client.DeleteAsync($"/api/LessonSignalEndpoint/{values[0].Id}");
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(deleteResponse.StatusCode));
            
            // delete student
            var deleteResponse2 = await _client.DeleteAsync($"/api/StudentEndpoint/{valuesStudents[0].Id}");
            Assert.IsTrue(new[] {HttpStatusCode.OK, HttpStatusCode.Accepted}.Contains(deleteResponse2.StatusCode));
        }

        [TearDown]
        public void Done()
        {
            _client.Dispose();
        }
    }
}
