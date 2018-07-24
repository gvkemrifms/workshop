using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Fleet_WorkShop.Models
{
    public class VehicleModel
    {
        [DisplayName("Mechanic Name")]
        public string AllotedMechanicName { get; set; }
   
        public SelectList Vehicle { get; set; }
        public int Id { get; set; }
        [DisplayName("Vehicle")]
        public string VehicleId { get; set; }
        public int VehId { get; set; }
        [DisplayName("Vehicle Number")]
        public string VehicleNumberString { get; set; }
        public int Odometer { get; set; }
        [DisplayName("Pilot Name")]
        public string PilotName { get; set; }

        [DisplayName("Approximate Cost")]
        public int ApproximateCost { get; set; }

        [DisplayName("Date Of Recovery")]
        public DateTime DateOfDelivery { get; set; }
        [DisplayName("Nature Of Complaint")]
        public int NatureOfComplaint { get; set; }
        public SelectList ComplaintsNature { get; set; }
        public string PilotId { get; set; }

        [DisplayName("Date Of Repair")]
        public DateTime DateOfRepair { get; set; }
        public int DistrictId { get; set; }
        [DisplayName("District")]
        public string DistrictName { get; set; }
        public int ManufacturerId { get; set; }
        [DisplayName("Manufacturer Name")]
        public string ManufacturerName { get; set; }

        [Required(ErrorMessage = "Please select District")]
        [DisplayName("District")]
        public SelectList District { get; set; }

        [Required(ErrorMessage = "Please Enter Location")]
        [DisplayName("LOC")]
        public string  LocationOfCommission { get; set; }
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

        [Required(ErrorMessage = "Manufacturer Name is Required")]
        public SelectList Manufacturer { get; set; }   
       public DateTime? DateOfCommission { get; set; }  
        public string DOC { get; set; }
        public string ReceivedLocation { get; internal set; }

        public IEnumerable<VehicleJobCardModel> VehicleJodcard { get; set; }

        [DisplayName("Model")]
        public int ModelNumber { get; internal set; }
        [DisplayName("Alloted Mechanic")]
        public int AllotedMechanic { get; set; }
        public int JobCardId { get; set; }
        public int HandOverTo { get; set; }
        public List<JobCardPendingCases> itemmodel { get; set; }
    }
}


public class VehicleJobCardModel
{
    public int WorkShopId { get; set; }
    [DisplayName("Vehicle Id")]
    public string VehicleId { get; set; }
    public int VehId { get; set; }
    public int ModelNumber { get; set; }
    public int Odometer { get; set; }
    public string PilotName { get; set; }
    public int ApproximateCost { get; set; }
    public DateTime DateOfDelivery { get; set; }
    public int NatureOfComplaint { get; set; }
    public string PilotId { get; set; }
    public DateTime DateOfRepair { get; set; }
    public int DistrictId { get; set; }
    public string ReceivedLocation { get; set; }
    public string DistrictName { get; set; }

    public SelectList Vehicle { get; set; }
    [Required(ErrorMessage = "Please select District")]
    [DisplayName("District")]
    public SelectList District { get; set; }

    [DisplayName("Alloted Mechanic")]
    public int AllotedMechanic { get; set; }
    [DisplayName("Service Incharge")]
    public int ServiceEngineer { get; set; }

}


public class JobCardPendingCases
{
    public Guid VehicleId { get; set; }
    public string VehicleNumber { get; set; }
public string DistrictName { get; set; }
public DateTime DateOfRepair { get; set; }
public string Complaint { get; set; }
public string WorkShopName { get; set; }
    public string EmployeeName { get; set; }
    public string Status { get; set; }
    public int SparePartId { get; set; }
    public int Quantity { get; set; }
    public int HandOverToId { get; set; }
    public int JobCardNumber { get; set; }
    public SelectList Vehicle { get; set; }
}
