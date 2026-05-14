// server/Models/DTOs/JobControllerDto.cs
namespace server.Models.DTOs
{
    public class JobControllerDto
    {
        public string AppointmentId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleYear { get; set; } = string.Empty;
        public string AdvisorName { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        public string ScheduledArrivalTime { get; set; } = string.Empty; // Format: "HH:mm"
        public string AppointmentDate { get; set; } = string.Empty;      // Format: "yyyy-MM-dd"

        // --- Bay Assignment ---
        // This is critical: React uses this ID to place the job in the correct row.
        public int? BayId { get; set; } 
        public string BayName { get; set; } = string.Empty;
        public string BayType { get; set; } = string.Empty;

        // --- Workshop Statuses (from jobcon_logs) ---
        // Using "WorkshopStatus" to match the React state management
        public string WorkshopStatus { get; set; } = "PENDING"; 
        public string? SaStatus { get; set; }
        public string? ChecklisterStatus { get; set; }
        public string? PartsStatus { get; set; }

        // --- Gantt Rendering Logic ---
        // StartSlot: (Time - 06:00) / 30 mins. 
        // Example: 08:30 is Slot 5.
        public int StartSlot { get; set; }   
        
        // Duration: Number of 30-min blocks. 
        // Example: 2 hours = 4 slots.
        public int Duration { get; set; }    
    }

    public class BayDto
    {
        public int BayId { get; set; }
        public string BayName { get; set; } = string.Empty;
        public string BayType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class JobControllerViewDto
    {
        public string Date { get; set; } = string.Empty;
        public List<BayDto> Bays { get; set; } = new();
        public List<JobControllerDto> Jobs { get; set; } = new();
    }
}