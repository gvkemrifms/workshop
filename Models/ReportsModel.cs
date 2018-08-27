using System;
using System.ComponentModel;

namespace Fleet_WorkShop.Models
{
    public class ReportsModel
    {
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
    }
}