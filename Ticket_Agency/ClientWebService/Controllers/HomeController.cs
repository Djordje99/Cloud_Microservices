using ClientWebService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ClientWebService.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var logged = HttpContext.Session.GetString("Logged");
            ViewBag.logged = logged;

            return View();
        }
    }
}
