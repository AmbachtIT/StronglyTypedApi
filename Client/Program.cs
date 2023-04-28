using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StronglyTypedApi.Client;
using StronglyTypedApi.Client.ApiClient;
using StronglyTypedApi.Shared;

namespace StronglyTypedApi.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddTransient<IWeatherForecastApi, WeatherForecastApiClient>();

            await builder.Build().RunAsync();
        }



    }
}