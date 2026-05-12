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

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        var all = await _repo.GetAllAppointmentsAsync();
        // Workflow: Checklister acts on vehicles marked as ARRIVED
        return Ok(all.Where(a => a.status == "ARRIVED"));
    }

    [HttpPatch("{id}/start")]
    public async Task<IActionResult> StartChecklist(string id)
    {
        // Transition to terminal status: PRE-SERVICE CHECKLISTING
        await _repo.UpdateAppointmentStatusAsync(id, "PRE-SERVICE CHECKLISTING");
        return Ok();
    }
}