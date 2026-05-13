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
            // Change _repository to _repo to match your constructor
            var success = await _repo.StartSaReceivingAsync(id); 
            
            // Ensure you return the status so the UI knows to flip the button
            return success ? Ok(new { status = "Started" }) : BadRequest();
        }

        [HttpPut("{id}/end")]
        public async Task<IActionResult> EndSa(string id)
        {
            // Changed _repository to _repo to match your constructor
            var success = await _repo.EndSaReceivingAsync(id); 
            if (success)
            {
                // Tell JobCon that SA is now Endorsed
                await _repo.SyncJobConGate1Async(id, "SA", "Endorsed");
                return Ok();
            }
            return BadRequest();
        }
    }
}