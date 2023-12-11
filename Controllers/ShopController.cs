using GetBaked.Data;
using GetBaked.Models;
using Microsoft.AspNetCore.Mvc;

namespace GetBaked.Controllers
{
    public class ShopController : Controller
    {
        //ADD DB CONECCION
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            //use db conection to fect categories
            var categories = _context.Categories.OrderBy(c=>c.Name).ToList();

            //pass categories to the view
            return View(categories);
        }

        
        public IActionResult ByCategory(int id)
        {
            //store category name
            var category = _context.Categories.Find(id);

            //return to shop if category not found
            if(category == null)
            {
                return RedirectToAction("Index");
            }

            ViewData["Category"] = category.Name;

            //list of products 
            var products = _context.Products.Where(p=>p.CategoryId == id)
                .OrderBy(p=>p.Name)
                .ToList();

            //loop for products
            /*for (int i = 1;i <=5; i++)
            {
                products.Add(new Product { ProductId = i, Name = "Product" + i.ToString(), Price = 8 });
            }
            */

            //send the products
            //return View();
            return View(products);

        }

    }
}
