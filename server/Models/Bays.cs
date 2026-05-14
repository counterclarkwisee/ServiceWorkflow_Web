using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace server.Models
{
    [Table("bays")] // This MUST match the table name in image_66659b.png
    public class Bays
    {
        [Key]
        public int bay_id { get; set; } 
        public string bay_name { get; set; }
        public string bay_type { get; set; }
        public int is_Active { get; set; }
    }
}