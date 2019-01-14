using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fleet_WorkShop.Models
{
    public class SS
    {
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int EstimatedCost { get; set; }
        public int DistrictId { get; set; }
        public int VehId { get; set; }
        public int ModelNumber { get; set; }
        public DateTime DateOfRepair { get; set; }
        public int Odometer { get; set; }
        public int PilotId { get; set; }
        public int ApproximateCost { get; set; }
        public int? AggregateId { get; set; }
        public DateTime DateOfDelivery { get; set; }
        public string ReceivedLocation { get; set; }
        public int AllotedMechanic { get; set; }
        public int ManufacturerId { get; set; }
        public int ServiceEngineer { get; set; }
        public string PilotName { get; set; }

    }
}