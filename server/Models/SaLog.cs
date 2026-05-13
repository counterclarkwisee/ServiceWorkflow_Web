using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace server.Models
{
    [Table("sa_logs")] // This MUST match the table name in image_66659b.png
    public class SaLog
    {
        [Key]
        public int sa_logs_id { get; set; } 
        public string appointment_id { get; set; } = string.Empty;
        public string? receiving_status { get; set; }
        public DateTime? receiving_start { get; set; }
        public DateTime? receiving_end { get; set; }
        public string? releasing_status { get; set; }
        public DateTime? releasing_start { get; set; }
        public DateTime? releasing_end { get; set; }

        [ForeignKey("appointment_id")]
        [JsonIgnore]
        public Appointment? Appointment { get; set; }
    }
}