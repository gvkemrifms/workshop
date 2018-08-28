using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Fleet_WorkShop.Models;
using Newtonsoft.Json;

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
            return Session["Employee_Id"] == null
                ? RedirectToAction("Login", "Account")
                : RedirectToAction("SaveVehicleDetails");
        }

        public ActionResult SaveVehicleDetails()
        {
            if (!ModelState.IsValid) return null;
            var dsVehicleDistrictDetails =
                _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            Session["VehicleDistrictDetails"] = dsVehicleDistrictDetails;
            ViewBag.Districts = new SelectList(dsVehicleDistrictDetails.Tables[0].AsDataView(), "Id", "District");
            ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id",
                "ManufacturerName");
            _vehModel.DistrictId = Convert.ToInt32(Session["Id"]);
            var dsGetVehicles = _helper.FillDropDownHelperMethodWithSp("spGetVehicleDetails");
            Session["GetVehicleDetails"] = dsGetVehicles;
            var vehModel = dsGetVehicles.Tables[0].AsEnumerable().ToList().Select(x => new VehicleModel
            {
                Id = x.Field<int>("Id"),
                DistrictName = x.Field<string>("District"),
                ManufacturerName = x.Field<string>("ManufacturerName"),
                VehicleNumberString = x.Field<string>("VehicleNumber"),
                ChasisNumber = x.Field<string>("ChasisNumber"),
                ModelYear = x.Field<int>("Model"),
                EngineNumber = x.Field<string>("EngineNumber"),
                LocationOfCommission = x.Field<string>("LocationOfCommission"),
                DateOfCommission = x.Field<DateTime?>("DateOfCommission")
            });
            return View(vehModel);
        }

        [HttpPost]
        public int SaveVehicleDetails(VehicleModel vehModel)
        {
            var vehDetails = new VehicleModel
            {
                VehicleNumber = vehModel.VehicleNumber,
                ManufacturerId = Convert.ToInt32(vehModel.ManufacturerId),
                DistrictId = Convert.ToInt32(vehModel.DistrictId),
                DateOfCommission = DateTime.Parse(vehModel.DateOfCommission.ToString()),
                Model = vehModel.Model,
                ChasisNumber = vehModel.ChasisNumber,
                EngineNumber = vehModel.EngineNumber,
                LocationOfCommission = vehModel.LocationOfCommission
            };
            var returnVal = _helper.ExecuteInsertVehicleDetails("InsetVehicleDetails", vehDetails.VehicleNumber,
                vehDetails.ManufacturerId, vehDetails.DistrictId, vehDetails.Model, vehDetails.ChasisNumber,
                vehDetails.EngineNumber, vehDetails.LocationOfCommission, vehDetails.DateOfCommission);


            return returnVal;
        }

        [HttpGet]
        public ActionResult Edit(int? id = null)
        {
            if (id == null)
                return RedirectToAction("SaveVehicleDetails");
            var dsGetVehicleManufacturers = Session["VehicleDistrictDetails"] as DataSet;
            var dsEditEmployee = Session["GetVehicleDetails"] as DataSet;
            var row = dsEditEmployee.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
            var model = new VehicleModel
            {
                District = new SelectList(dsGetVehicleManufacturers.Tables[0].AsDataView(), "Id", "District"),
                DistrictName = row["District"].ToString(),
                Manufacturer = new SelectList(dsGetVehicleManufacturers.Tables[1].AsDataView(), "Id",
                    "ManufacturerName"),
                ManufacturerName = row["ManufacturerName"].ToString(),
                VehicleNumberString = row["VehicleNumber"].ToString(),
                EngineNumber = row["EngineNumber"].ToString(),
                ModelYear = Convert.ToInt32(row["Model"]),
                ChasisNumber = row["ChasisNumber"].ToString(),
                DateOfCommission = Convert.ToDateTime(row["DateOfCommission"]),
                LocationOfCommission = row["LocationOfCommission"].ToString()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(VehicleModel postVehicle)
        {
            _helper.ExecuteVehicleUpdateStatement(postVehicle.Id, "spEditVehicle", postVehicle.DistrictId,
                postVehicle.ManufacturerId, postVehicle.VehicleNumberString, postVehicle.ModelYear,
                postVehicle.ChasisNumber, postVehicle.EngineNumber, postVehicle.LocationOfCommission,
                postVehicle.DateOfCommission);
            return RedirectToAction("SaveVehicleDetails");
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            _helper.ExecuteDeleteStatement("spDeleteVehicle", id);
            return RedirectToAction("SaveVehicleDetails");
        }

        public ActionResult GetLubesPendingStatusDetails()
        {
            var dtLubesPendingStatus = _helper.ExecuteSelectStmt("spGetVehiclesWithPendingStatus");
            var lubespendingCases = dtLubesPendingStatus.AsEnumerable().Select(x => new JobCardPendingCases
            {
                VehicleId = x.Field<Guid>("Id"),
                VehicleNumber = x.Field<string>("VehicleNumber"),
                DistrictName = x.Field<string>("District"),
                DateOfRepair = x.Field<DateTime>("DateOfRepair"),
                Complaint = x.Field<string>("Complaint"),
                WorkShopName = x.Field<string>("workshop_name"),
                EmployeeName = x.Field<string>("employeeName"),
                Status = x.Field<string>("status")
            });
            Session["LubesPendingStatus"] = dtLubesPendingStatus;
            var dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails");
            ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "id", "Designation");
            return View(lubespendingCases);
        }

        public PartialViewResult EditLubesPendingStatusDetails(Guid? id)
        {
            IEnumerable<JobCardPendingCases> lubespendingCases = new List<JobCardPendingCases>();
            if (id != null)
            {
                var dtLubePendingStatus = Session["LubesPendingStatus"] as DataTable;
                if (dtLubePendingStatus != null)
                    lubespendingCases = dtLubePendingStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id)
                        .Select(x => new JobCardPendingCases
                        {
                            VehicleId = x.Field<Guid>("Id"),
                            VehicleNumber = x.Field<string>("VehicleNumber"),
                            DistrictName = x.Field<string>("District"),
                            DateOfRepair = x.Field<DateTime>("DateOfRepair").Date,
                            Complaint = x.Field<string>("Complaint"),
                            WorkShopName = x.Field<string>("workshop_name"),
                            EmployeeName = x.Field<string>("employeeName"),
                            Status = x.Field<string>("status"),
                            JobCardNumber = x.Field<int>("JobCardNumber")
                        });
                Session["workshopName"] = lubespendingCases.Select(x => x.WorkShopName).FirstOrDefault();
            }
            var dtVehicleLubes = _helper.ExecuteSelectStmtusingSP("spGetVehicleLubricantDetails", null, null, null,
                null, "@vehicleNumber", lubespendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["spGetVehicleLubricantDetails"] = dtVehicleLubes;
            ViewBag.SpareParts = new SelectList(dtVehicleLubes.AsDataView(), "Id", "OilName");
            Session["JobCardNumber"] = lubespendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
            return PartialView("EditLubesPendingStatusDetails", lubespendingCases);
        }

        public ActionResult CheckVehicleNumber(string vehicleNumber)
        {
            var dtCheckVehicles = _helper.ExecuteSelectStmtusingSP("spCheckVehicleDetails", null, null, null, null,
                "@vehiclenumber", vehicleNumber);
            return dtCheckVehicles.Rows.Count == 0
                ? null
                : Json(dtCheckVehicles.Rows.Count, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPendingStatusDetails()
        {
            var dtPendingStatus = _helper.ExecuteSelectStmt("spGetVehiclesWithPendingStatus");
            IEnumerable<JobCardPendingCases> pendingCases =
                dtPendingStatus.AsEnumerable().Where(x => x.Field<long>("num") == 1).Select(
                    x => new JobCardPendingCases
                    {
                        VehicleId = x.Field<Guid>("Id"),
                        VehicleNumber = x.Field<string>("VehicleNumber"),
                        DistrictName = x.Field<string>("District"),
                        DateOfRepair = x.Field<DateTime>("DateOfRepair"),
                        Complaint = x.Field<string>("ServiceGroup_Name"),
                        WorkShopName = x.Field<string>("workshop_name"),
                        EmployeeName = x.Field<string>("Name"),
                        Status = x.Field<string>("status"),
                        JobCardNumber = x.Field<int>("JobCardNumber")
                    });
            Session["PendingStatus"] = dtPendingStatus;
            var dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails");
            ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "id", "Designation");
            return View(pendingCases);
        }

        public PartialViewResult EditPendingStatusDetails(Guid? id)
        {
            IEnumerable<JobCardPendingCases> pendingCases = new List<JobCardPendingCases>();
            if (id != null)
            {
                var dtPendingStatus = Session["PendingStatus"] as DataTable;
                if (dtPendingStatus != null)
                    pendingCases = dtPendingStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id)
                        .Select(x => new JobCardPendingCases
                        {
                            VehicleId = x.Field<Guid>("Id"),
                            VehicleNumber = x.Field<string>("VehicleNumber"),
                            DistrictName = x.Field<string>("District"),
                            DateOfRepair = x.Field<DateTime>("DateOfRepair").Date,
                            Complaint = x.Field<string>("Complaint"),
                            WorkShopName = x.Field<string>("workshop_name"),
                            EmployeeName = x.Field<string>("employeeName"),
                            Status = x.Field<string>("status"),
                            JobCardNumber = x.Field<int>("JobCardNumber")
                        });
                Session["workshopName"] = pendingCases.Select(x => x.WorkShopName).FirstOrDefault();
            }
            var dtVehicleSpareParts = _helper.ExecuteSelectStmtusingSP("spGetVehicleSpares", null, null, null, null,
                "@vehicleNumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["getVehicleSpares"] = dtVehicleSpareParts;
            ViewBag.SpareParts = new SelectList(dtVehicleSpareParts.AsDataView(), "Id", "PartName");
            Session["JobCardNumber"] = pendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
            return PartialView("_EditPendingStatusDetails", pendingCases);
        }


        public ActionResult EditPendingStatusDetails1(Guid? id)
        {
            IEnumerable<JobCardPendingCases> pendingCases = new List<JobCardPendingCases>();
            if (id != null)
            {
                var dtPendingStatus = Session["PendingStatus"] as DataTable;
                if (dtPendingStatus != null)
                    pendingCases = dtPendingStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id)
                        .Select(x => new JobCardPendingCases
                        {
                            VehicleId = x.Field<Guid>("Id"),
                            VehicleNumber = x.Field<string>("VehicleNumber"),
                            DistrictName = x.Field<string>("District"),
                            DateOfRepair = x.Field<DateTime>("DateOfRepair").Date,
                            Complaint = x.Field<string>("ServiceGroup_Name"),
                            WorkShopName = x.Field<string>("workshop_name"),
                            EmployeeName = x.Field<string>("Name"),
                            Status = x.Field<string>("status"),
                            JobCardNumber = x.Field<int>("JobCardNumber"),
                            OutSourcingAmount = x.Field<decimal?>("Amount")
                        });
                Session["workshopName"] = pendingCases.Select(x => x.WorkShopName).FirstOrDefault();
                ViewBag.VehicleNumber = pendingCases.Select(x => x.VehicleNumber).FirstOrDefault();
                ViewBag.DateOfRepair = pendingCases.Select(x => x.DateOfRepair).FirstOrDefault();
                ViewBag.WorkShopName = pendingCases.Select(x => x.WorkShopName).FirstOrDefault();
                ViewBag.Mechanic = pendingCases.Select(x => x.EmployeeName).FirstOrDefault();
                ViewBag.OutSourcingAmount = pendingCases.Select(x => x.OutSourcingAmount).FirstOrDefault();
            }
            else
            {
                return RedirectToAction("GetPendingStatusDetails");
            }

            var dtVehicleSpareParts = _helper.ExecuteSelectStmtusingSP("spGetVehicleSpares", null, null, null, null,
                "@vehicleNumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["getVehicleSpares"] = dtVehicleSpareParts;
            ViewBag.SpareParts = new SelectList(dtVehicleSpareParts.AsDataView(), "Id", "PartName");
            var dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails");
            ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "empId", "Name");
            var dtVehicleLubes = _helper.ExecuteSelectStmtusingSP("spGetVehicleLubricantDetails", null, null, null,
                null, "@vehicleNumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["spGetVehicleLubricantDetails"] = dtVehicleLubes;
            ViewBag.Lubes = new SelectList(dtVehicleLubes.AsDataView(), "Id", "OilName");
            Session["JobCardNumber"] = pendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
            ViewBag.VehicleNumber = pendingCases.Select(x => x.VehicleNumber).FirstOrDefault();
            var vehicleNumber = pendingCases.Select(x => x.VehicleNumber).FirstOrDefault();
            var dtgetComplaints = _helper.ExecuteSelectStmtusingSP("sp_getvehiclecomplaints", null, null, null, null,
                "@vehicleno", vehicleNumber);
            //------------------------------
            var dtvehicleInfoOnVehicleNumber = _helper.ExecuteSelectStmtusingSP("getVehicleInfoOnVehicleNumber", null,
                null, null, null, "@vehiclenumber", vehicleNumber);
            Session["VehicleInfoByNumber"] = dtvehicleInfoOnVehicleNumber;
            var vehicleId = dtvehicleInfoOnVehicleNumber.AsEnumerable().Select(x => x.Field<int>("VehicleId"))
                .FirstOrDefault();
            Session["VehicleId"] = vehicleId;
            var dtTotalCost =
                _helper.ExecuteSelectStmtusingSP("getTotalCostForVehicleNumber", "@vehicleid", vehicleId.ToString());
            var totalCost = dtTotalCost.AsEnumerable().Select(x => x.Field<int>("TotalCost")).FirstOrDefault();
            ViewBag.TotalCost = totalCost;
            //--------------------------------------------------------------------------------
            pendingCases = dtgetComplaints.AsEnumerable().Select(x => new JobCardPendingCases
            {
                VehicleIdData = x.Field<int>("VehicleId"),
                VehicleNumberData = x.Field<string>("VehicleNumber"),
                CategoryData = x.Field<string>("Categories"),
                CostApproximate = x.Field<int>("ApproxCost"),
                DateOfRepair = x.Field<DateTime>("Dor"),
                AggregateName = x.Field<string>("Aggregates"),
                SubCategoryName = x.Field<string>("Service_Name")
            });

            return View(pendingCases);
        }

        public ActionResult GetOutSourcingJobDetails(OutSourcingJobDetails outsourcing)
        {
            var vehicleId = Convert.ToInt32(Session["VehicleId"]);
            var returnVal = _helper.ExecuteUpdateOutSourcingJobDetails(vehicleId, "UpdateOutSoiurcingVehicleDetails",
                outsourcing.Vendor, outsourcing.WorkOrder, outsourcing.JobWork, outsourcing.CompletedDate,
                outsourcing.OutSourcingStatus, outsourcing.Amount);
            return Json(returnVal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddIssues(string vehicleNumber)
        {
            if (vehicleNumber == null)
                return RedirectToAction("GetPendingStatusDetails");
            var dtvehicleInfoOnVehicleNumber = _helper.ExecuteSelectStmtusingSP("getVehicleInfoOnVehicleNumber", null,
                null, null, null, "@vehiclenumber", vehicleNumber);
            Session["VehicleInfoByNumber"] = dtvehicleInfoOnVehicleNumber;
            var vehicleId = dtvehicleInfoOnVehicleNumber.AsEnumerable().Select(x => x.Field<int>("VehicleId"))
                .FirstOrDefault();
            var dtTotalCost =
                _helper.ExecuteSelectStmtusingSP("getTotalCostForVehicleNumber", "@vehicleid", vehicleId.ToString());
            var totalCost = dtTotalCost.AsEnumerable().Select(x => x.Field<int>("TotalCost")).FirstOrDefault();
            var manufacturerId = dtvehicleInfoOnVehicleNumber.AsEnumerable()
                .Select(x => new {ManufacturerId = x.Field<int>("ManufacturerId")}).FirstOrDefault();
            if (manufacturerId != null)
            {
                var dsFillAggregatesForManufacturer =
                    _helper.FillDropDownHelperMethodWithSp("getAggeregatesByManufacturers",
                        manufacturerId.ManufacturerId);
                ViewBag.Aggregates = new SelectList(dsFillAggregatesForManufacturer.Tables[0].AsDataView(),
                    "ServiceGroup_Id", "ServiceGroup_Name");
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddIssues(int aggregates, int categories, int subCategories)
        {
            var dtVehicleInfo = Session["VehicleInfoByNumber"] as DataTable;
            var costQuery =
                "select [CostFor_A_Grade] as [EstimatedCost] from dbo.M_FMS_MaintenanceWorksMasterDetails where Service_id = " +
                subCategories + "";
            var dtCostQuery = _helper.ExecuteSelectStmt(costQuery);

            var vehDetails = new VehicleModel
            {
                DistrictId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("DistrictId")).FirstOrDefault(),
                VehId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("VehicleId")).FirstOrDefault(),
                DateOfRepair = dtVehicleInfo.AsEnumerable().Select(x => x.Field<DateTime>("Dor")).FirstOrDefault(),
                ModelNumber = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("Model")).FirstOrDefault(),
                Odometer = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("Odometer")).FirstOrDefault(),
                ReceivedLocation = dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("ReceivedLoc"))
                    .FirstOrDefault(),
                PilotId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("PilotId")).FirstOrDefault(),
                PilotName = dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("PilotName")).FirstOrDefault(),
                ApproximateCost =
                    dtCostQuery.AsEnumerable().Select(x => x.Field<int>("EstimatedCost")).FirstOrDefault(),
                AllotedMechanic = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("AllotedMechanic"))
                    .FirstOrDefault(),
                WorkShopId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("WorkshopId")).FirstOrDefault(),
                ServiceEngineer = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("ServiceIncharge"))
                    .FirstOrDefault(),
                LaborCharges = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("LaborCharges")).FirstOrDefault(),
                ManufacturerId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("ManufacturerId"))
                    .FirstOrDefault(),
                Status = dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("Status")).FirstOrDefault(),
                DateOfDelivery = dtVehicleInfo.AsEnumerable().Select(x => x.Field<DateTime>("DateOfDelivery"))
                    .FirstOrDefault(),
                AggregateId = aggregates,
                IdCategory = categories,
                SubCategory = subCategories
            };
            var returnVal = _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", vehDetails.DistrictId,
                vehDetails.VehId, vehDetails.DateOfRepair, vehDetails.ModelNumber, vehDetails.Odometer,
                vehDetails.ReceivedLocation, vehDetails.PilotId, vehDetails.PilotName, vehDetails.DateOfDelivery,
                vehDetails.AggregateId, vehDetails.ApproximateCost, vehDetails.AllotedMechanic, vehDetails.WorkShopId,
                vehDetails.ServiceEngineer, vehDetails.LaborCharges, vehDetails.IdCategory, vehDetails.SubCategory,
                vehDetails.ManufacturerId);
            return Json(returnVal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPreviousRepairs(string vehicleNumber)
        {
            if (vehicleNumber == null)
                return RedirectToAction("GetPendingStatusDetails");
            var dtGetVehiclePreviousRepairDetails = _helper.ExecuteSelectStmtusingSP("getVehiclePreviousRepairs", null,
                null, null, null, "@vehiclenumber", vehicleNumber);
            var list = dtGetVehiclePreviousRepairDetails.AsEnumerable().Select(x => new JobCardPendingCases
            {
                JobCardNumber = x.Field<int>("JobCardNumber"),
                DistrictName = x.Field<string>("District"),
                DateOfRepair = x.Field<DateTime>("DateOfRepair"),
                AggregateName = x.Field<string>("ServiceGroup_Name"),
                LaborCharges = x.Field<int>("laborcharges"),
                WorkShopName = x.Field<string>("workshop_name"),
                Status = x.Field<string>("Status"),
                VehicleNumber = vehicleNumber
            });
            return View(list);
        }

        public ActionResult SaveCalculateFifo(VehicleModel pendingCases, string status)
        {
            var pendingStatusSpares = Session["pendingStatusSpareDetails"] as List<JobCardPendingCases>;

            if (pendingStatusSpares != null)
                foreach (var i in pendingStatusSpares)
                    pendingCases.itemmodel.Add(i);
            var result = 0;
            var workshopName = Session["workshopName"].ToString();
            var query = "select workshop_id from m_workshop where workshop_name='" + workshopName + "'";
            var dtWorkShopId = _helper.ExecuteSelectStmt(query);
            pendingCases.WorkShopId = dtWorkShopId.AsEnumerable().Select(x => x.Field<int>("workshop_id"))
                .FirstOrDefault();
            pendingCases.JobCardId = Convert.ToInt32(Session["JobCardNumber"]);
            var dtGetPartNumber = Session["getVehicleSpares"] as DataTable;
            if (dtGetPartNumber != null)
            {
                var spares = dtGetPartNumber.AsEnumerable()
                    .Select(x => new {PartName = x.Field<string>("partnumber"), PartId = x.Field<int>("Id")});
                foreach (var itemm in pendingCases.itemmodel)
                foreach (var spare in spares)
                    if (itemm.SparePartId == spare.PartId)
                    {
                        var dtcostDetails = _helper.ExecuteSelectStmtusingSP("GetCostBySparePartNumber", null, null,
                            null, null, "@partnumber", spare.PartName);
                        var sumOfQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).Sum();
                        if (itemm.Quantity <= sumOfQuantity)
                            if (itemm.Quantity <= dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity"))
                                    .FirstOrDefault())
                            {
                                var cost = dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("Cost"))
                                    .FirstOrDefault();
                                var totalAmount = cost * itemm.Quantity;
                                var res = _helper.ExecuteInsertSparesIssueStatement("InsertSpareIssueDetails",
                                    itemm.VehicleNumber, pendingCases.WorkShopId, itemm.SparePartId, itemm.Quantity,
                                    totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status);
                                if (res == 1)
                                {
                                    var itemTotalQuantity =
                                        dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity"))
                                            .FirstOrDefault();
                                    long receiptId = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Id"))
                                        .FirstOrDefault();
                                    var updatedQuantity = itemTotalQuantity - itemm.Quantity;

                                    _helper.ExecuteUpdateSparesIssueStatement("UpdateSpareIssueQuantityDetails",
                                        receiptId, updatedQuantity);
                                    return Json(res, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                foreach (DataRow row in dtcostDetails.Rows)
                                    if (Convert.ToInt32(itemm.Quantity) != 0)
                                    {
                                        var cost = Convert.ToDecimal(row["Cost"]);
                                        decimal totalAmount = 0;
                                        int qty;
                                        if (itemm.Quantity <= Convert.ToInt32(row["Quantity"]))
                                        {
                                            totalAmount = cost * itemm.Quantity;
                                            qty = itemm.Quantity;
                                        }
                                        else
                                        {
                                            totalAmount = cost * Convert.ToInt32(row["Quantity"]);
                                            qty = Convert.ToInt32(row["Quantity"]);
                                        }

                                        var res = _helper.ExecuteInsertSparesIssueStatement("InsertSpareIssueDetails",
                                            itemm.VehicleNumber, pendingCases.WorkShopId, itemm.SparePartId, qty,
                                            totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status);
                                        if (res == 1)
                                        {
                                            var itemTotalQuantity = Convert.ToInt32(row["Quantity"]);
                                            long receiptId = Convert.ToInt32(row["Id"]);
                                            var updatedQuantity = itemTotalQuantity - itemm.Quantity;
                                            var remainingQuantity = updatedQuantity;
                                            if (updatedQuantity <= 0)
                                                updatedQuantity = 0;

                                            result = _helper.ExecuteUpdateSparesIssueStatement(
                                                "UpdateSpareIssueQuantityDetails", receiptId, updatedQuantity);

                                            if (result == 1)
                                                if (remainingQuantity <= 0)
                                                    itemm.Quantity = -remainingQuantity;
                                            return Json(res, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                            }
                    }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCalculateLubesFifo(VehicleModel pendingCases, string status = null)
        {
            var result = 0;
            var workshopName = Session["workshopName"].ToString();
            var query = "select workshop_id from m_workshop where workshop_name='" + workshopName + "'";
            var dtWorkShopId = _helper.ExecuteSelectStmt(query);
            pendingCases.WorkShopId = dtWorkShopId.AsEnumerable().Select(x => x.Field<int>("workshop_id"))
                .FirstOrDefault();
            pendingCases.JobCardId = Convert.ToInt32(Session["JobCardNumber"]);
            var dtGetLubricantNumber = Session["spGetVehicleLubricantDetails"] as DataTable;
            if (dtGetLubricantNumber != null)
            {
                var spares = dtGetLubricantNumber.AsEnumerable()
                    .Select(x => new {PartName = x.Field<string>("LubricantNumber"), PartId = x.Field<int>("Id")});
                foreach (var itemm in pendingCases.itemmodel)
                foreach (var spare in spares)
                    if (itemm.LubricantId == spare.PartId)
                    {
                        var dtcostDetails = _helper.ExecuteSelectStmtusingSP("GetCostByLubricantNumber", null, null,
                            null, null, "@partnumber", spare.PartName);
                        var sumOfQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).Sum();
                        if (itemm.Quantity <= sumOfQuantity)
                            if (itemm.Quantity <= dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity"))
                                    .FirstOrDefault())
                            {
                                var cost = dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("CostPerLitre"))
                                    .FirstOrDefault();
                                var totalAmount = cost * itemm.Quantity;
                                var res = _helper.ExecuteInsertSparesIssueStatement("InsertLubeIssueDetails",
                                    itemm.VehicleNumber, pendingCases.WorkShopId, itemm.LubricantId, itemm.Quantity,
                                    totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status);
                                if (res == 1)
                                {
                                    var itemTotalQuantity =
                                        dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity"))
                                            .FirstOrDefault();
                                    long receiptId = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Id"))
                                        .FirstOrDefault();
                                    var updatedQuantity = itemTotalQuantity - itemm.Quantity;

                                    _helper.ExecuteUpdateSparesIssueStatement("UpdateLubesQuantityIssueDetails",
                                        receiptId, updatedQuantity);
                                }
                            }
                            else
                            {
                                foreach (DataRow row in dtcostDetails.Rows)
                                    if (Convert.ToInt32(itemm.Quantity) != 0)
                                    {
                                        var cost = Convert.ToDecimal(row["CostPerLitre"]);
                                        decimal totalAmount;
                                        int qty;
                                        if (itemm.Quantity <= Convert.ToInt32(row["Quantity"]))
                                        {
                                            totalAmount = cost * itemm.Quantity;
                                            qty = itemm.Quantity;
                                        }
                                        else
                                        {
                                            totalAmount = cost * Convert.ToInt32(row["Quantity"]);
                                            qty = Convert.ToInt32(row["Quantity"]);
                                        }

                                        var res = _helper.ExecuteInsertLubesIssueStatement("InsertLubesIssueDetails",
                                            itemm.VehicleNumber, pendingCases.WorkShopId, itemm.LubricantId, qty,
                                            totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status);
                                        switch (res)
                                        {
                                            case 1:
                                                var itemTotalQuantity = Convert.ToInt32(row["Quantity"]);
                                                long receiptId = Convert.ToInt32(row["Id"]);
                                                var updatedQuantity = itemTotalQuantity - itemm.Quantity;
                                                var remainingQuantity = updatedQuantity;
                                                if (updatedQuantity <= 0)
                                                    updatedQuantity = 0;

                                                result = _helper.ExecuteUpdateSparesIssueStatement(
                                                    "UpdateLubesIssueQuantityDetails", receiptId, updatedQuantity);
                                                switch (result)
                                                {
                                                    case 1:
                                                        if (remainingQuantity <= 0)
                                                            itemm.Quantity = -remainingQuantity;
                                                        break;
                                                }
                                                break;
                                        }
                                    }
                            }
                    }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSparePartCost(string spareId)
        {
            var getPendingSparesForCompletion = new List<JobCardPendingCases>();
            if (spareId == "") return Content("", "application/json");
            var costDetails = new List<JobCardPendingCases>();
            var vehicleSpareParts = Session["getVehicleSpares"] as DataTable;
            if (vehicleSpareParts != null)
            {
                var item = vehicleSpareParts.AsEnumerable().Where(x => x.Field<int>("Id") == Convert.ToInt32(spareId))
                    .Select(x => new
                    {
                        PartNumber = x.Field<string>("partnumber"),
                        ManufacturerId = x.Field<int>("ManufacturerId"),
                        VehicleNumber = x.Field<string>("VehicleNumber")
                    }).FirstOrDefault();
                if (item != null)
                {
                    var dtSparePArtscostDetails = _helper.ExecuteSelectStmtusingSP("spGetSparePartCostDetails",
                        "@manufacturerid", item.ManufacturerId.ToString(), null, null, "@partnumber", item.PartNumber);
                    var dtSpareIssueDetails = _helper.ExecuteSelectStmtusingSP("GetSpareIssueDetails", "@sparepartid",
                        spareId, null, null, "@vehiclenumber", item.VehicleNumber);

                    costDetails.AddRange(from DataRow row in dtSparePArtscostDetails.Rows
                        select new JobCardPendingCases
                        {
                            Manufacturer = row["ManufacturerName"].ToString(),
                            SparePart = row["PartName"].ToString(),
                            LastEntryDate = Convert.ToDateTime(row["lastentrydate"]).ToShortDateString(),
                            Cost = Convert.ToInt32(row["Cost"]),
                            Quantity = Convert.ToInt32(row["Quantity"])
                        });
                    if (dtSpareIssueDetails.Rows.Count > 0)
                        foreach (DataRow row in dtSpareIssueDetails.Rows)
                        {
                            var getSparesdetails =
                                new JobCardPendingCases
                                {
                                    SparePartId = Convert.ToInt32(spareId),
                                    Quantity = Convert.ToInt32(row["Quantity"]),
                                    TotalAmount = Convert.ToInt32(row["TotalAmount"]),
                                    IssuedDate = Convert.ToDateTime(row["IssuedDate"]).ToShortDateString(),
                                    StatusType = row["Status"].ToString()
                                };
                            costDetails.Add(getSparesdetails);
                            getPendingSparesForCompletion.Add(getSparesdetails);
                        }
                }
            }
            Session["pendingStatusSpareDetails"] = getPendingSparesForCompletion;
            var costDetails1 = JsonConvert.SerializeObject(costDetails, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(costDetails1, "application/json");
        }

        public ActionResult GetLubesCost(string lubricantId)
        {
            var costDetails = new List<JobCardPendingCases>();
            var vehicleLubes = Session["spGetVehicleLubricantDetails"] as DataTable;
            if (vehicleLubes != null)
            {
                var item = vehicleLubes.AsEnumerable().Where(x => x.Field<int>("Id") == Convert.ToInt32(lubricantId))
                    .Select(x => new
                    {
                        LubricantNumber = x.Field<string>("LubricantNumber"),
                        ManufacturerId = x.Field<int>("ManufacturerId"),
                        VehicleNumber = x.Field<string>("VehicleNumber")
                    }).FirstOrDefault();
                if (item != null)
                {
                    var dtLubricantcostDetails = _helper.ExecuteSelectStmtusingSP("spGetLubricantCostDetails",
                        "@manufacturerid", item.ManufacturerId.ToString(), null, null, "@partnumber",
                        item.LubricantNumber);
                    var dtLubesIssueDetails = _helper.ExecuteSelectStmtusingSP("GetLubesIssueDetails", "@lubricantid",
                        lubricantId, null, null, "@vehiclenumber", item.VehicleNumber);
                    foreach (DataRow row in dtLubricantcostDetails.Rows)
                    {
                        var details =
                            new JobCardPendingCases
                            {
                                Manufacturer = row["ManufacturerName"].ToString(),
                                SparePart = row["OilName"].ToString(),
                                LastEntryDate = Convert.ToDateTime(row["lastentrydate"]).ToShortDateString(),
                                Cost = Convert.ToInt32(row["CostPerLitre"]),
                                Quantity = Convert.ToInt32(row["Quantity"])
                            };
                        costDetails.Add(details);
                    }
                    if (dtLubesIssueDetails.Rows.Count > 0)
                        costDetails.AddRange(from DataRow row in dtLubesIssueDetails.Rows
                            select new JobCardPendingCases
                            {
                                Quantity = Convert.ToInt32(row["Quantity"]),
                                TotalAmount = Convert.ToInt32(row["TotalAmount"]),
                                IssuedDate = Convert.ToDateTime(row["IssuedDate"]).ToShortDateString(),
                                StatusType = row["Status"].ToString()
                            });
                }
            }
            var costDetails1 = JsonConvert.SerializeObject(costDetails, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(costDetails1, "application/json");
        }

        
    }
}