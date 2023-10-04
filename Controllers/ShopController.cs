using GetBaked.Models;
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

            //list of products
            var products = new List<Product>();

            //loop for products
            for (int i = 1;i <=5; i++)
            {
                products.Add(new Product { ProductId = i, Name = "Product" + i.ToString(), Price = 8 });
            }

            //send the products
            //return View();
            return View(products);

        }

    }
}
