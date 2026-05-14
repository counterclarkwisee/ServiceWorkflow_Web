using server.Data;
using server.Interfaces;
using server.Models;
using Microsoft.EntityFrameworkCore;

namespace server.Repositories;

public class SaRepository : ISaRepository {
    private readonly ApplicationDbContext _context;

    // ALIGNED: Now strictly using ISaRepository context
    public SaRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _context.Appointments
            // MATCH: Using lowercase 'sa_log' as defined in your corrected Model
            .Include(a => a.sa_log) 
            .OrderByDescending(a => a.created_at)
            .ToListAsync();
    }

    public async Task<bool> StartSaReceivingAsync(string id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.sa_log) 
            .FirstOrDefaultAsync(a => a.appointment_id == id);

        if (appointment == null) return false;

        if (appointment.sa_log == null)
        {
            appointment.sa_log = new SaLog 
            { 
                appointment_id = id,
                receiving_status = "STARTED",
                receiving_start = DateTime.Now 
            };
            _context.SaLogs.Add(appointment.sa_log);
        }
        else
        {
            appointment.sa_log.receiving_status = "STARTED";
        }

        appointment.status = "ARRIVED"; 

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
                // Internal logic to sync with Job Control Gate 1
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
            
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.appointment_id == appointmentId);

        if (log == null || appointment == null) return false;

        log.sa_status = "Endorsed";

        // Logic: Move to ENDORSED only if SA is finished AND Checklister is done
        if (log.sa_status == "Endorsed" && log.checklister_status == "WAITING RO")
        {
            log.workshop_status = "ENDORSED";
            appointment.status = "ENDORSED";
        }
        else
        {
            log.workshop_status = "PENDING"; 
        }

        return await _context.SaveChangesAsync() > 0;
    }
}