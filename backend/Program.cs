
using DotNetEnv;
using MySqlConnector;
using Microsoft.EntityFrameworkCore;

using backend.Data;
using backend.Routes;
using backend.Utils;

DotNetEnv.Env.Load();

var apikey = Environment.GetEnvironmentVariable("API_KEY") ?? throw new InvalidOperationException("API_KEY not found in .env");
var mysqlHost = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? throw new InvalidOperationException("MYSQL_HOST not found");
var mysqlPort = Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306";
var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? throw new InvalidOperationException("MYSQL_DATABASE not found");
var mysqlUser = Environment.GetEnvironmentVariable("MYSQL_USER") ?? throw new InvalidOperationException("MYSQL_USER not found");
var mysqlPassword = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? throw new InvalidOperationException("MYSQL_PASSWORD not found");

var mysqlConnString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDatabase};Uid={mysqlUser};Pwd={mysqlPassword};";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddHttpClient("TransportNSW", client => client.DefaultRequestHeaders.Add("Authorization", $"apikey {apikey}"));
builder.Services.AddDbContext<TransportDbContext>(options => options.UseMySql(mysqlConnString, ServerVersion.AutoDetect(mysqlConnString)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await app.Populate();

WeatherForecastEndpoints.MapWeatherForecastEndpoints(app);
TransportNSWEndpoints.GetTransportNSWTest(app);
SydneyTrainsEndpoints.GetSydneyTrains(app);

app.Run();
