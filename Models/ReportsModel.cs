using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Fleet_WorkShop.Models
{
    public class ReportsModel
    {
        [DisplayName("Employee Name")]
        public string EmployeeName { get; set; }
        [DisplayName("WorkShop Name")]
        public string WorkShopName { get; set; }
        public string Designation { get; set; }
        public string Experience { get; set; }
        [DisplayName("Date Of Joining")]
        public string DateOfJoining { get; set; }
        public string Infrastructure { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string Manufacturer { get; set; }
        public string SparePart { get; set; }
        public string Lubricant { get; set; }

    }
}