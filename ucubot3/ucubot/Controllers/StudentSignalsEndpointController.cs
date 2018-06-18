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

using ucubot.DBCode;


namespace ucubot.Controllers

{

    [Route("api/[controller]")]

    public class StudentSignalsEndpointController : Controller

    {

        private readonly IStudentSignalsRepository _studentSignalsRepository;
		
		
        public StudentSignalsEndpointController(IStudentSignalsRepository studentSignalsRepository)
        
        {
            
            _studentSignalsRepository = studentSignalsRepository;
            
        }



        [HttpGet]

        //public async Task<IActionResult> ShowStudentSignals()
        public IEnumerable<StudentSignal> ShowStudentSignals()

        {

            return _studentSignalsRepository.ShowStudentSignalsDB();
            //return Accepted();

        }

    }

}
