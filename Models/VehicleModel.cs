using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Fleet_WorkShop.Models
{
    public class VehicleModel
    {
        
        public int? OffroadId { get; set; }
        internal int pilotIdd;
        internal DateTime DateOfDeliveryy;

        public int HelperId { get; set; }
        public string Helper { get; set; }
        [Required(ErrorMessage = "Select Allotted Mechanic ")]
        [DisplayName("Mechanic Name")]
        public string AllotedMechanicName { get; set; }

        public int LubesQuantity { get; set; }
        public int LubesHandOverToId { get; set; }
        public string LubesHandOverToName { get; set; }
        public string AggregateName { get; set; }
        public SelectList Vehicle { get; set; }
        public int Id { get; set; }

        [DisplayName("Vehicle")]
        public string VehicleId { get; set; }
        [Required(ErrorMessage = "Select Vehicle")]
        public int VehId { get; set; }

        [DisplayName("Vehicle Number")]
        public string VehicleNumberString { get; set; }
        [Required(ErrorMessage = "Enter Odometer Reading")]
        public int Odometer { get; set; }
        [Required(ErrorMessage = "Enter Pilot Name")]
        [DisplayName("Pilot Name")]
        public string PilotName { get; set; }

        [Required(ErrorMessage = "Labor Charges Required")]
        public int ApproximateCost { get; set; }

        public int IdCategory { get; set; }


        public DateTime DateOfDelivery { get; set; }
        [Required(ErrorMessage = "Aggregates Required")]
        [DisplayName("Aggregates")]
        public int NatureOfComplaint { get; set; }

        public SelectList ComplaintsNature { get; set; }
        [Required(ErrorMessage = "PilotId Required")]
        public string PilotId { get; set; }

        [DisplayName("Date Of Repair")]
        public DateTime DateOfRepair { get; set; }
        [Required(ErrorMessage = "District is Required")]
        public int DistrictId { get; set; }

        [DisplayName("District")]
        public string DistrictName { get; set; }
        [Required(ErrorMessage = "Manufacturer is Required")]
        public int ManufacturerId { get; set; }

        [DisplayName("Manufacturer Name")]
        public string ManufacturerName { get; set; }

    
        [DisplayName("District")]
        public SelectList District { get; set; }

        [Required(ErrorMessage = "Please Enter Location")]
        [DisplayName("LOC")]
        public string LocationOfCommission { get; set; }

        [Required(ErrorMessage = "Please Select Vehicle")]
        [DisplayName("Vehicle Number")]
        public string VehicleNumber { get; set; }

        [Required(ErrorMessage = "Please Enter Hasis NUmber")]
        [DisplayName("Chasis Number")]
        public string ChasisNumber { get; set; }

        [DisplayName("Engine Number")]
        public string EngineNumber { get; set; }

        [Required(ErrorMessage = "Plese select Model")]
        public string Model { get; set; }

        [DisplayName("Model")]
        public int ModelYear { get; set; }

       
        public SelectList Manufacturer { get; set; }

        public DateTime? DateOfCommission { get; set; }
        public string DOC { get; set; }
        [Required(ErrorMessage = "Enter Received Location")]
        public string ReceivedLocation { get; internal set; }

        public IEnumerable<VehicleJobCardModel> VehicleJodcard { get; set; }
        [Required(ErrorMessage = "Model is Required")]
        [DisplayName("Model")]
        public int ModelNumber { get; internal set; }

        [DisplayName("Alloted Mechanic")]
        public int AllotedMechanic { get; set; }

        public int JobCardId { get; set; }
        public int HandOverTo { get; set; }
        public string WorkShopName { get; set; }
        public List<JobCardPendingCases> itemmodel { get; set; }
        public List<JobCardPendingCases> itemmodelPending { get; set; }
        public List<VehicleJobCardModel> jobcarditems { get; set; }
        public int WorkShopId { get; set; }
        public int LubricantId { get; set; }
        public string LubricantName { get; set; }
        public int AggregateId { get; internal set; }
        public string CategoryName { get; internal set; }
        public string CategoryId { get; internal set; }
        public int SubCategory { get; internal set; }
        public string SubCategoryName { get; internal set; }
        public string SubCategoryId { get; internal set; }
        public int EstimatedCost { get; internal set; }
        [Required(ErrorMessage = "Service Engineer is Required")]
        public int ServiceEngineer { get; internal set; }
        public int LaborCharges { get; internal set; }
        public string Status { get; set; }
        public int DistanceTravelled { get; internal set; }
        public int EndOdo { get; internal set; }
    }
}

