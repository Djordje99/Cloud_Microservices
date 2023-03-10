using ClientWebService.Models;
using Common.DTO;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
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

        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(RegisterUser user)
        {
            bool isLogged = false;

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    isLogged = await validator.ValidateUserLogIn(user);
                    //TODO: get value from pub sub and set session
                    if(isLogged)
                        HttpContext.Session.SetString("Logged", user.Username);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //TODO: wait for pab sub to get status of registration
            return RedirectToAction("ListDeparture", "Departure");
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

            //TODO: wait for pab sub to get status of registration
            return RedirectToAction("Register");
        }

        public IActionResult SignOut()
        {
            HttpContext.Session.SetString("Logged", "");
            var logged = HttpContext.Session.GetString("Logged");
            ViewBag.logged = logged;

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> UserPurchaseList()
        {
            var logged = HttpContext.Session.GetString("Logged");
            ViewBag.logged = logged;

            if (logged == null || logged == "")
                return RedirectToAction("Index", "Home");

            List<DetailPurchase> detailPurchases = new List<DetailPurchase>();

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    detailPurchases = await validator.PurchaseList(logged);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return View(detailPurchases);
        }

        [HttpPost]
        public async Task<IActionResult> CancelTicket(CancelPurchase cancelPurchase)
        {
            var logged = HttpContext.Session.GetString("Logged");
            ViewBag.logged = logged;

            if (logged == null || logged == "")
                return RedirectToAction("Index", "Home");

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpointAddress = new EndpointAddress("net.tcp://localhost:19999/WebCommunication");

            using (var channelFactory = new ChannelFactory<IValidatorService>(binding, endpointAddress))
            {
                IValidatorService validator = null;
                try
                {
                    validator = channelFactory.CreateChannel();
                    await validator.CancelPurchase(cancelPurchase);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return RedirectToAction("UserPurchaseList", "User");
        }
    }
}
