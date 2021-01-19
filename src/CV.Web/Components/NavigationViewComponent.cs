using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CV.Web.Models;
using CV.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV.Web.Components
{
    public class NavigationViewComponent : ViewComponent
    {
        private readonly DbContext dbContext;

        public NavigationViewComponent(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IViewComponentResult Invoke()
        {
            var u = dbContext.Set<User>().FirstOrDefault();

            if (u != null)
                return View(new NavViewModel
                {
                    Name = u.Name,
                    CV = u.CV,
                    Image = u.Image,
                    Nav = u.Nav
                });
            else
                return View("NoUser");
        }
    }
}
