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

    public class StudentEndpointController : Controller

    {

        private readonly IStudentRepository _studentRepository;
		
		
        public StudentEndpointController(IStudentRepository studentRepository)
        
        {
            
            _studentRepository = studentRepository;
            
        }



        [HttpGet]

        public IEnumerable<Student> ShowStudents()

        {

            return _studentRepository.ShowStudentsDB();

        }

        
        [HttpGet("{id}")]

        public Student ShowStudent(long id)

        {
            
            return _studentRepository.ShowStudentDB(id);
            
        }

        

        [HttpPost]

        public async Task<IActionResult> CreateStudent(Student student)

        {
         
            if(!_studentRepository.CreateStudentDB(student))
            {

                return StatusCode(409);

            }
            
            return Accepted();

        }


        
        [HttpPut]
        
        public async Task<IActionResult> UpdateStudent(Student student)

        {
            
            _studentRepository.UpdateStudentDB(student);
            
            return Accepted();

        }



        [HttpDelete("{id}")]
        
        public async Task<IActionResult> RemoveStudent(long id)

        {

            if(!_studentRepository.RemoveStudentDB(id))

            {

                return StatusCode(409);

            }

            return Accepted();

        }

    }

}
