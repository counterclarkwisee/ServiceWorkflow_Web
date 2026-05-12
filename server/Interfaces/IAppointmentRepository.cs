using server.Models;

namespace server.Interfaces;

public interface IAppointmentRepository
{
    Task<string> CreateAppointmentWithServiceAsync(Appointment appointment, string serviceCategory);
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<bool> UpdateAppointmentStatusAsync(string id, string newStatus);
}