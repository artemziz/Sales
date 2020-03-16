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

        private bool BookAlreadyAdd(int id)
        {
            if (!(cache.Get(User.Identity.Name) as List<Books>)
                .Contains(db.Books.FirstOrDefault(book => book.Id == id)))
            {
                return false;
            }
            else return true;
        }
        private decimal GetCost(List<Books> basket)
        {
            decimal totalCost = 0;
            if (basket != null)
            {
                basket.ForEach(book => totalCost += book.Price);
            }
            return totalCost;
        }

        [Authorize]
        
        public async Task<IActionResult> Index(int? id)
        {
            if (id != null)
            {
                if(cache.TryGetValue(User.Identity.Name,out List<Books> value))
                {
                    if(!BookAlreadyAdd((int)id))
                    {
                        value.Add(await db.Books.FirstOrDefaultAsync(book => book.Id == id));
                        cache.Set(User.Identity.Name, value);
                    }
                    
                }
                else
                {
                    var newOrder = new List<Books>();
                    newOrder.Add(await db.Books.FirstOrDefaultAsync(book => book.Id == id));
                    cache.Set(User.Identity.Name, newOrder);
                }
                
            }
            
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
            if (books != null)
            {
                books.ForEach(book =>
                {
                    if (book.Quantity == 0)
                    {
                        books.Remove(book);
                    }
                });
            }

            var shop = new Shop()
            {
                Books = books,
                Basket = basket == null ? new List<Books>() : basket,
                TotalCost = GetCost(basket),
                Code = User.Identity.Name,
                
            };
            return View(shop);
        }

        [Authorize]

        public async Task<IActionResult> Order(int? id)
        {

            var basket = cache.Get(User.Identity.Name) as List<Books>;
            var promoId =( await db.Promocode.FirstOrDefaultAsync(promo => promo.Code == User.Identity.Name)).Id;
            basket.ForEach(boughtBook =>
            {
                
                (db.Books.FirstOrDefault(book => book.Id == boughtBook.Id)).Quantity-=1;
                db.Orders.Add(new Orders()
                {
                    BookId = boughtBook.Id,
                    PromoId = promoId,
                }) ;
            });
            (await db.Promocode.FirstOrDefaultAsync(promo => promo.Code == User.Identity.Name)).IsUsed = true;
            await db.SaveChangesAsync();

            return RedirectToAction("Logout", "Auth");
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
