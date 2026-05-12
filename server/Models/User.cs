namespace server.Models;

public class User
{
    public int users_id { get; set; } 
    public string? full_name { get; set; }
    public int position_id { get; set; }
    public string? assigned_bay_id { get; set; }
}