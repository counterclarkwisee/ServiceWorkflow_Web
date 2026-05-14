using Microsoft.AspNetCore.Mvc; // Fixes ControllerBase, [ApiController], [Route]
using server.Interfaces;        // Fixes IReceptionRepository
using server.Models;

[ApiController]
[Route("api/[controller]")]
public class ReceptionController : ControllerBase
{
    private readonly IReceptionRepository _repo;

    public ReceptionController(IReceptionRepository repo) => _repo = repo;

    [HttpGet("today")]
    public async Task<IActionResult> GetToday() 
    {
        var result = await _repo.GetTodayAppointmentsAsync();
        return Ok(result);
    }

    [HttpPost("arrive/{id}")]
    public async Task<IActionResult> Arrive(string id)
    {
        var success = await _repo.MarkAsArrivedAsync(id);
        if (!success) return BadRequest("Could not update arrival status.");
        return Ok(new { message = "Vehicle marked as arrived" });
    }
}