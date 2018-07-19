using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Fleet_WorkShop.Models
{
    public class InfraStructure
    {
        [DisplayName("InfraStructure Name")]
        public string InfraName { get; set; }
        public int CategoryId { get; set; }
        public SelectList Category { get; set; }
        public string CategoryName { get; set; }
        public int? Quantity { get; set; }
        public int infra_id { get; set; }
    }
}