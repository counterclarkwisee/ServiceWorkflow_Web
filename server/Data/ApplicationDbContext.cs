// server/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ChecklisterLog> ChecklisterLogs { get; set; }
        public DbSet<SaLog> SaLogs { get; set; }
        public DbSet<Service> Services { get; set; } 
        public DbSet<JobconLog> JobconLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<User>().ToTable("users").HasKey(u => u.users_id);
            modelBuilder.Entity<Appointment>().ToTable("appointments").HasKey(a => a.appointment_id);

            // FIX: Match the property name to 'service_id' instead of 'id'
            modelBuilder.Entity<Service>().ToTable("services").HasKey(s => s.service_id);
            modelBuilder.Entity<ChecklisterLog>().ToTable("checklister_logs").HasKey(c => c.checklister_logs_id);
        }
    }
}