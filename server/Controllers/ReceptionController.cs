[ApiController]
[Route("api/[controller]")]
public class ReceptionController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;

    public ReceptionController(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    [HttpGet("booked")]
    public async Task<IActionResult> GetBooked()
    {
        var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
        var booked = appointments.Where(a => a.status == "BOOKED");
        return Ok(booked);
    }

    [HttpPatch("{id}/arrive")]
    public async Task<IActionResult> MarkArrived(string id)
    {
        var success = await _appointmentRepository.UpdateAppointmentStatusAsync(id, "ARRIVED");
        if (!success) return NotFound();
        return Ok(new { message = "Status updated to ARRIVED" });
    }
}