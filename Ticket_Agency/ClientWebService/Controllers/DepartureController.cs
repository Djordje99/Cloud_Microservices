using Common.DTO;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Common.Enums;
using Common.DTO.Weather;

namespace ClientWebService.Controllers
{
    public class DepartureController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateDeparture()
        {
            ViewBag.enumList = new List<TransportType>() { TransportType.BUS, TransportType.PLANE, TransportType.TRAIN };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeaprture(Departure departure)
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    await validator.CreateDeparture(departure);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //wait for pab sub to get status of registration

            return Redirect("/");
        }

        [HttpPost]
        public async Task<IActionResult> FilterDeparture(string transportType, DateTime fromDate, int availableTickets)
        {
            ViewBag.logged = HttpContext.Session.GetString("Logged");
            List<Departure> departureList = new List<Departure>();
            Dictionary<long, WeatherData> weatherDatas = new Dictionary<long, WeatherData>();

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    departureList = await validator.ListDepartureFilter(transportType, fromDate, availableTickets);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            foreach (var departure in departureList)
            {
                var weatherData = await GetWeatherData(departure.CityName);
                weatherDatas.Add(departure.ID, weatherData);
            }

            ViewBag.WeatherData = weatherDatas;

            return View("ListDeparture", departureList);
        }

        public async Task<IActionResult> ListDeparture()
        {
            ViewBag.logged = HttpContext.Session.GetString("Logged");


            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");
            List<Departure> departureList = new List<Departure>();
            Dictionary<long, WeatherData> weatherDatas = new Dictionary<long, WeatherData>();

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    departureList = await validator.ListDeparture();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            foreach (var departure in departureList)
            {
                var weatherData = await GetWeatherData(departure.CityName);
                weatherDatas.Add(departure.ID, weatherData);
            }

            ViewBag.WeatherData = weatherDatas;

            return View(departureList);
        }


        public async Task<IActionResult> ListHistoryDeparture()
        {
            ViewBag.logged = HttpContext.Session.GetString("Logged");

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");
            List<Departure> departureList = new List<Departure>();
            Dictionary<long, WeatherData> weatherDatas = new Dictionary<long, WeatherData>();

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    departureList = await validator.ListHistoryDeparture();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            foreach (var departure in departureList)
            {
                var weatherData = await GetWeatherData(departure.CityName);
                weatherDatas.Add(departure.ID, weatherData);
            }

            ViewBag.WeatherData = weatherDatas;

            return View(departureList);
        }


        private async Task<WeatherData> GetWeatherData(string cityName)
        {
            WeatherData weatherData = new WeatherData();

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:20045/WeatherService");

            using (var channelFactory = new ChannelFactory<IWeatherService>(binding, endpointAddress))
            {
                IWeatherService weather = null;
                try
                {
                    weather = channelFactory.CreateChannel();
                    weatherData = await weather.GetWeatherData(cityName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return weatherData;
        }

        [HttpPost]
        public async Task<IActionResult> BuyDepertureTicket(long id, int amount)
        {
            var logged = HttpContext.Session.GetString("Logged");
            ViewBag.logged = logged;

            if (logged == null || logged == "")
                return RedirectToAction("Index", "Home");

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");
            List<Departure> departureList = new List<Departure>();

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    await validator.BuyDepertureTicket(logged, id, amount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //get from pub sub

            return RedirectToAction("UserPurchaseList", "User");
        }
    }
}
