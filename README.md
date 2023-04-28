# Strongly Typed Api
This project is a proof of concept demonstrating how to use a shared interface to specify a server API
using ASP.net Core Web API and Blazor WebAssembly. The approach has a few advantages:

 * First of all, interfaces are the natural way to specify a contract in C#. If both the client and 
   the server speak C#, why not use all its features to express ideas?
 * Using interfaces to specify a web API surface allows developers to use Intellisense, greatly 
   improving discoverability.
 * You can use Visual Studio and/or Resharper to refactor an API without breaking the client.
 * Injecting `IMyWebApi` instead of `HttpClient` makes testing much easier.

With Blazor, we can share code between the client and the server. Using an interface to describe the 
API surface seems like a natural fit. In this repository, I have modified the ASP.net hosted Blazor 
WebAssembly template to do just that.

## The approach
We'll create an interface that is used both by the server and the client. On the server side, the controller implements
the interface. Client side, the interface is injected and called by the client side code. The client implementation 
creates a HTTP request and handles the response.

## Shared
The interface describing the Api lives in the shared assembly that is reference both by the server and the
client.

    public interface IWeatherForecastApi
    {

        Task<WeatherForecast[]?> Fetch(WeatherForecastRequest request, CancellationToken cancellationToken);

    }

I have slightly modified the API yielding the weather forecast. Compared to the default template, I have made these changes:

 * Since non-trivial API calls require input, I have added a request object. For this API, the request determines how 
   many rows to yield.
 * The Strongly Typed API approach marries the method implementing the server side call to the client method, which means
   that the method signatures need to be identical. Specifically:
    * Instead of `IEnumerable<WeatherForecast>`, the call yields a `Task<WeatherForecast[]>`. The client call needs 
      to be async and most server calls are too so I don't think this is a big problem.
    * The API accepts a cancellation token. So if you want to implement cancellation token support on the server side, you'll
      have to do it at the client side as well. 
 * In the original code, the method was named `Get`, which confused me because the method also used the `HTTP GET`
   verb. I am admittedly easily confused so I have renamed the method `Fetch` to prevent future me (and possibly
   you) from being confused.

## Client
The bulk of the work happens in the client. We'll need to implement the interface which is also implemented by
the controller:

    public class WeatherForecastApiClient : IWeatherForecastApi
    {
        ...
    }

The implementation will need to handle some details:

* HTTP Routing: The client needs to decide which endpoint url and HTTP method to use. In this example we 
  simply append the method name to a fixed endpoint and assume HTTP POST. More sophisticated approaches are also
  possible. For instance: We could decorate the interface with attributes and use these attributes to configure
  routing both on the server and the client.
* Serialization: Serializing rich data models is not trivial. Fortunately, Blazor allows you to reuse serialization
  code both on the frontend and the backend. In this example, we use the default serialization semantics.
* Error handling: Not every flow is a happy flow so you will need to handle this as well. In the example,
  we use `HttpResponseMessage.EnsureSuccessStatusCode()` which will throw a HttpRequestException with the offending
  status code if something went awry.

It sounds like a lot but the API client is only a few lines of code.

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

Of course, a few lines of code per API call add up if you have dozens of API calls. This is a perfect use case 
for [C# Source Generation](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/) and I'll
probably write a generator later.

Now we need to register the service in `Program.cs`

    builder.Services.AddTransient<IWeatherForecastApi, WeatherForecastApiClient>();

...and finally, we can use it in `FetchData.razor`:

    @page "/fetchdata"
    @using StronglyTypedApi.Shared
    @inject IWeatherForecastApi Api

    <PageTitle>Weather forecast</PageTitle>

    <h1>Weather forecast</h1>

    <p>This component demonstrates fetching data from the server.</p>

    @if (forecasts == null)
    {
        <p><em>Loading...</em></p>
    }
    else
    {
        ...
    }

    @code {
        private WeatherForecast[]? forecasts;

        protected override async Task OnInitializedAsync()
        {
            forecasts = await Api.Fetch(new WeatherForecastRequest()
            {
                Days = 5
            }, CancellationToken.None);
        }
    }

Admit it, isn't that much nice than raw-dogging with a HttpClient instance?

## Server
In the server, the `WeatherForecastController` now implements `IWeatherForecastApi`.


    [ApiController]
    [Route("[controller]/[action]")] // The template omits the /[action] which is kind of relevant if you want to add additional controller actions.
    [TypeFilter(typeof(YieldHttpStatusCodeExceptionFilter))]
    public class WeatherForecastController : ControllerBase, IWeatherForecastApi
    {
      
        ...

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

One interesting detail is using the YieldHttpStatusCodeException to propagate a HTTP result. There is no way to use
the WeatherForecast[] class to relay this information. One way to handle this in Web APIs would be to yield an 
IActionResult instead. But that's not very descriptive to the client, we might as well yield System.String instead.

That's why I use an exception, which is intercepted by an Exception filter. One might argue this is using exceptions
for flow control, which is an anti-pattern. I'd say that an exception is suitable since this is an exceptional event,
but I do hear you. I think the clarity of yielding a meaningful type outweighs the drawbacks of using exceptions in
this case, but your mileage may vary.