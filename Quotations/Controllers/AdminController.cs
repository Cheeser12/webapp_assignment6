using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Quotations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Quotations.Controllers
{
    [Authorize(Roles="admin")]
    public class AdminController : Controller
    {
        private QuotationsContext db;
        private UserManager<ApplicationUser> manager;
        private ApplicationDbContext users;
        public AdminController()
        {
            db = new QuotationsContext();
            manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            users = new ApplicationDbContext();
        }
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            var userNamesListItems = new List<SelectListItem>();
            foreach (var user in users.Users)
            {
                var selectListItem = new SelectListItem();
                selectListItem.Text = user.UserName;
                selectListItem.Value = user.Id;
                userNamesListItems.Add(selectListItem);
            }
            ViewBag.User = userNamesListItems;

            // Pass in counts
            ViewBag.UserCount = users.Users.Count();
            ViewBag.CategoryCount = db.Categories.Count();
            ViewBag.QuoteCount = db.Quotations.Count();

            // Pass in the sorted list of categories
            var sortedCategories = db.Categories.OrderByDescending(c => c.Quotations.Count);
            ViewBag.SortedCategories = sortedCategories;

            // Pass in the sorted list of users
            var sortedUsers = users.Users.OrderByDescending(u => u.UserQuotes.Count);
            ViewBag.SortedUsers = sortedUsers;

            return View();
        }

        public ActionResult UserQuotes(string User)
        {
            var user = manager.FindById(User);
            var quotes = db.Quotations.Where(q => q.User.Id == user.Id);
            ViewBag.UserName = user.UserName;
            return View(quotes.ToList());
        }
	}
}