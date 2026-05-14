using server.Data;
using server.Interfaces;
using server.Models;
using Microsoft.EntityFrameworkCore;

namespace server.Repositories;
public class SaRepository : ISaRepository {
    private readonly ApplicationDbContext _context;
    private readonly IAppointmentRepository _appointmentRepo;

    public SaRepository(ApplicationDbContext context, IAppointmentRepository appointmentRepo) {
        _context = context;
        _appointmentRepo = appointmentRepo;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _context.Appointments
            .Include(a => a.SaLog)
            .OrderByDescending(a => a.created_at)
            .ToListAsync();
    }

    public async Task<bool> StartSaReceivingAsync(string id) {
        if (await _context.SaLogs.AnyAsync(l => l.appointment_id == id)) return false;

        var newLog = new SaLog {
            appointment_id = id,
            receiving_status = "STARTED",
            receiving_start = DateTime.Now
        };
        _context.SaLogs.Add(newLog);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndSaReceivingAsync(string id) {
        var log = await _context.SaLogs
            .FirstOrDefaultAsync(l => l.appointment_id == id && l.receiving_end == null);

        if (log == null) return false;

        log.receiving_status = "FINISHED";
        log.receiving_end = DateTime.Now;

        var saved = await _context.SaveChangesAsync() > 0;
        if (saved) {
            try {
                // FIX: Call the local method you defined below, 
                // instead of calling the appointmentRepo with the wrong parameters.
                await this.SyncJobConGate1Async(id); 
            } catch (Exception ex) {
                Console.WriteLine($"Sync failed: {ex.Message}");
            }
        }
        return saved;
    }

    public async Task<bool> SyncJobConGate1Async(string appointmentId)
    {
        // 1. Fetch both the JobCon Log and the Appointment record
        var log = await _context.JobconLogs
            .FirstOrDefaultAsync(j => j.appointment_id == appointmentId);
            
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.appointment_id == appointmentId);

        if (log == null || appointment == null) return false;

        // 2. Update the status for the Service Advisor
        log.sa_status = "Endorsed";

        // 3. Evaluate Gate 1 Logic
        // If BOTH the SA is finished AND the Checklister is done, move to ENDORSED
        if (log.sa_status == "Endorsed" && log.checklister_status == "WAITING RO")
        {
            // Update JobCon table status
            log.workshop_status = "ENDORSED";
            
            // Update the master Appointment table status as requested
            appointment.status = "ENDORSED";
        }
        else
        {
            // Keep as PENDING if the Gate 1 requirements aren't fully met
            log.workshop_status = "PENDING"; 
        }

        // 4. Save Changes
        // Strictly avoiding 'updated_at' to prevent MySqlException
        return await _context.SaveChangesAsync() > 0;
    }
}