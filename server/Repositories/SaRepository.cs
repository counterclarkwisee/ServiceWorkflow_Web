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
        var log = await _context.JobconLogs
            .FirstOrDefaultAsync(j => j.appointment_id == appointmentId);

        if (log == null) return false;

        // 1. Update the status for the Service Advisor
        log.sa_status = "Endorsed";

        // 2. Evaluate Gate 1 Logic
        // If BOTH the SA is finished AND the Checklister is done, move to ENDORSED
        if (log.sa_status == "Endorsed" && log.checklister_status == "WAITING RO")
        {
            log.workshop_status = "ENDORSED";
        }
        else
        {
            // Keep as PENDING if the Checklister hasn't finished yet
            log.workshop_status = "PENDING"; 
        }

        // 3. Save Changes
        // Strictly avoided 'updated_at' to prevent MySqlException based on your schema
        return await _context.SaveChangesAsync() > 0;
    }
}