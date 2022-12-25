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
    }
}
