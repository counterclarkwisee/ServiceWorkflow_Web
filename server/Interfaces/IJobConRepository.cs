// server/Interfaces/IJobControllerRepository.cs
using server.Models.DTOs;

namespace server.Interfaces
{
    public interface IJobControllerRepository
    {
        /// <summary>
        /// Returns all bays and their appointments for the given date,
        /// joined with jobcon_logs for status info.
        /// </summary>
        Task<JobControllerViewDto> GetJobControllerViewAsync(DateOnly date);

        /// <summary>
        /// Updates the workshop_status of a specific jobcon_log entry.
        /// </summary>
        Task<bool> UpdateWorkshopStatusAsync(string appointmentId, string newStatus);

        /// <summary>
        /// Assigns a bay to an appointment (stored in jobcon_logs).
        /// Creates the log entry if it doesn't exist yet.
        /// </summary>
        Task<bool> AssignBayAsync(string appointmentId, int bayId);
    }
}