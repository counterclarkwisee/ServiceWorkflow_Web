using server.Models;

namespace server.Interfaces;

public interface IAppointmentRepository
{
    Task<string> CreateAppointmentWithServiceAsync(Appointment appointment, string serviceType);
    
    // Add these two lines:
    Task<bool> UpdateAppointmentStatusAsync(string id, string newStatus);
    // Task<IEnumerable<Appointment>> GetBookedAppointmentsAsync(); // If you use a repo for the GET

    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync(); 
}
