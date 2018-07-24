using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Fleet_WorkShop.Models;
using System.Data;
using Newtonsoft.Json;

namespace Fleet_WorkShop.Controllers
{
    public class JobController : Controller
    {
        private readonly EmployeeHelper _helper = new EmployeeHelper();
        private readonly VehicleModel _vehModel = new VehicleModel();
        // GET: Job
        public ActionResult GetJobCardDetails()
        {
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");
            return RedirectToAction("SaveJobCardDetails");
        }
       
        public ActionResult SaveJobCardDetails()
        {
            if(Session["WorkshopId"]==null)
                return RedirectToAction("Login", "Account");
            IEnumerable<VehicleModel> model = null;
            //IEnumerable<VehicleJobCardModel> vehModel = null;
            if (ModelState.IsValid)
            {
                DataSet dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
                Session["VehicleDistrictDetails"] = dsVehicleDistrictDetails;
                ViewBag.Districts = new SelectList(dsVehicleDistrictDetails.Tables[0].AsDataView(), "Id", "District");
                ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName");
                ViewBag.NatureOfComplaint = new SelectList(dsVehicleDistrictDetails.Tables[2].AsDataView(), "Id", "Complaint");
                ViewBag.AllotedMechanic= new SelectList(dsVehicleDistrictDetails.Tables[3].AsDataView(), "empId", "name");
                ViewBag.ServiceEngineer = new SelectList(dsVehicleDistrictDetails.Tables[4].AsDataView(), "empId", "name");
                _vehModel.DistrictId = Convert.ToInt32(Session["Id"]);
                DataSet dsGetJobCardDetails = _helper.FillDropDownHelperMethodWithSp("spGetJobCardDetails");
                Session["GetJobCardDetails"] = dsGetJobCardDetails;
                model = dsGetJobCardDetails.Tables[0].AsEnumerable().ToList().Select(x => new VehicleModel {Id=x.Field<int>("JobCardNumber"), DistrictName = x.Field<string>("District"),NatureOfComplaint=x.Field<int>("NatureOfComplaint"), VehicleId = x.Field<string>("vehicleNumber"), DateOfDelivery = x.Field<DateTime>("Dor"), ModelNumber = x.Field<int>("Model"), Odometer = x.Field<int>("Odometer"), PilotName = x.Field<string>("PilotName"), ApproximateCost = x.Field<int>("ApproxCost"),AllotedMechanicName= x.Field<string>("employeeName") });
             
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveJobCardDetails(VehicleJobCardModel model)
        {
        
            int returnVal = 0;
            VehicleJobCardModel _vehDetails = new VehicleJobCardModel()
            {
               WorkShopId= Convert.ToInt32(Session["WorkshopId"]),
               DistrictId  =model.DistrictId,
               VehId =model.VehId,
               DateOfDelivery=model.DateOfDelivery,
               ApproximateCost=model.ApproximateCost,
               ModelNumber=model.ModelNumber,
              NatureOfComplaint =model.NatureOfComplaint,
              Odometer=model.Odometer,
             PilotId =model.PilotId,
             PilotName=model.PilotName,
              ReceivedLocation = model.ReceivedLocation,
              DateOfRepair=model.DateOfRepair,
              AllotedMechanic=model.AllotedMechanic,
              ServiceEngineer=model.ServiceEngineer
              

            };
            returnVal = _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", _vehDetails.DistrictId, _vehDetails.VehId, _vehDetails.DateOfRepair, _vehDetails.ModelNumber, _vehDetails.Odometer, _vehDetails.ReceivedLocation, _vehDetails.PilotId, _vehDetails.PilotName,_vehDetails.DateOfDelivery,_vehDetails.NatureOfComplaint,_vehDetails.ApproximateCost,Convert.ToInt32(_vehDetails.AllotedMechanic),_vehDetails.WorkShopId,Convert.ToInt32(_vehDetails.ServiceEngineer));
            return RedirectToAction("SaveJobCardDetails");
        }

        [HttpPost]
        public ActionResult GetDistrictIds(string districtId)
        {
            string list = "";
            if (ModelState.IsValid)
            {
                _vehModel.DistrictId = int.Parse(districtId);
                Session["Id"] = _vehModel.DistrictId;
                DataSet dsFillVehiclesByDistrict = _helper.FillDropDownHelperMethodWithSp("spFillVehicleDetails", _vehModel.DistrictId);
                List<DataRow> data = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().ToList();
                List<string> names = new List<string>();
                foreach (DataRow row in data)
                {
                    _vehModel.VehicleNumberString = row["vehiclenumber"].ToString();
                    _vehModel.VehicleId = row["Id"].ToString();
                    names.Add(_vehModel.VehicleNumberString + "-" + _vehModel.VehicleId);
                }
                list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });

            }
            return Content(list, "application/json");

        }

