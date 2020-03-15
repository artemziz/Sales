using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sales.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sales.Controllers
{
    public class AuthController : Controller
    {
        static Random rnd = new Random();
        private string GenCode()
        {
            List<string> result = new List<string>();
            string[] alphabet = { "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"};
            for(int i = 0; i < 4; i++)
            {
                result.Add(alphabet[rnd.Next(0, alphabet.Length)]);
            }
            return string.Join("",result);
        }
        private SalesContext db;
        public AuthController(SalesContext context)
        {
            db = context;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Code)
        {
            if (Code != null)
            {
                Promocode user = await db.Promocode.FirstOrDefaultAsync(user => user.Code == Code);
                if(user != null)
                {
                    if (!user.IsUsed)
                    {
                        await Authenticate(user.Code); // аутентификация

                        return RedirectToAction("Index", "Home");
                    }
                   
                }
            }
            return RedirectToAction("Login", "Auth");
        }
        public async Task<IActionResult> Register()
        {
            var user = new Promocode()
            {
                Code = GenCode(),
                IsUsed = false,
            };
            while(await db.Promocode.FirstOrDefaultAsync(FindUser => FindUser.Code == user.Code)!=null)
            {
                user = new Promocode()
                {
                    Code = GenCode(),
                    IsUsed = false,
                };
            }
            db.Promocode.Add(user) ;
            await db.SaveChangesAsync();

            await Authenticate(user.Code); // аутентификация

            return RedirectToAction("Index", "Home");
            
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Promocode user = await db.Promocode.FirstOrDefaultAsync(user => user.Code == User.Identity.Name);
            user.IsUsed = true;
            db.Promocode.Update(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Login", "Auth");
        }

        private async Task Authenticate(string userCode)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userCode)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}
