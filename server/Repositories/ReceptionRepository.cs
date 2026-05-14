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
            .Where(a => a.created_at.HasValue && a.created_at.Value.Date == today)
            // Add "ENDORSED" to the status check so they don't disappear after Gate 1
            .Where(a => a.status == "BOOKED" || a.status == "ARRIVED" || a.status == "ENDORSED")
            .OrderBy(a => a.created_at)
            .ToListAsync();
    }

    public async Task<bool> MarkAsArrivedAsync(string id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return false;

        // Changing status to ARRIVED pushes it to Checklister/SA visibility
        appointment.status = "ARRIVED";
        return await _context.SaveChangesAsync() > 0;
    }
}