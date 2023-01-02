using Common.DTO.Weather;
using Common.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WeatherStatelessService.Services
{
    public class WeatherService : IWeatherService
    {
        private string _apiKey;
        private string _apiUrl;
        private HttpClient _client;
        public WeatherService()
        {
             this._client = new HttpClient();
            this._apiKey = "c913c803826c4d18abe173026230201";
            this._apiUrl = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}&aqi=no";
        }

        public async Task<WeatherData> GetWeatherData(string cityName)
        {
            WeatherData weatherData = new WeatherData();

            string formatedUrl = string.Format(this._apiUrl, this._apiKey, cityName);

            HttpResponseMessage response = _client.GetAsync(formatedUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;

                JObject jsonObject = JObject.Parse(json);

                weatherData.Temperature = (double)jsonObject["current"]["temp_c"];
                weatherData.IconLocation = (string)jsonObject["current"]["condition"]["icon"];
            }

            return weatherData;
        }
    }
}
