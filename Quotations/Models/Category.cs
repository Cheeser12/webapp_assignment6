using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Quotations.Models
{
    public class Category
    {
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [DisplayName("Category")]
        [Required]
        public string Name { get; set; }
        public virtual List<Quotation> Quotations { get; set; }
    }
}