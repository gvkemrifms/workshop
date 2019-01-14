using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Fleet_WorkShop.Models
{
    public class SparePartsModel
    {
        public int Id { get; set; }
        [Required]
        public SelectList Manufacturer { get; set; }
        public string ManufacturerName { get; set; }
        public int ManufacturerId { get; set; }
        [Required]
        public string PartName { get; set; }
        [Required]
        public string PartNumber { get; set; }
        [Required]
        public decimal Cost { get; set; }
        public int ScrapBinId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class LubesModel
    {
        public int Id { get; set; }
        public SelectList Manufacturer { get; set; }
        [Required(ErrorMessage = "Manufacturer is Required")]
        public int ManufacturerId { get; set; }
        [Required(ErrorMessage = "Lubricant is Required")]
        [DisplayName("Lubricant")]
        public string OilName { get; set; }
        [Required(ErrorMessage = "Cost is Required")]
        [DisplayName("Cost/Litre")]
        public decimal CostPerLitre { get; set; }
        [Required(ErrorMessage = "Lubricant Number is Required")]
        [DisplayName("Lubricant Number")]
        public string LubricantNumber { get; set; }
    }
}