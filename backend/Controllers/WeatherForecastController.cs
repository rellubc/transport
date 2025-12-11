using backend.Models;

namespace backend.Controllers
{
    public static class WeatherForecastController
    {
        public static WeatherForecastModels.WeatherForecast[] GetWeatherForecast()
        {
            var forecast =  Enumerable.Range(1, 5).Select(index =>
                new WeatherForecastModels.WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    WeatherForecastModels.summaries[Random.Shared.Next(WeatherForecastModels.summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    }
}
