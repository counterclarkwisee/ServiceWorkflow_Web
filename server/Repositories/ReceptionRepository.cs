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
        // Use DateOnly to match your database column type for appointment_date
        var today = DateOnly.FromDateTime(DateTime.Today);
        
        return await _context.Appointments
            .Where(a => a.appointment_date == today) // Filter by the actual visit date
            .Where(a => a.status == "BOOKED" || a.status == "ARRIVED" || a.status == "ENDORSED")
            .OrderBy(a => a.scheduled_arrival_time) // Order by their time slot
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