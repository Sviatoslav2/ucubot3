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

    public class LessonSignalEndpointController : Controller

    {

        private readonly ILessonSignalRepository _lessonSignalRepository;
		
		
        public LessonSignalEndpointController(ILessonSignalRepository lessonSignalRepository)
        
        {
        
            _lessonSignalRepository = lessonSignalRepository;
            
        }



        [HttpGet]

        public IEnumerable<LessonSignalDto> ShowSignals()

        {

            return _lessonSignalRepository.ShowSignalsDB();

        }

        
        

        [HttpGet("{id}")]

        public LessonSignalDto ShowSignal(long id)

        {

            return _lessonSignalRepository.ShowSignalDB(id);
            
        }

        

        [HttpPost]

        public async Task<IActionResult> CreateSignal(SlackMessage message)

        {

            if(!_lessonSignalRepository.CreateSignalDB(message))
            
            {

                return BadRequest();

            }
            
            return Accepted();

        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> RemoveSignal(long id)

        {

            _lessonSignalRepository.RemoveSignalDB(id);
            
            return Accepted();

        }

    }

}