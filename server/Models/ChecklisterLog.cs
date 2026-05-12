using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace server.Models;

public class ChecklisterLog
{
    [Key]
    public int checklister_logs_id { get; set; }
    public string appointment_id { get; set; }

    public string? preservice_status { get; set; }
    public DateTime? preservice_start { get; set; }
    public DateTime? preservice_end { get; set; }

    public string? postservice_status { get; set; }
    public DateTime? postservice_start { get; set; }
    public DateTime? postservice_end { get; set; }

    // Navigation property to link back to the Appointment
    [ForeignKey("appointment_id")]
    [JsonIgnore]
    public Appointment? Appointment { get; set; }
}