using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Fleet_WorkShop.Models;
using Fleet_WorkShop.WorkShpModels;

namespace Fleet_WorkShop.Controllers
{
    public class ReportsController : Controller
    {
        private readonly Helper _helper = new Helper();


        public object ManufacturerName { get; set; }
        public object DOR { get; private set; }

        // GET: Reports
        public ActionResult Reports()
        {
            return View();
        }

        public ActionResult WorkShopManPowerReport()
        {
            var dtWorkShopManPowerReport = _helper.ExecuteSelectStmt("sp_workshopmanpower_report");
            IEnumerable<ReportsModel> employeemodel = dtWorkShopManPowerReport.AsEnumerable()
                .Select(x => new ReportsModel
                {
                    ID = x.Field<long>("ID"),
                    EmployeeId = x.Field<string>("employeeId"),
                    EmployeeName = x.Field<string>("employeename"),
                    WorkShopName = x.Field<string>("workshop_name"),
                    Designation = x.Field<string>("Designation"),
                    Experience = x.Field<int>("Experience"),
                    DateOfJoining = x.Field<DateTime>("DOJ")
                });
            return PartialView("WorkShopManPowerReport", employeemodel);
        }

        public ActionResult InfraStructureReport()
        {
            var dtInfraReport = _helper.ExecuteSelectStmt("sp_InfraStructure_report");
            IEnumerable<ReportsModel> inframodel = dtInfraReport.AsEnumerable().Select(x => new ReportsModel
            {
                ID = x.Field<long>("ID"),
                WorkShopName = x.Field<string>("workshop_name"),
                Infrastructure = x.Field<string>("infra_name"),
                Category = x.Field<string>("category_name"),
                Quantity = x.Field<int>("Quantity")
            });
            return PartialView("InfraStructureReport", inframodel);
        }

        public ActionResult WorkshopWiseSparepartsReport()
        {
            var dtworkshopwisespareparts = _helper.ExecuteSelectStmt("sp_workshopwisespareparts_report");
            IEnumerable<ReportsModel> sparePartsmodel = dtworkshopwisespareparts.AsEnumerable()
                .Select(x => new ReportsModel
                {
                    ID = x.Field<long>("ID"),
                    WorkShopName = x.Field<string>("workshop_name"),
                    SparePart = x.Field<string>("PartName"),
                    Quantity = x.Field<int>("Quantity"),
                    Manufacturer = x.Field<string>("ManufacturerName")
                });
            return PartialView("WorkshopWiseSparepartsReport", sparePartsmodel);
        }

        public ActionResult WorkshopWiseLubesReport()
        {
            var dtworkshopwiseLubes = _helper.ExecuteSelectStmt("sp_workshopwiselubes_report");
            IEnumerable<ReportsModel> lubesmodel = dtworkshopwiseLubes.AsEnumerable().Select(x => new ReportsModel
            {
                ID = x.Field<long>("ID"),
                WorkShopName = x.Field<string>("workshop_name"),
                Lubricant = x.Field<string>("OilName"),
                Quantity = x.Field<int>("Quantity"),
                Manufacturer = x.Field<string>("ManufacturerName")
            });
            return PartialView("WorkshopWiseLubesReport", lubesmodel);
        }

        public ActionResult SparePartWiseConsumption(int? id)
        {
            if (id != null)
                Session["Consumption"] = null;
            if (Session["Consumption"] != null)
            {
                var list = Session["Consumption"] as IEnumerable<VehicleReport>;
                return PartialView("_GetSparePartWiseConsumption", list);
            }
            var sparepartQuery = "select Id,PartName from m_spareparts";
            var dtSpares = _helper.ExecuteSelectStmt(sparepartQuery);
            ViewBag.SpareParts = new SelectList(dtSpares.AsDataView(), "Id", "PartName");

            return View();
        }

