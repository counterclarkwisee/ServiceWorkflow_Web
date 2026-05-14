// Controllers/JobconController.cs
using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobconController : ControllerBase
{
    private readonly IJobconRepository _repo;

    public JobconController(IJobconRepository repo) => _repo = repo;

    // Matches axios.get(".../api/jobcon/queue")
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        var queue = await _repo.GetJobControllerQueueAsync();
        return Ok(queue);
    }

    // Matches axios.post(".../api/jobcon/dispatch/{id}")
    [HttpPost("dispatch/{id}")]
    public async Task<IActionResult> Dispatch(int id)
    {
        var success = await _repo.DispatchToTechnicianAsync(id);
        
        if (!success) 
        {
            return BadRequest("Workflow Error: Could not dispatch to technician.");
        }
        
        return Ok(new { message = "Vehicle dispatched successfully" });
    }
}