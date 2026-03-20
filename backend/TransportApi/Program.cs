using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

using DotNetEnv;

using TransportApi.Data;
using TransportApi.Services;
using TransportApi.Utils;

Env.Load();
var apikey = Environment.GetEnvironmentVariable("API_KEY") ?? throw new InvalidOperationException("API_KEY not found in .env");
var mysqlHost = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? throw new InvalidOperationException("MYSQL_HOST not found");
var mysqlPort = Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306";
var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? throw new InvalidOperationException("MYSQL_DATABASE not found");
var mysqlUser = Environment.GetEnvironmentVariable("MYSQL_USER") ?? throw new InvalidOperationException("MYSQL_USER not found");
var mysqlPassword = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? throw new InvalidOperationException("MYSQL_PASSWORD not found");
var mysqlConnString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDatabase};Uid={mysqlUser};Pwd={mysqlPassword};";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("TransportNSW", client => 
    client.DefaultRequestHeaders.Add("Authorization", $"apikey {apikey}"));
builder.Services.AddDbContext<TransportDbContext>(options =>
{
    options.UseMySql(mysqlConnString, ServerVersion.AutoDetect(mysqlConnString));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", 
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.AddScoped<IAgencyService, AgencyService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IRealtimeService, RealtimeService>();
builder.Services.AddScoped<IShapeService, ShapeService>();
builder.Services.AddScoped<IStopService, StopService>();
builder.Services.AddScoped<IStopTimeService, StopTimeService>();
builder.Services.AddScoped<ITripService, TripService>();

builder.Services.AddScoped<BackgroundService, RealtimePollingService>();
// builder.Services.AddHostedService<RealtimePollingService>(); TO REVISIT

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();

app.MapControllers();
app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext http) =>
{
    var feature = http.Features.Get<IExceptionHandlerFeature>();
    var message = feature?.Error?.Message ?? "An unexpected error occurred.";
    return Results.Problem(detail: message);
});

// app.Logger.LogInformation("Metro");
// await app.PopulateSydney("v2", "metro");

// app.Logger.LogInformation("SydneyTrains");
// await app.PopulateSydney("v1", "sydneytrains");

app.Logger.LogInformation("Start Server");
app.Run();