        [HttpPost]
        public ActionResult SparePartsConsumption(DateTime startDate, DateTime endDate, int sparePartId)
        {
            var dtgetSPareConsumptionReport = _helper.ExecuteSelectStmtForDateTime("sparepartwise_consumption_report",
                "@sdate", startDate.ToString(CultureInfo.CurrentCulture), "@edate",
                endDate.ToString(CultureInfo.CurrentCulture), null, null, "@siid", sparePartId.ToString());
            var spareConsumption = dtgetSPareConsumptionReport.AsEnumerable().Select(x => new VehicleReport
            {
                Id = x.Field<int>("ID"),
                Workshop = x.Field<string>("WORKSHOP"),
                Vehicle = x.Field<string>("VEHICLENUMBER"),
                Sparepart = x.Field<string>("SPAREPART"),
                Quantity = x.Field<int>("QUANTITY"),
                Manufacturer = x.Field<string>("MANUFACTURE")
            });
            Session["Consumption"] = spareConsumption;
            return RedirectToAction("SparePartWiseConsumption");
        }

      
        public ActionResult LubesWiseConsumption( int? id)
        {
            if (id != null)
            {
                Session["LubesConsumption"] = null;
            }
            if (Session["LubesConsumption"] != null)
            {
                var list = Session["LubesConsumption"] as IEnumerable<VehicleReport>;
                return PartialView("_GetLubesWiseConsumption", list);
            }
            var LubesQuery = "select Id,OilName from m_lubes";
            var dtSpares = _helper.ExecuteSelectStmt(LubesQuery);
            ViewBag.Lubes = new SelectList(dtSpares.AsDataView(), "Id", "OilName");
            id = null;
            return View();
        }

        [HttpPost]
        public ActionResult LubesConsumption(DateTime startDate, DateTime endDate, int lubesId)
        {
            var dtgetSPareConsumptionReport = _helper.ExecuteSelectStmtForDateTime("lubricantwise_consumption_report",
                "@sdate", startDate.ToString(CultureInfo.CurrentCulture), "@edate",
                endDate.ToString(CultureInfo.CurrentCulture), null, null, "@liid", lubesId.ToString());
            var lubesConsumption = dtgetSPareConsumptionReport.AsEnumerable().Select(x => new VehicleReport
            {
                Id = x.Field<int>("ID"),
                Workshop = x.Field<string>("WORKSHOP"),
                Vehicle = x.Field<string>("VEHICLENUMBER"),
                Lubricant = x.Field<string>("OILNAME"),
                Quantity = x.Field<int>("QUANTITY"),
                Manufacturer = x.Field<string>("MANUFACTURER")
            });
            Session["LubesConsumption"] = lubesConsumption;
            return RedirectToAction("LubesWiseConsumption");
        }

        [HttpGet]
        public ActionResult GetVehicleWiseStocksReport(int? id)
        {
            if (id != null)
                Session["View"] = null;
            if (Session["View"] != null)
            {
                var list = Session["View"] as IEnumerable<VehicleReport>;             
                return PartialView("_GetVehicleWiseStocksReportDetails", list);
            }
            return View();
        }

        [HttpPost]
        public ActionResult GetVehicleWiseStocksReportDetails(DateTime startDate, DateTime endDate)
        {
            var dtgetStocksReport = _helper.ExecuteSelectStmtForDateTime("Vehicle_wise_Stockused", "@sdate",
                startDate.ToString(CultureInfo.CurrentCulture), "@edate", endDate.ToString(CultureInfo.CurrentCulture));
            var list = dtgetStocksReport.AsEnumerable().Select(x => new VehicleReport
            {
                Id = x.Field<long>("ID"),
                Workshop = x.Field<string>("workshop"),
                Vehicle = x.Field<string>("VehicleNumber"),
                Sparepart = x.Field<string>("SparePart"),
                Quantity = x.Field<int>("Quantity"),
                Amount = x.Field<decimal>("Amount"),
                JobcardId = x.Field<int>("Jobcard"),
                HandOverTo = x.Field<string>("HandOverto"),
                IssuedDate = x.Field<DateTime>("IssuedDate"),
                Status = x.Field<string>("status")
            });
            Session["View"] = list;
            return RedirectToAction("GetVehicleWiseStocksReport", "Reports");
        }

