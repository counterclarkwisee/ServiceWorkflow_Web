using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

[Table("jobcon_logs")] // Matches your SQL Table Name exactly
public class JobconLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int jobcon_logs_id { get; set; } // Matches PK in SQL

    [Required]
    public string appointment_id { get; set; } = string.Empty;

    public string workshop_status { get; set; } = "PENDING"; // Matches Default in SQL
    
    public string? sa_status { get; set; } // Nullable to match SQL
    public string? checklister_status { get; set; } // Nullable to match SQL
    public string? parts_status { get; set; } // Nullable to match SQL
}