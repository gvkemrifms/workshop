using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Fleet_WorkShop.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace Fleet_WorkShop.Controllers
{
    public class VehicleController : Controller
    {
        private readonly EmployeeHelper _helper = new EmployeeHelper();
        private readonly VehicleModel _vehModel = new VehicleModel();
        public string UserName { get; set; }
        // GET: Vehicle
        public ActionResult GetVehicleDetails()
        {
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");
            return RedirectToAction("SaveVehicleDetails");
        }
        public ActionResult SaveVehicleDetails()
        {

            IEnumerable<VehicleModel> vehModel = null;
            if (ModelState.IsValid)
            {

               DataSet dsVehicleDistrictDetails=_helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
                Session["VehicleDistrictDetails"] = dsVehicleDistrictDetails;
                ViewBag.Districts= new SelectList(dsVehicleDistrictDetails.Tables[0].AsDataView(), "Id", "District");
                ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName");
                _vehModel.DistrictId = Convert.ToInt32(Session["Id"]);
                DataSet dsGetVehicles = _helper.FillDropDownHelperMethodWithSp("spGetVehicleDetails");
                Session["GetVehicleDetails"] = dsGetVehicles;
                vehModel = dsGetVehicles.Tables[0].AsEnumerable().ToList().Select(x => new VehicleModel {Id=x.Field<int>("Id"), DistrictName = x.Field<string>("District"), ManufacturerName = x.Field<string>("ManufacturerName"), VehicleNumberString = x.Field<string>("VehicleNumber"), ChasisNumber = x.Field<string>("ChasisNumber"), ModelYear = x.Field<int>("Model"),EngineNumber=x.Field<string>("EngineNumber"), LocationOfCommission = x.Field<string>("LocationOfCommission"), DateOfCommission = x.Field<DateTime?>("DateOfCommission") });
                vehModel.ToList();
            }
            return View(vehModel);

        }
        [HttpPost]
        public int SaveVehicleDetails(VehicleModel vehModel)
        {
            int returnVal=0;
            VehicleModel _vehDetails = new VehicleModel()
            {
                //VehId = Convert.ToInt32(vehModel.VehicleId),
                VehicleNumber = vehModel.VehicleNumber,
                ManufacturerId = Convert.ToInt32(vehModel.ManufacturerId),
                DistrictId = Convert.ToInt32(vehModel.DistrictId),
                DateOfCommission =DateTime.Parse(vehModel.DateOfCommission.ToString()),
                    Model = vehModel.Model,
                    ChasisNumber = vehModel.ChasisNumber,
                    EngineNumber = vehModel.EngineNumber,
                    LocationOfCommission = vehModel.LocationOfCommission
                    
            };
                //returnVal= _helper.ExecuteInsertVehicleDetails("InsetVehicleDetails", _vehDetails.VehId, _vehDetails.ManufacturerId, _vehDetails.DistrictId, _vehDetails.Model, _vehDetails.ChasisNumber, _vehDetails.EngineNumber, _vehDetails.LocationOfCommission);
            returnVal = _helper.ExecuteInsertVehicleDetails("InsetVehicleDetails", _vehDetails.VehicleNumber, _vehDetails.ManufacturerId, _vehDetails.DistrictId, _vehDetails.Model, _vehDetails.ChasisNumber, _vehDetails.EngineNumber, _vehDetails.LocationOfCommission,_vehDetails.DateOfCommission);


            return returnVal;
        }
        //[HttpPost]
        //public ActionResult GetDistrictIds(string districtId)
        //{
        //    string list = "";
        //    if(ModelState.IsValid)
        //    {
        //        _vehModel.DistrictId = int.Parse(districtId);
        //        Session["Id"] = _vehModel.DistrictId;
        //        DataSet dsFillVehiclesByDistrict = _helper.FillDropDownHelperMethodWithSp("spFillVehicles", _vehModel.DistrictId);
        //        List<DataRow> data = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().ToList();
        //        List<string> names = new List<string>();
        //        foreach (DataRow row in data)
        //        {
        //            _vehModel.VehicleNumberString = row["vehiclenumber"].ToString();
        //            _vehModel.VehicleId = row["Id"].ToString();
        //            names.Add(_vehModel.VehicleNumberString + "-" + _vehModel.VehicleId);
        //        }
        //         list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings()
        //        {
        //            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        //        });
               
        //    }
        //    return Content(list, "application/json");

        //}
        [HttpGet]
        public ActionResult Edit(int? Id = null)
        {
            if (Id == null)
            {
                return RedirectToAction("SaveVehicleDetails");
            }
            DataSet dsGetVehicleManufacturers = Session["VehicleDistrictDetails"] as DataSet;
            DataSet dsEditEmployee = Session["GetVehicleDetails"] as DataSet;
            DataRow row = dsEditEmployee.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == Id);
          //  string query = "select v.id, v.vehiclenumber,v.districtId from tblvehicles v join tbldistricts d on v.districtId=d.Id where v.vehiclenumber = '" + row["VehicleNumber"]+"'";
          //DataTable dtVehicle=  _helper.ExecuteSelectStmt(query);
          //  string fillVehicleDropQuery = "select * from tblvehicles where districtId=" + dtVehicle.Rows[0][2].ToString() + "";
          //  DataTable dtVehicledet = _helper.ExecuteSelectStmt(fillVehicleDropQuery);
            VehicleModel model = new VehicleModel()
            {

                District = new SelectList(dsGetVehicleManufacturers.Tables[0].AsDataView(), "Id", "District"),
                DistrictName = row["District"].ToString(),
                Manufacturer = new SelectList(dsGetVehicleManufacturers.Tables[1].AsDataView(), "Id", "ManufacturerName"),
                //VehicleNumber=new SelectList(dtVehicledet.AsDataView(),"id","VehicleNumber"),
                ManufacturerName = row["ManufacturerName"].ToString(),
                VehicleNumberString=row["VehicleNumber"].ToString(),
                //VehId= Convert.ToInt32(dtVehicle.Rows[0][0].ToString()),
                EngineNumber = row["EngineNumber"].ToString(),
                ModelYear = Convert.ToInt32(row["Model"]),
                ChasisNumber = row["ChasisNumber"].ToString(),
                DateOfCommission=Convert.ToDateTime(row["DateOfCommission"]),
                LocationOfCommission = row["LocationOfCommission"].ToString()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(VehicleModel postVehicle)
        {

            //_helper.ExecuteVehicleUpdateStatement(postVehicle.Id, "spEditVehicle", postVehicle.DistrictId, postVehicle.ManufacturerId, postVehicle.VehId, postVehicle.ModelYear, postVehicle.ChasisNumber, postVehicle.EngineNumber, postVehicle.LocationOfCommission);
            _helper.ExecuteVehicleUpdateStatement(postVehicle.Id, "spEditVehicle", postVehicle.DistrictId, postVehicle.ManufacturerId, postVehicle.VehicleNumberString, postVehicle.ModelYear, postVehicle.ChasisNumber, postVehicle.EngineNumber, postVehicle.LocationOfCommission,postVehicle.DateOfCommission);
            return RedirectToAction("SaveVehicleDetails");
            //return RedirectToAction("SaveEmployeeDetails");
        }

        [HttpDelete]
        public ActionResult Delete(int Id)
        {
            _helper.ExecuteDeleteStatement("spDeleteVehicle", Id);
            return RedirectToAction("SaveVehicleDetails");
        }

    }
    }
