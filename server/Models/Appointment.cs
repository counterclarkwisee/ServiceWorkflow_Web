namespace server.Models;

public class Appointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string? appointment_id { get; set; }
    public DateTime? created_at { get; set; }
    public string? last_name { get; set; }
    public string? first_name { get; set; }
    public string? plate_number { get; set; }
    public DateOnly? appointment_date { get; set; }
    public TimeOnly? scheduled_arrival_time { get; set; }
    public string? service_category { get; set; }
    public string service_type { get; set; } // Add this line
    public string? status { get; set; }
}