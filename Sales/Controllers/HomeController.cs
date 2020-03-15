using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sales.Models;
using Sales.Models.ViewModels;
namespace Sales.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private SalesContext db;
        private IMemoryCache cache;
        public HomeController(ILogger<HomeController> logger, SalesContext context, IMemoryCache memoryCache)
        {
            _logger = logger;
            db = context;
            cache = memoryCache;
        }

        [Authorize]
        
        public async Task<IActionResult> Index(int? id)
        {
            if (id != null)
            {
                if(cache.TryGetValue(User.Identity.Name,out List<Books> value))
                {
                    value.Add(await db.Books.FirstOrDefaultAsync(book => book.Id == id));
                    cache.Set(User.Identity.Name, value);
                }
                else
                {
                    var newOrder = new List<Books>();
                    newOrder.Add(await db.Books.FirstOrDefaultAsync(book => book.Id == id));
                    cache.Set(User.Identity.Name, newOrder);
                }
                
            }
            var b = cache.Get(User.Identity.Name);
            var basket  = cache.Get(User.Identity.Name)as List<Books>;
            var books = db.Books.ToList();
            if (basket != null)
            {
                basket.ForEach(book =>
                {
                    if (books.Contains(book))
                    {
                        books.Remove(book);
                    }
                });
            }
            
            var shop = new Shop()
            {
                Books = books,
                Basket = basket==null?new List<Books>():basket
            };
            return View(shop);
        }
       


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
