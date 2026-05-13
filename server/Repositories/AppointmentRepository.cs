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

    public async Task<string> CreateAppointmentWithServiceAsync(Appointment appointment, string serviceName)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string appointmentId = $"APT-{timestamp}-{new Random().Next(100, 999)}";
        string serviceId = $"SVC-{timestamp}-{new Random().Next(1000, 9999)}";

        appointment.appointment_id = appointmentId;
        appointment.created_at = DateTime.Now;
        appointment.service_category = serviceName;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Appointments.Add(appointment);

            var serviceRecord = new Service 
            { 
                service_id = serviceId, 
                appointment_id = appointmentId, 
                service_type = serviceName 
            };
            _context.Services.Add(serviceRecord);

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
    
    // FIXED: Added Include and corrected variable names to clear error CS1061
    public async Task<bool> UpdatePreServiceStatusAsync(string id, string preStatus)
    {
        var appointment = await _context.Appointments
            .Include(a => a.ChecklisterLog) // Must include the join
            .FirstOrDefaultAsync(a => a.appointment_id == id);

        if (appointment == null) return false;

        if (appointment.ChecklisterLog != null)
        {
            appointment.ChecklisterLog.preservice_status = preStatus; // Matches parameter name
        } 
        
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAppointmentStatusAsync(string id, string status)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return false;

        appointment.status = status; 
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(string status)
    {
        return await _context.Appointments
            .Where(a => a.status == status)
            .OrderByDescending(a => a.created_at)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetArrivedAppointmentsAsync()
    {
        return await _context.Appointments
            .Where(a => a.status == "ARRIVED")
            .OrderByDescending(a => a.created_at)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetChecklisterQueueAsync()
    {
        return await _context.Appointments
            .Include(a => a.ChecklisterLog) // Correctly pulls joined data
            .Where(a => a.status == "ARRIVED")
            .ToListAsync();
    }

    public async Task<bool> StartPreServiceLogAsync(string id)
    {
        var newLog = new ChecklisterLog
        {
            appointment_id = id,
            preservice_status = "Started",
            preservice_start = DateTime.Now
        };

        _context.ChecklisterLogs.Add(newLog);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndPreServiceLogAsync(string id)
    {
        var log = await _context.ChecklisterLogs
            .FirstOrDefaultAsync(l => l.appointment_id == id && l.preservice_end == null);

        if (log == null) return false;

        log.preservice_status = "Finished";
        log.preservice_end = DateTime.Now;

        return await _context.SaveChangesAsync() > 0;
    }
    public async Task<bool> StartSaReceivingAsync(string appointmentId)
    {
        var log = await _context.SaLogs
            .FirstOrDefaultAsync(l => l.appointment_id == appointmentId);

        if (log == null)
        {
            log = new SaLog
            {
                appointment_id = appointmentId,
                receiving_status = "Started",
                receiving_start = DateTime.Now
            };
            _context.SaLogs.Add(log);
        }
        else
        {
            log.receiving_status = "In-Progress";
            log.receiving_start = DateTime.Now;
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndSaReceivingAsync(string appointmentId)
    {
        var log = await _context.SaLogs
            .FirstOrDefaultAsync(l => l.appointment_id == appointmentId);

        if (log == null) return false;

        log.receiving_status = "Finished";
        log.receiving_end = DateTime.Now;

        return await _context.SaveChangesAsync() > 0;
    }

    // These are duplicates of the "Log" versions above, 
    // but kept so your other controllers don't break.
    public async Task<bool> StartPreServiceAsync(string appointmentId) => await StartPreServiceLogAsync(appointmentId);
    public async Task<bool> EndPreServiceAsync(string appointmentId) => await EndPreServiceLogAsync(appointmentId);
}