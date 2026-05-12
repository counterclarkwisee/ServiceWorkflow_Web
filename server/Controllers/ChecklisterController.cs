using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChecklisterController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;

    public ChecklisterController(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    // GET: api/checklister/queue
    [HttpGet("queue")]
    public async Task<IActionResult> GetChecklistQueue()
    {
        var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
        // Workflow: Checklister acts on vehicles marked as ARRIVED
        var queue = appointments.Where(a => a.status == "ARRIVED");
        return Ok(queue);
    }

    // PATCH: api/checklister/{id}/start
    [HttpPatch("{id}/start")]
    public async Task<IActionResult> StartChecklisting(string id)
    {
        // Terminal Status: PRE-SERVICE CHECKLISTING
        var success = await _appointmentRepository.UpdateAppointmentStatusAsync(id, "PRE-SERVICE CHECKLISTING");
        if (!success) return NotFound();

        return Ok(new { message = "Vehicle moved to Pre-Service Checklisting" });
    }
}