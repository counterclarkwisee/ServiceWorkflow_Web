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

    public async Task<bool> StartPreServiceAsync(string id) {
        var newLog = new ChecklisterLog {
            appointment_id = id,
            preservice_status = "STARTED",
            preservice_start = DateTime.Now
        };
        _context.ChecklisterLogs.Add(newLog);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndPreServiceAsync(string id) {
        var log = await _context.ChecklisterLogs
            .FirstOrDefaultAsync(l => l.appointment_id == id && l.preservice_end == null);

        if (log == null) return false;

        log.preservice_status = "FINISHED";
        log.preservice_end = DateTime.Now;

        var saved = await _context.SaveChangesAsync() > 0;
        if (saved) {
            try {
                // Call the shared sync logic in AppointmentRepo
                await _appointmentRepo.SyncJobConGate1Async(id, "Checklister", "WAITING RO");
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

        log.checklister_status = "WAITING RO";
        log.workshop_status = "PENDING"; 

        return await _context.SaveChangesAsync() > 0;
    }
}