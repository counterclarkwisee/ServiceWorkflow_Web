using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Interfaces;
using server.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services & Configure JSON Naming (PascalCase preservation)
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        // This ensures the JSON keys match your C# DTO property names exactly (e.g., AppointmentId)
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddOpenApi();

// 2. Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 3. SOLID: Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IChecklisterRepository, ChecklisterRepository>();
builder.Services.AddScoped<ISaRepository, SaRepository>();
builder.Services.AddScoped<IReceptionRepository, ReceptionRepository>();
builder.Services.AddScoped<IJobControllerRepository, JobControllerRepository>();

// 4. CORS
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => 
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Ensure CORS is called before MapControllers
app.UseCors();

app.UseHttpsRedirection();

app.MapControllers(); 

app.Run();