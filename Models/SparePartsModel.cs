using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Fleet_WorkShop.Models
{
    public class SparePartsModel
    {
        public int Id { get; set; }
        public SelectList Manufacturer { get; set; }
        public string ManufacturerName { get; set; }
        public int ManufacturerId { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public decimal Cost { get; set; }
    }

    public class LubesModel
    {
        public SelectList Manufacturer { get; set; }
        public int ManufacturerId { get; set; }
        public string OilName { get; set; }
        public decimal CostPerLitre { get; set; }
    }
}