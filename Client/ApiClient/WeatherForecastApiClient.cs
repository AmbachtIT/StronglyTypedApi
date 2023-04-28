using StronglyTypedApi.Shared;
using System.Net.Http;
using System.Net.Http.Json;

namespace StronglyTypedApi.Client.ApiClient
{
    public class WeatherForecastApiClient : IWeatherForecastApi
    {
        public WeatherForecastApiClient(HttpClient client)
        {
            _client = client;
        }

        private readonly HttpClient _client;

        public async Task<WeatherForecast[]?> Fetch(WeatherForecastRequest request, CancellationToken cancellationToken)
        {
            var uri = CreateUri(nameof(Fetch));
            var response = await _client.PostAsJsonAsync(uri, request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WeatherForecast[]>(cancellationToken: cancellationToken);
        }

        private Uri CreateUri(string methodName) => new ($"WeatherForecast/{methodName}", UriKind.Relative);
    }
}