public class Aggregates
{
    public int IdSubCategory { get; set; }
    public int JobCardNumber { get; set; }

    [DisplayName("Time Taken")]
    public int timeTaken { get; set; }

    public int AggregateId { get; internal set; }

    [DisplayName("Aggregates")]
    public string AggregateName { get; set; }

    [DisplayName("Category Name")]
    public string CategoryName { get; internal set; }

    [DisplayName("Category Id")]
    public string CategoryId { get; internal set; }

    public int SubCategory { get; set; }
    public string SubCategoryName { get; internal set; }
    public string SubCategoryId { get; internal set; }
    public int CreatedBy { get; set; }

    [DisplayName("Manufacturer")]
    public int ManufacturerId { get; set; }

    [DisplayName("Category Id")]
    public int IdCategory { get; set; }

    public SelectList Categories { get; set; }
    public SelectList Manufacturer { get; set; }

    [DisplayName("Aggregates")]
    public SelectList AggregateList { get; set; }

    [DisplayName("Aggregate Id")]
    public int ServiceGroupId { get; set; }

    public string Status { get; set; }

    [DisplayName("SubCategory Id")]
    public int ServiceId { get; set; }

    [DisplayName("SubCategory Name")]
    public string ServiceName { get; set; }

    [DisplayName("Manufacturer Name")]
    public string ManufacturerName { get; set; }

    public int IdAggregate { get; set; }
    public int? ApproxCost { get; set; }
    public SelectList SubCategories { get; set; }
    public int? Cost { get; set; }
}

public class VehicleJobCardModel
{
    public int EndOdo { get; set; }
    public int ManufacturerId { get; set; }
    public int LaborCharges { get; set; }
    public int WorkShopId { get; set; }

    [DisplayName("Vehicle Id")]
    public string VehicleId { get; set; }
    [Required(ErrorMessage = "Please select VehicleId")]
    public int VehId { get; set; }
    [Required(ErrorMessage = "Please select Model")]
    public int ModelNumber { get; set; }
    [Required(ErrorMessage = "Please select Odometer")]
    public int Odometer { get; set; }
    [Required(ErrorMessage = "Please select Pilot Name")]
    public string PilotName { get; set; }
    [Required(ErrorMessage = "Please select Cost")]
    public int ApproximateCost { get; set; }

    public DateTime DateOfDelivery { get; set; }
    public int NatureOfComplaint { get; set; }
    [Required(ErrorMessage = "Please select PilotId")]
    public string PilotId { get; set; }
    public DateTime DateOfRepair { get; set; }
    [Required(ErrorMessage = "Please select District")]
    public int DistrictId { get; set; }
    [Required(ErrorMessage = "Please select Received Location")]
    public string ReceivedLocation { get; set; }
    public string DistrictName { get; set; }

    public SelectList Vehicle { get; set; }

 
    [DisplayName("District")]
    public SelectList District { get; set; }

    [DisplayName("Alloted Mechanic")]
    [Required(ErrorMessage = "Please select Allotted Mechanic")]
    public int AllotedMechanic { get; set; }

    [DisplayName("Service Incharge")]
    [Required(ErrorMessage = "Please select Service Engineer")]
    public int ServiceEngineer { get; set; }

    public int LubricantId { get; set; }
    public string LubricantName { get; set; }
    public int AggregateId { get; internal set; }
    public string CategoryName { get; internal set; }
    public string CategoryId { get; internal set; }
    public int SubCategory { get; internal set; }
    public string SubCategoryName { get; internal set; }
    public string SubCategoryId { get; internal set; }
    [Required(ErrorMessage = "Please select Category")]
    [DisplayName("Category")]
    public int CategoryIdd { get; set; }
    [Required(ErrorMessage = "Please select Sub Category")]
    public int SubCat { get; set; }
    public string SubCatName { get; set; }
}


