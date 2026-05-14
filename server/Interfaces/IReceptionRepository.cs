using server.Models;

namespace server.Interfaces;
public interface IReceptionRepository
{
    Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync();
    Task<bool> MarkAsArrivedAsync(string id);
}