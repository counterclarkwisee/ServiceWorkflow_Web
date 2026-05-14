// server/Controllers/JobControllerController.cs
using Microsoft.AspNetCore.Mvc;
using server.Interfaces;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobControllerController : ControllerBase
    {
        private readonly IJobControllerRepository _repo;

        public JobControllerController(IJobControllerRepository repo)
        {
            _repo = repo;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/jobcontroller?date=2026-05-14
        // Returns all bays + all appointments plotted on the Gantt for that date.
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetView([FromQuery] string? date)
        {
            DateOnly targetDate;

            if (string.IsNullOrWhiteSpace(date) || !DateOnly.TryParse(date, out targetDate))
                targetDate = DateOnly.FromDateTime(DateTime.Today);

            try
            {
                var result = await _repo.GetJobControllerViewAsync(targetDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load Job Controller view.", error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PATCH /api/jobcontroller/{appointmentId}/status
        // Body: { "status": "IN PROGRESS" }
        // Updates the workshop_status in jobcon_logs.
        // ─────────────────────────────────────────────────────────────────────
        [HttpPatch("{appointmentId}/status")]
        public async Task<IActionResult> UpdateStatus(
            string appointmentId,
            [FromBody] UpdateStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Status))
                return BadRequest(new { message = "Status is required." });

            var validStatuses = new[]
            {
                "PENDING", "IN PROGRESS", "JOB QUEUE",
                "DISPATCHED", "RESUME WIP", "WAITING FOR SERVICE",
                "STOPPAGE", "COMPLETE"
            };

            if (!validStatuses.Contains(request.Status.ToUpper()))
                return BadRequest(new { message = $"Invalid status: {request.Status}" });

            var success = await _repo.UpdateWorkshopStatusAsync(appointmentId, request.Status.ToUpper());

            if (!success)
                return NotFound(new { message = $"Appointment {appointmentId} not found." });

            return Ok(new { message = "Status updated.", appointmentId, status = request.Status });
        }

        // ─────────────────────────────────────────────────────────────────────
        // PATCH /api/jobcontroller/{appointmentId}/bay
        // Body: { "bayId": 3 }
        // Assigns a bay to an appointment.
        // ─────────────────────────────────────────────────────────────────────
        [HttpPatch("{appointmentId}/bay")]
        public async Task<IActionResult> AssignBay(
            string appointmentId,
            [FromBody] AssignBayRequest request)
        {
            if (request?.BayId == null || request.BayId <= 0)
                return BadRequest(new { message = "A valid bayId is required." });

            var success = await _repo.AssignBayAsync(appointmentId, request.BayId.Value);

            if (!success)
                return StatusCode(500, new { message = "Failed to assign bay." });

            return Ok(new { message = "Bay assigned.", appointmentId, bayId = request.BayId });
        }
    }

    // ── Request body models ──────────────────────────────────────────────────
    public class UpdateStatusRequest
    {
        public string? Status { get; set; }
    }

    public class AssignBayRequest
    {
        public int? BayId { get; set; }
    }
}