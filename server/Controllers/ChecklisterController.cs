using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChecklisterController : ControllerBase
{
    private readonly IAppointmentRepository _repo;

    public ChecklisterController(IAppointmentRepository repo) => _repo = repo;

    // 1. Get the Queue (Unified with Log data)
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        // This uses the Join/Include logic we discussed to show Start/End buttons
        var queue = await _repo.GetChecklisterQueueAsync();
        return Ok(queue);
    }

    // 2. Start the Checklist (Creates a new Log entry)
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartChecklist(string id)
    {
        // This calls the repo method that performs an INSERT into checklister_logs
        var success = await _repo.StartPreServiceLogAsync(id);
        if (!success) return BadRequest("Could not start checklist log.");
        
        return Ok(new { message = "Checklist started" });
    }

    // 3. End the Checklist (Updates the existing Log entry)
    [HttpPut("{id}/end")]
    public async Task<IActionResult> EndChecklist(string id)
    {
        // This calls the repo method that updates preservice_end and status to "Finished"
        var success = await _repo.EndPreServiceLogAsync(id);
        if (!success) return NotFound("Active checklist log not found.");
        
        return Ok(new { message = "Checklist finished" });
    }
}