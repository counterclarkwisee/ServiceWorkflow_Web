using System.ComponentModel.DataAnnotations;

namespace server.Models;

public class ServiceEntry
{
    [Key]
    public string service_id { get; set; } = null!; // Must be string for "SVC-..."
    public string appointment_id { get; set; } = null!;
    public string service_type { get; set; } = null!;
}