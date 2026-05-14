using server.Models;

namespace server.Interfaces;

public interface IJobconRepository
{
    // Fetches logs where SA is Endorsed and Checklister is WAITING RO
    Task<IEnumerable<JobconLog>> GetJobControllerQueueAsync();
    
    // Updates status to DISPATCHED once the technician is assigned
    Task<bool> DispatchToTechnicianAsync(int logId);
}