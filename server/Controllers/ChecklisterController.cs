using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChecklisterController : ControllerBase
{
    private readonly IChecklisterRepository _repo;

    public ChecklisterController(IChecklisterRepository repo) => _repo = repo;

    // 1. Get the Queue 
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        var queue = await _repo.GetChecklisterQueueAsync();
        return Ok(queue);
    }

    // 2. Start the Checklist
    // Unified to match the method name in your new IChecklisterRepository
    [HttpPost("start/{id}")]
    public async Task<IActionResult> StartChecklist(string id)
    {
        // Removed "Log" from the method call to match your interface
        var success = await _repo.StartPreServiceAsync(id);
        
        if (!success) return BadRequest("Could not start checklist log.");
        
        return Ok(new { message = "Checklist started", success });
    }

    // 3. End the Checklist
    // Changed to HttpPost to match your recent repository pattern
    [HttpPost("end/{id}")]
    public async Task<IActionResult> EndChecklist(string id)
    {
        // Removed "Log" from the method call to match your interface
        var success = await _repo.EndPreServiceAsync(id);
        
        if (!success) 
        {
            // This prevents the "Workflow Error" popup in the UI
            return BadRequest("Workflow Error: Could not finish pre-service log."); 
        }
        
        return Ok(new { message = "Checklist finished", success });
    }
}