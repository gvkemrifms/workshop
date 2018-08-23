using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Fleet_WorkShop.Models;
using Fleet_WorkShop.WorkShpModels;
using System.Data;

namespace Fleet_WorkShop.Controllers
{
    public class ReportsController : Controller
    {
        public readonly Helper _helper = new Helper();
        public readonly ReportsModel _reportModel = new ReportsModel();
        // GET: Reports
        public ActionResult Reports()
        {
            return View();
        }
        public ActionResult WorkShopManPowerReport()
        {
           
            DataTable dtWorkShopManPowerReport = _helper.ExecuteSelectStmt("sp_workshopmanpower_report");
           IEnumerable<ReportsModel> Employeemodel= dtWorkShopManPowerReport.AsEnumerable().Select(x => new ReportsModel {ID=x.Field<long>("ID"), EmployeeId = x.Field<string>("employeeId"), EmployeeName = x.Field<string>("employeename"), WorkShopName = x.Field<string>("workshop_name"), Designation = x.Field<string>("Designation"), Experience = x.Field<int>("Experience"), DateOfJoining = x.Field<DateTime>("DOJ") });
            return PartialView("WorkShopManPowerReport",Employeemodel);
        }
        public ActionResult InfraStructureReport()
        {

            DataTable dtInfraReport = _helper.ExecuteSelectStmt("sp_InfraStructure_report");
            IEnumerable<ReportsModel> Inframodel = dtInfraReport.AsEnumerable().Select(x => new ReportsModel { ID = x.Field<long>("ID"), WorkShopName = x.Field<string>("workshop_name"), Infrastructure = x.Field<string>("infra_name"), Category = x.Field<string>("category_name"), Quantity = x.Field<int>("Quantity") });
            return PartialView("InfraStructureReport",Inframodel);
        }
        public ActionResult WorkshopWiseSparepartsReport()
        {
            DataTable dtworkshopwisespareparts = _helper.ExecuteSelectStmt("sp_workshopwisespareparts_report");
            IEnumerable<ReportsModel> sparePartsmodel = dtworkshopwisespareparts.AsEnumerable().Select(x => new ReportsModel { ID = x.Field<long>("ID"), WorkShopName = x.Field<string>("workshop_name"), SparePart = x.Field<string>("PartName"), Quantity = x.Field<int>("Quantity"), Manufacturer = x.Field<string>("ManufacturerName") });
            return PartialView("WorkshopWiseSparepartsReport",sparePartsmodel);
        }
        public ActionResult WorkshopWiseLubesReport()
        {
            DataTable dtworkshopwiseLubes = _helper.ExecuteSelectStmt("sp_workshopwiselubes_report");
            IEnumerable<ReportsModel> Lubesmodel = dtworkshopwiseLubes.AsEnumerable().Select(x => new ReportsModel { ID = x.Field<long>("ID"), WorkShopName = x.Field<string>("workshop_name"), Lubricant = x.Field<string>("OilName"), Quantity = x.Field<int>("Quantity"), Manufacturer = x.Field<string>("ManufacturerName") });
            return PartialView("WorkshopWiseLubesReport",Lubesmodel);
        }
        
        [HttpGet]
        public ActionResult GetVehicleWiseStocksReport()
        {
            if (Session["View"] != null)
            {
                var list = Session["View"] as IEnumerable<VehicleReport>;
                return PartialView("_GetVehicleWiseStocksReportDetails",list);
            }
            return View();
        }
        [HttpPost]
        public ActionResult GetVehicleWiseStocksReportDetails(DateTime startDate, DateTime endDate)
        {
           DataTable dtgetStocksReport= _helper.ExecuteSelectStmtForDateTime("Vehicle_wise_Stockused", "@sdate", startDate.ToString(), "@edate", endDate.ToString());
            var list = dtgetStocksReport.AsEnumerable().Select(x => new VehicleReport { Id = x.Field<long>("ID"), Workshop = x.Field<string>("workshop"), Vehicle = x.Field<string>("VehicleNumber"), Sparepart = x.Field<string>("SparePart"), Quantity = x.Field<int>("Quantity"), Amount = x.Field<decimal>("Amount"), JobcardId = x.Field<int>("Jobcard"), HandOverTo = x.Field<string>("HandOverto"), IssuedDate = x.Field<DateTime>("IssuedDate"), Status = x.Field<string>("status") });
            Session["View"] = list;
            return RedirectToAction("GetVehicleWiseStocksReport");
        }
        [HttpGet]
        public ActionResult GetVehicleWiseLubesReport()
        {
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
            DataTable dtgetLubesReport = _helper.ExecuteSelectStmtForDateTime("vehicle_wise_lubesused", "@sdate", startDate.ToString(), "@edate", endDate.ToString());
            var list = dtgetLubesReport.AsEnumerable().Select(x => new VehicleReport { Id = x.Field<long>("ID"), Workshop = x.Field<string>("workshop"), Vehicle = x.Field<string>("VehicleNumber"), Sparepart = x.Field<string>("Lube"), Quantity = x.Field<int>("Quantity"), Amount = x.Field<decimal>("Amount"), JobcardId = x.Field<int>("Jobcard"), HandOverTo = x.Field<string>("HandOverto"), IssuedDate = x.Field<DateTime>("IssuedDate"), Status = x.Field<string>("status") });
            Session["ViewLubes"] = list;
            return RedirectToAction("GetVehicleWiseLubesReport");
        }
    }
}