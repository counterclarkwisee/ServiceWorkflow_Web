using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _repo;

    public AppointmentsController(IAppointmentRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        var appointments = await _repo.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
    {
        appointment.status = "BOOKED";
        // Passing the category value from the frontend
        var id = await _repo.CreateAppointmentWithServiceAsync(appointment, appointment.service_category);
        return Ok(new { id, message = "Successfully Booked" });
    }

    [HttpGet("endorsed")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetEndorsedAppointments()
    {
        // Filter for appointments that have reached the 'Endorsed' status
        var endorsed = await _repo.GetAppointmentsByStatusAsync("Endorsed");
        return Ok(endorsed);
    }
}