public class JobCardPendingCases
{
    public int EndOdo { get; set; }
    public int HiddenId { get; set; }
    public int LaborCharges { get; set; }

    [DisplayName("Sub Category")]
    public int SubCategory { get; internal set; }

    public string SubCategoryName { get; internal set; }
    public string SubCategoryId { get; internal set; }
    public Guid VehicleId { get; set; }

    [DisplayName("Vehicle Number")]
    public string VehicleNumber { get; set; }

    public string DistrictName { get; set; }

    [DisplayName("Vehicle Received Date")]
    public DateTime DateOfRepair { get; set; }

    [DisplayName("Category")]
    public string Complaint { get; set; }

    [DisplayName("WorkShop")]
    public string WorkShopName { get; set; }

    [DisplayName("Mechanic")]
    public string EmployeeName { get; set; }

    public string Status { get; set; }
    public int SparePartId { get; set; }
    public int Quantity { get; set; }
    public int HandOverToId { get; set; }

    [DisplayName("JobCard")]
    public int JobCardNumber { get; set; }

    public SelectList Vehicle { get; set; }
    public string Manufacturer { get; set; }
    public string SparePart { get; set; }
    public int Cost { get; set; }
    public string ManufacturedDate { get; set; }
    public string LastEntryDate { get; set; }
    public int LubricantId { get; set; }
    public string LubricantName { get; set; }
    public Decimal LubesQuantity { get; set; }
    public int LubesHandOverToId { get; set; }
    public string LubesHandOverToName { get; set; }
    public int TotalAmount { get; set; }
    public string IssuedDate { get; set; }
    public string StatusType { get; set; }
    public int VehicleIdData { get; set; }
    public string VehicleNumberData { get; set; }
    public string CategoryData { get; set; }
    public int CostApproximate { get; set; }

    [DisplayName("Aggregates")]
    public string AggregateName { get; set; }

    [DisplayName("Asssigned Mechanic")]
    public string Mechanic { get; set; }

    public decimal? OutSourcingAmount { get; set; } = 0;
    public decimal SparePartsBill { get; internal set; }
    public decimal LubesBill { get; internal set; }
    public decimal FinalBillAmount { get; internal set; }
    public object CompletedDate { get; internal set; }
    public decimal OutSoucingSum { get; internal set; }
    public int? ManufacturerId { get; internal set; }
    [DisplayName("Estimated Time(In Min)")]
    public string EstimatedTime { get; internal set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime DateOfDelivery { get; internal set; }
    public DateTime? StartTime1 { get; internal set; }
    public DateTime? EndTime1 { get; internal set; }
    public int Odometer { get; internal set; }
}

public class GetSparePartCostDetails
{
    public string Manufacturer { get; set; }
    public string SparePart { get; set; }
    public int Cost { get; set; }
    public string ManufacturedDate { get; set; }
    public int LubricantId { get; set; }
    public string LubricantName { get; set; }
}

public class OutSourcingJobDetails
{
    public string Vendor { get; set; }
    public string JobWork { get; set; }
    public string WorkOrder { get; set; }
    public DateTime CompletedDate { get; set; }
    public decimal Amount { get; set; }
    public int OutSourcingStatus { get; set; }
}

public class GetPODetailsSpareParts
{
    [DisplayName("SparePart")]
    public string PartName { get; set; }

    [DisplayName("Part Number")]
    public string PartNumber { get; set; }

    [DisplayName("P.O Quantity")]
    public decimal PoQuantity { get; set; }

    [DisplayName("Last Received Date")]
    public DateTime? LastReceivedDate { get; set; }

    [DisplayName("Received Quantity")]
    public int ReceivedQuantity { get; set; }

    [DisplayName("Pending Quantity")]
    public decimal PendingQuantity { get; set; }

    [DisplayName("P.O Date")]
    public string PoDate { get; set; }

    public string LubricantName { get; internal set; }
    public string LubricantNumber { get; internal set; }
    public int ManufacturerId { get; internal set; }
    public string ManufacturerName { get; internal set; }
    public int SparePartId { get; internal set; }
    public int LubricantId { get; set; }
public string GetLastReceivedDate { get; set; }
    public decimal LubesReceivedQuantity { get; internal set; }
}