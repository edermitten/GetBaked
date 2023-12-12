using GetBaked.Data;
using GetBaked.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        //Post
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

        //GET: /Shop/Cart
        public IActionResult Cart()
        {
            //identify wuch cart to fetch & display
            var customerId = GetCustomerId();

            //query the db for the cart items; include or Join parent Product
            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CustomerId == customerId).ToList();

            //count total of items in cart
            var itemCount = (from c in cartItems 
                             select c.Quantity).Sum();

            HttpContext.Session.SetInt32("ItemCount", itemCount);

            return View(cartItems);  
        }

        //GET /Shop/removefromcart
        public IActionResult RemoveFromCart(int id)
        {
            var cartItem = _context.CartItems.Find(id);

            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();

            return RedirectToAction("Cart");

        }

    }
}

            
