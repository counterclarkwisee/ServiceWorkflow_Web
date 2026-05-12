namespace server.DTOs;

public class AppointmentRequestDto 
{
    public string first_name { get; set; } = null!;
    public string last_name { get; set; } = null!;
    public string plate_number { get; set; } = null!;
    public DateOnly appointment_date { get; set; }
    public string service_type { get; set; } = null!;
}