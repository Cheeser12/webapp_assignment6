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
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        [HttpGet]
        public string Random()
        {
            Random rand = new Random();
            var ids = db.Quotations.Select(q => q.QuotationId);
            var idArray = ids.ToArray();
            int randomIndex = rand.Next(1, idArray.Length);
            int id = idArray[randomIndex];
            var quote = db.Quotations.Where(q => q.QuotationId == id).First();
            return quote.Quote;
        }
    }
}