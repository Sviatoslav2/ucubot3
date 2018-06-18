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

    public class StudentSignalsEndpointDBCode : IStudentSignalsRepository

    {

        private readonly IConfiguration _configuration;



        public StudentSignalsEndpointDBCode(IConfiguration configuration)

        {

            _configuration = configuration;

        }



        public IEnumerable<StudentSignal> ShowStudentSignalsDB()

        {

            var connectionString = _configuration.GetConnectionString("BotDatabase");

            var query = "SELECT first_name AS FirstName, last_name AS LastName, SignalType, Count FROM student_signals;";

            var conn = new MySqlConnection(connectionString); 
            
            conn.Open();

            var enumerable = conn.Query<StudentSignal>(query).ToList();

            conn.Close();

            return enumerable;

        }

    }

}