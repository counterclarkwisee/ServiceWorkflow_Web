using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Interfaces;
using server.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers(); // Required for the new Controller to work
builder.Services.AddOpenApi();

// 2. Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 3. SOLID: Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// 4. CORS
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => 
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();
app.UseCors();

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();

app.MapControllers(); // This tells the app to look into the Controllers folder

app.Run();