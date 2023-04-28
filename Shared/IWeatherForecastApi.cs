using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StronglyTypedApi.Shared
{
    public interface IWeatherForecastApi
    {

        Task<WeatherForecast[]?> Fetch(WeatherForecastRequest request, CancellationToken cancellationToken);

    }
}
