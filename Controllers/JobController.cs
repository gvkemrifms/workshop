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
        private readonly Aggregates _jobModel = new Aggregates();
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
                ViewBag.NatureOfComplaint = new SelectList(dsVehicleDistrictDetails.Tables[2].AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");
                ViewBag.AllotedMechanic= new SelectList(dsVehicleDistrictDetails.Tables[3].AsDataView(), "empId", "name");
                ViewBag.ServiceEngineer = new SelectList(dsVehicleDistrictDetails.Tables[4].AsDataView(), "empId", "name");
                _vehModel.DistrictId = Convert.ToInt32(Session["Id"]);
                DataSet dsGetJobCardDetails = _helper.FillDropDownHelperMethodWithSp("spGetJobCardDetails");
                Session["GetJobCardDetails"] = dsGetJobCardDetails;
                model = dsGetJobCardDetails.Tables[0].AsEnumerable().ToList().Select(x => new VehicleModel {Id=x.Field<int>("JobCardNumber"), DistrictName = x.Field<string>("District"),NatureOfComplaint=x.Field<int>("NatureOfComplaint"), VehicleId = x.Field<string>("vehicleNumber"), DateOfDelivery = x.Field<DateTime>("Dor"), ModelNumber = x.Field<int>("Model"), Odometer = x.Field<int>("Odometer"), PilotName = x.Field<string>("PilotName"), ApproximateCost = x.Field<int>("ApproxCost"),AllotedMechanicName= x.Field<string>("Name"),LaborCharges= x.Field<int>("LaborCharges") });
             
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveJobCardDetails(VehicleModel model)
        {       
            int returnVal = 0;
                foreach (var item in model.jobcarditems)
                {
                    VehicleJobCardModel _vehDetails = new VehicleJobCardModel()
                    {
                        WorkShopId = Convert.ToInt32(Session["WorkshopId"]),
                        DistrictId = item.DistrictId,
                        VehId = item.VehId,
                        DateOfDelivery = item.DateOfDelivery,
                        ApproximateCost = item.ApproximateCost,
                        ModelNumber = item.ModelNumber,
                        NatureOfComplaint = item.NatureOfComplaint,
                        Odometer = item.Odometer,
                        PilotId = item.PilotId,
                        PilotName = item.PilotName,
                        ReceivedLocation = item.ReceivedLocation,
                        DateOfRepair = item.DateOfRepair,
                        AllotedMechanic = item.AllotedMechanic,
                        ServiceEngineer = item.ServiceEngineer,
                        LaborCharges = item.LaborCharges,
                        CategoryIdd = item.CategoryIdd,
                        SubCat = item.SubCat,
                        ManufacturerId = item.ManufacturerId

                    };

                    returnVal = _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", _vehDetails.DistrictId, _vehDetails.VehId, _vehDetails.DateOfRepair, _vehDetails.ModelNumber, _vehDetails.Odometer, _vehDetails.ReceivedLocation, _vehDetails.PilotId, _vehDetails.PilotName, _vehDetails.DateOfDelivery, _vehDetails.NatureOfComplaint, _vehDetails.ApproximateCost, Convert.ToInt32(_vehDetails.AllotedMechanic), _vehDetails.WorkShopId, Convert.ToInt32(_vehDetails.ServiceEngineer), Convert.ToInt32(_vehDetails.LaborCharges), Convert.ToInt32(_vehDetails.CategoryIdd), Convert.ToInt32(item.SubCat), Convert.ToInt32(item.ManufacturerId));

                }
           
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
                DataSet dsFillVehiclesByDistrict = _helper.FillDropDownHelperMethodWithSp("spFillVehiclePendingStatusDetails", _vehModel.DistrictId);
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
        public ActionResult GetAggregatesBasedOnManufacturers(int manufacturerId)
        {
            string list = "";
            if (ModelState.IsValid)
            {
                _vehModel.ManufacturerId = Convert.ToInt32(manufacturerId);
                Session["ManufacturerId"] = _vehModel.ManufacturerId;
                DataSet dsFillManufacturerBasedOnAggregates = _helper.FillDropDownHelperMethodWithSp("getAggeregatesByManufacturers", _vehModel.ManufacturerId);
                List<DataRow> data = dsFillManufacturerBasedOnAggregates.Tables[0].AsEnumerable().ToList();
                List<string> names = new List<string>();
                foreach (DataRow row in data)
                {
                    _vehModel.AggregateId = Convert.ToInt32(row["ServiceGroup_Id"]);
                    _vehModel.AggregateName = row["ServiceGroup_Name"].ToString();

                    names.Add(_vehModel.AggregateName + "-" + _vehModel.AggregateId);
                }
                list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });

            }
            return Content(list, "application/json");

        }




        [HttpPost]
        public ActionResult GetSubCategoryIds(string categoryId)
        {
            string list = "";
            if (ModelState.IsValid)
            {
                _vehModel.SubCategory = int.Parse(categoryId);
                Session["SubCategoryId"] = _vehModel.SubCategory;
                DataSet dsFillVehiclesByDistrict = _helper.FillDropDownHelperMethodWithSpCategory("spGetSubCategoryDetails", _vehModel.SubCategory);
                List<DataRow> data = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().ToList();
                List<string> names = new List<string>();
                foreach (DataRow row in data)
                {
                    _vehModel.SubCategoryName = row["SubCategories"].ToString();
                    _vehModel.SubCategoryId = row["SubCategory_Id"].ToString();

                    names.Add(_vehModel.SubCategoryName + "-" + _vehModel.SubCategoryId);
                }
                list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });

            }
            return Content(list, "application/json");

        }

        [HttpPost]
        public ActionResult GetCategoryIds(string aggregatedId)
        {
            string list = "";
            if (ModelState.IsValid)
            {
                _vehModel.AggregateId = int.Parse(aggregatedId);
                Session["AggregateId"] = _vehModel.AggregateId;
                DataSet dsFillCategoryByAggregate = _helper.FillDropDownHelperMethodWithSp("spGetCategory", _vehModel.AggregateId);
                List<DataRow> data = dsFillCategoryByAggregate.Tables[0].AsEnumerable().ToList();
                List<string> names = new List<string>();
                foreach (DataRow row in data)
                {
                    _vehModel.CategoryName = row["Categories"].ToString();
                    _vehModel.CategoryId = row["Category_Id"].ToString();

                    names.Add(_vehModel.CategoryName + "-" + _vehModel.CategoryId);
                }
                list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });

            }
            return Content(list, "application/json");

        }

        [HttpPost]
        public ActionResult GetEstimatedCostDetails(string categoryId)
        {
            decimal list = 0;
            if (ModelState.IsValid)
            {
                _vehModel.EstimatedCost = int.Parse(categoryId);
                DataSet dsgetCostBySubCategory = _helper.getCost("spGetCostDetails", _vehModel.EstimatedCost);
                list = dsgetCostBySubCategory.Tables[0].AsEnumerable().Select(x => x.Field<int>("EstimatedCost")).FirstOrDefault();

            }
            return Content(list.ToString(), "application/json");

        }

        [HttpPost]
        public ActionResult GetModelNumber(string vehicleid)
        {
            if (vehicleid == null) return null;
            int list = 0;
            if (ModelState.IsValid)
            {
                _vehModel.VehId = int.Parse(vehicleid);
                if (_vehModel.VehId == 0) return null;
             
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
            string complaintsQuery = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            DataTable dtComplaintsQuery = _helper.ExecuteSelectStmt(complaintsQuery);
            VehicleModel model = new VehicleModel()
            {
                District = new SelectList(dsGetVehicleManufacturers.Tables[0].AsDataView(), "Id", "District"),
                ComplaintsNature= new SelectList(dtComplaintsQuery.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name"),
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
            _helper.ExecuteJobUpdateStatement(postVehicle.DistrictId, "spEditJobcardDetails", postVehicle.VehId, postVehicle.ModelYear, postVehicle.Odometer, postVehicle.ApproximateCost,postVehicle.DateOfRepair,postVehicle.NatureOfComplaint,postVehicle.Id);
            return RedirectToAction("SaveJobCardDetails");
        }

  [HttpDelete]
        public ActionResult Delete(int? Id)
        {
            _helper.ExecuteDeleteStatement("spDeleteJobCardDetails", Id);
            return RedirectToAction("SaveJobCardDetails");
        }

        public ActionResult ServiceGroup()
        {
            DataSet dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName");
            var Aggregates = dsVehicleDistrictDetails.Tables[2].AsEnumerable().Select(x => new Aggregates { AggregateId = x.Field<int>("ServiceGroup_Id"), AggregateName = x.Field<string>("ServiceGroup_Name"), ManufacturerName = x.Field<string>("ManufacturerName") });
            return View(Aggregates);
        }
        [HttpPost]
        public ActionResult ServiceGroup(Aggregates aggregates)
        {
           int result= _helper.InsertJobAggregateDetails("spInsertJobAggregateDetails", aggregates.ManufacturerId, aggregates.AggregateName, Convert.ToInt32(Session["Employee_Id"]));
            return Json(result,JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditAggregates(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("ServiceGroup");
            }
            DataSet dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            DataRow row = dsVehicleDistrictDetails.Tables[2].AsEnumerable().ToList().Single(x => x.Field<int>("ServiceGroup_Id") == id);
            Aggregates aggregates = new Aggregates()
            {
                AggregateList = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName"),
                AggregateName = row["ServiceGroup_Name"].ToString(),
                ManufacturerId = Convert.ToInt32(row["Manufacturer_Id"]),
                AggregateId = Convert.ToInt32(row["ServiceGroup_Id"])

            };
            Session["Id"]=aggregates.AggregateId;
            return View(aggregates);
        }
        [HttpPost]
        public ActionResult EditAggregates(Aggregates aggregates)
        {
            _helper.ExecuteJobAggregateUpdateStatement("spEditJobAggregateDetails", aggregates.ManufacturerId, aggregates.AggregateName,Convert.ToInt32(Session["Id"]));
            return RedirectToAction("ServiceGroup");
        }
        [HttpDelete]
        public ActionResult DeleteAggregates(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("ServiceGroup");
            }
            _helper.ExecuteDeleteStatement("spDeleteAggregates", id);
            return RedirectToAction("ServiceGroup");

        }
        public ActionResult CategoryGroup()
        {

            string queryCategories = "select * from M_FMS_Categories c join M_FMS_MaintenanceWorksServiceGroupDetails m on m.ServiceGroup_Id=c.Aggregate_ID";
            string Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            DataTable dtAgregates = _helper.ExecuteSelectStmt(Agrregates);
            ViewBag.AggregatesDrop = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");
            DataTable dtCategories = _helper.ExecuteSelectStmt(queryCategories);
            Session["Categories"] = queryCategories;
            var Categories = dtCategories.AsEnumerable().Select(x => new Aggregates { IdCategory = x.Field<int>("Category_Id"), AggregateName = x.Field<string>("ServiceGroup_Name"), CategoryName = x.Field<string>("Categories") });
            return View(Categories);
        }
        [HttpPost]
        public ActionResult CategoryGroup(string AggregateId,string CategoryName)
        {
            int result = _helper.InsertJobCategoryDetails("spInsertJobCategoryDetails", Convert.ToInt32(AggregateId), CategoryName);
           return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditCategories(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("CategoryGroup");
            }
            string queryCategories = "select * from M_FMS_Categories";
            string Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            DataTable dtAgregates = _helper.ExecuteSelectStmt(Agrregates);
            DataTable dtCategories = _helper.ExecuteSelectStmt(queryCategories);
            DataRow row = dtCategories.AsEnumerable().ToList().Single(x => x.Field<int>("Category_Id") == id);
            Aggregates aggregates = new Aggregates()
            {
                AggregateList = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name"),
              CategoryName = row["Categories"].ToString(),
              AggregateId=Convert.ToInt32(row["Aggregate_ID"])

        };
            return View(aggregates);
        }
        [HttpPost]
        public ActionResult EditCategories(Aggregates aggregates)
        {
            _helper.ExecuteJobAggregateUpdateStatement("spEditJobAggregateDetails", aggregates.ManufacturerId, aggregates.AggregateName, Convert.ToInt32(Session["Id"]));
            return RedirectToAction("ServiceGroup");
        }
        [HttpDelete]
        public ActionResult DeleteCategories(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("ServiceGroup");
            }
            _helper.ExecuteDeleteStatement("spDeleteCategories", id);
            return RedirectToAction("ServiceGroup");

        }
        public ActionResult SubCategoryGroup()
        {
            string manufacturerQuery = "select * from m_VehicleManufacturer";
            DataTable dtmanufacturers = _helper.ExecuteSelectStmt(manufacturerQuery);
            ViewBag.Manufacturers = new SelectList(dtmanufacturers.AsDataView(), "Id", "ManufacturerName");
            //string Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";//spGetAggregatesForManufacturers
           // DataTable dtAgregates = _helper.ExecuteSelectStmtusingSP("spGetAggregatesForManufacturers", "@manufacturerid",);
           // ViewBag.AggregatesDrop = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");
            string querySubCategories = "select * from M_FMS_MaintenanceWorksMasterDetails m join m_VehicleManufacturer v on m.ManufacturerId=v.Id ";
            string categories = "select * from M_FMS_Categories";
            DataTable dtCategories = _helper.ExecuteSelectStmt(categories);
            ViewBag.CategoryDrop = new SelectList(dtCategories.AsDataView(), "Category_id", "Categories");
            DataTable dtSubCategories = _helper.ExecuteSelectStmt(querySubCategories);
            Session["SubCategories"] = dtSubCategories;
            var subCategories = dtSubCategories.AsEnumerable().Select(x => new Aggregates {  ServiceId = x.Field<int>("Service_Id"), ServiceName = x.Field<string>("Service_Name"),ManufacturerName=x.Field<string>("ManufacturerName") });
            return View(subCategories);
        }
        [HttpPost]
        public ActionResult SubCategoryGroup(Aggregates aggregates)
        {
            int result = _helper.InsertJobSubCategoriesDetails("spInsertSubCategoryMasterDetails", aggregates.ManufacturerId, aggregates.ServiceGroupId,aggregates.ServiceName,aggregates.timeTaken, Convert.ToInt32(Session["Employee_Id"]));
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditSubCategories(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("SubCategoryGroup");
            }
            string queryCategories = "select * from M_FMS_Categories";
            string Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            string subcategories = "select * from M_FMS_MaintenanceWorksMasterDetails";
            DataSet dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            DataTable dtAgregates = _helper.ExecuteSelectStmt(Agrregates);
            DataTable dtCategories = _helper.ExecuteSelectStmt(queryCategories);
            DataTable dtSubCategories = _helper.ExecuteSelectStmt(subcategories);
            DataRow row = dtSubCategories.AsEnumerable().ToList().Single(x => x.Field<int>("Service_id") == id);
           DataRow r1= dtCategories.AsEnumerable().Where(x => x.Field<int>("Aggregate_ID") == Convert.ToInt32(row["ServiceGroup_Id"])).FirstOrDefault();
            Session["Id"] = id;
            Aggregates aggregates = new Aggregates()
            {
                Categories = new SelectList(dtCategories.AsDataView(), "Category_Id", "Categories"),
                AggregateList = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name"),
                ServiceGroupId = Convert.ToInt32(row["ServiceGroup_Id"]),
                IdCategory = Convert.ToInt32(r1["Aggregate_id"]),
                Manufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName"),
                ManufacturerId = Convert.ToInt32(row["ManufacturerId"]),
                ServiceName = row["Service_Name"].ToString(),
                timeTaken = Convert.ToInt32(row["Time_Taken"])
            };
            return View(aggregates);
        }
        [HttpPost]
        public ActionResult EditSubCategories(Aggregates aggregates)
        {
            _helper.ExecuteJobSubCategoryUpdateStatement("spEditJobSubCategoryDetails", aggregates.ManufacturerId, aggregates.ServiceGroupId,aggregates.ServiceName,aggregates.timeTaken,Convert.ToInt32(Session["Id"]));
            return RedirectToAction("SubCategoryGroup");
        }
        public ActionResult GetAggregateCostDetails()
        {
            string Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            DataTable dtAgregates = _helper.ExecuteSelectStmt(Agrregates);
            ViewBag.Aggregates = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");


            return View();
        }
        public ActionResult CheckOdoReading(int vehicleid,int Odo)
        {
            DataRow list=null;
            if (ModelState.IsValid)
            {
                DataTable dtLubesNumber = _helper.ExecuteSelectStmtusingSP("getOdoReading", "@vehicleid", vehicleid.ToString());
                list = dtLubesNumber.AsEnumerable().Where(x => x.Field<int>("odometer") >= Odo).FirstOrDefault();
                if (list == null)
                    return null ;
            }

            return Content(list[0].ToString(),"application/Json");
        }
        [HttpPost]
        public ActionResult GetManufacturerNameForAggregates(string ManufacturerId)
        {
            string list = "";
            if (ModelState.IsValid)
            {
                _jobModel.ManufacturerId = int.Parse(ManufacturerId);
                Session["ManufacturerId"] = _jobModel.ManufacturerId;
                DataSet dsFillAggregatesByManufacturers = _helper.FillDropDownHelperMethodWithSp2("spGetAggregatesForManufacturers", _jobModel.ManufacturerId);
                //DataSet dsFillManufacturersByAggregates = _helper.FillDropDownHelperMethodWithSp1("getManufacturerNameForAggregate", _jobModel.AggregateId);
                List<DataRow> data = dsFillAggregatesByManufacturers.Tables[0].AsEnumerable().ToList();
                List<string> names = new List<string>();
                foreach (DataRow row in data)
                {
                    _jobModel.ServiceName = row["ServiceGroup_Name"].ToString();
                    _jobModel.ServiceGroupId = Convert.ToInt32(row["ServiceGroup_Id"]);

                    names.Add(_jobModel.ServiceName + "-" + _jobModel.ServiceGroupId);
                }
                list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });

            }
            return Content(list, "application/json");

        }

        // GetCategoriesForAggregates
        public ActionResult GetCategoriesForAggregates(string AggregateId)
        {
            string list = "";
            if (ModelState.IsValid)
            {
                _jobModel.AggregateId = int.Parse(AggregateId);
                Session["AggregateId"] = _jobModel.AggregateId;
                DataSet dsFillCategoriesForAggregates = _helper.FillDropDownHelperMethodWithSp3("getCategoriesForAggregates", _jobModel.AggregateId);
                //DataSet dsFillManufacturersByAggregates = _helper.FillDropDownHelperMethodWithSp1("getManufacturerNameForAggregate", _jobModel.AggregateId);
                List<DataRow> data = dsFillCategoriesForAggregates.Tables[0].AsEnumerable().ToList();
                List<string> names = new List<string>();
                foreach (DataRow row in data)
                {
                    _jobModel.CategoryName = row["Categories"].ToString();
                    _jobModel.IdCategory = Convert.ToInt32(row["Category_Id"]);

                    names.Add(_jobModel.CategoryName + "-" + _jobModel.IdCategory);
                }
                list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });

            }
            return Content(list, "application/json");

        }
        [HttpPost]
        public ActionResult getCategorySubCategoryCostDetails(int aggregateid,int manufacturerid)
        {
           DataTable dtDetails=  _helper.ExecuteSelectStmtusingSP("getAggregateCategoryDetails", "@aggregateid", aggregateid.ToString(), "@manufacturerid", manufacturerid.ToString());
          var aggregates=  dtDetails.AsEnumerable().Select(x => new Aggregates { CategoryName = x.Field<string>("categories"), SubCategoryName = x.Field<string>("subcategories"), ApproxCost = x.Field<int>("ApproxCost")});
            Session["Aggregates"] = dtDetails;
            return RedirectToAction("GetManufacturerAggregatesDetails");          
        }

        public ActionResult GetManufacturerAggregatesDetails()
        {
            string queryManufacturers = "select * from m_VehicleManufacturer";
            DataTable dtManufacturers = _helper.ExecuteSelectStmt(queryManufacturers);
            ViewBag.Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName");
            _helper.ExecuteSelectStmtusingSP("getAggregateCategoryDetails");
            if (Session["Aggregates"] != null)
            {
                var dtDetails = Session["Aggregates"] as DataTable;            
                    var aggregates = dtDetails.AsEnumerable().Select(x => new Aggregates {IdCategory= x.Field<int>("Category_Id"), CategoryName = x.Field<string>("categories"), SubCategoryName = x.Field<string>("SubCategories"),ApproxCost = x.Field<int?>("ApproxCost"),ServiceId= x.Field<int>("Service_Id") });
               
                    return View(aggregates);
                
            }
            else
            {
                return View();
            }
        }

        public ActionResult EditSubCategoryCostDetails(int? id=0)
        {
            if (id == null)
            {
                return RedirectToAction("GetManufacturerAggregatesDetails");
            }
            string categorystr = "select * from [M_FMS_Categories]";
            string subCategorystr = "select Service_Id,Service_Name from [dbo].[M_FMS_MaintenanceWorksMasterDetails] ";
            DataTable dtCategories=_helper.ExecuteSelectStmt(categorystr);
            DataTable dtSubcatStr= _helper.ExecuteSelectStmt(subCategorystr);
            DataTable dtcategoryIdd = Session["Aggregates"] as DataTable; 
            DataRow categoryIdd = dtcategoryIdd.AsEnumerable().Where(x => x.Field<int>("Service_Id") == id).FirstOrDefault();
            DataTable dtSubCostDetails= _helper.ExecuteSelectStmtusingSP("getJobCardVehicleDetails", "@jobcardnumber",id.ToString());
            _jobModel.Categories= new SelectList(dtCategories.AsDataView(), "Category_Id", "Categories");
            _jobModel.SubCategories= new SelectList(dtSubcatStr.AsDataView(), "Service_Id", "Service_Name");
            _jobModel.IdCategory = Convert.ToInt32(categoryIdd["Category_Id"]);
            _jobModel.SubCategory= dtSubCostDetails.AsEnumerable().Select(x => x.Field<int>("Service_Id")).FirstOrDefault();
            _jobModel.ApproxCost= dtSubCostDetails.AsEnumerable().Select(x => x.Field<int?>("CostFor_A_Grade")).FirstOrDefault();
            return View(_jobModel);
        }
        [HttpPost]
        public ActionResult EditSubCategoryCostDetails(Aggregates aggregates)
        {
            _helper.ExecuteJobSubCategoryUpdateCost("spEditJobSubCategoryCostDetails",aggregates.ApproxCost,aggregates.SubCategory );
            return RedirectToAction("SubCategoryGroup");
        }
    }
}