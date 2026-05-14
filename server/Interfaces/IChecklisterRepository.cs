using server.Models;

namespace server.Interfaces;

public interface IChecklisterRepository {
    Task<IEnumerable<Appointment>> GetChecklisterQueueAsync();
    Task<bool> StartPreServiceAsync(string id);
    Task<bool> EndPreServiceAsync(string id);
}