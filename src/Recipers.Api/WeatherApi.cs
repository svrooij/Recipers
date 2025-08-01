using Microsoft.OpenApi.Models;

namespace Recipers.Api;

public static class WeatherApi
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public static void MapWeatherApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                  new WeatherForecast
                  (
                      DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                      Random.Shared.Next(-20, 55),
                      Summaries[Random.Shared.Next(Summaries.Length)]
                  ))
                  .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .RequireAuthorization()
        .Produces<IEnumerable<WeatherForecast>>(StatusCodes.Status200OK)
        .WithTags("Weather")
      .WithOpenApi(operation =>
      {
          operation.Summary = "Get weather forecast";
          operation.Description = "Returns a 5-day weather forecast with random temperatures and summaries.";
          operation.Security.AddScopes("jwt", "api://c8ccec6e-9f74-4de1-a6cd-18e665c3e685/user-impersonation");
          return operation;
      });
    }

    public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
