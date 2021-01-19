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
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using HtmlAgilityPack;

namespace CV.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbContext dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, DbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            this._logger = logger;
            this.dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var u = dbContext.Set<User>().FirstOrDefault();

            if (u != null)
            {
                var model = new HomeViewModel
                {
                    Name = u.Name,
                    Body = u.Body
                };

                ViewData["Nav"] = u.Nav;

                return View(model);
            }

            return Redirect("/signup");
        }

        [Route("login")]
        public IActionResult Login() => View();

        [Route("signup")]
        public IActionResult Signup() => View(new SignupViewModel { Block = dbContext.Set<User>().Any() });

        [Route("login")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            User u = dbContext.Set<User>().FirstOrDefault(x => x.Email == model.Email);
            
            if (u!=null && u.Password == HashHelper.CreateMD5(model.Password))
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

        [Route("signup")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (dbContext.Set<User>().Count() == 0) // only one user :)
            {
                // register
                var u = new Models.User
                {
                    Name = model.Name,
                    Email = model.Email,

                    // NOTE: this is not good way to Hash password, consider using Salt with random string :)
                    Password = HashHelper.CreateMD5(model.Password)
                };

                dbContext.Set<User>().Add(u);

                dbContext.SaveChanges();

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, u.Email)
                }, CookieAuthenticationDefaults.AuthenticationScheme)));

                return Redirect("/dashboard");
            }

            return Redirect("/login");
        }

        [Authorize]
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
                Name  = u.Name,
                Email  = u.Email,
                Body = u.Body,
                Nav = u.Nav,
                TinyMCEApi = u.TinyMCEApi,
                CV = u.CV,
                Image = u.Image
            };

            return View(model);
        }


        [Route("dashboard")]
        [Authorize]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Dashboard(DashboardViewModel model) 
        {
            var u = dbContext.Set<User>().FirstOrDefault();

            if ((!string.IsNullOrEmpty(model.TinyMCEApi) || model.UseRawHtml) && string.IsNullOrEmpty(model.Email))
            {
                // set api key
                u.TinyMCEApi = !string.IsNullOrEmpty(model.TinyMCEApi) ? model.TinyMCEApi : ":)";
            }
            else 
            {
                if (!ModelState.IsValid)
                    return View(model);

                u.Name = model.Name;
                u.Email = model.Email;

                if (!string.IsNullOrEmpty(model.Password))
                    u.Password = HashHelper.CreateMD5(model.Password);

                //u.Nav = model.Nav; // change to HtmlAgilityPack
                u.Body = model.Body;

                if (!string.IsNullOrEmpty(u.Body))
                {
                    u.Nav = "";
                    var document = new HtmlDocument();
                    document.LoadHtml(u.Body);
                    if (document.DocumentNode.SelectNodes("//a") != null)
                    {
                        foreach (HtmlNode link in document.DocumentNode.SelectNodes("//a"))
                        {
                            string hrefValue = link.GetAttributeValue("id", string.Empty);
                            if (!string.IsNullOrEmpty(hrefValue))
                                u.Nav += $"<li class=\"nav-item\"><a class=\"nav-link\" href=\"/#{hrefValue}\">{link.ParentNode.InnerText}</a></li>";
                        }
                    }
                }

            }

            dbContext.Update<User>(u);
            dbContext.SaveChanges();

            return Redirect("/dashboard");
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Upload(List<IFormFile> files)
        {
            var pdf = files.FirstOrDefault(x=>x.ContentType.ToLower() == "application/pdf");
            var jpg = files.FirstOrDefault(x=>x.ContentType.ToLower() == "image/jpeg");

            if (pdf == null && jpg == null)
                return Redirect("/dashboard");

            var u = dbContext.Set<User>().FirstOrDefault();

            var path = Path.Combine(_webHostEnvironment.WebRootPath, "files");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (pdf != null) 
            {
                var pdfFile = Path.Combine(path, pdf.FileName);

                using (var stream = System.IO.File.Create(pdfFile))
                {
                    await pdf.CopyToAsync(stream);
                }
                
                u.CV = $"/files/{pdf.FileName}";
            }

            if (jpg != null)
            {
                var jpgFile = Path.Combine(path, jpg.FileName);

                using (var stream = System.IO.File.Create(jpgFile))
                {
                    await jpg.CopyToAsync(stream);
                }

                u.Image = $"/files/{jpg.FileName}";
            }

            dbContext.Update<User>(u);
            dbContext.SaveChanges();

            return Redirect("/dashboard");
        }

        [Route("delete/{t}")]
        [Authorize]
        public IActionResult Delete(string t) 
        {
            var u = dbContext.Set<User>().FirstOrDefault();

            switch (t)
            {
                case "img":
                    u.Image = null;
                break;
                case "pdf":
                    u.CV = null;
                    break;
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
