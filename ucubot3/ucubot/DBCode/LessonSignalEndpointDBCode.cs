using System;

using System.Collections.Generic;

using System.Data;

using System.Linq;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Configuration;

using MySql.Data.MySqlClient;

using ucubot.Model;

using Dapper;


namespace ucubot.DBCode

{

    [Route("api/[controller]")]

    public class LessonSignalEndpointDBCode : ILessonSignalRepository

    {

        private readonly IConfiguration _configuration;



        public LessonSignalEndpointDBCode(IConfiguration configuration)

        {

            _configuration = configuration;

        }




        public IEnumerable<LessonSignalDto> ShowSignalsDB()

        {

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query = "SELECT lesson_signal.id as Id, time_stamp as Timestamp, signal_type as Type, student.user_id as UserId FROM lesson_signal JOIN student ON student_id = student.id;";

            var conn = new MySqlConnection(connectionString); 
            
            conn.Open();

            var enumerable = conn.Query<LessonSignalDto>(query).ToList();

            conn.Close();

            return enumerable;

        }

        
        

        public LessonSignalDto ShowSignalDB(long id)

        {

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query = "SELECT lesson_signal.id as Id, time_stamp as Timestamp, signal_type as Type, student.user_id as UserId FROM lesson_signal JOIN student ON student_id = student.id WHERE lesson_signal.id=@id;";

            var conn = new MySqlConnection(connectionString); 
            
            conn.Open();

            var enumerable = conn.Query<LessonSignalDto>(query, new {Id = id}).ToList();

            conn.Close();

            if (enumerable.Count < 1)
            
            {
            
                return null;
                
            }
            
            return enumerable[0];
        }

        


        public bool CreateSignalDB(SlackMessage message)

        {

            var userId = message.user_id;

            var signalType = message.text.ConvertSlackMessageToSignalType();

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query_create = "INSERT INTO lesson_signal(signal_type, student_id, time_stamp) " +

                           "VALUES(@signal_type, @user_id, @time_stamp);";

            var query_get_students = "SELECT * FROM student WHERE user_id=@user_id;";
            
            var conn = new MySqlConnection(connectionString);        
            
            var enumerable = conn.Query<Student>(query_get_students, new {user_id = userId}).ToList();

            if (enumerable.Count == 0)

            {

                return false;

            }

            conn.Execute(query_create, new {signal_type = signalType, user_id = enumerable[0].Id, time_stamp = DateTime.Now});
            
            return true;

        }



        public void RemoveSignalDB(long id)

        {

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query = "DELETE FROM lesson_signal WHERE id=@id;";

            var conn = new MySqlConnection(connectionString);

            conn.Execute(query, new {Id = id});

        }

    }

}