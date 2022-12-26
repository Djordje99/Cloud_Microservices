using Common.DTO;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ClientWebService.Controllers
{
    public class BankController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateBankAccount()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBankAccount(BankAccount account)
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    await validator.CreateBankAccount(account);
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
