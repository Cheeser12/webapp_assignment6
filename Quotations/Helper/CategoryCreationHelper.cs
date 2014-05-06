using Quotations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quotations.Helper
{
    public class CategoryCreationHelper : IDisposable
    {
        private QuotationsContext db;
        public CategoryCreationHelper()
        {
            db = new QuotationsContext();
        }

        public void TryCreateCategory(string categoryName) 
        {
            // Add the category if it doesn't already exist in the database
            if (db.Categories.Where(c => c.Name.Equals(categoryName)).Count() == 0)
            {
                db.Categories.Add(new Category { Name = categoryName });
                db.SaveChanges();
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}