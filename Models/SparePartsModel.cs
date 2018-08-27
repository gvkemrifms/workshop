using System.ComponentModel;
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
        public int ScrapBinId { get; set; }
    }

    public class LubesModel
    {
        public int Id { get; set; }
        public SelectList Manufacturer { get; set; }
        public int ManufacturerId { get; set; }

        [DisplayName("Lubricant")]
        public string OilName { get; set; }

        [DisplayName("Cost/Litre")]
        public decimal CostPerLitre { get; set; }

        [DisplayName("Lubricant Number")]
        public string LubricantNumber { get; set; }
    }
}