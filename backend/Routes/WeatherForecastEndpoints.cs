using backend.Controllers;

namespace backend.Routes
{
    public static class WeatherForecastEndpoints
    {
        public static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/weatherforecast", () =>
            {
                return WeatherForecastController.GetWeatherForecast();
            })
            .WithName("GetWeatherForecast");
        }
    }
}
