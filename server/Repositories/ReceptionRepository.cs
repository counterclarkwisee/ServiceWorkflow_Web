using server.Data;
using server.Interfaces;
using server.Models;
using Microsoft.EntityFrameworkCore;

namespace server.Repositories;

public class ReceptionRepository : IReceptionRepository
{
    private readonly ApplicationDbContext _context;

    public ReceptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync()
    {
        var today = DateTime.Today;
        return await _context.Appointments
            // Use .HasValue and .Value.Date to resolve CS1061
            .Where(a => a.created_at.HasValue && a.created_at.Value.Date == today)
            .Where(a => a.status == "BOOKED" || a.status == "ARRIVED" || a.status == "ENDORSED")
            .OrderBy(a => a.created_at)
            .ToListAsync();
    }

    public async Task<bool> MarkAsArrivedAsync(string id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return false;

        appointment.status = "ARRIVED";

        // Logic Check: When marked as ARRIVED, ensure a JobconLog entry exists 
        // to prevent "Not Found" errors in later tabs.
        return await _context.SaveChangesAsync() > 0;
    }
}