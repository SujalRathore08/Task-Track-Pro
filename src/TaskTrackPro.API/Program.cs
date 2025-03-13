using Npgsql;
using TaskTrackPro.Core.Repositories.Commands.Implementations;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin() // Allows requests from any frontend (Avoid in production)
                  .AllowAnyMethod() // Allows GET, POST, PUT, DELETE, etc.
                  .AllowAnyHeader(); // Allows all headers
        });
});

// ✅ Configure PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("pgconnection");
builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connectionString));
builder.Services.AddScoped<ITaskInterface, TaskRepository>();

// ✅ Add services for Controllers & API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Use CORS Middleware (Must be before `UseAuthorization`)
app.UseCors("AllowAllOrigins");

// ✅ Configure Swagger for API documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Middleware Pipeline Configuration
app.UseHttpsRedirection();
app.UseAuthorization();  // Handles authentication & authorization

app.MapControllers(); // ✅ This replaces `UseRouting()` and `UseEndpoints()`

app.Run();
