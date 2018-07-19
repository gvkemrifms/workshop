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
        public string AllotedMechanic { get; set; }
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
    public string AllotedMechanic { get; set; }
    [DisplayName("Service Incharge")]
    public string ServiceEngineer { get; set; }

}
