using GetBaked.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GetBaked.Controllers
{
    public class HomeController : Controller
    {
        
        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View("Privacy");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}