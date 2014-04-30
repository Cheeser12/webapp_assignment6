using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Quotations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public ActionResult Index(string importResult, bool? imported)
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

            // If we came here from an attempt to import quotations, pass in the result message
            if (!string.IsNullOrEmpty(importResult))
            {
                ViewBag.ImportResult = importResult;
                ViewBag.Imported = imported;
            }

            return View();
        }

        public ActionResult UserQuotes(string User)
        {
            var user = manager.FindById(User);
            var quotes = db.Quotations.Where(q => q.User.Id == user.Id);
            ViewBag.UserName = user.UserName;
            return View(quotes.ToList());
        }

        public ActionResult ListAll()
        {
            return View(db.Quotations.ToList());
        }

        [HttpPost]
        public ActionResult ImportQuotes(string importUrl)
        {
            // Message to send back to the index method
            string importResult;

            // Make a request to the URL
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            // Wrap this to check for an invalid URL
            try
            {
                response = client.GetAsync(importUrl).Result;
            }
            catch (InvalidOperationException e)
            {
                importResult = "That URL was invalid. Make sure you use the FULL url (http:// included).";
                return RedirectToAction("Index", new { importResult = importResult, imported = false });
            }

            // If we couldn't get the quotes, redirect with an error message
            if (response.StatusCode != HttpStatusCode.OK)
            {
                importResult = "There was a problem importing from that URL.";
                return RedirectToAction("Index", new { importResult = importResult, imported = false });
            }

            // Else, try to get the quotes
            IEnumerable<QuotationViewModel> quotes;   
            try
            {
                quotes = response.Content.ReadAsAsync<IEnumerable<QuotationViewModel>>().Result;
            }
            catch (UnsupportedMediaTypeException e)
            {
                importResult = "There was a problem importing from that URL.";
                return RedirectToAction("Index", new { importResult = importResult, imported = false });
            }

            foreach (var quote in quotes)
            {
                // We'll only add a quote if it doesn't already exist in the database 
                if (db.Quotations.Where(q => q.Quote.Equals(quote.Quote)).Count() == 0)
                {
                    // Add the category if it doesn't already exist in the database
                    if (db.Categories.Where(c => c.Name.Equals(quote.Category)).Count() == 0)
                    {
                        db.Categories.Add(new Category { Name = quote.Category });
                        db.SaveChanges();
                    }

                    // Grab the category
                    var category = db.Categories.Where(c => c.Name.Equals(quote.Category)).First();

                    // Add the new quotation via the Quotations controller
                    var user = manager.FindById(User.Identity.GetUserId());
                    db.Quotations.Add(new Quotation { Quote = quote.Quote, Author = quote.Author, 
                        CategoryId = category.CategoryId, User = user, DateAdded = DateTime.Today });
                    db.SaveChanges();
                }
            }

            importResult = "Quotes imported successfully!";
            return RedirectToAction("Index", new { importResult = importResult, imported = true });
        }
	}
}