using Microsoft.AspNetCore.Mvc;
using server.Interfaces;
using server.Models;

namespace server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _repo;

    public AppointmentsController(IAppointmentRepository appointmentRepository)
    {
        _repo = appointmentRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // This ensures the saLog object isn't null in the JSON response
        var data = await _repo.GetAllAppointmentsAsync(); 
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
    {
        appointment.status = "BOOKED";
        // Passing the category value from the frontend
        var id = await _repo.CreateAppointmentWithServiceAsync(appointment, appointment.service_category);
        return Ok(new { id, message = "Successfully Booked" });
    }
}