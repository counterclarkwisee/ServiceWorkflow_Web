using Microsoft.AspNetCore.Mvc;
using server.Interfaces;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaController : ControllerBase
    {
        private readonly IAppointmentRepository _repo;

        public SaController(IAppointmentRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _repo.GetAllAppointmentsAsync();
            return Ok(appointments);
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartReceiving(string id)
        {
            var success = await _repo.StartSaReceivingAsync(id);
            return success ? Ok(new { message = "Started" }) : BadRequest();
        }

        [HttpPut("{id}/end")]
        public async Task<IActionResult> EndReceiving(string id)
        {
            var success = await _repo.EndSaReceivingAsync(id);
            return success ? Ok(new { message = "Finished" }) : NotFound();
        }
    }
}