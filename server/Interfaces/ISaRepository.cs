using server.Models;

namespace server.Interfaces;

public interface ISaRepository {
    Task<bool> StartSaReceivingAsync(string id);
    Task<bool> EndSaReceivingAsync(string id);
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
}