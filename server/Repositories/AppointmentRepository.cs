using server.Data;
using server.Interfaces;
using server.Models;
using Microsoft.EntityFrameworkCore;

namespace server.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _context.Appointments
            .OrderByDescending(a => a.created_at)
            .ToListAsync();
    }

    public async Task<string> CreateAppointmentWithServiceAsync(Appointment appointment, string serviceType)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string appointmentId = $"APT-{timestamp}-{new Random().Next(100, 999)}";
        // Generate the Service ID string
        string serviceId = $"SVC-{timestamp}-{new Random().Next(1000, 9999)}";

        appointment.appointment_id = appointmentId;
        appointment.created_at = DateTime.Now;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Appointments.Add(appointment);

            var serviceEntry = new ServiceEntry 
            { 
                service_id = serviceId, 
                appointment_id = appointmentId, 
                service_type = serviceType 
            };
            _context.Services.Add(serviceEntry);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return appointmentId;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<bool> UpdateAppointmentStatusAsync(string id, string newStatus)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return false;

        appointment.status = newStatus;
        
        // Optional: Log when it arrived
        // appointment.arrival_status = "Arrived"; 

        await _context.SaveChangesAsync();
        return true;
    }
}