        [HttpPost]
        public ActionResult GetModelNumber(string vehicleid)
        {
            int list = 0;
            if (ModelState.IsValid)
            {
                _vehModel.VehId = int.Parse(vehicleid);
                DataSet dsFillVehiclesByDistrict = _helper.FillModelNumbers("spGetVehicleModelNumber", _vehModel.VehId);
                list = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().Select(x => x.Field<int>("model")).FirstOrDefault();
                
                //list = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings()
                //{
                //    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                //});

            }
            return Content(list.ToString(), "application/json");

        }

        public ActionResult Edit(int? Id = null)
        {
            if (Id == null)
            {
                return RedirectToAction("SaveJobCardDetails");
            }
            DataSet dsGetVehicleManufacturers = Session["VehicleDistrictDetails"] as DataSet;
            DataSet dsGetJobCardDetails = Session["GetJobCardDetails"] as DataSet;
            DataRow row = dsGetJobCardDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("JobCardNumber") == Id);
              string query = "select v.id, v.vehiclenumber,v.districtId from m_GetVehicleDetails v join m_districts d on v.districtId=d.Id where v.vehiclenumber = '" + row["vehicleNumber"] +"'";
            DataTable dtVehicle=  _helper.ExecuteSelectStmt(query);
              string fillVehicleDropQuery = "select * from m_GetVehicleDetails where districtId=" + dtVehicle.Rows[0][2].ToString() + "";
              DataTable dtVehicledet = _helper.ExecuteSelectStmt(fillVehicleDropQuery);
            VehicleModel model = new VehicleModel()
            {
                District = new SelectList(dsGetVehicleManufacturers.Tables[0].AsDataView(), "Id", "District"),
                ComplaintsNature= new SelectList(dsGetVehicleManufacturers.Tables[2].AsDataView(), "Id", "Complaint"),
                DistrictName = row["District"].ToString(), 
                NatureOfComplaint=Convert.ToInt32(row["NatureOfComplaint"]),              
                Vehicle=new SelectList(dtVehicledet.AsDataView(),"Id","VehicleNumber"),
                VehicleNumberString = row["VehicleNumber"].ToString(),
                VehId= Convert.ToInt32(dtVehicle.Rows[0][0].ToString()),
                DistrictId = Convert.ToInt32(dtVehicle.Rows[0][2].ToString()),
                DateOfRepair =Convert.ToDateTime(row["Dor"]),
            ModelYear=Convert.ToInt32(row["Model"]),
            Odometer=Convert.ToInt32(row["Odometer"]),
            ApproximateCost=Convert.ToInt32(row["ApproxCost"])
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(VehicleModel postVehicle)
        {
            _helper.ExecuteJobUpdateStatement(postVehicle.DistrictId, "spEditJobcardDetails", postVehicle.VehId, postVehicle.ModelYear, postVehicle.Odometer, postVehicle.ApproximateCost,postVehicle.DateOfRepair,postVehicle.NatureOfComplaint);
            return RedirectToAction("SaveJobCardDetails");
        }

  [HttpDelete]
        public ActionResult Delete(int? Id)
        {
            _helper.ExecuteDeleteStatement("spDeleteJobCardDetails", Id);
            return RedirectToAction("SaveJobCardDetails");
        }

    }
}