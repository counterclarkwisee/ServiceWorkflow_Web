using server.Models;

namespace server.Interfaces;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<string> CreateAppointmentWithServiceAsync(Appointment appointment, string serviceType);
    
    // Updated to support specific workflow metadata if needed
    Task<bool> UpdateAppointmentStatusAsync(string id, string newStatus);
}