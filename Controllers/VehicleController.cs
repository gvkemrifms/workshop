using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Fleet_WorkShop.Models;
using Newtonsoft.Json;
using Rotativa.MVC;

namespace Fleet_WorkShop.Controllers
{
    public class VehicleController : Controller
    {
        private readonly EmployeeHelper _helper = new EmployeeHelper();
        private readonly VehicleModel _vehModel = new VehicleModel();

        public string UserName { get; set; }

        // GET: Vehicle
        public ActionResult GetVehicleDetails() { return Session["Employee_Id"] == null ? RedirectToAction("Login", "Account") : RedirectToAction("SaveVehicleDetails"); }

        public ActionResult SaveVehicleDetails()
        {
            if (!ModelState.IsValid) return null;

            using (var dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers"))
            {
                Session["VehicleDistrictDetails"] = dsVehicleDistrictDetails;
                ViewBag.Districts = new SelectList(dsVehicleDistrictDetails.Tables[0].AsDataView(), "Id", "District");
                ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName");
            }

            _vehModel.DistrictId = Convert.ToInt32(Session["Id"]);
            IEnumerable<VehicleModel> vehModel;

            using (var dsGetVehicles = _helper.FillDropDownHelperMethodWithSp("spGetVehicleDetails"))
            {
                Session["GetVehicleDetails"] = dsGetVehicles;
                vehModel = dsGetVehicles.Tables[0].AsEnumerable().ToList().Select(x => new VehicleModel {Id = x.Field<int>("Id"), DistrictName = x.Field<string>("District"), ManufacturerName = x.Field<string>("ManufacturerName"), VehicleNumberString = x.Field<string>("VehicleNumber"), ChasisNumber = x.Field<string>("ChasisNumber"), ModelYear = x.Field<int>("Model"), EngineNumber = x.Field<string>("EngineNumber"), LocationOfCommission = x.Field<string>("LocationOfCommission"), DateOfCommission = x.Field<DateTime?>("DateOfCommission")});
            }

            return View(vehModel);
        }

        [HttpPost] public int SaveVehicleDetails(VehicleModel vehModel)
        {
            var vehDetails = new VehicleModel {VehicleNumber = vehModel.VehicleNumber, ManufacturerId = Convert.ToInt32(vehModel.ManufacturerId), DistrictId = Convert.ToInt32(vehModel.DistrictId), DateOfCommission = DateTime.Parse(vehModel.DateOfCommission.ToString()), Model = vehModel.Model, ChasisNumber = vehModel.ChasisNumber, EngineNumber = vehModel.EngineNumber, LocationOfCommission = vehModel.LocationOfCommission};
            var returnVal = _helper.ExecuteInsertVehicleDetails("InsetVehicleDetails", vehDetails.VehicleNumber, vehDetails.ManufacturerId, vehDetails.DistrictId, vehDetails.Model, vehDetails.ChasisNumber, vehDetails.EngineNumber, vehDetails.LocationOfCommission, vehDetails.DateOfCommission);
            return returnVal;
        }

        [HttpGet] public ActionResult Edit(int? id = null)
        {
            if (id == null) return RedirectToAction("SaveVehicleDetails");
            VehicleModel model;

            using (var dsGetVehicleManufacturers = Session["VehicleDistrictDetails"] as DataSet)
            {
                var dsEditEmployee = Session["GetVehicleDetails"] as DataSet;
                var row = dsEditEmployee.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
                model = new VehicleModel {District = new SelectList(dsGetVehicleManufacturers.Tables[0].AsDataView(), "Id", "District"), DistrictName = row["District"].ToString(), Manufacturer = new SelectList(dsGetVehicleManufacturers.Tables[1].AsDataView(), "Id", "ManufacturerName"), ManufacturerName = row["ManufacturerName"].ToString(), VehicleNumberString = row["VehicleNumber"].ToString(), EngineNumber = row["EngineNumber"].ToString(), ModelYear = Convert.ToInt32(row["Model"]), ChasisNumber = row["ChasisNumber"].ToString(), DateOfCommission = Convert.ToDateTime(row["DateOfCommission"]), LocationOfCommission = row["LocationOfCommission"].ToString()};
            }

            return View(model);
        }

        [HttpPost] public ActionResult Edit(VehicleModel postVehicle)
        {
            _helper.ExecuteVehicleUpdateStatement(postVehicle.Id, "spEditVehicle", postVehicle.DistrictId, postVehicle.ManufacturerId, postVehicle.VehicleNumberString, postVehicle.ModelYear, postVehicle.ChasisNumber, postVehicle.EngineNumber, postVehicle.LocationOfCommission, postVehicle.DateOfCommission);
            return RedirectToAction("SaveVehicleDetails");
        }

        [HttpDelete] public ActionResult Delete(int id)
        {
            _helper.ExecuteDeleteStatement("spDeleteVehicle", id);
            return RedirectToAction("SaveVehicleDetails");
        }

        public ActionResult GetLubesPendingStatusDetails()
        {
            var dtLubesPendingStatus = _helper.ExecuteSelectStmt("spGetVehiclesWithPendingStatus");
            var lubespendingCases = dtLubesPendingStatus.AsEnumerable().Select(x => new JobCardPendingCases {VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair"), Complaint = x.Field<string>("Complaint"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("employeeName"), Status = x.Field<string>("status")});
            Session["LubesPendingStatus"] = dtLubesPendingStatus;

            using (var dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails"))
            {
                ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "id", "Designation");
            }

            return View(lubespendingCases);
        }

        public PartialViewResult EditLubesPendingStatusDetails(Guid? id)
        {
            IEnumerable<JobCardPendingCases> lubespendingCases = new List<JobCardPendingCases>();

            if (id != null)
            {
                var dtLubePendingStatus = Session["LubesPendingStatus"] as DataTable;
                if (dtLubePendingStatus != null) lubespendingCases = dtLubePendingStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id).Select(x => new JobCardPendingCases {VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair").Date, Complaint = x.Field<string>("Complaint"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("employeeName"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber")});
                Session["workshopName"] = lubespendingCases.Select(x => x.WorkShopName).FirstOrDefault();
            }

            using (var dtVehicleLubes = _helper.ExecuteSelectStmtusingSP("spGetVehicleLubricantDetails", null, null, null, null, "@vehicleNumber", lubespendingCases.Select(x => x.VehicleNumber).FirstOrDefault()))
            {
                Session["spGetVehicleLubricantDetails"] = dtVehicleLubes;
                ViewBag.SpareParts = new SelectList(dtVehicleLubes.AsDataView(), "Id", "OilName");
            }

            Session["JobCardNumber"] = lubespendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
            return PartialView("EditLubesPendingStatusDetails", lubespendingCases);
        }

        public ActionResult CheckVehicleNumber(string vehicleNumber)
        {
            using (var dtCheckVehicles = _helper.ExecuteSelectStmtusingSP("spCheckVehicleDetails", null, null, null, null, "@vehiclenumber", vehicleNumber))
            {
                return dtCheckVehicles.Rows.Count == 0 ? null : Json(dtCheckVehicles.Rows.Count, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPendingStatusDetails()
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            var workshopId = Session["WorkshopId"].ToString();
            IEnumerable<JobCardPendingCases> pendingCases;

            using (var dtPendingStatus = _helper.ExecuteSelectStmtusingSP("spGetVehiclesWithPendingStatus", "@workshopid", workshopId))
            {
                pendingCases = dtPendingStatus.AsEnumerable().Where(x => x.Field<long>("num") == 1).Select(x => new JobCardPendingCases {VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair"), DateOfDelivery = x.Field<DateTime>("DateOfDelivery"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("Name"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber"), Odometer = x.Field<int>("Odometer")});
                Session["PendingStatus"] = dtPendingStatus;
            }

            using (var dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails"))
            {
                ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "id", "Designation");
            }

            return View(pendingCases);
        }

        public ActionResult PrintPendingStatusDetails(Guid? id)
        {
            JobCardPendingCases pendingCases;
            var vehNum = "";

            using (var dtPrintPending = Session["PendingStatus"] as DataTable)
            {
                pendingCases = dtPrintPending.AsEnumerable().Where(x => x.Field<long>("num") == 1).Select(x => new JobCardPendingCases {VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair"), DateOfDelivery = x.Field<DateTime>("DateOfDelivery"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("Name"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber"), Odometer = x.Field<int>("Odometer")}).FirstOrDefault();
                if (pendingCases != null) vehNum = pendingCases.VehicleNumber;
                ViewBag.VehicleNumber = dtPrintPending.AsEnumerable().Select(x => x.Field<string>("VehicleNumber")).FirstOrDefault();
                ViewBag.District = dtPrintPending.AsEnumerable().Select(x => x.Field<string>("District")).FirstOrDefault();
                ViewBag.DOR = dtPrintPending.AsEnumerable().Select(x => x.Field<DateTime>("DateOfRepair")).FirstOrDefault();
                ViewBag.DOD = dtPrintPending.AsEnumerable().Select(x => x.Field<DateTime>("DateOfDelivery")).FirstOrDefault();
                ViewBag.Workshop = dtPrintPending.AsEnumerable().Select(x => x.Field<string>("workshop_name")).FirstOrDefault();
            }

            var dtgetComplaints = _helper.ExecuteSelectStmtusingSP("sp_getvehiclecomplaints", null, null, null, null, "@vehicleno", vehNum);
            ViewBag.Issues = dtgetComplaints;
            ViewBag.Cost = dtgetComplaints.AsEnumerable().Sum(x => x.Field<int>("ApproxCost"));
            return new PartialViewAsPdf("_GetPrintJobCardDetails", pendingCases);
        }

        public ActionResult GetInvoicePrint()
        {
            var completedCasesModel = Session["completedCasesModel"] as JobCardPendingCases;

            if (completedCasesModel != null)
            {
                var aggregateQuery = "select mw.ServiceGroup_Name from t_VehicleJobCardDetails vj join M_FMS_MaintenanceWorksServiceGroupDetails mw on vj.NatureOfComplaint=mw.ServiceGroup_Id where vj.VehicleId=" + completedCasesModel.VehicleIdData + "";
                ViewBag.NatureOfComplaint = _helper.ExecuteSelectStmt(aggregateQuery);
                var workshopdetails = "select* from m_workshop where workshop_name = '" + completedCasesModel.WorkShopName + "'";
                var dtWorkShopDetails = _helper.ExecuteSelectStmt(workshopdetails);
                ViewBag.Location = dtWorkShopDetails.AsEnumerable().Select(x => x.Field<string>("location")).FirstOrDefault();
                ViewBag.Mobile = dtWorkShopDetails.AsEnumerable().Select(x => x.Field<string>("phone")).FirstOrDefault();
            }

            if (Session == null) return new PartialViewAsPdf("_GetVehicleCompletedStatusDetails", completedCasesModel);

            if (completedCasesModel == null) return new PartialViewAsPdf("_GetVehicleCompletedStatusDetails", completedCasesModel);
            ViewBag.WorkShopName = completedCasesModel.WorkShopName;
            var vehicleNumber = completedCasesModel.VehicleNumber;
            ViewBag.VehicleNumber = vehicleNumber;
            ViewBag.VehId = completedCasesModel.VehicleIdData;
            ViewBag.AddressDetails = Session["AddressDetails"] as DataTable;
            ViewBag.ShowSparesGrid = Session["SparePartsGrid"] as DataTable;
            ViewBag.SparePartsBill = completedCasesModel.SparePartsBill;
            ViewBag.LaborBill = completedCasesModel.LaborCharges;
            ViewBag.GetLaborChargesReport = Session["LabourChargesGrid"] as DataTable;
            ViewBag.ShowLubesGrid = Session["LubesGrid"] as DataTable;
            ViewBag.GetOutSourcingDetailsList = Session["GetOutSourcingDetails"] as DataTable;
            ViewBag.OutSourceBill = completedCasesModel.OutSoucingSum;
            ViewBag.LubesBill = completedCasesModel.LubesBill;
            ViewBag.FinalBillAmount = completedCasesModel.FinalBillAmount;
            ViewBag.CompletedDate = Convert.ToDateTime(completedCasesModel.CompletedDate).ToShortDateString();
            ViewBag.DateOfRepair = Convert.ToDateTime(completedCasesModel.DateOfRepair).ToShortDateString();

            return new PartialViewAsPdf("_GetVehicleCompletedStatusDetails", completedCasesModel);
        }

        public ActionResult GetCompletedStatusDetails()
        {
            var dtCompletedStatus = _helper.ExecuteSelectStmt("spGetVehiclesWithCompletedStatus");
            IEnumerable<JobCardPendingCases> completedCases = dtCompletedStatus.AsEnumerable().Where(x => x.Field<long>("num") == 1).Select(x => new JobCardPendingCases {VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair"), Complaint = x.Field<string>("ServiceGroup_Name"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("Name"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber"), CompletedDate = x.Field<DateTime?>("CompletedDate")});
            Session["CompletedStatus"] = dtCompletedStatus;
            var dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails");
            ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "id", "Designation");
            return View(completedCases);
        }