        [HttpGet]
        public ActionResult GetVehicleWiseLubesReport(int? id)
        {
            if (id != null)
                Session["ViewLubes"] = null;
            if (Session["ViewLubes"] != null)
            {
                var list = Session["ViewLubes"] as IEnumerable<VehicleReport>;
                return PartialView("_GetVehicleWiseStocksReportDetails", list);
            }
            return View();
        }

        [HttpPost]
        public ActionResult GetVehicleWiseLubesDetails(DateTime startDate, DateTime endDate)
        {
            var dtgetLubesReport = _helper.ExecuteSelectStmtForDateTime("vehicle_wise_lubesused", "@sdate",
                startDate.ToString(CultureInfo.CurrentCulture), "@edate",
                endDate.ToString(CultureInfo.CurrentCulture));
            var list = dtgetLubesReport.AsEnumerable().Select(x => new VehicleReport
            {
                Id = x.Field<long>("ID"),
                Workshop = x.Field<string>("workshop"),
                Vehicle = x.Field<string>("VehicleNumber"),
                Sparepart = x.Field<string>("Lube"),
                Quantity = x.Field<int>("Quantity"),
                Amount = x.Field<decimal>("Amount"),
                JobcardId = x.Field<int>("Jobcard"),
                HandOverTo = x.Field<string>("HandOverto"),
                IssuedDate = x.Field<DateTime>("IssuedDate"),
                Status = x.Field<string>("status")
            });
            Session["ViewLubes"] = list;
            return RedirectToAction("GetVehicleWiseLubesReport");
        }

        public ActionResult VehicleWiseRepairReport(int? id)
        {

            if (id != null)
            {
                Session["VehicleWiseRepairReports"] = null;
            }
            var getVehicleQuery = "select Id,VehicleNUmber from m_getvehicledetails";
            var dtgetvehicles = _helper.ExecuteSelectStmt(getVehicleQuery);
            ViewBag.Vehicles = new SelectList(dtgetvehicles.AsDataView(), "Id", "VehicleNumber");

            if (Session["VehicleWiseRepairReports"] != null)
            {
                var list = Session["VehicleWiseRepairReports"] as IEnumerable<VehicleReport>;
                return PartialView("_GetVehicleWiseRepairsReportDetails", list);
            }
            return View();
        }

        [HttpPost]
        public ActionResult VehicleWiseRepairReports(DateTime startDate, DateTime endDate, int vehicleId)
        {
            var dtVehicleWiseRepairReports = _helper.ExecuteSelectStmtForDateTime("vehiclewise_repair_report",
                "@sdate", startDate.ToShortDateString(), "@edate", endDate.ToShortDateString(), null, null, "@veid",
                vehicleId.ToString());
            var vehicleWiseRepairs = dtVehicleWiseRepairReports.AsEnumerable().Select(x => new VehicleReport
            {
                JobcardId = x.Field<int>("JOBCARDID"),
                Workshop = x.Field<string>("WORKSHOP"),
                Vehicle = x.Field<string>("VEHICLENO"),
                District = x.Field<string>("DISTRICT"),
                DateOfRepair = x.Field<DateTime>("DATEOFREPAIR"),
                DateOfDelivery = x.Field<DateTime>("DATEOFDELIVERY"),
                Aggregate = x.Field<string>("AGGRIGATENAME"),
                Mechanic = x.Field<string>("MECHANIC"),
                Status = x.Field<string>("STATUS"),
                Category = x.Field<string>("CATEGORIES"),
                SubCategory = x.Field<string>("SUBCATEGORY")
            });
            Session["VehicleWiseRepairReports"] = vehicleWiseRepairs;
            return RedirectToAction("GetVehicleWiseLubesReport");
        }


