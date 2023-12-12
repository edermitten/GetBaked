using GetBaked.Data;
using GetBaked.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace GetBaked.Controllers
{
    public class ShopController : Controller
    {
        //ADD DB CONECCION
        private readonly ApplicationDbContext _context;

        //ADD STRIPE READER SETUP
        private readonly IConfiguration _configuration;

        public ShopController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

        // GET: /Shop/Checkout => show empty checkout page to capture customer info
        [Authorize]
        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Checkout([Bind("FirstName,LastName,Address,City,Province,PostalCode,Phone")] Order order)
        {
            // 7 fields bound from form inputs in method header
            // now auto-fill 3 of the fields we removed from the form
            order.OrderDate = DateTime.Now;
            order.CustomerId = User.Identity.Name;

            var cartItems = _context.CartItems.Where(c => c.CustomerId == GetCustomerId());
            order.OrderTotal = (from c in cartItems
                                select c.Quantity * c.Price).Sum();

            /*
            order.OrderTotal = (from c in _context.CartItems
                                where c.CustomerId == HttpContext.Session.GetString("CustomerId")
                                select c.Quantity * c.Price).Sum();
            */
            // store the order as session var so we can proceed to payment attempt
            HttpContext.Session.SetObject("Order", order);

            // redirect to payment
            return RedirectToAction("Payment");
            
        }
        public IActionResult Payment() 
        {
            // retrieve order session var (convert json string back to our Order class object
            var order = HttpContext.Session.GetObject<Order>("Order");

            // set up Stripe payment attempt
            // 1 - get Stripe API key 
            StripeConfiguration.ApiKey = _configuration.GetValue<string>("StripeSecretKey");

            // 2 - set up payment screen from Stripe API
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long?)(order.OrderTotal * 100),
                            Currency = "cad",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "GetBaked Online Purchase"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "https://" + Request.Host + "/Shop/SaveOrder",
                CancelUrl = "https://" + Request.Host + "/Shop/Cart"
            };

            // 3 - invoke Stripe
            var service = new SessionService();
            Session session = service.Create(options);

            // 4 - redirect based on Stripe response
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        // GET: /Shop/SaveOrder => save order to db after payment
        public IActionResult SaveOrder()
        {
            // get the order from session var
            var order = HttpContext.Session.GetObject<Order>("Order");

            // create new order in db
            _context.Orders.Add(order);
            _context.SaveChanges();

            // copy each cartItem to a new OrderDetail as a child of the new order
            // identify which cart to fetch & display
            var customerId = GetCustomerId();

            // query the db for the cart items; include or JOIN to parent Product to get Product details
            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CustomerId == customerId);

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    Quantity = item.Quantity,
                    Price = item.Price,
                    ProductId = item.ProductId,
                    OrderId = order.OrderId
                };

                _context.OrderDetails.Add(orderDetail);
            }
            _context.SaveChanges();

            // empty cart
            foreach (var item in cartItems)
            {
                _context.CartItems.Remove(item);
            }
            _context.SaveChanges();

            HttpContext.Session.Clear();


            // redirect to Orders/Details/5
            return RedirectToAction("Details", "Orders", new { @id = order.OrderId });
        }
    }
}

            
