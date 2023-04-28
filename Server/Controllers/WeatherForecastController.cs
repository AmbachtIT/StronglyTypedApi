using System.Net;
using Microsoft.AspNetCore.Mvc;
using StronglyTypedApi.Server.ErrorHandling;
using StronglyTypedApi.Shared;

namespace StronglyTypedApi.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [TypeFilter(typeof(YieldHttpStatusCodeExceptionFilter))]
    public class WeatherForecastController : ControllerBase, IWeatherForecastApi
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpPost()]
        public async Task<WeatherForecast[]?> Fetch(WeatherForecastRequest request, CancellationToken cancellationToken)
        {
            if (request.Days is < 0 or > 100)
            {
                throw new YieldHttpStatusCodeException(HttpStatusCode.BadRequest, "Invalid request count");
            }
            await Task.Delay(100 * request.Days, cancellationToken);
            return Enumerable.Range(1, request.Days).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

    }
}