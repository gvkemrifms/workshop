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
        public ActionResult GetLubesPendingStatusDetails()
        {
            IEnumerable<JobCardPendingCases> LubespendingCases = new List<JobCardPendingCases>();
            DataTable dtLubesPendingStatus = _helper.ExecuteSelectStmt("spGetVehiclesWithPendingStatus");
            LubespendingCases = dtLubesPendingStatus.AsEnumerable().Select(x => new JobCardPendingCases { VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair"), Complaint = x.Field<string>("Complaint"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("employeeName"), Status = x.Field<string>("status"), });
            Session["LubesPendingStatus"] = dtLubesPendingStatus;
            //DataTable dtSpareParts = _helper.ExecuteSelectStmtusingSP("spGetSpares");
            //Session["getSpares"] = dtSpareParts;
            //ViewBag.SpareParts =  new SelectList(dtSpareParts.AsDataView(), "Id", "PartName");
            DataTable dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails");
            ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "id", "Designation");
            return View(LubespendingCases);
        }
        public PartialViewResult EditLubesPendingStatusDetails(Guid? id)
        {
            IEnumerable<JobCardPendingCases> LubespendingCases = new List<JobCardPendingCases>();
            if (id != null)
            {
                DataTable dtLubePendingStatus = Session["LubesPendingStatus"] as DataTable;
                LubespendingCases = dtLubePendingStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id).Select(x => new JobCardPendingCases { VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair").Date, Complaint = x.Field<string>("Complaint"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("employeeName"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber") });
                Session["workshopName"] = LubespendingCases.Select(x => x.WorkShopName).FirstOrDefault();

            }
            DataTable dtVehicleLubes = _helper.ExecuteSelectStmtusingSP("spGetVehicleLubricantDetails", null, null, null, null, "@vehicleNumber", LubespendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["spGetVehicleLubricantDetails"] = dtVehicleLubes;
            ViewBag.SpareParts = new SelectList(dtVehicleLubes.AsDataView(), "Id", "OilName");
         
            Session["JobCardNumber"] = LubespendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
            return PartialView("EditLubesPendingStatusDetails", LubespendingCases);
        }
        public ActionResult CheckVehicleNumber(string vehicleNumber)
        {
            string vehicleNumberStringLower = vehicleNumber.ToLower().ToString();
          DataTable dtCheckVehicles=  _helper.ExecuteSelectStmtusingSP("spCheckVehicleDetails", null, null, null, null, "@vehiclenumber", vehicleNumber);
            if(dtCheckVehicles.Rows.Count==0)
            {
                return null;
            }
            return Json(dtCheckVehicles.Rows.Count, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPendingStatusDetails()
        {
            IEnumerable<JobCardPendingCases> pendingCases = new List<JobCardPendingCases>();
            DataTable dtPendingStatus= _helper.ExecuteSelectStmt("spGetVehiclesWithPendingStatus");
            pendingCases = dtPendingStatus.AsEnumerable().Where(x=>x.Field<long>("num")==1).Select(x => new JobCardPendingCases { VehicleId= x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"),DistrictName= x.Field<string>("District"),DateOfRepair= x.Field<DateTime>("DateOfRepair"),Complaint= x.Field<string>("ServiceGroup_Name"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("Name"), Status = x.Field<string>("status"),JobCardNumber=x.Field<int>("JobCardNumber") });
            Session["PendingStatus"] = dtPendingStatus;
            //DataTable dtSpareParts = _helper.ExecuteSelectStmtusingSP("spGetSpares");
            //Session["getSpares"] = dtSpareParts;
            //ViewBag.SpareParts =  new SelectList(dtSpareParts.AsDataView(), "Id", "PartName");
            DataTable dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails");
            ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "id", "Designation");
            return View(pendingCases);
        }
        public PartialViewResult EditPendingStatusDetails(Guid? id)
        {
            IEnumerable<JobCardPendingCases> pendingCases = new List<JobCardPendingCases>();
            if (id!=null)
            {
                DataTable dtPendingStatus = Session["PendingStatus"] as DataTable;
                pendingCases =  dtPendingStatus.AsEnumerable().Where(x=>x.Field<Guid>("Id") == id).Select(x => new JobCardPendingCases { VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair").Date, Complaint = x.Field<string>("Complaint"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("employeeName"), Status = x.Field<string>("status"),JobCardNumber= x.Field<int>("JobCardNumber") });
              Session["workshopName"]=  pendingCases.Select(x => x.WorkShopName).FirstOrDefault();

            }
            DataTable dtVehicleSpareParts = _helper.ExecuteSelectStmtusingSP("spGetVehicleSpares",null,null,null,null, "@vehicleNumber",pendingCases.Select(x=>x.VehicleNumber).FirstOrDefault());
            Session["getVehicleSpares"] = dtVehicleSpareParts;
            ViewBag.SpareParts = new SelectList(dtVehicleSpareParts.AsDataView(), "Id", "PartName");
            Session["JobCardNumber"] = pendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
            return PartialView("_EditPendingStatusDetails",pendingCases);
        }


        public ActionResult EditPendingStatusDetails1(Guid? id)
        {
            decimal? OutSourcingAmount = 0;
            IEnumerable<JobCardPendingCases> pendingCases = new List<JobCardPendingCases>();
            if (id != null)
            {
                DataTable dtPendingStatus = Session["PendingStatus"] as DataTable;
                pendingCases = dtPendingStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id).Select(x => new JobCardPendingCases { VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair").Date, Complaint = x.Field<string>("ServiceGroup_Name"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("Name"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber"), OutSourcingAmount= x.Field<decimal?>("Amount") });
                Session["workshopName"] = pendingCases.Select(x => x.WorkShopName).FirstOrDefault();
                //ViewBag.VehicleNumber= dtPendingStatus.AsEnumerable().Select(x => x.Field<string>("VehicleNumber")).FirstOrDefault();
                ViewBag.VehicleNumber = pendingCases.Select(x => x.VehicleNumber).FirstOrDefault(); ;
                //ViewBag.DateOfRepair= dtPendingStatus.AsEnumerable().Select(x => x.Field<DateTime>("DateOfRepair")).FirstOrDefault();
                ViewBag.DateOfRepair = pendingCases.Select(x => x.DateOfRepair).FirstOrDefault(); ;
                //ViewBag.WorkShopName= dtPendingStatus.AsEnumerable().Select(x => x.Field<string>("workshop_name")).FirstOrDefault();
                ViewBag.WorkShopName = pendingCases.Select(x => x.WorkShopName).FirstOrDefault(); ;
                //ViewBag.Mechanic= dtPendingStatus.AsEnumerable().Select(x => x.Field<string>("Name")).FirstOrDefault();
                ViewBag.Mechanic = pendingCases.Select(x => x.EmployeeName).FirstOrDefault(); 
                ViewBag.OutSourcingAmount = pendingCases.Select(x => x.OutSourcingAmount).FirstOrDefault(); 

            }
            else
            {
                return RedirectToAction("GetPendingStatusDetails");
            }
           
              DataTable dtVehicleSpareParts = _helper.ExecuteSelectStmtusingSP("spGetVehicleSpares", null, null, null, null, "@vehicleNumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["getVehicleSpares"] = dtVehicleSpareParts;
            ViewBag.SpareParts = new SelectList(dtVehicleSpareParts.AsDataView(), "Id", "PartName");
            DataTable dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails");
            ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "empId", "Name");
            DataTable dtVehicleLubes = _helper.ExecuteSelectStmtusingSP("spGetVehicleLubricantDetails", null, null, null, null, "@vehicleNumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["spGetVehicleLubricantDetails"] = dtVehicleLubes;
            ViewBag.Lubes = new SelectList(dtVehicleLubes.AsDataView(), "Id", "OilName");
            Session["JobCardNumber"] = pendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
           ViewBag.VehicleNumber= pendingCases.Select(x => x.VehicleNumber).FirstOrDefault();
            string vehicleNumber= pendingCases.Select(x => x.VehicleNumber).FirstOrDefault();
            DataTable dtgetComplaints = _helper.ExecuteSelectStmtusingSP("sp_getvehiclecomplaints", null, null, null,null, "@vehicleno", vehicleNumber);
            //------------------------------
            DataTable dtvehicleInfoOnVehicleNumber = _helper.ExecuteSelectStmtusingSP("getVehicleInfoOnVehicleNumber", null, null, null, null, "@vehiclenumber", vehicleNumber);
            Session["VehicleInfoByNumber"] = dtvehicleInfoOnVehicleNumber;
            int vehicleId = dtvehicleInfoOnVehicleNumber.AsEnumerable().Select(x => x.Field<int>("VehicleId")).FirstOrDefault();
            Session["VehicleId"] = vehicleId;
            DataTable dtTotalCost = _helper.ExecuteSelectStmtusingSP("getTotalCostForVehicleNumber", "@vehicleid", vehicleId.ToString());
            int totalCost = dtTotalCost.AsEnumerable().Select(x => x.Field<int>("TotalCost")).FirstOrDefault();
            ViewBag.TotalCost = totalCost;
            //--------------------------------------------------------------------------------
            pendingCases =  dtgetComplaints.AsEnumerable().Select(x => new JobCardPendingCases { VehicleIdData = x.Field<int>("VehicleId"), VehicleNumberData = x.Field<string>("VehicleNumber"), CategoryData = x.Field<string>("Categories"), CostApproximate = x.Field<int>("ApproxCost"),DateOfRepair= x.Field<DateTime>("Dor"),AggregateName= x.Field<string>("Aggregates"),SubCategoryName= x.Field<string>("Service_Name") });
           
            return View(pendingCases);
        }

        public ActionResult GetOutSourcingJobDetails(OutSourcingJobDetails outsourcing)
        {
            int vehicleId = Convert.ToInt32(Session["VehicleId"]);
            int jobCardNumber = Convert.ToInt32(Session["JobCardNumber"]);
           int returnVal= _helper.ExecuteUpdateOutSourcingJobDetails(vehicleId, "UpdateOutSoiurcingVehicleDetails", outsourcing.Vendor, outsourcing.WorkOrder, outsourcing.JobWork, outsourcing.CompletedDate, outsourcing.OutSourcingStatus,outsourcing.Amount);


            return Json(returnVal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddIssues(string vehicleNumber)
        {
            if (vehicleNumber == null)
            {
                return RedirectToAction("GetPendingStatusDetails");
            }
           DataTable dtvehicleInfoOnVehicleNumber= _helper.ExecuteSelectStmtusingSP("getVehicleInfoOnVehicleNumber", null, null, null, null, "@vehiclenumber", vehicleNumber);
            Session["VehicleInfoByNumber"] = dtvehicleInfoOnVehicleNumber;
            int vehicleId = dtvehicleInfoOnVehicleNumber.AsEnumerable().Select(x => x.Field<int>("VehicleId")).FirstOrDefault();
            DataTable dtTotalCost = _helper.ExecuteSelectStmtusingSP("getTotalCostForVehicleNumber", "@vehicleid", vehicleId.ToString());
            int totalCost = dtTotalCost.AsEnumerable().Select(x => x.Field<int>("TotalCost")).FirstOrDefault();
           
            var manufacturerId = dtvehicleInfoOnVehicleNumber.AsEnumerable().Select(x => new { ManufacturerId = x.Field<int>("ManufacturerId") }).FirstOrDefault();
            DataSet dsFillAggregatesForManufacturer = _helper.FillDropDownHelperMethodWithSp("getAggeregatesByManufacturers", manufacturerId.ManufacturerId);
            ViewBag.Aggregates= new SelectList(dsFillAggregatesForManufacturer.Tables[0].AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");
            return View();
        }
        [HttpPost]
        public ActionResult AddIssues(int Aggregates,int Categories,int SubCategories)
        {
            DataTable dtVehicleInfo = Session["VehicleInfoByNumber"] as DataTable;
            string costQuery = "select [CostFor_A_Grade] as [EstimatedCost] from dbo.M_FMS_MaintenanceWorksMasterDetails where Service_id = "+ SubCategories+"";
            DataTable dtCostQuery=_helper.ExecuteSelectStmt(costQuery);
            int returnVal = 0;
           
            VehicleModel _vehDetails = new VehicleModel()
            {
                DistrictId=dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("DistrictId")).FirstOrDefault(),
                VehId= dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("VehicleId")).FirstOrDefault(),
                DateOfRepair= dtVehicleInfo.AsEnumerable().Select(x => x.Field<DateTime>("Dor")).FirstOrDefault(),
                ModelNumber= dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("Model")).FirstOrDefault(),
                Odometer= dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("Odometer")).FirstOrDefault(),
                ReceivedLocation= dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("ReceivedLoc")).FirstOrDefault(),
                PilotId= dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("PilotId")).FirstOrDefault(),
                PilotName= dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("PilotName")).FirstOrDefault(),
                ApproximateCost= dtCostQuery.AsEnumerable().Select(x => x.Field<int>("EstimatedCost")).FirstOrDefault(),
                AllotedMechanic= dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("AllotedMechanic")).FirstOrDefault(),
                WorkShopId= dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("WorkshopId")).FirstOrDefault(),
                ServiceEngineer= dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("ServiceIncharge")).FirstOrDefault(),
                LaborCharges= dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("LaborCharges")).FirstOrDefault(),
                ManufacturerId= dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("ManufacturerId")).FirstOrDefault(),
                Status= dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("Status")).FirstOrDefault(),
                DateOfDelivery= dtVehicleInfo.AsEnumerable().Select(x => x.Field<DateTime>("DateOfDelivery")).FirstOrDefault(),
                AggregateId = Aggregates,
                IdCategory = Categories,
                SubCategory= SubCategories
            };
            returnVal = _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", _vehDetails.DistrictId, _vehDetails.VehId, _vehDetails.DateOfRepair, _vehDetails.ModelNumber, _vehDetails.Odometer, _vehDetails.ReceivedLocation, _vehDetails.PilotId, _vehDetails.PilotName, _vehDetails.DateOfDelivery, _vehDetails.AggregateId, _vehDetails.ApproximateCost, _vehDetails.AllotedMechanic, _vehDetails.WorkShopId, _vehDetails.ServiceEngineer, _vehDetails.LaborCharges, _vehDetails.IdCategory,_vehDetails.SubCategory, _vehDetails.ManufacturerId);
            
            return Json(returnVal, JsonRequestBehavior.AllowGet); ;
        }
        public ActionResult GetPreviousRepairs(string vehicleNumber)
        {
            if(vehicleNumber==null)
            {
                return RedirectToAction("GetPendingStatusDetails");
            }
            JobCardPendingCases completedCases = new JobCardPendingCases();
           DataTable dtGetVehiclePreviousRepairDetails= _helper.ExecuteSelectStmtusingSP("getVehiclePreviousRepairs",null,null,null,null,"@vehiclenumber", vehicleNumber);
            var list = dtGetVehiclePreviousRepairDetails.AsEnumerable().Select(x => new JobCardPendingCases { JobCardNumber = x.Field<int>("JobCardNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair"), AggregateName = x.Field<string>("ServiceGroup_Name"), LaborCharges = x.Field<int>("laborcharges"),WorkShopName= x.Field<string>("workshop_name"),Status= x.Field<string>("Status"),VehicleNumber=vehicleNumber });
            return View(list);
        }
        public ActionResult SaveCalculateFIFO(VehicleModel pendingCases,string status)
        {
            var pendingStatusSpares = Session["pendingStatusSpareDetails"] as List<JobCardPendingCases>; 
           
           foreach(var i in pendingStatusSpares)
            {
                pendingCases.itemmodel.Add(i);
            }
            int result = 0;
            string workshopName = Session["workshopName"].ToString();
            string query = "select workshop_id from m_workshop where workshop_name='" + workshopName + "'";
            DataTable dtWorkShopId = _helper.ExecuteSelectStmt(query);
            pendingCases.WorkShopId = dtWorkShopId.AsEnumerable().Select(x => x.Field<int>("workshop_id")).FirstOrDefault();
            pendingCases.JobCardId = Convert.ToInt32(Session["JobCardNumber"]);
            DataTable dtGetPartNumber = Session["getVehicleSpares"] as DataTable;
            var spares = dtGetPartNumber.AsEnumerable().Select(x => new { PartName = x.Field<string>("partnumber"), PartId = x.Field<int>("Id") });
            foreach (var itemm in pendingCases.itemmodel)
            {
                foreach (var spare in spares)
                {
                    if (itemm.SparePartId == spare.PartId)
                    {
                        DataTable dtcostDetails = _helper.ExecuteSelectStmtusingSP("GetCostBySparePartNumber", null, null, null, null, "@partnumber", spare.PartName);
                        int sumOfQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).Sum();
                        if (itemm.Quantity <= sumOfQuantity)
                        {
                            if (itemm.Quantity <= dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).FirstOrDefault())
                            {
                                decimal Cost = dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("Cost")).FirstOrDefault();
                                decimal totalAmount = Cost * itemm.Quantity;
                                int res = _helper.ExecuteInsertSparesIssueStatement("InsertSpareIssueDetails", itemm.VehicleNumber, pendingCases.WorkShopId, itemm.SparePartId, itemm.Quantity, totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status);
                                if (res == 1)
                                {
                                    int itemTotalQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).FirstOrDefault();
                                    long receiptId = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
                                    int updatedQuantity = itemTotalQuantity - itemm.Quantity;

                                    _helper.ExecuteUpdateSparesIssueStatement("UpdateSpareIssueQuantityDetails", receiptId, updatedQuantity);
                                    return Json(res, JsonRequestBehavior.AllowGet);
                                }

                            }
                            else
                            {
                                int count = dtcostDetails.Rows.Count;
                                foreach (DataRow row in dtcostDetails.Rows)
                                {
                                    if (Convert.ToInt32(itemm.Quantity) != 0)
                                    {
                                        decimal Cost = Convert.ToDecimal(row["Cost"]);
                                        decimal totalAmount = 0;
                                        int Qty = 0;
                                        if (itemm.Quantity <= Convert.ToInt32(row["Quantity"]))
                                        {
                                            totalAmount = Cost * itemm.Quantity;
                                            Qty = itemm.Quantity;
                                        }
                                        else
                                        {
                                            totalAmount = Cost * Convert.ToInt32(row["Quantity"]);
                                            Qty = Convert.ToInt32(row["Quantity"]);
                                        }
                                       
                                        int res = _helper.ExecuteInsertSparesIssueStatement("InsertSpareIssueDetails", itemm.VehicleNumber, pendingCases.WorkShopId, itemm.SparePartId, Qty, totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status);
                                        if (res == 1)
                                        {
                                            int itemTotalQuantity = Convert.ToInt32(row["Quantity"]);
                                            long receiptId = Convert.ToInt32(row["Id"]);
                                            int updatedQuantity = itemTotalQuantity - itemm.Quantity;
                                            int remainingQuantity = updatedQuantity;
                                            if (updatedQuantity <= 0)
                                                updatedQuantity = 0;

                                           result = _helper.ExecuteUpdateSparesIssueStatement("UpdateSpareIssueQuantityDetails", receiptId, updatedQuantity);
                                           
                                            if (result == 1)
                                            {
                                                if(remainingQuantity<=0)
                                                itemm.Quantity = -(remainingQuantity);
                                               
                                            }
                                            return Json(res, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }               
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCalculateLubesFIFO(VehicleModel pendingCases, string status=null)
        {
            int result = 0;
            string workshopName = Session["workshopName"].ToString();
            string query = "select workshop_id from m_workshop where workshop_name='" + workshopName + "'";
            DataTable dtWorkShopId = _helper.ExecuteSelectStmt(query);
            pendingCases.WorkShopId = dtWorkShopId.AsEnumerable().Select(x => x.Field<int>("workshop_id")).FirstOrDefault();
            pendingCases.JobCardId = Convert.ToInt32(Session["JobCardNumber"]);
            DataTable dtGetLubricantNumber = Session["spGetVehicleLubricantDetails"] as DataTable;
            var spares = dtGetLubricantNumber.AsEnumerable().Select(x => new { PartName = x.Field<string>("LubricantNumber"), PartId = x.Field<int>("Id") });
            foreach (var itemm in pendingCases.itemmodel)
            {
                foreach (var spare in spares)
                {
                    if (itemm.LubricantId == spare.PartId)
                    {
                        DataTable dtcostDetails = _helper.ExecuteSelectStmtusingSP("GetCostByLubricantNumber", null, null, null, null, "@partnumber", spare.PartName);
                        int sumOfQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).Sum();
                        if (itemm.Quantity <= sumOfQuantity)
                        {
                            if (itemm.Quantity <= dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).FirstOrDefault())
                            {
                                decimal Cost = dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("CostPerLitre")).FirstOrDefault();
                                decimal totalAmount = Cost * itemm.Quantity;
                                int res = _helper.ExecuteInsertSparesIssueStatement("InsertLubeIssueDetails", itemm.VehicleNumber, pendingCases.WorkShopId, itemm.LubricantId, itemm.Quantity, totalAmount, itemm.HandOverToId, pendingCases.JobCardId,status);
                                if (res == 1)
                                {
                                    int itemTotalQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).FirstOrDefault();
                                    long receiptId = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
                                    int updatedQuantity = itemTotalQuantity - itemm.Quantity;

                                    _helper.ExecuteUpdateSparesIssueStatement("UpdateLubesQuantityIssueDetails", receiptId, updatedQuantity);
                                }

                            }
                            else
                            {
                                int count = dtcostDetails.Rows.Count;
                                foreach (DataRow row in dtcostDetails.Rows)
                                {
                                    if (Convert.ToInt32(itemm.Quantity) != 0)
                                    {
                                        decimal Cost = Convert.ToDecimal(row["CostPerLitre"]);
                                        decimal totalAmount = 0;
                                        int Qty = 0;
                                        if (itemm.Quantity <= Convert.ToInt32(row["Quantity"]))
                                        {
                                            totalAmount = Cost * itemm.Quantity;
                                            Qty = itemm.Quantity;
                                        }
                                        else
                                        {
                                            totalAmount = Cost * Convert.ToInt32(row["Quantity"]);
                                            Qty = Convert.ToInt32(row["Quantity"]);
                                        }

                                        int res = _helper.ExecuteInsertLubesIssueStatement("InsertLubesIssueDetails", itemm.VehicleNumber, pendingCases.WorkShopId, itemm.LubricantId, Qty, totalAmount, itemm.HandOverToId, pendingCases.JobCardId,status);
                                        if (res == 1)
                                        {
                                            int itemTotalQuantity = Convert.ToInt32(row["Quantity"]);
                                            long receiptId = Convert.ToInt32(row["Id"]);
                                            int updatedQuantity = itemTotalQuantity - itemm.Quantity;
                                            int remainingQuantity = updatedQuantity;
                                            if (updatedQuantity <= 0)
                                                updatedQuantity = 0;

                                            result = _helper.ExecuteUpdateSparesIssueStatement("UpdateLubesIssueQuantityDetails", receiptId, updatedQuantity);
                                            if (result == 1)
                                            {
                                                if (remainingQuantity <= 0)
                                                    itemm.Quantity = -(remainingQuantity);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSparePartCost(string spareId)
        {
            List<JobCardPendingCases> getPendingSparesForCompletion = new List<JobCardPendingCases>();
            if (spareId == "") return Content("", "application/json");
            List<JobCardPendingCases> costDetails = new List<JobCardPendingCases>();
            DataTable VehicleSpareParts = Session["getVehicleSpares"] as DataTable;
            var item = VehicleSpareParts.AsEnumerable().Where(x => x.Field<int>("Id") == Convert.ToInt32(spareId)).Select(x => new { PartNumber = x.Field<string>("partnumber"),ManufacturerId= x.Field<int>("ManufacturerId"), VehicleNumber = x.Field<string>("VehicleNumber") }).FirstOrDefault();
            DataTable dtSparePArtscostDetails = _helper.ExecuteSelectStmtusingSP("spGetSparePartCostDetails", "@manufacturerid", item.ManufacturerId.ToString(), null, null, "@partnumber", item.PartNumber);
            DataTable dtSpareIssueDetails= _helper.ExecuteSelectStmtusingSP("GetSpareIssueDetails", "@sparepartid", spareId, null, null, "@vehiclenumber", item.VehicleNumber);
            
            foreach (DataRow row in dtSparePArtscostDetails.Rows)
            {
                JobCardPendingCases details = new JobCardPendingCases();
                details.Manufacturer = row["ManufacturerName"].ToString();
                details.SparePart = row["PartName"].ToString();
                details.LastEntryDate = Convert.ToDateTime(row["lastentrydate"]).ToShortDateString();
                details.Cost = Convert.ToInt32(row["Cost"]);
                details.Quantity = Convert.ToInt32(row["Quantity"]);
                costDetails.Add(details);
            }
            if (dtSpareIssueDetails.Rows.Count > 0)
            {
               
                foreach (DataRow row in dtSpareIssueDetails.Rows)
                {
                    JobCardPendingCases getSparesdetails = new JobCardPendingCases();
                    getSparesdetails.SparePartId = Convert.ToInt32(spareId);
                    getSparesdetails.Quantity = Convert.ToInt32(row["Quantity"]);
                    getSparesdetails.TotalAmount = Convert.ToInt32(row["TotalAmount"]);
                    getSparesdetails.IssuedDate = Convert.ToDateTime(row["IssuedDate"]).ToShortDateString();
                    getSparesdetails.StatusType = row["Status"].ToString();
                    costDetails.Add(getSparesdetails);
                    getPendingSparesForCompletion.Add(getSparesdetails);
                }
            }
            Session["pendingStatusSpareDetails"] = getPendingSparesForCompletion;
           string costDetails1 = JsonConvert.SerializeObject(costDetails, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            });
            return Content(costDetails1, "application/json");
        }
        public ActionResult GetLubesCost(string LubricantId)
        {
            List<JobCardPendingCases> costDetails = new List<JobCardPendingCases>();
            DataTable VehicleLubes = Session["spGetVehicleLubricantDetails"] as DataTable;
            var item = VehicleLubes.AsEnumerable().Where(x => x.Field<int>("Id") == Convert.ToInt32(LubricantId)).Select(x => new { LubricantNumber = x.Field<string>("LubricantNumber"), ManufacturerId = x.Field<int>("ManufacturerId"), VehicleNumber = x.Field<string>("VehicleNumber") }).FirstOrDefault();
            DataTable dtLubricantcostDetails = _helper.ExecuteSelectStmtusingSP("spGetLubricantCostDetails", "@manufacturerid", item.ManufacturerId.ToString(), null, null, "@partnumber", item.LubricantNumber);
            DataTable dtLubesIssueDetails = _helper.ExecuteSelectStmtusingSP("GetLubesIssueDetails", "@lubricantid", LubricantId, null, null, "@vehiclenumber", item.VehicleNumber);
            foreach (DataRow row in dtLubricantcostDetails.Rows)
            {
                JobCardPendingCases details = new JobCardPendingCases();
                details.Manufacturer = row["ManufacturerName"].ToString();
                details.SparePart = row["OilName"].ToString();
                details.LastEntryDate = Convert.ToDateTime(row["lastentrydate"]).ToShortDateString();
                details.Cost = Convert.ToInt32(row["CostPerLitre"]);
                details.Quantity = Convert.ToInt32(row["Quantity"]);
                costDetails.Add(details);
            }
            if (dtLubesIssueDetails.Rows.Count > 0)
            {
                foreach (DataRow row in dtLubesIssueDetails.Rows)
                {
                    JobCardPendingCases getLubesdetails = new JobCardPendingCases();
                    getLubesdetails.Quantity = Convert.ToInt32(row["Quantity"]);
                    getLubesdetails.TotalAmount = Convert.ToInt32(row["TotalAmount"]);
                    getLubesdetails.IssuedDate = Convert.ToDateTime(row["IssuedDate"]).ToShortDateString();
                    getLubesdetails.StatusType = row["Status"].ToString();
                    costDetails.Add(getLubesdetails);
                }
            }
            string costDetails1 = JsonConvert.SerializeObject(costDetails, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            });
            return Content(costDetails1, "application/json");
        }
    }
    }
