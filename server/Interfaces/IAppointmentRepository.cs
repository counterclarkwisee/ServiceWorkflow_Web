using server.Models;

namespace server.Interfaces;

public interface IAppointmentRepository
{
    Task<string> CreateAppointmentWithServiceAsync(Appointment appointment, string serviceCategory);
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<bool> UpdateAppointmentStatusAsync(string id, string status); 
    // New method for the Checklister's "Start" button
    Task<bool> UpdatePreServiceStatusAsync(string id, string preStatus);
    Task<IEnumerable<Appointment>> GetArrivedAppointmentsAsync();
    Task<IEnumerable<Appointment>> GetChecklisterQueueAsync();
    Task<bool> StartPreServiceLogAsync(string id);
    Task<bool> EndPreServiceLogAsync(string id);
}