        public ActionResult WorkShopMechanicReport(int? id)
        {
            if (id != null)
                Session["WSMCReports"] = null;
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");

            var query =
                "select e.name,e.empId from emp_details e join emp_designation  d on e.DesgID=d.id where d.id=2 and DOR is NULL";
            var dtGetMechanics = _helper.ExecuteSelectStmt(query);
            ViewBag.Mechanics = new SelectList(dtGetMechanics.AsDataView(), "empid", "name");
            if (Session["WSMCReports"] != null)
            {
                var list = Session["WSMCReports"] as IEnumerable<VehicleReport>;
                return PartialView("_getWorkShopMechanicReport", list);
            }
            return View();
        }

        [HttpPost]
        public ActionResult WorkShopMechanicWiseReport(DateTime startDate, DateTime endDate, int mechanicId)
        {
            if (Session["WorkshopId"] == null)
                return RedirectToAction("Login", "Account");
            var dtMechanicWiseReports = _helper.ExecuteSelectStmtForDateTime("WORKSHOP_MECHANIC_WISE_report",
                "@sdate", startDate.ToShortDateString(), "@edate", endDate.ToShortDateString(), null, null, "@wsid",
                Session["WorkshopId"].ToString(), "@mcid", mechanicId.ToString());
            var workshopMechanicWiseRepairs = dtMechanicWiseReports.AsEnumerable().Select(x => new VehicleReport
            {
                JobcardId = x.Field<int>("JOBCARDNO"),
                Workshop = x.Field<string>("WORKSHOP"),
                Vehicle = x.Field<string>("VEHICLENUMBER"),
                District = x.Field<string>("DISTRICT"),
                DateOfRepair = x.Field<DateTime>("DATEOFREPAIR"),
                DateOfDelivery = x.Field<DateTime>("DATEOFDELIVERY"),
                Aggregate = x.Field<string>("AGGRIGATENAME"),
                Mechanic = x.Field<string>("MECHANIC"),
                ServiceInchargeId = x.Field<int>("SERVICEINCHARGE"),
                Status = x.Field<string>("STATUS"),
                Category = x.Field<string>("CATEGORIES"),
                SubCategory = x.Field<string>("SUBCATEGORY")
            });
            Session["WSMCReports"] = workshopMechanicWiseRepairs;
            return RedirectToAction("WorkShopMechanicReport");
        }

        public ActionResult RmeWisePerformanceReport(int? id)
        {
            if (id != null)
                Session["PerformanceReports"] = null;
            var rm = "select name,empid from emp_Details where desgid=8";
            var pm = "select name,empid from emp_Details where desgid=9";
            var eme = "select name,empid from emp_Details where desgid=10";
            var dtRm = _helper.ExecuteSelectStmt(rm);
            var dtPm = _helper.ExecuteSelectStmt(pm);
            var dteme = _helper.ExecuteSelectStmt(eme);
            ViewBag.RM = new SelectList(dtRm.AsDataView(), "empid", "name");
            ViewBag.PM = new SelectList(dtPm.AsDataView(), "empid", "name");
            ViewBag.EME = new SelectList(dteme.AsDataView(), "empid", "name");
            if (Session["PerformanceReports"] != null)
            {
                var list = Session["PerformanceReports"] as IEnumerable<ReportsModel>;
                return PartialView("RmeWisePerformanceReports", list);
            }
            return View();
        }

