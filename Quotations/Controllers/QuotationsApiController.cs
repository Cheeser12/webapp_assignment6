using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Quotations.Helper;
using Quotations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Quotations.Controllers
{
    public class QuotationsApiController : ApiController
    {
        private QuotationsContext db;
        private UserManager<ApplicationUser> userManager;

        public QuotationsApiController()
        {
            db = new QuotationsContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        // GET api/<controller>/5
        [HttpGet]
        public QuotationViewModel Quotation(int id)
        {
            var quote = db.Quotations.Where(q => q.QuotationId == id).FirstOrDefault();
            if (quote != null)
            {
                return new QuotationViewModel { Quote = quote.Quote, Author = quote.Author, Category = quote.Category.Name };
            }
            else
            {
                // Return a null view model
                return new QuotationViewModel();
            }
        }

        // DELETE a quotation
        // Requires the user to own this quote or be an admin
        [HttpGet]
        public HttpResponseMessage DeleteQuote(int id)
        {
            var quote = db.Quotations.Where(q => q.QuotationId == id).FirstOrDefault();
            var user = userManager.FindById(User.Identity.GetUserId());

            // Check if the quote exists
            if (quote == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            
            // Check if the user has the authentication to remove this quote
            if (User.IsInRole("admin") || (User.Identity.IsAuthenticated && quote.User.Id.Equals(user.Id))) 
            {
                db.Quotations.Remove(quote);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }
        
        // CREATE a quotation
        [HttpPost]
        public HttpResponseMessage CreateQuote(QuotationViewModel quote)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = userManager.FindById(User.Identity.GetUserId());

                using (var categoryCreator = new CategoryCreationHelper())
                {
                    categoryCreator.TryCreateCategory(quote.Category);
                }

                // Grab the category
                var category = db.Categories.Where(c => c.Name.Equals(quote.Category)).First();

                // Create the quote
                var newQuote = new Quotation { Author = quote.Author, Category = category, DateAdded = DateTime.Now, Quote = quote.Quote, User = user };
                db.Quotations.Add(newQuote);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        // UPDATE a quotation
        [HttpPost]
        public HttpResponseMessage UpdateQuote(int id, QuotationViewModel quote)
        {
            var quoteToUpdate = db.Quotations.Where(q => q.QuotationId == id).FirstOrDefault();
            var user = userManager.FindById(User.Identity.GetUserId());
            if (quoteToUpdate == null) 
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else if (User.IsInRole("admin") || (User.Identity.IsAuthenticated && quoteToUpdate.User.Id.Equals(user.Id))) 
            {
                using (var categoryCreator = new CategoryCreationHelper())
                {
                    categoryCreator.TryCreateCategory(quote.Category);
                }

                // Grab the category
                var category = db.Categories.Where(c => c.Name.Equals(quote.Category)).First();
                

                // Update the quotation
                db.Entry(quoteToUpdate).State = System.Data.Entity.EntityState.Modified;
                var entity = db.Entry(quoteToUpdate).Entity;
                entity.Category = category;
                entity.Author = quote.Author;
                entity.Quote = quote.Quote;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        // Get all quotations
        [HttpGet]
        public IEnumerable<QuotationViewModel> Quotations()
        {
            return from quote in db.Quotations.ToList()
                        select new QuotationViewModel { Quote = quote.Quote, Author = quote.Author, Category = quote.Category.Name };
        }

        [HttpGet]
        public QuotationViewModel RandomQuotation()
        {
            Random rand = new Random();
            var ids = db.Quotations.Select(q => q.QuotationId);
            var idArray = ids.ToArray();
            int randomIndex = rand.Next(0, idArray.Length);
            int id = idArray[randomIndex];
            var quote = db.Quotations.Where(q => q.QuotationId == id).First();
            return new QuotationViewModel { Quote = quote.Quote, Author = quote.Author, Category = quote.Category.Name };
        }
    }
}