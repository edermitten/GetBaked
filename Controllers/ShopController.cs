using Microsoft.AspNetCore.Mvc;

namespace GetBaked.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ByCategory(String name)
        {
            //store category name
            ViewData["Category"] = name;
            return View();
        }

    }
}