        public PartialViewResult EditPendingStatusDetails(Guid? id)
        {
            IEnumerable<JobCardPendingCases> pendingCases = new List<JobCardPendingCases>();

            if (id != null)
            {
                var dtPendingStatus = Session["PendingStatus"] as DataTable;
                if (dtPendingStatus != null) pendingCases = dtPendingStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id).Select(x => new JobCardPendingCases {VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair").Date, Complaint = x.Field<string>("Complaint"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("employeeName"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber")});
                Session["workshopName"] = pendingCases.Select(x => x.WorkShopName).FirstOrDefault();
            }

            var dtVehicleSpareParts = _helper.ExecuteSelectStmtusingSP("spGetVehicleSpares", null, null, null, null, "@vehicleNumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["getVehicleSpares"] = dtVehicleSpareParts;
            ViewBag.SpareParts = new SelectList(dtVehicleSpareParts.AsDataView(), "Id", "PartName");
            Session["JobCardNumber"] = pendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
            return PartialView("_EditPendingStatusDetails", pendingCases);
        }

        public ActionResult GetVehicleCompletedStatusDetails(Guid? id)
        {
            if (id == null) return RedirectToAction("GetCompletedStatusDetails");
            var completedCasesModel = new JobCardPendingCases();
            IEnumerable<JobCardPendingCases> completedCases = new List<JobCardPendingCases>();

            if (id != null)
            {
                var dtCompletedStatus = Session["CompletedStatus"] as DataTable;
                if (dtCompletedStatus != null) completedCases = dtCompletedStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id).Select(x => new JobCardPendingCases {VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair").Date, CompletedDate = x.Field<DateTime?>("CompletedDate"), Complaint = x.Field<string>("ServiceGroup_Name"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("Name"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber"), OutSourcingAmount = x.Field<decimal?>("Amount")});
                ViewBag.WorkShopName = completedCases.Select(x => x.WorkShopName).FirstOrDefault();
                completedCasesModel.WorkShopName = completedCases.Select(x => x.WorkShopName).FirstOrDefault();
                completedCasesModel.VehicleNumber = completedCases.Select(x => x.VehicleNumber).FirstOrDefault();
                completedCasesModel.CompletedDate = completedCases.Select(x => x.CompletedDate).FirstOrDefault();
                completedCasesModel.DateOfRepair = completedCases.Select(x => x.DateOfRepair).FirstOrDefault();
                ViewBag.VehicleNumber = completedCasesModel.VehicleNumber;
                var getVehIdQuery = "select Id from m_GetVehicleDetails where VehicleNumber='" + completedCasesModel.VehicleNumber + "'";
                var dtVeh = _helper.ExecuteSelectStmt(getVehIdQuery);
                var vehId = dtVeh.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
                completedCasesModel.VehicleIdData = vehId;
                var getAddressDetails = "select top 1 d.District,jc.ReceivedLoc as Location,jc.Odometer from [t_VehicleJobCardDetails] jc join m_districts d on d.Id=jc.DistrictId where jc.VehicleId=" + vehId + "";
                var dtVehicleRepairInfo = _helper.ExecuteSelectStmt(getAddressDetails);
                ViewBag.AddressDetails = dtVehicleRepairInfo;
                Session["AddressDetails"] = dtVehicleRepairInfo;
                var showSparePartGrid = _helper.ExecuteSelectStmtusingSP("spGetSparePartGrid", null, null, null, null, "@vehiclenumber", completedCasesModel.VehicleNumber);
                var sParesSum = showSparePartGrid.AsEnumerable().Sum(x => x.Field<decimal>("totalamount"));
                ViewBag.SparePartsBill = sParesSum;
                completedCasesModel.SparePartsBill = sParesSum;
                ViewBag.ShowSparesGrid = showSparePartGrid;
                Session["SparePartsGrid"] = showSparePartGrid;
                var dtGetLaborChargesReport = _helper.ExecuteSelectStmtusingSP("GetLaborChargesReport", "@vehicleid", vehId.ToString());
                ViewBag.GetLaborChargesReport = dtGetLaborChargesReport;
                Session["LabourChargesGrid"] = dtGetLaborChargesReport;
                var labourSum = dtGetLaborChargesReport.AsEnumerable().Sum(x => x.Field<int>("LaborCharges"));

                ViewBag.LaborBill = labourSum;
                completedCasesModel.LaborCharges = labourSum;
                var showLubesGrid = _helper.ExecuteSelectStmtusingSP("spGetLubesGrid", null, null, null, null, "@vehiclenumber", completedCasesModel.VehicleNumber);
                ViewBag.ShowLubesGrid = showLubesGrid;
                Session["LubesGrid"] = showLubesGrid;
                var lubesSum = showLubesGrid.AsEnumerable().Sum(x => x.Field<decimal>("totalamount"));
                ViewBag.LubesBill = lubesSum;
                completedCasesModel.LubesBill = lubesSum;
                var getoutSoucingDetails = "select top 1 VendorName,jobwork,workorder,CompletedDate,Amount,outsourcingStatus from t_VehicleJobCardDetails where vehicleid=" + vehId + "";
                var dtOutSoucingDetails = _helper.ExecuteSelectStmt(getoutSoucingDetails);
                ViewBag.GetOutSourcingDetails = dtOutSoucingDetails;
                Session["GetOutSourcingDetails"] = dtOutSoucingDetails;
                var outSourcingSum = dtOutSoucingDetails.AsEnumerable().Sum(x => x.Field<decimal?>("Amount")) ?? 0;

                ViewBag.OutSourcingSum = outSourcingSum;
                completedCasesModel.OutSoucingSum = Convert.ToDecimal(outSourcingSum);
                var finalBillAmount = sParesSum + labourSum + lubesSum + Convert.ToDecimal(outSourcingSum);
                ViewBag.FinalBillAmount = finalBillAmount;
                completedCasesModel.FinalBillAmount = finalBillAmount;
                Session["completedCasesModel"] = completedCasesModel;
            }

            var dtVehicleSpareParts = _helper.ExecuteSelectStmtusingSP("spGetVehicleSpares", null, null, null, null, "@vehicleNumber", completedCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["getVehicleSpares"] = dtVehicleSpareParts;
            return View();
        }

        public ActionResult EditPendingStatusDetails1(Guid? id)
        {
            decimal? outSourcingAmount = 0;
            IEnumerable<JobCardPendingCases> pendingCases = new List<JobCardPendingCases>();

            if (id == null) return RedirectToAction("GetPendingStatusDetails");

            var dtPendingStatus = Session["PendingStatus"] as DataTable;
            if (dtPendingStatus != null) pendingCases = dtPendingStatus.AsEnumerable().Where(x => x.Field<Guid>("Id") == id).Select(x => new JobCardPendingCases {VehicleId = x.Field<Guid>("Id"), VehicleNumber = x.Field<string>("VehicleNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair"), DateOfDelivery = x.Field<DateTime>("DateOfDelivery"), WorkShopName = x.Field<string>("workshop_name"), EmployeeName = x.Field<string>("Name"), Status = x.Field<string>("status"), JobCardNumber = x.Field<int>("JobCardNumber"), OutSourcingAmount = x.Field<decimal?>("Amount"), ManufacturerId = x.Field<int?>("ManufacturerId")});
            Session["VehicleIdsOnJobCard"] = pendingCases.Select(x => x.VehicleId).FirstOrDefault();
            Session["workshopName"] = pendingCases.Select(x => x.WorkShopName).FirstOrDefault();
            ViewBag.VehicleNumber = pendingCases.Select(x => x.VehicleNumber).FirstOrDefault();
            ViewBag.DateOfRepair = pendingCases.Select(x => x.DateOfRepair).FirstOrDefault();
            ViewBag.WorkShopName = pendingCases.Select(x => x.WorkShopName).FirstOrDefault();
            ViewBag.Mechanic = pendingCases.Select(x => x.EmployeeName).FirstOrDefault();
            ViewBag.DateOfDelivery = pendingCases.Select(x => x.DateOfDelivery).FirstOrDefault();
            outSourcingAmount = pendingCases.Select(x => x.OutSourcingAmount).FirstOrDefault() ?? 0;
            ViewBag.OutSourcingAmount = outSourcingAmount;
            if (pendingCases.Select(x => x.Status).FirstOrDefault() == "Completed") return RedirectToAction("GetPendingStatusDetails");

            var dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            ViewBag.AllotedMechanic = new SelectList(dsVehicleDistrictDetails.Tables[3].AsDataView(), "empId", "name");
            var dtgrabInprogressSpares = _helper.ExecuteSelectStmtusingSP("getSparesIssueGrid", null, null, null, null, "@vehiclenumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            ViewBag.InprogressSpares = dtgrabInprogressSpares;
            var totalSparesPendingCost = dtgrabInprogressSpares.AsEnumerable().Sum(x => x.Field<decimal?>("TotalCost"));
            ViewBag.TotalSparesPendingCost = totalSparesPendingCost;
            var dtgrabInprogressLubes = _helper.ExecuteSelectStmtusingSP("getLubesIssueGrid", null, null, null, null, "@vehiclenumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            ViewBag.InprogressLubes = dtgrabInprogressLubes;
            var totalLubesPendingCost = dtgrabInprogressLubes.AsEnumerable().Sum(x => x.Field<decimal?>("TotalCost"));
            ViewBag.TotalLubesPendingCost = totalLubesPendingCost;
            var dtVehicleSpareParts = _helper.ExecuteSelectStmtusingSP("spGetVehicleSpares", "@manufacturerid", Convert.ToString(pendingCases.Select(x => x.ManufacturerId).FirstOrDefault()), null, null, "@vehicleNumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["getVehicleSpares"] = dtVehicleSpareParts;
            ViewBag.SpareParts = new SelectList(dtVehicleSpareParts.AsDataView(), "Id", "PartName");
            var dtHandOver = _helper.ExecuteSelectStmtusingSP("spGetDesidnationDetails");
            ViewBag.HandOver = new SelectList(dtHandOver.AsDataView(), "empId", "Name");
            var dtVehicleLubes = _helper.ExecuteSelectStmtusingSP("spGetVehicleLubricantDetails", "@manufacturerid", Convert.ToString(pendingCases.Select(x => x.ManufacturerId).FirstOrDefault()), null, null, "@vehicleNumber", pendingCases.Select(x => x.VehicleNumber).FirstOrDefault());
            Session["spGetVehicleLubricantDetails"] = dtVehicleLubes;
            ViewBag.Lubes = new SelectList(dtVehicleLubes.AsDataView(), "Id", "OilName");
            Session["JobCardNumber"] = pendingCases.Select(x => x.JobCardNumber).FirstOrDefault();
            ViewBag.VehicleNumber = pendingCases.Select(x => x.VehicleNumber).FirstOrDefault();
            var vehicleNumber = pendingCases.Select(x => x.VehicleNumber).FirstOrDefault();
            var dtgetComplaints = _helper.ExecuteSelectStmtusingSP("sp_getvehiclecomplaints", null, null, null, null, "@vehicleno", vehicleNumber);
            //------------------------------
            var dtvehicleInfoOnVehicleNumber = _helper.ExecuteSelectStmtusingSP("getVehicleInfoOnVehicleNumber", null, null, null, null, "@vehiclenumber", vehicleNumber);
            Session["VehicleInfoByNumber"] = dtvehicleInfoOnVehicleNumber;
            if (dtvehicleInfoOnVehicleNumber == null || dtvehicleInfoOnVehicleNumber.Rows.Count <= 0) return null;
            var vehicleId = dtvehicleInfoOnVehicleNumber.AsEnumerable().Select(x => x.Field<int>("VehicleId")).FirstOrDefault();
            Session["VehicleId"] = vehicleId;
            Session["VehicNumber"] = vehicleNumber;
            var dtTotalCost = _helper.ExecuteSelectStmtusingSP("getTotalCostForVehicleNumber", "@vehicleid", vehicleId.ToString());
            var totalCost = dtTotalCost.AsEnumerable().Select(x => x.Field<int?>("TotalCost")).FirstOrDefault();
            ViewBag.TotalCost = totalCost;
            ViewBag.TotalBillAmount = totalSparesPendingCost + totalCost + totalLubesPendingCost + outSourcingAmount;
            //--------------------------------------------------------------------------------
            pendingCases = dtgetComplaints.AsEnumerable().Select(x => new JobCardPendingCases {JobCardNumber = x.Field<int>("jobcardnumber"), VehicleIdData = x.Field<int>("VehicleId"), VehicleNumberData = x.Field<string>("VehicleNumber"), CategoryData = x.Field<string>("Categories"), CostApproximate = x.Field<int>("ApproxCost"), DateOfRepair = x.Field<DateTime>("Dor"), AggregateName = x.Field<string>("Aggregates"), SubCategoryName = x.Field<string>("Service_Name"), EstimatedTime = x.Field<string>("EstimatedTime"), StartTime1 = x.Field<DateTime?>("starttime"), EndTime1 = x.Field<DateTime?>("endtime")});

            return View(pendingCases);
        }

        public ActionResult UpdateJobCardIssues(int? id, string startdate, string enddate, int? mechanicId)
        {
            int result;

            if (startdate != null && enddate != null && startdate != "" && enddate != "")
            {
                var startDate = DateTime.Parse(startdate);
                var attendant = DateTime.Parse(enddate);

                if (startDate > attendant)
                {
                    result = -1;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            if (startdate != "" && enddate != "" && mechanicId != null && startdate != null && enddate != null)
            {
                var startDate = DateTime.Parse(startdate);
                var starterDate = startDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                var attendant = DateTime.Parse(enddate);
                var trendDate = attendant.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                var mechanistic = "select jobcardnumber,AllotedMechanic from t_VehicleJobCardDetails where starttime>= '" + starterDate + "' and endtime<= '" + trendDate + "' and AllotedMechanic=" + mechanicId + "";
                var dtMechanicAvailability = _helper.ExecuteSelectStmt(mechanistic);

                if (dtMechanicAvailability.Rows.Count > 0)
                {
                    result = -2;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                result = _helper.ExecuteInsertCheckStartAndEndDateDetails("UpdateTimeTakenForAggregates", Convert.ToInt32(id), Convert.ToDateTime(startdate), Convert.ToDateTime(enddate), Convert.ToInt32(mechanicId));
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            result = 0;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteStockIssueDetails(int? id)
        {
            var qty = "select * from t_SparepartsIssue where Id=" + id + "";
            var dtgetQty = _helper.ExecuteSelectStmt(qty);
            var quantityIssued = dtgetQty.AsEnumerable().Select(x => x.Field<int>("quantity")).FirstOrDefault();
            var billnumber = dtgetQty.AsEnumerable().Select(x => x.Field<string>("BillNumber")).FirstOrDefault();
            var sparepartId = dtgetQty.AsEnumerable().Select(x => x.Field<int>("SparePartId")).FirstOrDefault();
            var updatestocks = "update [t_SpareParts_Stock] set quantity=(quantity + " + quantityIssued + ") where BillNumber='" + billnumber + "' and SparePartId=" + sparepartId + "";
            _helper.ExecuteSelectStmt(updatestocks);
            var count = _helper.ExecuteInsertStmtusingSp("RemoveIssuedStock", "@id", id.ToString());

            return Json(count, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteLubesIssueDetails(int? id)
        {
            var qty = "select * from t_lubesIssue where Id=" + id + "";
            var dtgetQty = _helper.ExecuteSelectStmt(qty);
            var quantityIssued = dtgetQty.AsEnumerable().Select(x => x.Field<decimal>("quantity")).FirstOrDefault();
            var lubricantId = dtgetQty.AsEnumerable().Select(x => x.Field<int>("LubricantId")).FirstOrDefault();
            var updatestocks = "update [t_lubes_Stock] set quantity=(quantity + " + quantityIssued + ") where lubricantId=" + lubricantId + "";
            _helper.ExecuteSelectStmt(updatestocks);
            var count = _helper.ExecuteInsertStmtusingSp("RemoveIssuedLubesStock", "@id", id.ToString());

            return Json(count, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOutSourcingJobDetails(OutSourcingJobDetails outsourcing)
        {
            var returnVal = 0;

            if (outsourcing.Amount != 0 && outsourcing.Vendor != null && outsourcing.WorkOrder != null && outsourcing.OutSourcingStatus != 0 && outsourcing.CompletedDate != null)
            {
                var vehicleId = Convert.ToInt32(Session["VehicleId"]);
                returnVal = _helper.ExecuteUpdateOutSourcingJobDetails(vehicleId, "UpdateOutSoiurcingVehicleDetails", outsourcing.Vendor, outsourcing.WorkOrder, outsourcing.JobWork, outsourcing.CompletedDate, outsourcing.OutSourcingStatus, outsourcing.Amount);
                return Json(returnVal, JsonRequestBehavior.AllowGet);
            }

            return Json(returnVal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddIssues(string vehicleNumber)
        {
            Session["VehicleNumber"] = vehicleNumber;
            if (vehicleNumber == null) return RedirectToAction("GetPendingStatusDetails");
            var dtvehicleInfoOnVehicleNumber = _helper.ExecuteSelectStmtusingSP("getVehicleInfoOnVehicleNumber", null, null, null, null, "@vehiclenumber", vehicleNumber);
            Session["VehicleInfoByNumber"] = dtvehicleInfoOnVehicleNumber;
            var vehicleId = dtvehicleInfoOnVehicleNumber.AsEnumerable().Select(x => x.Field<int>("VehicleId")).FirstOrDefault();
            var dtTotalCost = _helper.ExecuteSelectStmtusingSP("getTotalCostForVehicleNumber", "@vehicleid", vehicleId.ToString());
            var totalCost = dtTotalCost.AsEnumerable().Select(x => x.Field<int>("TotalCost")).FirstOrDefault();
            var manufacturerId = dtvehicleInfoOnVehicleNumber.AsEnumerable().Select(x => new {ManufacturerId = x.Field<int>("ManufacturerId")}).FirstOrDefault();

            if (manufacturerId != null)
            {
                var dsFillAggregatesForManufacturer = _helper.FillDropDownHelperMethodWithSp("getAggeregatesByManufacturers", manufacturerId.ManufacturerId);
                ViewBag.Aggregates = new SelectList(dsFillAggregatesForManufacturer.Tables[0].AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");
            }

            return View();
        }

        [HttpPost] public ActionResult AddIssues(int aggregates, int categories, int subCategories)
        {
            var vehicleNumber = Session["VehicleNumber"].ToString();
            int emt;
            int pm;
            int rm;

            using (var dtGetEmpmre = _helper.ExecuteSelectStmtusingSP("spGetEMEPMRM", null, null, null, null, "@vehiclenumber", vehicleNumber))
            {
                emt = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "EMT").Select(x => x.Field<int>("empid")).FirstOrDefault();
                pm = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "PM").Select(x => x.Field<int>("empid")).FirstOrDefault();
                rm = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "RM").Select(x => x.Field<int>("empid")).FirstOrDefault();
            }

            VehicleModel vehDetails;

            using (var dtVehicleInfo = Session["VehicleInfoByNumber"] as DataTable)
            {
                var costQuery = "select [CostFor_A_Grade] as [EstimatedCost] from dbo.M_FMS_MaintenanceWorksMasterDetails where Service_id = " + subCategories + "";
                var dtCostQuery = _helper.ExecuteSelectStmt(costQuery);
                vehDetails = new VehicleModel {DistrictId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("DistrictId")).FirstOrDefault(), VehId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("VehicleId")).FirstOrDefault(), DateOfRepair = dtVehicleInfo.AsEnumerable().Select(x => x.Field<DateTime>("Dor")).FirstOrDefault(), ModelNumber = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("Model")).FirstOrDefault(), Odometer = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("Odometer")).FirstOrDefault(), ReceivedLocation = dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("ReceivedLoc")).FirstOrDefault(), PilotId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("PilotId")).FirstOrDefault(), PilotName = dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("PilotName")).FirstOrDefault(), ApproximateCost = dtCostQuery.AsEnumerable().Select(x => x.Field<int>("EstimatedCost")).FirstOrDefault(), AllotedMechanic = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("AllotedMechanic")).FirstOrDefault(), WorkShopId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("WorkshopId")).FirstOrDefault(), ServiceEngineer = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("ServiceIncharge")).FirstOrDefault(), ManufacturerId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("ManufacturerId")).FirstOrDefault(), Status = dtVehicleInfo.AsEnumerable().Select(x => x.Field<string>("Status")).FirstOrDefault(), DateOfDelivery = dtVehicleInfo.AsEnumerable().Select(x => x.Field<DateTime>("DateOfDelivery")).FirstOrDefault(), AggregateId = aggregates, IdCategory = categories, SubCategory = subCategories, DistanceTravelled = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("DistanceTravelled")).FirstOrDefault(), OffroadId = dtVehicleInfo.AsEnumerable().Select(x => x.Field<int>("OffroadId")).FirstOrDefault()};
            }

            int helperId;

            using (var dtgetHelperId = _helper.ExecuteSelectStmtusingSP("gethelperId", "@vehicleid", vehDetails.VehId.ToString()))
            {
                helperId = dtgetHelperId.AsEnumerable().Select(x => x.Field<int>("helperid")).FirstOrDefault();
            }

            var returnVal = _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", vehDetails.DistrictId, vehDetails.VehId, vehDetails.DateOfRepair, vehDetails.ModelNumber, vehDetails.Odometer, vehDetails.ReceivedLocation, vehDetails.PilotId, vehDetails.PilotName, vehDetails.DateOfDelivery, vehDetails.AggregateId, vehDetails.ApproximateCost, vehDetails.AllotedMechanic, vehDetails.WorkShopId, vehDetails.ServiceEngineer, vehDetails.IdCategory, vehDetails.SubCategory, vehDetails.ManufacturerId, Convert.ToInt32(rm), Convert.ToInt32(pm), Convert.ToInt32(emt), helperId, vehDetails.DistanceTravelled, 0, 0, vehDetails.OffroadId);
            return Json(returnVal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPreviousRepairs(string vehicleNumber)
        {
            if (vehicleNumber == null) return RedirectToAction("GetPendingStatusDetails");
            EnumerableRowCollection<JobCardPendingCases> list;

            using (var dtGetVehiclePreviousRepairDetails = _helper.ExecuteSelectStmtusingSP("getVehiclePreviousRepairs", null, null, null, null, "@vehiclenumber", vehicleNumber))
            {
                list = dtGetVehiclePreviousRepairDetails.AsEnumerable().Select(x => new JobCardPendingCases {JobCardNumber = x.Field<int>("JobCardNumber"), DistrictName = x.Field<string>("District"), DateOfRepair = x.Field<DateTime>("DateOfRepair"), AggregateName = x.Field<string>("ServiceGroup_Name"), WorkShopName = x.Field<string>("workshop_name"), Status = x.Field<string>("Status"), VehicleNumber = vehicleNumber});
            }

            return View(list);
        }

        public ActionResult FlipStatus(string Status, int? EndOdo = 0)
        {
            UpdateStatusToComplete(Status, EndOdo, Session["VehicNumber"].ToString());
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCalculateFifo(VehicleModel pendingCases, string status, int? EndOdo = 0)
        {
            if (pendingCases.itemmodel == null && status == "Completed")
            {
                UpdateStatusToComplete(status, EndOdo, Session["VehicNumber"].ToString());
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            var vehicleNumber = pendingCases.itemmodel.AsEnumerable().Select(x => x.VehicleNumber).FirstOrDefault();
            var pendingStatusSpares = Session["pendingStatusSpareDetails"] as List<JobCardPendingCases>;

            if (pendingStatusSpares != null)
                foreach (var i in pendingStatusSpares)
                    pendingCases.itemmodel.Add(i);
            var result = 0;
            var res = 0;
            var workshopName = Session["workshopName"].ToString();
            var query = "select workshop_id from m_workshop where workshop_name='" + workshopName + "'";

            using (var dtWorkShopId = _helper.ExecuteSelectStmt(query))
            {
                pendingCases.WorkShopId = dtWorkShopId.AsEnumerable().Select(x => x.Field<int>("workshop_id")).FirstOrDefault();
            }

            pendingCases.JobCardId = Convert.ToInt32(Session["JobCardNumber"]);
            var dtGetPartNumber = Session["getVehicleSpares"] as DataTable;

            if (dtGetPartNumber != null)
            {
                var spares = dtGetPartNumber.AsEnumerable().Select(x => new {PartName = x.Field<string>("partnumber"), PartId = x.Field<int>("Id")});

                foreach (var itemm in pendingCases.itemmodel)
                foreach (var spare in spares)
                    if (itemm.SparePartId == spare.PartId)
                    {
                        var dtcostDetails = _helper.ExecuteSelectStmtusingSP("GetCostBySparePartNumber", null, null, null, null, "@partnumber", spare.PartName);
                        var sumOfQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).Sum();

                        if (itemm.Quantity <= sumOfQuantity)
                            if (itemm.Quantity <= dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).FirstOrDefault())
                            {
                                var cost = dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("Cost")).FirstOrDefault();
                                var billNumber = dtcostDetails.AsEnumerable().Select(x => x.Field<string>("BillNumber")).FirstOrDefault();
                                var totalAmount = cost * itemm.Quantity;
                                res = _helper.ExecuteInsertSparesIssueStatement("InsertSpareIssueDetails", itemm.VehicleNumber, pendingCases.WorkShopId, itemm.SparePartId, itemm.Quantity, totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status, Convert.ToString(billNumber, CultureInfo.CurrentCulture));

                                if (res == 1)
                                {
                                    var itemTotalQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Quantity")).FirstOrDefault();
                                    long receiptId = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
                                    var updatedQuantity = itemTotalQuantity - itemm.Quantity;

                                    _helper.ExecuteUpdateSparesIssueStatement("UpdateSpareIssueQuantityDetails", receiptId, updatedQuantity);

                                    if (status == "Completed") UpdateStatusToComplete(status, EndOdo, vehicleNumber);
                                }
                            }
                            else
                            {
                                foreach (DataRow row in dtcostDetails.Rows)
                                    if (Convert.ToInt32(itemm.Quantity) != 0 && Convert.ToInt32(row["Quantity"]) != 0)
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

                                        var billNumber = row["BillNumber"].ToString();
                                        res = _helper.ExecuteInsertSparesIssueStatement("InsertSpareIssueDetails", itemm.VehicleNumber, pendingCases.WorkShopId, itemm.SparePartId, qty, totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status, Convert.ToString(billNumber, CultureInfo.CurrentCulture));

                                        if (res == 1)
                                        {
                                            var itemTotalQuantity = Convert.ToInt32(row["Quantity"]);
                                            long receiptId = Convert.ToInt32(row["Id"]);
                                            var updatedQuantity = itemTotalQuantity - itemm.Quantity;
                                            var remainingQuantity = updatedQuantity;
                                            if (updatedQuantity <= 0) updatedQuantity = 0;

                                            result = _helper.ExecuteUpdateSparesIssueStatement("UpdateSpareIssueQuantityDetails", receiptId, updatedQuantity);

                                            if (result == 1)
                                                if (remainingQuantity <= 0)
                                                    itemm.Quantity = -remainingQuantity;
                                            var deleteZeroQty = "delete from t_SpareParts_Stock where quantity=0";
                                            _helper.ExecuteSelectStmt(deleteZeroQty);
                                        }
                                    }
                            }
                        else
                            return Json(result, JsonRequestBehavior.AllowGet);

                        if (status == "Completed") UpdateStatusToComplete(status, EndOdo, vehicleNumber);
                    }

                return Json(res, JsonRequestBehavior.AllowGet);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCostForEachSpare(int spareId)
        {
            var costQuery = "select cost from m_spareparts where Id=" + spareId + "";
            decimal unitPrice;

            using (var dtCost = _helper.ExecuteSelectStmt(costQuery))
            {
                unitPrice = dtCost.AsEnumerable().Select(x => x.Field<decimal>("Cost")).FirstOrDefault();
            }

            return Json(unitPrice, JsonRequestBehavior.AllowGet);
        }

        public void UpdateStatusToComplete(string status, int? endOdo, string vehicleNumber)
        {
            var vehicId = 0;
            var getVehicId = "select Id from m_GetVehicleDetails where vehiclenumber = '" + vehicleNumber + "'";

            using (var dtGetVehId = _helper.ExecuteSelectStmt(getVehicId))
            {
                vehicId = dtGetVehId.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
            }

            var updateQuerySpares = "update t_VehicleJobCardDetails set status='Completed',EndingOdo=" + endOdo + " where VehicleId=" + vehicId + " and status='Pending'";
            _helper.ExecuteSelectStmt(updateQuerySpares);
        }

        public ActionResult CheckSpareQuantity(int sparePartId, int quantity)
        {
            var sparepartstotalQty = "select sum(quantity) as TotalQuantity from t_SpareParts_Stock where SparePartId=" + Convert.ToInt32(sparePartId) + "";
            int? response;

            using (var dtCheckStocks = _helper.ExecuteSelectStmt(sparepartstotalQty))
            {
                response = dtCheckStocks.AsEnumerable().Select(x => x.Field<int?>("TotalQuantity")).FirstOrDefault();
            }

            var remainingQuantity = response - quantity;
            return Json(remainingQuantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckLubesQuantity(int lubricantId, decimal quantity)
        {
            var lubespartstotalQty = "select sum(quantity) as TotalQuantity from t_Lubes_Stock where LubricantId=" + Convert.ToInt32(lubricantId) + "";
            decimal? response;

            using (var dtCheckStocks = _helper.ExecuteSelectStmt(lubespartstotalQty))
            {
                response = dtCheckStocks.AsEnumerable().Select(x => x.Field<decimal?>("TotalQuantity")).FirstOrDefault();
            }

            var remainingQuantity = response - quantity;
            if (response == null) remainingQuantity = -1;
            return Json(remainingQuantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCalculateLubesFifo(VehicleModel pendingCases, string status = null, int? EndOdo = 0)
        {
            var vehicleNumber = pendingCases.itemmodel.AsEnumerable().Select(x => x.VehicleNumber).FirstOrDefault();
            var result = 0;
            var workshopName = Session["workshopName"].ToString();
            var query = "select workshop_id from m_workshop where workshop_name='" + workshopName + "'";

            using (var dtWorkShopId = _helper.ExecuteSelectStmt(query))
            {
                pendingCases.WorkShopId = dtWorkShopId.AsEnumerable().Select(x => x.Field<int>("workshop_id")).FirstOrDefault();
            }

            pendingCases.JobCardId = Convert.ToInt32(Session["JobCardNumber"]);
            var dtGetLubricantNumber = Session["spGetVehicleLubricantDetails"] as DataTable;

            if (dtGetLubricantNumber != null)
            {
                var spares = dtGetLubricantNumber.AsEnumerable().Select(x => new {PartName = x.Field<string>("LubricantNumber"), PartId = x.Field<int>("Id")});

                foreach (var itemm in pendingCases.itemmodel)
                foreach (var spare in spares)
                    if (itemm.LubricantId == spare.PartId)
                    {
                        var dtcostDetails = _helper.ExecuteSelectStmtusingSP("GetCostByLubricantNumber", null, null, null, null, "@partnumber", spare.PartName);
                        var sumOfQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("Quantity")).Sum();

                        if (itemm.LubesQuantity <= sumOfQuantity)
                            if (itemm.LubesQuantity <= dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("Quantity")).FirstOrDefault())
                            {
                                var cost = dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("CostPerLitre")).FirstOrDefault();
                                var totalAmount = cost * itemm.LubesQuantity;
                                var res = _helper.ExecuteInsertSparesIssueStatement("InsertLubeIssueDetails", itemm.VehicleNumber, pendingCases.WorkShopId, itemm.LubricantId, itemm.LubesQuantity, totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status);

                                if (res == 1)
                                {
                                    var itemTotalQuantity = dtcostDetails.AsEnumerable().Select(x => x.Field<decimal>("Quantity")).FirstOrDefault();
                                    long receiptId = dtcostDetails.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
                                    var updatedQuantity = itemTotalQuantity - itemm.LubesQuantity;
                                    _helper.ExecuteUpdateSparesIssueStatement("UpdateLubesQuantityIssueDetails", receiptId, updatedQuantity);
                                    if (status == "Completed") UpdateStatusToComplete(status, EndOdo, vehicleNumber);
                                    // return Json(res, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                foreach (DataRow row in dtcostDetails.Rows)
                                    if (Convert.ToInt32(itemm.LubesQuantity) != 0)
                                    {
                                        var cost = Convert.ToDecimal(row["CostPerLitre"]);
                                        decimal totalAmount;
                                        decimal qty;

                                        if (itemm.LubesQuantity <= Convert.ToDecimal(row["Quantity"]))
                                        {
                                            totalAmount = cost * itemm.LubesQuantity;
                                            qty = itemm.LubesQuantity;
                                        }
                                        else
                                        {
                                            totalAmount = cost * Convert.ToDecimal(row["Quantity"]);
                                            qty = Convert.ToDecimal(row["Quantity"]);
                                        }

                                        var res = _helper.ExecuteInsertSparesIssueStatement("InsertLubeIssueDetails", itemm.VehicleNumber, pendingCases.WorkShopId, itemm.LubricantId, qty, totalAmount, itemm.HandOverToId, pendingCases.JobCardId, status);

                                        if (res == 1)
                                        {
                                            var itemTotalQuantity = Convert.ToDecimal(row["Quantity"]);
                                            long receiptId = Convert.ToInt32(row["Id"]);
                                            var updatedQuantity = itemTotalQuantity - itemm.LubesQuantity;
                                            var remainingQuantity = updatedQuantity;
                                            if (updatedQuantity <= 0) updatedQuantity = 0;

                                            result = _helper.ExecuteUpdateSparesIssueStatement("UpdateLubesIssueQuantityDetails", receiptId, updatedQuantity);

                                            if (result == 1)
                                                if (remainingQuantity <= 0)
                                                    itemm.LubesQuantity = -remainingQuantity;
                                            var deleteZeroQty = "delete from t_Lubes_Stock where quantity=0";
                                            _helper.ExecuteSelectStmt(deleteZeroQty);
                                        }
                                    }
                            }
                        else
                            return Json(result, JsonRequestBehavior.AllowGet);

                        if (status == "Completed") UpdateStatusToComplete(status, EndOdo, vehicleNumber);
                    }

                return Json(result, JsonRequestBehavior.AllowGet);
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
                var item = vehicleSpareParts.AsEnumerable().Where(x => x.Field<int>("Id") == Convert.ToInt32(spareId)).Select(x => new {PartNumber = x.Field<string>("partnumber"), ManufacturerId = x.Field<int>("ManufacturerId"), VehicleNumber = x.Field<string>("VehicleNumber")}).FirstOrDefault();

                if (item != null)
                {
                    var dtSparePArtscostDetails = _helper.ExecuteSelectStmtusingSP("spGetSparePartCostDetails", "@manufacturerid", item.ManufacturerId.ToString(), null, null, "@partnumber", item.PartNumber);

                    using (var dtSpareIssueDetails = _helper.ExecuteSelectStmtusingSP("GetSpareIssueDetails", "@sparepartid", spareId, null, null, "@vehiclenumber", item.VehicleNumber))
                    {
                        costDetails.AddRange(from DataRow row in dtSparePArtscostDetails.Rows select new JobCardPendingCases {Manufacturer = row["ManufacturerName"].ToString(), SparePart = row["PartName"].ToString(), LastEntryDate = Convert.ToDateTime(row["lastentrydate"]).ToShortDateString(), Cost = Convert.ToInt32(row["Cost"]), Quantity = Convert.ToInt32(row["Quantity"])});

                        if (dtSpareIssueDetails.Rows.Count > 0)
                            foreach (DataRow row in dtSpareIssueDetails.Rows)
                            {
                                var getSparesdetails = new JobCardPendingCases {SparePartId = Convert.ToInt32(spareId), Quantity = Convert.ToInt32(row["Quantity"]), TotalAmount = Convert.ToInt32(row["TotalAmount"]), IssuedDate = Convert.ToDateTime(row["IssuedDate"]).ToShortDateString(), StatusType = row["Status"].ToString()};
                                costDetails.Add(getSparesdetails);
                                getPendingSparesForCompletion.Add(getSparesdetails);
                            }
                    }
                }
            }

            Session["pendingStatusSpareDetails"] = getPendingSparesForCompletion;
            var costDetails1 = JsonConvert.SerializeObject(costDetails, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(costDetails1, "application/json");
        }

        public ActionResult GetLubesCost(string lubricantId)
        {
            var costDetails = new List<JobCardPendingCases>();
            var vehicleLubes = Session["spGetVehicleLubricantDetails"] as DataTable;

            if (vehicleLubes != null)
            {
                var item = vehicleLubes.AsEnumerable().Where(x => x.Field<int>("Id") == Convert.ToInt32(lubricantId)).Select(x => new {LubricantNumber = x.Field<string>("LubricantNumber"), ManufacturerId = x.Field<int>("ManufacturerId"), VehicleNumber = x.Field<string>("VehicleNumber")}).FirstOrDefault();

                if (item != null)
                {
                    DataTable dtLubesIssueDetails;

                    using (var dtLubricantcostDetails = _helper.ExecuteSelectStmtusingSP("spGetLubricantCostDetails", "@manufacturerid", item.ManufacturerId.ToString(), null, null, "@partnumber", item.LubricantNumber))
                    {
                        dtLubesIssueDetails = _helper.ExecuteSelectStmtusingSP("GetLubesIssueDetails", "@lubricantid", lubricantId, null, null, "@vehiclenumber", item.VehicleNumber);

                        foreach (DataRow row in dtLubricantcostDetails.Rows)
                        {
                            var details = new JobCardPendingCases {Manufacturer = row["ManufacturerName"].ToString(), SparePart = row["OilName"].ToString(), LastEntryDate = Convert.ToDateTime(row["lastentrydate"]).ToShortDateString(), Cost = Convert.ToInt32(row["CostPerLitre"]), LubesQuantity = Convert.ToDecimal(row["Quantity"])};
                            costDetails.Add(details);
                        }
                    }

                    if (dtLubesIssueDetails.Rows.Count > 0) costDetails.AddRange(from DataRow row in dtLubesIssueDetails.Rows select new JobCardPendingCases {LubesQuantity = Convert.ToDecimal(row["Quantity"]), TotalAmount = Convert.ToInt32(row["TotalAmount"]), IssuedDate = Convert.ToDateTime(row["IssuedDate"]).ToShortDateString(), StatusType = row["Status"].ToString()});
                }
            }

            var costDetails1 = JsonConvert.SerializeObject(costDetails, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(costDetails1, "application/json");
        }

        public ActionResult ReOpenCompletedJobs(string vehicleNumber,DateTime? DOR)
        {
            var vehIdString = "select Id from m_GetVehicleDetails where VehicleNumber='" + vehicleNumber + "'";
            int vehId;

            using (var dtgetId = _helper.ExecuteSelectStmt(vehIdString))
            {
                vehId = dtgetId.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
            }

            var updatejobCard = _helper.ExecuteInsertStmtusingSp("updateStatusOnReopen", null, null, null, null, "@vehiclenumber", vehicleNumber);
            var status = _helper.ExecuteInsertStmtusingSp("InsertStatusDetails", "@vehicleid", vehId.ToString(),null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null, "@dor", DOR.ToString());
            if (status > 0 || updatejobCard > 0) return RedirectToAction("GetPendingStatusDetails");
            return null;
        }
    }
}