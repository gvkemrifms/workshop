using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fleet_WorkShop.Models
{
    public class ScrapBinModel
    {
        public int ScrapBinId { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public int Quantity { get; set; }
    }
}