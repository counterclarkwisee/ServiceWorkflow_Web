namespace server.Models;

public class Service
{
    public string? service_id { get; set; }
    public string? appointment_id { get; set; }
    public DateTime? created_at { get; set; }
    public string? created_by { get; set; }
    public string? service_type { get; set; }
    public int? estimated_duration_minutes { get; set; }
    public int? current_duration_minutes { get; set; }
    public TimeOnly? original_start_time { get; set; }
    public string? original_bay_id { get; set; }
    public TimeOnly? current_start_time { get; set; } 
    public string? current_bay_id { get; set; }
    public string? status { get; set; }
    public DateTime? last_modified_at { get; set; }
    public string? last_modified_by { get; set; }
}