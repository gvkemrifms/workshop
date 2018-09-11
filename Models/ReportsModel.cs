using System;
using System.ComponentModel;

namespace Fleet_WorkShop.Models
{
    public class ReportsModel
    {
        internal DateTime DOD;

        [DisplayName("Employee Name")]
        public string EmployeeName { get; set; }

        [DisplayName("WorkShop Name")]
        public string WorkShopName { get; set; }

        public string Designation { get; set; }
        public int Experience { get; set; }

        [DisplayName("Date Of Joining")]
        public DateTime DateOfJoining { get; set; }

        public string Infrastructure { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string Manufacturer { get; set; }

        [DisplayName("Spare Parts")]
        public string SparePart { get; set; }

        public string Lubricant { get; set; }
        public long ID { get; set; }

        [DisplayName("Employee Id")]
        public string EmployeeId { get; set; }
        public int JobCardNumber { get; internal set; }
        public object District { get; internal set; }
        public string VehicleNumber { get; internal set; }
        public DateTime DOR { get; internal set; }
        public string Aggregate { get; internal set; }
        public string ServiceIncharge { get; internal set; }
        public string Status { get; internal set; }
        public string Mechanic { get; internal set; }
        public string SubCategory { get; internal set; }
    }

    public class VehicleReport
    {
        public long Id { get; set; }
        public string Workshop { get; set; }
        public string Vehicle { get; set; }
        public string Sparepart { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public int JobcardId { get; set; }
        public string HandOverTo { get; set; }
        public DateTime IssuedDate { get; set; }
        public string Status { get; set; }
        public string District { get; set; }
        public DateTime DateOfRepair { get; set; }
        public DateTime DateOfDelivery { get; set; }
        public string Aggregate { get; set; }
        public string Mechanic { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int ManufacturerName { get; internal set; }
        public string Lubricant { get; internal set; }
        public string ServiceIncharge { get; internal set; }
        public string Manufacturer { get; set; }
        public int ServiceInchargeId { get; internal set; }
    }
}