using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shop.Context;
using Shop.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shop.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == int.Parse(userId))
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            var cartItem = new CartItem
            {
                UserId = int.Parse(userId),
                ProductId = productId,
                Quantity = 1,
                Price = product.PriceProduct
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        public IActionResult BuyItem(int id)
        {
            var item = _context.CartItems.Include(ci => ci.Product).FirstOrDefault(ci => ci.IdCartItem == id);
            if (item == null)
            {
                return NotFound();
            }

            int currentTotalQuantity = int.TryParse(Request.Cookies["TotalQuantity"], out currentTotalQuantity) ? currentTotalQuantity : 0;
            decimal currentTotalAmount = decimal.TryParse(Request.Cookies["TotalAmount"], out currentTotalAmount) ? currentTotalAmount : 0m;

            int totalQuantity = currentTotalQuantity + item.Quantity;
            decimal totalAmount = currentTotalAmount + (item.Quantity * item.Price);

            Response.Cookies.Append("TotalQuantity", totalQuantity.ToString(), new CookieOptions { Expires = DateTime.Now.AddDays(30) });
            Response.Cookies.Append("TotalAmount", totalAmount.ToString(), new CookieOptions { Expires = DateTime.Now.AddDays(30) });

            var productSales = Request.Cookies["ProductSales"];
            var productSalesDict = string.IsNullOrEmpty(productSales) ? new Dictionary<string, int>() : JsonConvert.DeserializeObject<Dictionary<string, int>>(productSales);

            if (productSalesDict.ContainsKey(item.Product.NameProduct))
            {
                productSalesDict[item.Product.NameProduct] += item.Quantity;
            }
            else
            {
                productSalesDict[item.Product.NameProduct] = item.Quantity;
            }

            Response.Cookies.Append("ProductSales", JsonConvert.SerializeObject(productSalesDict), new CookieOptions { Expires = DateTime.Now.AddDays(30) });

            _context.CartItems.Remove(item);
            _context.SaveChanges();

            return RedirectToAction("Index", "Cart");
        }


        [HttpPost]
        public IActionResult BuyAll()
        {
            var cartItems = _context.CartItems.Include(ci => ci.Product).ToList();
            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            int totalQuantity = cartItems.Sum(item => item.Quantity);
            decimal totalAmount = cartItems.Sum(item => item.Price * item.Quantity);

            int currentTotalQuantity = int.TryParse(Request.Cookies["TotalQuantity"], out currentTotalQuantity) ? currentTotalQuantity : 0;
            decimal currentTotalAmount = decimal.TryParse(Request.Cookies["TotalAmount"], out currentTotalAmount) ? currentTotalAmount : 0m;

            totalQuantity += currentTotalQuantity;
            totalAmount += currentTotalAmount;

            Response.Cookies.Append("TotalQuantity", totalQuantity.ToString(), new CookieOptions { Expires = DateTime.Now.AddDays(30) });
            Response.Cookies.Append("TotalAmount", totalAmount.ToString(), new CookieOptions { Expires = DateTime.Now.AddDays(30) });

            var productSales = Request.Cookies["ProductSales"];
            var productSalesDict = string.IsNullOrEmpty(productSales) ? new Dictionary<string, int>() : JsonConvert.DeserializeObject<Dictionary<string, int>>(productSales);

            foreach (var item in cartItems)
            {
                if (productSalesDict.ContainsKey(item.Product.NameProduct))
                {
                    productSalesDict[item.Product.NameProduct] += item.Quantity;
                }
                else
                {
                    productSalesDict[item.Product.NameProduct] = item.Quantity;
                }
            }

            Response.Cookies.Append("ProductSales", JsonConvert.SerializeObject(productSalesDict), new CookieOptions { Expires = DateTime.Now.AddDays(30) });

            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

    }
}
