using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CV.Web.Models;
using CV.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using CV.Web.Helper;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CV.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbContext dbContext;
        public HomeController(ILogger<HomeController> logger, DbContext dbContext)
        {
            this._logger = logger;
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var u = dbContext.Set<User>().FirstOrDefault();

            if (u != null) 
            {
                var model = new HomeViewModel 
                {
                    Email = u.Email,
                    Body = u.Body
                };

                ViewData["Nav"] = u.Nav;

                return View(model);
            }

            return View();
        }

        [Route("login")]
        public IActionResult Login()
        {


            return View();
        }

        [Route("login")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            User u;

            if (dbContext.Set<User>().Count() == 0)
            {
                // register
                u = new Models.User
                {
                    Email = model.Email,
                    Password = HashHelper.CreateMD5(model.Password)
                };

                dbContext.Set<User>().Add(u);

                dbContext.SaveChanges();
            }
            else 
            {
                u = dbContext.Set<User>().FirstOrDefault(x => x.Email == model.Email);

                if (u.Password != model.Password) // hash
                {
                    u = null;
                }
            }

            if (u != null)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, u.Email)
                }, CookieAuthenticationDefaults.AuthenticationScheme)));

                return Redirect("/dashboard");
            }

            ModelState.AddModelError("password", "email or password wrong!");

            return View(model);
        }

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return Redirect("/");
        }

        [Authorize]
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            var u = dbContext.Set<User>().FirstOrDefault();

            var model = new DashboardViewModel 
            {
                Email  = u.Email,
                Body = u.Body,
                Nav = u.Nav,
                TinyMCEApi = u.TinyMCEApi
            };

            return View(model);
        }


        [Route("dashboard")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Dashboard(DashboardViewModel model) 
        {
            var u = dbContext.Set<User>().FirstOrDefault();

            if (!string.IsNullOrEmpty(model.TinyMCEApi) && string.IsNullOrEmpty(model.Email))
            {
                // set api key
                u.TinyMCEApi = model.TinyMCEApi;
            }
            else 
            {
                if (!ModelState.IsValid)
                    return View(model);

                u.Email = model.Email;
                u.Nav = model.Nav;
                u.Body = model.Body;
            }

            dbContext.Update<User>(u);
            dbContext.SaveChanges();

            return Redirect("/dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
