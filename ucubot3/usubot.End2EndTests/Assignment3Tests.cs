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
    [Category("Assignment3")]
    public class Assignment3Tests : Assignment2Tests
    {
        [SetUp]
        public void Init() 
        {
            base.Init();
        }
        
        [Test, Order(35)]
        public void Test_StudentSignals_ViewWasCreated()
        {
            // create database test
            var dbScript = Utils.ReadMysqlScript("student-signals");
            using (var conn = new MySqlConnection(Connection.CONNECTION_STRING))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = dbScript;
                command.ExecuteNonQuery();
                
                var tables = Utils.MapDataTableToStringCollection(Utils.ExecuteDataTable("SHOW TABLES;", conn)).ToArray();
                tables.Should().Contain("student_signals");
            }
        }
        
        [Test, Order(120)]
        public async Task Test_StudentSignals_Aggregation()
        {
            // create more students if needed
            var getResponseStudents = await _client.GetStringAsync("/api/StudentEndpoint");
            var valuesStudents = Utils.ParseJson<Student[]>(getResponseStudents);

            if (valuesStudents.Length < 2)
            {
                // create
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("UserId", "tttt"),
                    new KeyValuePair<string, string>("FirstName", "thanos"),
                    new KeyValuePair<string, string>("LastName", "thetitan")
                });
                var createResponse = await _client.PostAsync("/api/StudentEndpoint", content);
                createResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
                
                getResponseStudents = await _client.GetStringAsync("/api/StudentEndpoint");
                valuesStudents = Utils.ParseJson<Student[]>(getResponseStudents);
            }
            
            // create random signals
            for (var i = 0; i < 100; i++)
            {
                var r = new Random();
                var studentIdx = r.Next(2);
                var student = valuesStudents[studentIdx];

                var levelIdx = r.Next(3);
                var level = "";
                switch (levelIdx)
                {
                    case 0: level = "simple";
                        break;
                    case 1: level = "interesting";
                        break;
                    case 2: level = "hard";
                        break;
                }
                
                // create with user_id already exists
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("user_id", student.UserId),
                    new KeyValuePair<string, string>("text", level)
                });
                var createResponse = await _client.PostAsync("/api/LessonSignalEndpoint", content);
                createResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }
            
            // get results
            var getResponse = await _client.GetStringAsync("/api/LessonSignalEndpoint");
            var values = Utils.ParseJson<LessonSignalDto[]>(getResponse);
            var aggregatedExpected = values
                .Join(valuesStudents, l => l.UserId, s => s.UserId, Tuple.Create)
                .GroupBy(l => new {l.Item2.Id, l.Item2.FirstName, l.Item2.LastName, l.Item1.Type})
                .ToDictionary(t => t.Key, t => t.Count())
                .Select(d => new StudentSignal
                {
                    FirstName = d.Key.FirstName,
                    LastName = d.Key.LastName,
                    Count = d.Value,
                    SignalType = ConvertStudentSignalType(d.Key.Type)
                });
            
            // get aggregation results
            var getAggResponse = await _client.GetStringAsync("/api/StudentSignalsEndpoint");
            var aggregated = Utils.ParseJson<StudentSignal[]>(getAggResponse);
            var sumActual = aggregated.Select(a => a.Count).Sum();
            var sumExpected = aggregatedExpected.Select(a => a.Count).Sum();
            sumActual.Should().Be(sumExpected);
            foreach (var studentSignal in aggregated)
            {
                aggregatedExpected.Should().Contain(studentSignal);
            }
        }

        [TearDown]
        public void Done()
        {
            base.Done();
        }

        private string ConvertStudentSignalType(LessonSignalType type)
        {
            switch (type)
            {
                case LessonSignalType.BoringSimple: return "Simple";
                case LessonSignalType.Interesting: return "Normal";
                case LessonSignalType.BoringHard: return "Hard";
                default: throw new Exception();
            }
        }
    }
}