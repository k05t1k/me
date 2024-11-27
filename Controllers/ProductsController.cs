using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop.Context;

namespace Shop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Отладочное сообщение перед запросом к базе данных
            Console.WriteLine("Fetching products from the database...");

            // Получение названия базы данных
            using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                connection.Open();
                var databaseName = connection.Database;
                Console.WriteLine($"Connected to database: {databaseName}");
            }

            var products = _context.Products.ToList();

            // Отладочное сообщение после запроса к базе данных
            Console.WriteLine($"Found {products.Count} products in the database.");

            // Вывод информации о каждом продукте
            foreach (var product in products)
            {
                Console.WriteLine($"Product ID: {product.IdProduct}, Name: {product.NameProduct}, Price: {product.PriceProduct}");
            }

            return View(products);
        }
    }
}
