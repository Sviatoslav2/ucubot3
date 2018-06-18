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

    public class StudentEndpointDBCode : IStudentRepository

    {

        private readonly IConfiguration _configuration;



        public StudentEndpointDBCode(IConfiguration configuration)

        {

            _configuration = configuration;

        }



        public IEnumerable<Student> ShowStudentsDB()

        {

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query = "SELECT id as Id, user_id as UserId, first_name as FirstName, last_name as LastName FROM student;";

            var conn = new MySqlConnection(connectionString); 
            
            conn.Open();

            var enumerable = conn.Query<Student>(query).ToList();

            conn.Close();

            return enumerable;

        }

        
        

        public Student ShowStudentDB(long id)

        {

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query = "SELECT id as Id, user_id as UserId, first_name as FirstName, last_name as LastName FROM student WHERE id=@id;";

            var conn = new MySqlConnection(connectionString); 
            
            conn.Open();

            var enumerable = conn.Query<Student>(query, new {Id = id}).ToList();

            conn.Close();

            if (enumerable.Count < 1)
            
            {
            
                return null;
                
            }
            
            return enumerable[0];
        }

        


        public bool CreateStudentDB(Student student)

        {

            var userId = student.UserId;
            
            var firstName = student.FirstName;
            
            var lastName = student.LastName;

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query_create = "INSERT INTO student(first_name, last_name, user_id) " +

                           "VALUES(@first_name, @last_name, @user_id);";
            
            var conn = new MySqlConnection(connectionString);

            try

            {

                conn.Execute(query_create, new {first_name = firstName, last_name = lastName, user_id = userId});

            }

            catch

            {

                return false;

            }
            
            return true;

        }


        
        public void UpdateStudentDB(Student student)

        {
            
            var id = student.Id;
            
            var userId = student.UserId;
            
            var firstName = student.FirstName;
            
            var lastName = student.LastName;

            var connectionString = _configuration.GetConnectionString("BotDatabase");
            
            var conn = new MySqlConnection(connectionString);
            
            var query =
                "UPDATE student SET user_id = @user_id, first_name = @first_name, last_name = @last_name WHERE id = @id;";

            conn.Execute(query, new {user_id = userId, first_name = firstName, last_name = lastName, Id = id});

        }


        public bool RemoveStudentDB(long id)

        {

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query = "DELETE FROM student WHERE id=@id";

            var conn = new MySqlConnection(connectionString);

            try
            
            {

                conn.Execute(query, new {Id = id});
                
            }
            
            catch

            {

                return false;

            }

            return true;

        }

    }

}