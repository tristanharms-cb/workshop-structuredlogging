using System;
using System.Threading.Tasks;
using Coolblue.Utilities.MonitoringEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using workshop_structuredlogging.Api.Materials;

namespace workshop_structuredlogging.Api.Controllers
{
    public class WorkshopController : Controller
    {
        private readonly ILongRunningProcess _longRunningProcess;
        public MonitoringEvents MonitoringEvents { get; set; }

        public WorkshopController(ILongRunningProcess longRunningProcess)
        {
            _longRunningProcess = longRunningProcess ?? throw new ArgumentNullException(nameof(longRunningProcess));
        }
        
        [HttpGet]
        [Route("api/workshop/StartLongRunningProcess")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                _longRunningProcess.Run();
            }
            catch (Exception e)
            {
                MonitoringEvents.Logger.Error(e.ToString());
                return new StatusCodeResult(500);
            }
            
            return Ok("Done!");
        }
    }
}