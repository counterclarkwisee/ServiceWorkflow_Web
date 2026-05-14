using Microsoft.AspNetCore.Mvc;
using server.Interfaces;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaController : ControllerBase
    {
        private readonly ISaRepository _repo;

        public SaController(ISaRepository repo) => _repo = repo;

        // 1. Get all appointments for the SA Dashboard
        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _repo.GetAllAppointmentsAsync();
            return Ok(appointments);
        }

        // 2. Start Service Advisor Receiving
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartSaReceiving(string id)
    {
        // Log the ID to your console to ensure it's arriving correctly
        Console.WriteLine($"Attempting to start SA for ID: {id}");
        
        var result = await _repo.StartSaReceivingAsync(id);
        
        if (!result) 
        {
            // This triggers the "Could not start session" alert in React
            return BadRequest("Could not start session."); 
        }
        
        return Ok();
    }

        // 3. End Service Advisor Receiving
        [HttpPost("{id}/end")] // Changed to HttpPost to match your recent pattern
        public async Task<IActionResult> EndSa(string id)
        {
            // FIX: Removed manual SyncJobConGate1Async call.
            // Your SaRepository.EndSaReceivingAsync already calls the sync logic internally.
            var success = await _repo.EndSaReceivingAsync(id); 
            
            if (success)
            {
                return Ok(new { status = "Finished" });
            }
            
            // This prevents the "Workflow Error" popup in your React UI
            return BadRequest("Workflow Error: Could not end SA session.");
        }
    }
}