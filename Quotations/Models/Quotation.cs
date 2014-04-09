using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Quotations.Models
{
    public class Quotation
    {
        public int QuotationId { get; set; }
        [Required]
        public string Quote { get; set; }
        [Required]
        public string Author { get; set; }
        public virtual Category Category { get; set; }
        public int CategoryId { get; set; }
        [DisplayName("Date Added")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateAdded { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}