using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;
using server.DTOs;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
    {
        var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> PostAppointment([FromBody] AppointmentRequestDto request)
    {
        var newAppointment = new Appointment
        {
            first_name = request.first_name,
            last_name = request.last_name,
            plate_number = request.plate_number,
            appointment_date = request.appointment_date,
            // Change this line from "Pending" to "BOOKED"
            status = "BOOKED" 
        };

        try
        {
            string generatedId = await _appointmentRepository.CreateAppointmentWithServiceAsync(newAppointment, request.service_type);
            return Ok(new { message = "Success", appointmentId = generatedId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
    {
        var success = await _appointmentRepository.UpdateAppointmentStatusAsync(id, status);
        if (!success) return NotFound();

        return Ok(new { message = "Status updated successfully" });
    }

    [HttpGet("booked")]
    public async Task<IActionResult> GetBookedAppointments()
    {
        // Use the repo, NOT _context
        var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
        
        // Filter for "booked" status here or in the Repo
        var bookedOnly = appointments.Where(a => a.status == "BOOKED");
        
        return Ok(bookedOnly);
    }
}
