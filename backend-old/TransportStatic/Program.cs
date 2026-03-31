using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

using TransportStatic.Data;
using TransportStatic.Services;

Env.Load("../.env");

var apiKey = Environment.GetEnvironmentVariable("API_KEY") ?? throw new InvalidOperationException("API_KEY not found in .env");
var psqlHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? throw new InvalidOperationException("POSTGRES_HOST not found");
var psqlPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
var psqlDatabase = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? throw new InvalidOperationException("POSTGRES_DB not found");
var psqlUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? throw new InvalidOperationException("POSTGRES_USER not found");
var psqlPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? throw new InvalidOperationException("POSTGRES_PASSWORD not found");
var psqlConnString = $"Host={psqlHost};Port={psqlPort};Database={psqlDatabase};Username={psqlUser};Password={psqlPassword};";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("TransportNSW", client => 
    client.DefaultRequestHeaders.Add("Authorization", $"apikey {apiKey}"));
builder.Services.AddDbContext<TransportDbContext>(options =>
{
    options.UseNpgsql(psqlConnString, o => o.UseNetTopologySuite());
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

app.Logger.LogInformation("Start Server");
app.Run();