        [HttpPost]
        public ActionResult RmeWisePerformanceReports(int rm, int pm, int eme, DateTime fromDate, DateTime toDate)
        {
            var repmodels = new List<ReportsModel>();

            var dtgetEMEDetails = _helper.ExecuteSelectStmtForDateTime("rmpmeme_wise_performance_report",
                "@sdate", fromDate.ToShortDateString(), "@edate", toDate.ToShortDateString(), null, null, "@emeid",
                rm.ToString(), "@pmid", pm.ToString(), "@rmid", eme.ToString());
            foreach (DataRow row in dtgetEMEDetails.Rows)
                if (row != null)
                {
                    var reports = new ReportsModel();
                    reports.JobCardNumber = Convert.ToInt32(row["JOBCARDNO"]);
                    reports.WorkShopName = row["WORKSHOP"].ToString();
                    reports.District = row["DISTRICT"].ToString();
                    reports.VehicleNumber = row["VEHICLENUMBER"].ToString();
                    reports.DOR = Convert.ToDateTime(row["DATEOFREPAIR"]);
                    reports.DOD = Convert.ToDateTime(row["DATEOFDELIVERY"]);
                    reports.Aggregate = row["AGGRIGATENAME"].ToString();
                    reports.ServiceIncharge = row["SERVICEINCHARGE"].ToString();
                    reports.Status = row["STATUS"].ToString();
                    reports.Mechanic = row["MECHANIC"].ToString();
                    reports.Category = row["CATEGORIES"].ToString();
                    reports.SubCategory = row["SUBCATEGORY"].ToString();
                    repmodels.Add(reports);
                }
            Session["PerformanceReports"] = repmodels;
            return RedirectToAction("RmeWisePerformanceReport");
        }

        public ActionResult VehicleDeviationReport(int? id)
        {
            if (id != null)
                Session["VehicleDeviationReport"] = null;
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");
            var workShopId = Convert.ToInt32(Session["WorkshopId"]);
            Session["workshopid"] = workShopId;
            var query = "select workshop_name from m_workshop where workshop_id =" + workShopId + "";
            var dtworkshopName = _helper.ExecuteSelectStmt(query);
            ViewBag.WorkShopName = dtworkshopName.AsEnumerable().Select(x => x.Field<string>("workshop_name")).FirstOrDefault();
            if (Session["VehicleDeviationReport"] != null)
            {
                var list = Session["VehicleDeviationReport"] as IEnumerable<VehicleReport>;
                return PartialView("_VehicleDeviationReports", list);
            }
            return View();
        }

        [HttpPost]
        public ActionResult VehicleDeviationReport(DateTime startDate, DateTime endDate)
        {
            int workshopId = Convert.ToInt32(Session["workshopid"]);
            DataTable dtVehicleDeviationReport = _helper.ExecuteSelectStmtForDateTime("vehicle_repair_deviationtime_report","@sdate",
                startDate.ToShortDateString(), "@edate", endDate.ToShortDateString(),
                null, null, "@wsid", workshopId.ToString());
            var vehicleDeviationReport = dtVehicleDeviationReport.AsEnumerable().Select(x => new VehicleReport
            {
                JobcardId = x.Field<int>("JOBCARDNO"),
                Workshop = x.Field<string>("WORKSHOP"),
                District = x.Field<string>("DISTRICT"),
                Vehicle = x.Field<string>("VEHICLENUMBER"),
                DateOfRepair = x.Field<DateTime>("DATEOFREPAIR"),
                RepairDate= x.Field<DateTime>("DATEOFREPAIR").ToShortDateString(),
                DateOfRepairCompletion = x.Field<DateTime>("REPAIRCOMPLETED"),
                DeviationTime=x.Field<int>("DEVIATIONTIMEINHOURS"),
                DateOfDelivery = x.Field<DateTime>("DATEOFDELIVERY"),
                DalivaryDate = x.Field<DateTime>("DATEOFDELIVERY").ToShortDateString(),
                Aggregate = x.Field<string>("AGGRIGATENAME"),
                Mechanic = x.Field<string>("MECHANIC"),
                ServiceInchargeId = x.Field<int>("SERVICEINCHARGE"),
                Status = x.Field<string>("STATUS"),
                Category = x.Field<string>("CATEGORIES"),
                SubCategory = x.Field<string>("SUBCATEGORY")
            });
            Session["VehicleDeviationReport"] = vehicleDeviationReport;
            return RedirectToAction("VehicleDeviationReport");
        }
    }
}