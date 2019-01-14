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
        public decimal Quantityy { get; internal set; }
    }

    public class IssueItemsReport
    {
        public int? Sparesid { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string VehicleNumber { get; set; }

    }
    public class VehicleReport
    {
        public long Id { get; set; }
        [DisplayName("ZWS Name")]
        public string Workshop { get; set; }
        public string Vehicle { get; set; }
        [DisplayName("Spare Part")]
        public string Sparepart { get; set; }
        public int Quantityy { get; set; }
        public decimal Amount { get; set; }
        [DisplayName("Job Card")]
        public int JobcardId { get; set; }
        [DisplayName("HandOver To")]
        public string HandOverTo { get; set; }
        [DisplayName("Issued Date")]
        public DateTime IssuedDate { get; set; }
        public string Status { get; set; }
        public string District { get; set; }
        [DisplayName("Date Of Repair")]
        public DateTime DateOfRepair { get; set; }
        [DisplayName("Date Of Delivary")]
        public DateTime DateOfDelivery { get; set; }
        public string Aggregate { get; set; }
        public string Mechanic { get; set; }
        public string Category { get; set; }
        [DisplayName("Sub Category")]
        public string SubCategory { get; set; }
        [DisplayName("Namufacturer")]
        public int ManufacturerName { get; internal set; }
        public string Lubricant { get; internal set; }
        [DisplayName("Service Incharge")]
        public string ServiceIncharge { get; internal set; }
        public string Manufacturer { get; set; }
        public int ServiceInchargeId { get; internal set; }
        [DisplayName(" Repair Completion Date")]
        public DateTime? DateOfRepairCompletion { get; internal set; }
        public int? DeviationTime { get; internal set; }
        [DisplayName("Delivary Date")]
        public string DalivaryDate { get; internal set; }
        [DisplayName("Repair Date")]
        public string RepairDate { get; internal set; }
        public string Project { get; internal set; }
        [DisplayName("Vehicle Location")]
        public string VehicleLocation { get; internal set; }
        public int Odometer { get; internal set; }
        [DisplayName("Total Completion Time")]
        public string TotalCompletionTime { get; internal set; }
        [DisplayName("Distance Travelled")]
        public int DistanceTravelled { get; internal set; }
        public int ScheduledSeervice { get; internal set; }
        [DisplayName("Scheduled Service")]
        public string ScheduledService { get; internal set; }
        [DisplayName("OffRoad Id")]
        public int? OffroadId { get; internal set; }
        public decimal Quantity { get; internal set; }
    }
}