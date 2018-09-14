using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Fleet_WorkShop.Models;
using Newtonsoft.Json;

namespace Fleet_WorkShop.Controllers
{
    public class JobController : Controller
    {
        private readonly EmployeeHelper _helper = new EmployeeHelper();
        private readonly Aggregates _jobModel = new Aggregates();

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
            if (Session["WorkshopId"] == null)
                return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return null;
            var dsVehicleDistrictDetails =
                _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            Session["VehicleDistrictDetails"] = dsVehicleDistrictDetails;
            ViewBag.Districts = new SelectList(dsVehicleDistrictDetails.Tables[0].AsDataView(), "Id", "District");
            ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id",
                "ManufacturerName");
            ViewBag.NatureOfComplaint = new SelectList(dsVehicleDistrictDetails.Tables[2].AsDataView(),
                "ServiceGroup_Id", "ServiceGroup_Name");
            ViewBag.AllotedMechanic = new SelectList(dsVehicleDistrictDetails.Tables[3].AsDataView(), "empId", "name");
            ViewBag.ServiceEngineer = new SelectList(dsVehicleDistrictDetails.Tables[4].AsDataView(), "empId", "name");
            _vehModel.DistrictId = Convert.ToInt32(Session["Id"]);
            var dsGetJobCardDetails = _helper.FillDropDownHelperMethodWithSp("spGetJobCardDetails");
            Session["GetJobCardDetails"] = dsGetJobCardDetails;
            var model = dsGetJobCardDetails.Tables[0].AsEnumerable().ToList().Select(x => new VehicleModel
            {
                Id = x.Field<int>("JobCardNumber"),
                DistrictName = x.Field<string>("District"),
                NatureOfComplaint = x.Field<int>("NatureOfComplaint"),
                VehicleId = x.Field<string>("vehicleNumber"),
                DateOfDelivery = x.Field<DateTime>("Dor"),
                ModelNumber = x.Field<int>("Model"),
                Odometer = x.Field<int>("Odometer"),
                PilotName = x.Field<string>("PilotName"),
                ApproximateCost = x.Field<int>("ApproxCost"),
                AllotedMechanicName = x.Field<string>("Name"),
                LaborCharges = x.Field<int>("LaborCharges")
            });
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveJobCardDetails(VehicleModel model, string vehicleNumber)
        {
            foreach (var item in model.jobcarditems)
            {
                var vehDetails = new VehicleJobCardModel
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
                var dtGetEmpmre =
                    _helper.ExecuteSelectStmtusingSP("spGetEMEPMRM", null, null, null, null, "@vehiclenumber",
                        vehicleNumber);
                var emt = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "EMT")
                    .Select(x => x.Field<int>("empid")).FirstOrDefault();
                var pm = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "PM")
                    .Select(x => x.Field<int>("empid")).FirstOrDefault();
                var rm = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "RM")
                    .Select(x => x.Field<int>("empid")).FirstOrDefault();
                _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", vehDetails.DistrictId, vehDetails.VehId,
                    vehDetails.DateOfRepair, vehDetails.ModelNumber, vehDetails.Odometer, vehDetails.ReceivedLocation,
                    vehDetails.PilotId, vehDetails.PilotName, vehDetails.DateOfDelivery, vehDetails.NatureOfComplaint,
                    vehDetails.ApproximateCost, Convert.ToInt32(vehDetails.AllotedMechanic), vehDetails.WorkShopId,
                    Convert.ToInt32(vehDetails.ServiceEngineer), Convert.ToInt32(vehDetails.LaborCharges),
                    Convert.ToInt32(vehDetails.CategoryIdd), Convert.ToInt32(item.SubCat),
                    Convert.ToInt32(item.ManufacturerId), Convert.ToInt32(rm), Convert.ToInt32(pm),
                    Convert.ToInt32(emt));
            }

            return RedirectToAction("SaveJobCardDetails");
        }

        [HttpPost]
        public ActionResult GetDistrictIds(string districtId)
        {
            if (districtId == null) throw new ArgumentNullException(nameof(districtId));
            var list = string.Empty;
            if (!ModelState.IsValid) return Content(list, "application/json");
            _vehModel.DistrictId = int.Parse(districtId);
            Session["Id"] = _vehModel.DistrictId;
            var dsFillVehiclesByDistrict =
                _helper.FillDropDownHelperMethodWithSp("spFillVehiclePendingStatusDetails", _vehModel.DistrictId);
            var data = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();
            foreach (var row in data)
            {
                _vehModel.VehicleNumberString = row["vehiclenumber"].ToString();
                _vehModel.VehicleId = row["Id"].ToString();
                names.Add(_vehModel.VehicleNumberString + "-" + _vehModel.VehicleId);
            }
            list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(list, "application/json");
        }


        [HttpPost]
        public ActionResult GetAggregatesBasedOnManufacturers(int manufacturerId)
        {
            var list = "";
            if (!ModelState.IsValid) return null;
            _vehModel.ManufacturerId = Convert.ToInt32(manufacturerId);
            Session["ManufacturerId"] = _vehModel.ManufacturerId;
            var dsFillManufacturerBasedOnAggregates =
                _helper.FillDropDownHelperMethodWithSp("getAggeregatesByManufacturers", _vehModel.ManufacturerId);
            var data = dsFillManufacturerBasedOnAggregates.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();
            foreach (var row in data)
            {
                _vehModel.AggregateId = Convert.ToInt32(row["ServiceGroup_Id"]);
                _vehModel.AggregateName = row["ServiceGroup_Name"].ToString();

                names.Add(_vehModel.AggregateName + "-" + _vehModel.AggregateId);
            }
            list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(list, "application/json");
        }

        [HttpPost]
        public ActionResult GetSubCategoryIds(string categoryId)
        {
            if (categoryId == null) throw new ArgumentNullException(nameof(categoryId));
            if (!ModelState.IsValid) return null;
            _vehModel.SubCategory = int.Parse(categoryId);
            Session["SubCategoryId"] = _vehModel.SubCategory;
            var dsFillVehiclesByDistrict =
                _helper.FillDropDownHelperMethodWithSpCategory("spGetSubCategoryDetails", _vehModel.SubCategory);
            var data = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();
            foreach (var row in data)
            {
                _vehModel.SubCategoryName = row["SubCategories"].ToString();
                _vehModel.SubCategoryId = row["SubCategory_Id"].ToString();

                names.Add(_vehModel.SubCategoryName + "-" + _vehModel.SubCategoryId);
            }
            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(list, "application/json");
        }

        [HttpPost]
        public ActionResult GetCategoryIds(string aggregatedId)
        {
            if (aggregatedId == null) throw new ArgumentNullException(nameof(aggregatedId));
            if (!ModelState.IsValid) return null;
            _vehModel.AggregateId = int.Parse(aggregatedId);
            Session["AggregateId"] = _vehModel.AggregateId;
            var dsFillCategoryByAggregate =
                _helper.FillDropDownHelperMethodWithSp("spGetCategory", _vehModel.AggregateId);
            var data = dsFillCategoryByAggregate.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();
            foreach (var row in data)
            {
                _vehModel.CategoryName = row["Categories"].ToString();
                _vehModel.CategoryId = row["Category_Id"].ToString();

                names.Add(_vehModel.CategoryName + "-" + _vehModel.CategoryId);
            }
            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(list, "application/json");
        }

        [HttpPost]
        public ActionResult GetEstimatedCostDetails(string categoryId)
        {
            decimal list = 0;
            if (!ModelState.IsValid) return null;
            _vehModel.EstimatedCost = int.Parse(categoryId);
            var dsgetCostBySubCategory = _helper.getCost("spGetCostDetails", _vehModel.EstimatedCost);
            list = dsgetCostBySubCategory.Tables[0].AsEnumerable().Select(x => x.Field<int>("EstimatedCost"))
                .FirstOrDefault();
            return Content(list.ToString(CultureInfo.InvariantCulture), "application/json");
        }

        [HttpPost]
        public ActionResult GetModelNumber(string vehicleid)
        {
            if (vehicleid == null) return null;
            var list = 0;
            if (!ModelState.IsValid) return null;
            _vehModel.VehId = int.Parse(vehicleid);
            if (_vehModel.VehId == 0) return null;
            var dsFillVehiclesByDistrict = _helper.FillModelNumbers("spGetVehicleModelNumber", _vehModel.VehId);
            list = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().Select(x => x.Field<int>("model"))
                .FirstOrDefault();
            return Content(list.ToString(), "application/json");
        }

        public ActionResult Edit(int? id = null)
        {
            if (id == null)
                return RedirectToAction("SaveJobCardDetails");
            var dsGetVehicleManufacturers = Session["VehicleDistrictDetails"] as DataSet;
            var dsGetJobCardDetails = Session["GetJobCardDetails"] as DataSet;
            var row = dsGetJobCardDetails.Tables[0].AsEnumerable().ToList()
                .Single(x => x.Field<int>("JobCardNumber") == id);
            var query =
                "select v.id, v.vehiclenumber,v.districtId from m_GetVehicleDetails v join m_districts d on v.districtId=d.Id where v.vehiclenumber = '" +
                row["vehicleNumber"] + "'";
            var dtVehicle = _helper.ExecuteSelectStmt(query);
            var fillVehicleDropQuery = "select * from m_GetVehicleDetails where districtId=" + dtVehicle.Rows[0][2] +
                                       "";
            var dtVehicledet = _helper.ExecuteSelectStmt(fillVehicleDropQuery);
            var complaintsQuery = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            var dtComplaintsQuery = _helper.ExecuteSelectStmt(complaintsQuery);
            var model = new VehicleModel
            {
                District = new SelectList(dsGetVehicleManufacturers.Tables[0].AsDataView(), "Id", "District"),
                ComplaintsNature = new SelectList(dtComplaintsQuery.AsDataView(), "ServiceGroup_Id",
                    "ServiceGroup_Name"),
                DistrictName = row["District"].ToString(),
                NatureOfComplaint = Convert.ToInt32(row["NatureOfComplaint"]),
                Vehicle = new SelectList(dtVehicledet.AsDataView(), "Id", "VehicleNumber"),
                VehicleNumberString = row["VehicleNumber"].ToString(),
                VehId = Convert.ToInt32(dtVehicle.Rows[0][0].ToString()),
                DistrictId = Convert.ToInt32(dtVehicle.Rows[0][2].ToString()),
                DateOfRepair = Convert.ToDateTime(row["Dor"]),
                ModelYear = Convert.ToInt32(row["Model"]),
                Odometer = Convert.ToInt32(row["Odometer"]),
                ApproximateCost = Convert.ToInt32(row["ApproxCost"])
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(VehicleModel postVehicle)
        {
            _helper.ExecuteJobUpdateStatement(postVehicle.DistrictId, "spEditJobcardDetails", postVehicle.VehId,
                postVehicle.ModelYear, postVehicle.Odometer, postVehicle.ApproximateCost, postVehicle.DateOfRepair,
                postVehicle.NatureOfComplaint, postVehicle.Id);
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
            var dsVehicleDistrictDetails =
                _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id",
                "ManufacturerName");
            var aggregates = dsVehicleDistrictDetails.Tables[2].AsEnumerable().Select(x => new Aggregates
            {
                AggregateId = x.Field<int>("ServiceGroup_Id"),
                AggregateName = x.Field<string>("ServiceGroup_Name"),
                ManufacturerName = x.Field<string>("ManufacturerName")
            });
            return View(aggregates);
        }

        [HttpPost]
        public ActionResult ServiceGroup(Aggregates aggregates)
        {
            var result = _helper.InsertJobAggregateDetails("spInsertJobAggregateDetails", aggregates.ManufacturerId,
                aggregates.AggregateName, Convert.ToInt32(Session["Employee_Id"]));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditAggregates(int? id)
        {
            if (id == null)
                return RedirectToAction("ServiceGroup");
            var dsVehicleDistrictDetails =
                _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            var row = dsVehicleDistrictDetails.Tables[2].AsEnumerable().ToList()
                .Single(x => x.Field<int>("ServiceGroup_Id") == id);
            var aggregates = new Aggregates
            {
                AggregateList = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id",
                    "ManufacturerName"),
                AggregateName = row["ServiceGroup_Name"].ToString(),
                ManufacturerId = Convert.ToInt32(row["Manufacturer_Id"]),
                AggregateId = Convert.ToInt32(row["ServiceGroup_Id"])
            };
            Session["Id"] = aggregates.AggregateId;
            return View(aggregates);
        }

        [HttpPost]
        public ActionResult EditAggregates(Aggregates aggregates)
        {
            if (aggregates == null) throw new ArgumentNullException(nameof(aggregates));
            _helper.ExecuteJobAggregateUpdateStatement("spEditJobAggregateDetails", aggregates.ManufacturerId,
                aggregates.AggregateName, Convert.ToInt32(Session["Id"]));
            return RedirectToAction("ServiceGroup");
        }

        [HttpDelete]
        public ActionResult DeleteAggregates(int? id)
        {
            if (id != null)
            {
                _helper.ExecuteDeleteStatement("spDeleteAggregates", id);
                return RedirectToAction("ServiceGroup");
            }
            return RedirectToAction("ServiceGroup");
        }

        public ActionResult CategoryGroup()
        {
            var queryCategories =
                "select * from M_FMS_Categories c join M_FMS_MaintenanceWorksServiceGroupDetails m on m.ServiceGroup_Id=c.Aggregate_ID";
            var Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            var dtAgregates = _helper.ExecuteSelectStmt(Agrregates);
            ViewBag.AggregatesDrop = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");
            var dtCategories = _helper.ExecuteSelectStmt(queryCategories);
            Session["Categories"] = queryCategories;
            var categories = dtCategories.AsEnumerable().Select(x => new Aggregates
            {
                IdCategory = x.Field<int>("Category_Id"),
                AggregateName = x.Field<string>("ServiceGroup_Name"),
                CategoryName = x.Field<string>("Categories")
            });
            return View(categories);
        }

        [HttpPost]
        public ActionResult CategoryGroup(string aggregateId, string categoryName)
        {
            var result = _helper.InsertJobCategoryDetails("spInsertJobCategoryDetails", Convert.ToInt32(aggregateId),
                categoryName);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditCategories(int? id)
        {
            if (id == null)
                return RedirectToAction("CategoryGroup");
            const string queryCategories = "select * from M_FMS_Categories";
            const string agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            var dtAgregates = _helper.ExecuteSelectStmt(agrregates);
            var dtCategories = _helper.ExecuteSelectStmt(queryCategories);
            var row = dtCategories.AsEnumerable().ToList().Single(x => x.Field<int>("Category_Id") == id);
            var aggregates = new Aggregates
            {
                AggregateList = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name"),
                CategoryName = row["Categories"].ToString(),
                AggregateId = Convert.ToInt32(row["Aggregate_ID"])
            };
            return View(aggregates);
        }

        [HttpPost]
        public ActionResult EditCategories(Aggregates aggregates)
        {
            if (aggregates == null) throw new ArgumentNullException(nameof(aggregates));
            _helper.ExecuteJobAggregateUpdateStatement("spEditJobAggregateDetails", aggregates.ManufacturerId,
                aggregates.AggregateName, Convert.ToInt32(Session["Id"]));
            return RedirectToAction("ServiceGroup");
        }

        [HttpDelete]
        public ActionResult DeleteCategories(int? id)
        {
            if (id == null)
                return RedirectToAction("ServiceGroup");
            _helper.ExecuteDeleteStatement("spDeleteCategories", id);
            return RedirectToAction("ServiceGroup");
        }

        public ActionResult SubCategoryGroup()
        {
            var manufacturerQuery = "select * from m_VehicleManufacturer";
            var dtmanufacturers = _helper.ExecuteSelectStmt(manufacturerQuery);
            ViewBag.Manufacturers = new SelectList(dtmanufacturers.AsDataView(), "Id", "ManufacturerName");
            var querySubCategories =
                "select * from M_FMS_MaintenanceWorksMasterDetails m join m_VehicleManufacturer v on m.ManufacturerId=v.Id ";
            var categories = "select * from M_FMS_Categories";
            var dtCategories = _helper.ExecuteSelectStmt(categories);
            ViewBag.CategoryDrop = new SelectList(dtCategories.AsDataView(), "Category_id", "Categories");
            var dtSubCategories = _helper.ExecuteSelectStmt(querySubCategories);
            Session["SubCategories"] = dtSubCategories;
            var subCategories = dtSubCategories.AsEnumerable().Select(x => new Aggregates
            {
                ServiceId = x.Field<int>("Service_Id"),
                ServiceName = x.Field<string>("Service_Name"),
                ManufacturerName = x.Field<string>("ManufacturerName")
            });
            return View(subCategories);
        }

        [HttpPost]
        public ActionResult SubCategoryGroup(Aggregates aggregates)
        {
            var result = _helper.InsertJobSubCategoriesDetails("spInsertSubCategoryMasterDetails",
                aggregates.ManufacturerId, aggregates.ServiceGroupId, aggregates.ServiceName, aggregates.timeTaken,
                Convert.ToInt32(Session["Employee_Id"]));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditSubCategories(int? id)
        {
            if (id == null)
                return RedirectToAction("SubCategoryGroup");
            var queryCategories = "select * from M_FMS_Categories";
            var Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            var subcategories = "select * from M_FMS_MaintenanceWorksMasterDetails";
            var dsVehicleDistrictDetails =
                _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            var dtAgregates = _helper.ExecuteSelectStmt(Agrregates);
            var dtCategories = _helper.ExecuteSelectStmt(queryCategories);
            var dtSubCategories = _helper.ExecuteSelectStmt(subcategories);
            var row = dtSubCategories.AsEnumerable().ToList().Single(x => x.Field<int>("Service_id") == id);
            var r1 = dtCategories.AsEnumerable()
                .FirstOrDefault(x => x.Field<int>("Aggregate_ID") == Convert.ToInt32(row["ServiceGroup_Id"]));
            Session["Id"] = id;
            var aggregates = new Aggregates
            {
                Categories = new SelectList(dtCategories.AsDataView(), "Category_Id", "Categories"),
                AggregateList = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name"),
                ServiceGroupId = Convert.ToInt32(row["ServiceGroup_Id"]),
                IdCategory = Convert.ToInt32(r1["Aggregate_id"]),
                Manufacturer =
                    new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName"),
                ManufacturerId = Convert.ToInt32(row["ManufacturerId"]),
                ServiceName = row["Service_Name"].ToString(),
                timeTaken = Convert.ToInt32(row["Time_Taken"])
            };
            return View(aggregates);
        }

        [HttpPost]
        public ActionResult EditSubCategories(Aggregates aggregates)
        {
            if (aggregates == null) throw new ArgumentNullException(nameof(aggregates));
            _helper.ExecuteJobSubCategoryUpdateStatement("spEditJobSubCategoryDetails", aggregates.ManufacturerId,
                aggregates.ServiceGroupId, aggregates.ServiceName, aggregates.timeTaken,
                Convert.ToInt32(Session["Id"]));
            return RedirectToAction("SubCategoryGroup");
        }

        public ActionResult GetAggregateCostDetails()
        {
            var Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            var dtAgregates = _helper.ExecuteSelectStmt(Agrregates);
            ViewBag.Aggregates = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");


            return View();
        }

        public ActionResult CheckOdoReading(int vehicleid, int odo)
        {
            var dtLubesNumber =
                _helper.ExecuteSelectStmtusingSP("getOdoReading", "@vehicleid", vehicleid.ToString());
            var list = dtLubesNumber.AsEnumerable().FirstOrDefault(x => x.Field<int>("odometer") >= odo);
            return list == null ? null : Content(list[0].ToString(), "application/Json");
        }

        [HttpPost]
        public ActionResult GetManufacturerNameForAggregates(string manufacturerId)
        {
            _jobModel.ManufacturerId = int.Parse(manufacturerId);
            Session["ManufacturerId"] = _jobModel.ManufacturerId;
            var dsFillAggregatesByManufacturers =
                _helper.FillDropDownHelperMethodWithSp2("spGetAggregatesForManufacturers", _jobModel.ManufacturerId);
            var data = dsFillAggregatesByManufacturers.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();
            foreach (var row in data)
            {
                _jobModel.ServiceName = row["ServiceGroup_Name"].ToString();
                _jobModel.ServiceGroupId = Convert.ToInt32(row["ServiceGroup_Id"]);

                names.Add(_jobModel.ServiceName + "-" + _jobModel.ServiceGroupId);
            }
            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(list, "application/json");
        }

        // GetCategoriesForAggregates
        public ActionResult GetCategoriesForAggregates(string aggregateId)
        {
            _jobModel.AggregateId = int.Parse(aggregateId);
            Session["AggregateId"] = _jobModel.AggregateId;
            var dsFillCategoriesForAggregates =
                _helper.FillDropDownHelperMethodWithSp3("getCategoriesForAggregates", _jobModel.AggregateId);
            var data = dsFillCategoriesForAggregates.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();
            foreach (var row in data)
            {
                _jobModel.CategoryName = row["Categories"].ToString();
                _jobModel.IdCategory = Convert.ToInt32(row["Category_Id"]);

                names.Add(_jobModel.CategoryName + "-" + _jobModel.IdCategory);
            }
            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(list, "application/json");
        }

        [HttpPost]
        public ActionResult GetCategorySubCategoryCostDetails(int aggregateid, int manufacturerid)
        {
            var dtDetails = _helper.ExecuteSelectStmtusingSP("getAggregateCategoryDetails", "@aggregateid",
                aggregateid.ToString(), "@manufacturerid", manufacturerid.ToString());
            dtDetails.AsEnumerable().Select(x => new Aggregates
            {
                CategoryName = x.Field<string>("categories"),
                SubCategoryName = x.Field<string>("subcategories"),
                ApproxCost = x.Field<int>("ApproxCost")
            });
            Session["Aggregates"] = dtDetails;
            return RedirectToAction("GetManufacturerAggregatesDetails");
        }

        public ActionResult GetManufacturerAggregatesDetails()
        {
            var queryManufacturers = "select * from m_VehicleManufacturer";
            var dtManufacturers = _helper.ExecuteSelectStmt(queryManufacturers);
            ViewBag.Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName");
            _helper.ExecuteSelectStmtusingSP("getAggregateCategoryDetails");
            if (Session["Aggregates"] == null) return null;
            var dtDetails = Session["Aggregates"] as DataTable;
            var aggregates = dtDetails.AsEnumerable().Select(x => new Aggregates
            {
                IdCategory = x.Field<int>("Category_Id"),
                CategoryName = x.Field<string>("categories"),
                SubCategoryName = x.Field<string>("SubCategories"),
                ApproxCost = x.Field<int?>("ApproxCost"),
                ServiceId = x.Field<int>("Service_Id")
            });

            return View(aggregates);
        }

        public ActionResult EditSubCategoryCostDetails(int? id = 0)
        {
            if (id == null)
                return RedirectToAction("GetManufacturerAggregatesDetails");
            var categorystr = "select * from [M_FMS_Categories]";
            var subCategorystr = "select Service_Id,Service_Name from [dbo].[M_FMS_MaintenanceWorksMasterDetails] ";
            var dtCategories = _helper.ExecuteSelectStmt(categorystr);
            var dtSubcatStr = _helper.ExecuteSelectStmt(subCategorystr);
            var dtcategoryIdd = Session["Aggregates"] as DataTable;
            if (dtcategoryIdd == null) return null;
            var categoryIdd = dtcategoryIdd.AsEnumerable().FirstOrDefault(x => x.Field<int>("Service_Id") == id);
            var dtSubCostDetails =
                _helper.ExecuteSelectStmtusingSP("getJobCardVehicleDetails", "@jobcardnumber", id.ToString());
            _jobModel.Categories = new SelectList(dtCategories.AsDataView(), "Category_Id", "Categories");
            _jobModel.SubCategories = new SelectList(dtSubcatStr.AsDataView(), "Service_Id", "Service_Name");
            if (categoryIdd != null) _jobModel.IdCategory = Convert.ToInt32(categoryIdd["Category_Id"]);
            _jobModel.SubCategory = dtSubCostDetails.AsEnumerable().Select(x => x.Field<int>("Service_Id"))
                .FirstOrDefault();
            _jobModel.ApproxCost = dtSubCostDetails.AsEnumerable().Select(x => x.Field<int?>("CostFor_A_Grade"))
                .FirstOrDefault();
            return View(_jobModel);
        }

        [HttpPost]
        public ActionResult EditSubCategoryCostDetails(Aggregates aggregates)
        {
            _helper.ExecuteJobSubCategoryUpdateCost("spEditJobSubCategoryCostDetails", aggregates.ApproxCost,
                aggregates.SubCategory);
            return RedirectToAction("SubCategoryGroup");
        }
    }
}