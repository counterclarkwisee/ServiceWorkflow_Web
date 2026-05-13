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
            .Include(a => a.SaLog) // CRITICAL: This allows the UI to see the current SA status
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
        appointment.status = "BOOKED";

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

            var initialJobCon = new JobconLog
            {
                appointment_id = appointmentId,
                workshop_status = "PENDING",
                sa_status = "PENDING",
                checklister_status = "PENDING",
                parts_status = "PENDING"
            };
            _context.JobconLogs.Add(initialJobCon);

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
    
    public async Task<bool> UpdatePreServiceStatusAsync(string id, string preStatus)
    {
        var appointment = await _context.Appointments
            .Include(a => a.ChecklisterLog)
            .FirstOrDefaultAsync(a => a.appointment_id == id);

        if (appointment == null) return false;

        if (appointment.ChecklisterLog != null)
        {
            appointment.ChecklisterLog.preservice_status = preStatus;
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
            .Include(a => a.ChecklisterLog)
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

    public async Task<bool> StartSaReceivingAsync(string id)
    {
        var existingLog = await _context.SaLogs.FirstOrDefaultAsync(l => l.appointment_id == id);
        if (existingLog != null) return false;

        var newLog = new SaLog
        {
            appointment_id = id,
            receiving_status = "Started",
            receiving_start = DateTime.Now
        };

        _context.SaLogs.Add(newLog); 
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EndSaReceivingAsync(string id)
    {
        var log = await _context.SaLogs
            .FirstOrDefaultAsync(l => l.appointment_id == id && l.receiving_end == null);

        if (log == null) return false;

        log.receiving_status = "Finished";
        log.receiving_end = DateTime.Now;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task UpdateJobConStatusAsync(string appointmentId)
    {
        var log = await _context.JobconLogs
            .FirstOrDefaultAsync(l => l.appointment_id == appointmentId);

        if (log != null && log.sa_status == "Endorsed" && log.checklister_status == "WAITING RO")
        {
            log.workshop_status = "PENDING"; 

            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null) appointment.status = "Endorsed";

            await _context.SaveChangesAsync();
        }
    }

    public async Task SyncJobConGate1Async(string appointmentId, string type, string value)
    {
        var log = await _context.JobconLogs
            .FirstOrDefaultAsync(l => l.appointment_id == appointmentId);

        if (log == null) return;

        if (type == "SA") log.sa_status = value;
        if (type == "Checklister") log.checklister_status = value;

        if (log.sa_status == "Endorsed" && log.checklister_status == "WAITING RO")
        {
            log.workshop_status = "READY";
            
            var app = await _context.Appointments.FindAsync(appointmentId);
            if (app != null) app.status = "Endorsed";
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> StartPreServiceAsync(string appointmentId) => await StartPreServiceLogAsync(appointmentId);
    public async Task<bool> EndPreServiceAsync(string appointmentId) => await EndPreServiceLogAsync(appointmentId);
}