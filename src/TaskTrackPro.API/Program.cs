    using Npgsql;
    using StackExchange.Redis;
    using TaskTrackPro.API.Controllers;
    using TaskTrackPro.Core.Repositories.Commands.Implementations;
    using TaskTrackPro.Core.Repositories.Commands.Interfaces;
    using TaskTrackPro.Core.Repositories.Queries.Implementations;
    using TaskTrackPro.Core.Repositories.Queries.Interfaces;
    using TaskTrackPro.Core.Services.Messaging;



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


    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });


    // ✅ Configure PostgreSQL connection
    var connectionString = builder.Configuration.GetConnectionString("pgconnection");
    builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connectionString));
    builder.Services.AddScoped<ITaskInterface, TaskRepository>();
    builder.Services.AddScoped<IAdminQuery, AdminQuery>();
    builder.Services.AddScoped<IAdminCommand, AdminCommand>();
    builder.Services.AddScoped<IAccountCommand, AccountCommand>();
    builder.Services.AddScoped<ChatService>();

    // ✅ Add services for Controllers & API
    builder.Services.AddControllers();
    builder.Services.AddSignalR();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ✅ Register Distributed Memory Cache for session storage
    builder.Services.AddDistributedMemoryCache();

    // ✅ Configure Redis Connection
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = ConfigurationOptions.Parse("127.0.0.1:6379"); // Local Redis
        return ConnectionMultiplexer.Connect(configuration);
    });

    // Configuring cors 
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("corsapp", policy =>
        {
            policy.WithOrigins("http://localhost:5122", "http://localhost:5285") // Add frontend URL(s)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Configuring connection string
    builder.Services.AddSingleton<NpgsqlConnection>((ServiceProvider) =>
    {
        var connection = ServiceProvider.GetRequiredService<IConfiguration>().GetConnectionString("pgconnection");
        return new NpgsqlConnection(connection);
    });

    var app = builder.Build();

    // ✅ Use CORS Middleware (Must be before `UseAuthorization`)
    app.UseCors("corsapp");

    // ✅ Configure Swagger for API documentation
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }


    // app.UseCors("corsapp");

    app.UseHttpsRedirection();
    app.UseAuthorization();  // Handles authentication & authorization

    app.MapControllers(); // ✅ This replaces `UseRouting()` and `UseEndpoints()`
    app.MapHub<ChatHub>("/chathub");
    app.Run();
