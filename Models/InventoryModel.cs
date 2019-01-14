using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Fleet_WorkShop.Models
{
    public class InventoryModel
    {
        public string SpareName { get; set; }
        public int WorkShopId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        [DisplayName("Manufacturer Name")]
        public string ManName { get; set; }

        [DisplayName("SparePart Name")]
        public string PartName { get; set; }

        [DisplayName("SparePart Number")]
        public string PartNumber { get; set; }

        [DisplayName("Unit Price")]
        public decimal Uprice { get; set; }

        [DisplayName("Quantity")]
        public decimal Qty { get; set; }

        [DisplayName("Amount")]
        public decimal Amt { get; set; }

        [DisplayName("Bill Number")]
        [Required(ErrorMessage = "Please select Bill Number")]
        public string BillNo { get; set; }

        [DisplayName("Bill Date")]
        [Required(ErrorMessage = "Please select Bill Date")]
        public DateTime BillDate { get; set; }

        [DisplayName("Vendor")]
        [Required(ErrorMessage = "Please select Vendor")]
        public SelectList Vendor { get; set; }

        public string VendorName { get; set; }
        public string LubricantName { get; set; }
        public int LubricantId { get; set; }
        public SelectList Lubricant { get; set; }
        public int VendorId { get; set; }

        [DisplayName("Bill Amount")]
        public decimal BillAmount { get; set; }

        [Required(ErrorMessage = "Please select Manufacturer")]
        public SelectList Manufacturer { get; set; }

        public SelectList SpareParts { get; set; }
        public int ManufacturerId { get; set; }
        public int SparePartId { get; set; }

        public string PoNumber { get; set; }
        public DateTime PoDate { get; set; }
        public List<InventoryDetails> itemmodel { get; set; }
        public int ScrapBinId { get; set; }
        public int EmployeeId { get; internal set; }
        public string Status { get; internal set; }
        public int RoleId { get; internal set; }
        public decimal Quantity { get; internal set; }
        public string WorkShopName { get; internal set; }
        public string SentBy { get; internal set; }
        public DateTime SentOn { get; internal set; }
        public string PONo { get; internal set; }
        public decimal POQuantity { get; set; }
    }

    public class InventoryDetails
    {
        public int ManufacturerId { get; set; }
        public int SparePartId { get; set; }

        public string ManufacturerName { get; set; }

        [DisplayName("Spare Parts")]
        [Required(ErrorMessage = "Please select Spare Parts")]
        public SelectList SpareParts { get; set; }

        public string SparePartName { get; set; }

        [DisplayName("Unit Price")]
        [Required(ErrorMessage = "Please select Unit Price")]
        public int UnitPrice { get; set; }

        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select Quantity")]
        public int Quantity { get; set; }

        public decimal LubesQty { get; set; }
        public string LubricantName { get; set; }
        public int LubricantId { get; set; }
    }

    public class SparePartStocks
    {
        public int WorkShopId { get; set; }
        public int ManufacturerId { get; set; }
        public int SparePartId { get; set; }
        public int Quantity { get; set; }
        public int LubricantId { get; set; }
        public decimal TotalAmount { get; set; }
        public int VendorId { get; set; }
    }

    public class GetDistanceAndProject
    {
        public double distance { get; set; }
        public string project { get; set; }
    }

}