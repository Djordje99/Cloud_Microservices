using Common.DTO.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IWeatherService
    {
        [OperationContract]
        Task<WeatherData> GetWeatherData(string cityName);
    }
}
