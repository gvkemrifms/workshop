using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Fleet_WorkShop.Models
{
    public class InfraStructure
    {
        [Required(ErrorMessage = "Infra Name is Required")]
        [DisplayName("InfraStructure Name")]
        public string InfraName { get; set; }
        [Required(ErrorMessage = "Category is Required")]
        public int CategoryId { get; set; }
        public SelectList Category { get; set; }
        public string CategoryName { get; set; }
        [Required(ErrorMessage = "Quantity is Required")]
        public int? Quantity { get; set; }
        public int infra_id { get; set; }
        public int WorkShopId { get; set; }
        public decimal Cost { get; set; }
    }
}