using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentsController(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    // POST: api/appointments
    [cite_start]// This handles the initial "Booked" phase from the web form [cite: 1, 3]
    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
    {
        if (appointment == null) return BadRequest();

        [cite_start]// Ensure new bookings always start with the correct Phase 1 status 
        appointment.status = "BOOKED";
        
        // service_type logic here if needed for the repository method
        var appointmentId = await _appointmentRepository.CreateAppointmentWithServiceAsync(appointment, appointment.service_type);

        return Ok(new { id = appointmentId, message = "Appointment booked successfully" });
    }

    // Optional: Get single appointment for status tracking
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(string id)
    {
        var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
        var appointment = appointments.FirstOrDefault(a => a.appointment_id == id);
        
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }
}