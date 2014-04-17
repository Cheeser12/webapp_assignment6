using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Quotations.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;

namespace Quotations.Controllers
{
    public class QuotationsController : Controller
    {
        private QuotationsContext db;
        private UserManager<ApplicationUser> manager;
        public QuotationsController()
        {
            db = new QuotationsContext();
            manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        // GET: /Quotations/
        public ActionResult Index(string search)
        {
            var quotations = db.Quotations.Include(q => q.Category);
            ViewBag.UserLoggedIn = User.Identity.IsAuthenticated;
            // If the user is an admin, pass that to the viewbag
            if (User.IsInRole("admin"))
            {
                ViewBag.IsAdmin = true;
            }
            // Process search
            if (!string.IsNullOrEmpty(search))
            {
                quotations = quotations.Where(q => q.Category.Name.Contains(search) || q.Author.Contains(search) || q.Quote.Contains(search));
                ViewBag.ShowDisplayAllButton = true;
            }
            else
            {
                ViewBag.ShowDisplayAllButton = false;
            }

            // Process hidden quotation cookie
            if (Request.Cookies.AllKeys.Contains("hidden_quotations"))
            {
                HttpCookie cookie = Request.Cookies["hidden_quotations"];
                string[] string_ids = cookie.Value.Split(' ');
                int[] ids = string_ids.Select(int.Parse).ToArray();

                foreach (int id in ids)
                {
                    quotations = quotations.Where(q => q.QuotationId != id);
                }
            }

            return View(quotations.ToList());
        }

        // GET: /Quotations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quotation quotation = db.Quotations.Find(id);
            if (quotation == null)
            {
                return HttpNotFound();
            }
            return View(quotation);
        }

        // GET: /Quotations/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: /Quotations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Create([Bind(Include="QuotationId,Quote,Author,CategoryId,DateAdded")] Quotation quotation)
        {
            var user = await manager.FindByIdAsync(User.Identity.GetUserId());
            quotation.DateAdded = DateTime.Today;
            quotation.User = user;
            if (ModelState.IsValid)
            {
                db.Quotations.Add(quotation);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", quotation.CategoryId);
            return View(quotation);
        }

        // GET /Quotations/CreateCategory
        [Authorize]
        public ActionResult CreateCategory()
        {
            return View();
        }

        // POST /Quotations/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateCategory([Bind(Include = "Name")] Category category)
        {

            if (ModelState.IsValid)
            {
                if (db.Categories.Where(c => c.Name == category.Name).Count() == 0)
                {
                    db.Categories.Add(category);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.CategoryExistsError = string.Format("Category '{0}' already exists.", category.Name);
                    return View(category);
                }
            }
            return View(category);
        }

        // GET: /Quotations/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quotation quotation = db.Quotations.Find(id);
            if (quotation == null)
            {
                return HttpNotFound();
            }
            var user = manager.FindById(User.Identity.GetUserId());
            if (quotation.User.Id != user.Id && !User.IsInRole("admin"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", quotation.CategoryId);
            return View(quotation);
        }

        // POST: /Quotations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="QuotationId,Quote,Author,CategoryId,DateAdded")] Quotation quotation)
        {
            quotation.DateAdded = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Entry(quotation).State = EntityState.Modified;
                db.SaveChanges();

                if (User.IsInRole("admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index");
                }    
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", quotation.CategoryId);
            return View(quotation);
        }

        // GET: /Quotations/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quotation quotation = db.Quotations.Find(id);
            if (quotation == null)
            {
                return HttpNotFound();
            }
            var user = manager.FindById(User.Identity.GetUserId());
            if (quotation.User.Id != user.Id && !User.IsInRole("admin"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(quotation);
        }

        // POST: /Quotations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Quotation quotation = db.Quotations.Find(id);
            db.Quotations.Remove(quotation);
            db.SaveChanges();

            if (User.IsInRole("admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Hide(int id)
        {
            HttpCookie cookie;
            if (Request.Cookies.AllKeys.Contains("hidden_quotations"))
            {
                cookie = Request.Cookies["hidden_quotations"];
                cookie.Value += " " + id;
                Response.Cookies.Add(cookie);
            }
            else
            {
                
                cookie = new HttpCookie("hidden_quotations");
                cookie.Value = id.ToString();
                cookie.Expires = DateTime.Now.AddDays(7);
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ViewUserQuotations()
        {
            var user = manager.FindById(User.Identity.GetUserId());
            var quotes = db.Quotations.Where(q => q.User.Id == user.Id);
            return View(quotes.ToList());
        }

        public ActionResult Unhide()
        {
            if (Request.Cookies.AllKeys.Contains("hidden_quotations"))
            {
                var cookie = Request.Cookies["hidden_quotations"];
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
