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
        private QuotationsContext db = new QuotationsContext();
        // GET api/<controller>
        //public IEnumerable<string> Get()
        //{
          //  return new string[] { "value1", "value2" };
        //}

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