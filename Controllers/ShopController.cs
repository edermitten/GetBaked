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
            var categories = _context.Categories.OrderBy(c => c.Name).ToList();

            //pass categories to the view
            return View(categories);
        }


        public IActionResult ByCategory(int id)
        {
            //store category name
            var category = _context.Categories.Find(id);

            //return to shop if category not found
            if (category == null)
            {
                return RedirectToAction("Index");
            }

            ViewData["Category"] = category.Name;

            //list of products 
            var products = _context.Products.Where(p => p.CategoryId == id)
                .OrderBy(p => p.Name)
                .ToList();

            return View(products);

        }
        [HttpPost]
        public IActionResult AddToCart(int ProductId, int Quantity)
        {
            //get price of product
            var product = _context.Products.Find(ProductId);

            //does this cart already have this product
            var cartItem = _context.CartItems.SingleOrDefault(c => c.ProductId == ProductId &&
                c.CustomerId == GetCustomerId());

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = product.Price,
                    CustomerId = GetCustomerId()
                };

                _context.Add(cartItem);

            }
            else
            {
                cartItem.Quantity += Quantity;
                _context.Update(cartItem);
            }
            
            _context.SaveChanges();

            return RedirectToAction("Cart");
        }

        //identify customer to ensure unique carts
        private string GetCustomerId()
        {
            //check if we already have a session var for this user
            if (HttpContext.Session.GetString("CustomerId") == null)
            {
                //create new session var using GUID
                HttpContext.Session.SetString("CustomerId", Guid.NewGuid().ToString());
            }

            //pass back the session
            return HttpContext.Session.GetString("CustomerId");
        }

    }
}

            
