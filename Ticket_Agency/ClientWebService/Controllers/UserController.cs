using ClientWebService.Models;
using Common.DTO;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ClientWebService.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser user)
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    await validator.ValidateUserRegister(user);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return Redirect("/");
        }
    }
}
