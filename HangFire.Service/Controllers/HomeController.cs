using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using HangfireExtensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HangFire.Service.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }
        // GET: /<controller>/
       public IActionResult Index()
       {
            RecurringJob.AddOrUpdate<ISendMessageJob>(x => x.SendMessageService(), "0 0/5 * * * ?");
            return View();
        }
    }
}
