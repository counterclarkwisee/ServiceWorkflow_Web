using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

[Table("appointments")] // Ensure this matches your actual table name
public class Appointment
{
    [Key]
    public string? appointment_id { get; set; }
    public DateTime? created_at { get; set; }
    public string? created_by { get; set; }
    public string? last_name { get; set; }
    public string? first_name { get; set; }
    public string? client_phone { get; set; }
    public string? plate_number { get; set; }
    public string? cs_no { get; set; }
    public string? vehicle_model { get; set; }
    public string? vehicle_year { get; set; }
    public DateOnly? appointment_date { get; set; }
    public TimeOnly? scheduled_arrival_time { get; set; }
    public string? assigned_advisor_name { get; set; }
    public string? source { get; set; }
    public string? service_category { get; set; }
    public string? status { get; set; }
    public string? reschedule_id { get; set; }
    public string? n1d_confirmation { get; set; }
    public string? n1h_confirmation { get; set; }
    public string? status_remarks { get; set; }
    public string? olb_no { get; set; }
    public string? assignee_last_name { get; set; }
    public string? assignee_first_name { get; set; }
    public string? assignee_contact { get; set; }
    public string? remarks { get; set; }
    public DateTime? last_modified_at { get; set; }
    public string? last_modified_by { get; set; }
    public string? arrival_status { get; set; } 
    public ChecklisterLog? ChecklisterLog { get; set; }
}