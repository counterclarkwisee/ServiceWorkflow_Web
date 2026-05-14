using server.Data;
using server.Interfaces;
using server.Models;
using Microsoft.EntityFrameworkCore;

namespace server.Repositories;

public class ChecklisterRepository : IChecklisterRepository {
    private readonly ApplicationDbContext _context;
    private readonly IAppointmentRepository _appointmentRepo; // Inject to access Sync logic

    public ChecklisterRepository(ApplicationDbContext context, IAppointmentRepository appointmentRepo) {
        _context = context;
        _appointmentRepo = appointmentRepo;
    }

    public async Task<IEnumerable<Appointment>> GetChecklisterQueueAsync() 
    {
        return await _context.Appointments
            .Include(a => a.ChecklisterLog)
            // Allow both ARRIVED (Pending) and ENDORSED (Finished Gate 1) statuses
            .Where(a => a.status == "ARRIVED" || a.status == "ENDORSED")
            .OrderByDescending(a => a.created_at) // Keep recent ones at the top
            .ToListAsync();
    }

    public async Task<bool> StartPreServiceAsync(string id)
    {
        var newLog = new ChecklisterLog
        {
            appointment_id = id,
            preservice_status = "STARTED", // Updated from 'status'
            preservice_start = DateTime.Now // Updated from 'start_time'
        };
        _context.ChecklisterLogs.Add(newLog);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndPreServiceAsync(string id)
    {
        var log = await _context.ChecklisterLogs
            .FirstOrDefaultAsync(l => l.appointment_id == id && l.preservice_end == null);

        if (log == null) return false;

        log.preservice_status = "FINISHED"; // Updated from 'status'
        log.preservice_end = DateTime.Now;    // Updated from 'end_time'

        var saved = await _context.SaveChangesAsync() > 0;
        if (saved)
        {
            await SyncJobConGate1Async(id);
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

        // 2. Update the status for the Checklister
        log.checklister_status = "WAITING RO";

        // 3. Evaluate Gate 1 Logic
        // Workshop status should only be "ENDORSED" if both criteria are met
        if (log.sa_status == "Endorsed" && log.checklister_status == "WAITING RO")
        {
            // Update JobCon table
            log.workshop_status = "ENDORSED";

            // Update master Appointment table
            appointment.status = "ENDORSED";
        }
        else
        {
            // Keep as PENDING if the SA hasn't finished their endorsed status yet
            log.workshop_status = "PENDING"; 
        }

        // 4. Save Changes
        // Strictly avoiding 'updated_at' to maintain SQL schema integrity
        return await _context.SaveChangesAsync() > 0;
    }
}