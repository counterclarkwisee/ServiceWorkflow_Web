// server/Repositories/JobControllerRepository.cs
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Interfaces;
using server.Models;
using server.Models.DTOs;

namespace server.Repositories
{
    public class JobControllerRepository : IJobControllerRepository
    {
        private readonly ApplicationDbContext _context;

        // Gantt grid starts at 06:00 and uses 30-minute slots.
        // A job at 08:30 → slot index 5  (08:30 - 06:00 = 150 min / 30 = 5)
        private const int GRID_START_HOUR = 6;
        private const int SLOT_MINUTES = 30;

        // Default duration (in slots) when we cannot compute end time
        private const int DEFAULT_DURATION_SLOTS = 4; // 2 hours

        public JobControllerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET FULL VIEW
        // ─────────────────────────────────────────────────────────────────────
        public async Task<JobControllerViewDto> GetJobControllerViewAsync(DateOnly date)
        {
            // 1. Fetch all active bays
            var bays = await _context.Set<Bays>()
                .Where(b => b.is_Active == 1)
                .OrderBy(b => b.bay_id)
                .Select(b => new BayDto
                {
                    BayId    = b.bay_id,
                    BayName  = b.bay_name,
                    BayType  = b.bay_type,
                    IsActive = b.is_Active == 1
                })
                .ToListAsync();

            // 2. Fetch appointments for the requested date, left-joined with jobcon_logs
            //    We use a left join so appointments without a log entry still appear.
            var jobs = await (
                from appt in _context.Appointments
                join log in _context.JobconLogs
                    on appt.appointment_id equals log.appointment_id into logGroup
                from log in logGroup.DefaultIfEmpty()           // LEFT JOIN
                where appt.appointment_date == date
                   && appt.status != "CANCELLED"               // exclude cancelled
                select new
                {
                    appt.appointment_id,
                    appt.first_name,
                    appt.last_name,
                    appt.plate_number,
                    appt.vehicle_model,
                    appt.vehicle_year,
                    appt.assigned_advisor_name,
                    appt.service_category,
                    appt.scheduled_arrival_time,
                    appt.appointment_date,

                    // jobcon_logs fields — nullable because of LEFT JOIN
                    WorkshopStatus     = log != null ? log.workshop_status    : "PENDING",
                    SaStatus           = log != null ? log.sa_status          : null,
                    ChecklisterStatus  = log != null ? log.checklister_status : null,
                    PartsStatus        = log != null ? log.parts_status       : null,
                    BayId              = log != null ? log.bay_id             : null // Add this line
                }
            ).ToListAsync();

            // 3. Map to DTOs, computing Gantt slot positions
            var jobDtos = jobs.Select(j =>
            {
                var (startSlot, duration) = ComputeSlot(j.scheduled_arrival_time);

                return new JobControllerDto
                {
                    AppointmentId        = j.appointment_id ?? string.Empty,
                    ClientName           = $"{j.first_name} {j.last_name}".Trim(),
                    PlateNumber          = j.plate_number ?? string.Empty,
                    VehicleModel         = j.vehicle_model ?? string.Empty,
                    VehicleYear          = j.vehicle_year ?? string.Empty,
                    AdvisorName          = j.assigned_advisor_name ?? string.Empty,
                    ServiceCategory      = j.service_category ?? string.Empty,
                    ScheduledArrivalTime = j.scheduled_arrival_time?.ToString("HH:mm") ?? string.Empty,
                    AppointmentDate      = j.appointment_date?.ToString("yyyy-MM-dd") ?? string.Empty,

                    // Bay info — null until a bay is assigned via AssignBayAsync
                    BayId   = null,
                    BayName = string.Empty,
                    BayType = string.Empty,

                    WorkshopStatus    = j.WorkshopStatus,
                    SaStatus          = j.SaStatus,
                    ChecklisterStatus = j.ChecklisterStatus,
                    PartsStatus       = j.PartsStatus,

                    StartSlot = startSlot,
                    Duration  = duration,
                };
            }).ToList();

            return new JobControllerViewDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Bays = bays,
                Jobs = jobDtos,
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE STATUS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> UpdateWorkshopStatusAsync(string appointmentId, string newStatus)
        {
            var log = await _context.JobconLogs
                .FirstOrDefaultAsync(l => l.appointment_id == appointmentId);

            if (log == null)
            {
                // Auto-create the log entry if it doesn't exist
                log = new JobconLog
                {
                    appointment_id   = appointmentId,
                    workshop_status  = newStatus,
                };
                _context.JobconLogs.Add(log);
            }
            else
            {
                log.workshop_status = newStatus;
                _context.JobconLogs.Update(log);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ASSIGN BAY
        // ─────────────────────────────────────────────────────────────────────
    public async Task<bool> AssignBayAsync(string appointmentId, int bayId)
    {
        var log = await _context.JobconLogs
            .FirstOrDefaultAsync(l => l.appointment_id == appointmentId);

        if (log == null)
        {
            log = new JobconLog 
            { 
                appointment_id = appointmentId, 
                bay_id = bayId,
                workshop_status = "PENDING" // Set a default status for new logs
            };
            _context.JobconLogs.Add(log);
        }
        else
        {
            log.bay_id = bayId;
            _context.JobconLogs.Update(log);
        }

        return await _context.SaveChangesAsync() > 0;
    }

        // ─────────────────────────────────────────────────────────────────────
        // HELPER: Convert scheduled_arrival_time → (startSlot, duration)
        // ─────────────────────────────────────────────────────────────────────
        private static (int startSlot, int duration) ComputeSlot(TimeOnly? arrivalTime)
        {
            if (arrivalTime == null)
                return (0, DEFAULT_DURATION_SLOTS);

            var t = arrivalTime.Value;
            int totalMinutesFromGridStart =
                (t.Hour - GRID_START_HOUR) * 60 + t.Minute;

            // Clamp to grid (06:00–18:00)
            int startSlot = Math.Max(0, totalMinutesFromGridStart / SLOT_MINUTES);
            startSlot = Math.Min(startSlot, 23); // max slot index for 18:00

            return (startSlot, DEFAULT_DURATION_SLOTS);
        }
    }
}