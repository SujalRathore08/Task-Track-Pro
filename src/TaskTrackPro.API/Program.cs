using Npgsql;
using StackExchange.Redis;
using TaskTrackPro.API.Controllers;
using TaskTrackPro.Core.Repositories.Commands.Implementations;
using TaskTrackPro.Core.Repositories.Commands.Interfaces;
using TaskTrackPro.Core.Repositories.Queries.Implementations;
using TaskTrackPro.Core.Repositories.Queries.Interfaces;
using TaskTrackPro.Core.Services.Messaging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using TaskTrackPro.API.Services;
using Elastic.Transport;
using Elastic.Clients.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure CORS Policy (Ensures cross-origin access for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("corsapp", policy =>
    {
        policy.WithOrigins("http://localhost:5122", "http://localhost:5285") // Frontend URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ✅ Configure PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("pgconnection");
builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connectionString));
builder.Services.AddScoped<ITaskInterface, TaskRepository>();
builder.Services.AddScoped<IAdminQuery, AdminQuery>();
builder.Services.AddScoped<IAdminCommand, AdminCommand>();
builder.Services.AddScoped<IAccountCommand, AccountCommand>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddSingleton<ElasticsearchServices>();

// ✅ Configure Redis Connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration["Redis:ConnectionString"] ?? "127.0.0.1:6379");
    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddSingleton<IDatabase>(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});

// ✅ Register Custom Redis Services
builder.Services.AddSingleton<RedisService>();
builder.Services.AddSingleton<CacheStringsStack>();

// ✅ Configure Redis Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "Session_"; // Prefix for session keys
});

// ✅ Configure Session Storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Register API Services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache(); // For in-memory session storage

builder.Services.AddSingleton(provider =>
{
    var configuration = builder.Configuration;
    var settings = new ElasticsearchClientSettings(new
    Uri(configuration["Elasticsearch:Uri"]))
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
    .DefaultIndex(configuration["Elasticsearch:DefaultIndex"])
    .Authentication(new
    BasicAuthentication(configuration["Elasticsearch:Username"],
    configuration["Elasticsearch:Password"]))
    .DisableDirectStreaming();
    return new ElasticsearchClient(settings);
});


var app = builder.Build();

async Task IndexDataOnStartup(){
    using var scope = app.Services.CreateScope();
    var contactRepo = scope.ServiceProvider.GetRequiredService<ITaskInterface>();
    var elasticService = scope.ServiceProvider.GetRequiredService<ElasticsearchServices>();    

    try{
        await elasticService.CreateIndexAsync();
        var tasks = await contactRepo.GetAllTask();
        if(tasks.Count > 0){
            foreach(var task in tasks){
                await elasticService.IndexTaskAsync(task);
            }
            Console.WriteLine($" {tasks.Count} contacts indexed successfully in ElasticSearch.");
        }else{
            Console.WriteLine("No tasks found to index in ElasticSearch.");
        }
    }
    catch(Exception ex){
        Console.WriteLine($"Error while indexing data in ElasticSearch: {ex.Message}");
    }
}

await IndexDataOnStartup();


// ✅ Use CORS Middleware (Must be before UseAuthorization)
app.UseCors("corsapp");

// ✅ Configure Swagger for API documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession(); // Enables session middleware
app.UseAuthorization(); // Handles authentication & authorization

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();