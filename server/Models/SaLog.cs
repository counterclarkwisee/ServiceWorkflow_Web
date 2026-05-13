using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class SaLog
    {
        [Key]
        public int sa_logs_id { get; set; } // Matches PK
        public string appointment_id { get; set; } // Matches VARCHAR(50)
        public string? receiving_status { get; set; }
        public DateTime? receiving_start { get; set; }
        public DateTime? receiving_end { get; set; }
        public string? releasing_status { get; set; }
        public DateTime? releasing_start { get; set; }
        public DateTime? releasing_end { get; set; }
    }
}