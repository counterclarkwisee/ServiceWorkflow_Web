// Repositories/JobconRepository.cs
using server.Data;
using server.Interfaces;
using server.Models;
using Microsoft.EntityFrameworkCore;

namespace server.Repositories;

public class JobconRepository : IJobconRepository
{
    private readonly ApplicationDbContext _context;

    public JobconRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobconLog>> GetJobControllerQueueAsync()
    {
        return await _context.JobconLogs
            .Where(j => j.sa_status == "Endorsed" && j.checklister_status == "WAITING RO")
            // Sort by ID instead of the missing updated_at column
            .OrderByDescending(j => j.jobcon_logs_id) 
            .ToListAsync();
    }

    public async Task<bool> DispatchToTechnicianAsync(int logId)
    {
        var log = await _context.JobconLogs.FindAsync(logId);
        if (log == null) return false;

        log.sa_status = "DISPATCHED"; 
        // REMOVED: log.updated_at = DateTime.Now; (We don't need this)

        return await _context.SaveChangesAsync() > 0;
    }
}