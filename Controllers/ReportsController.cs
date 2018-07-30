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
           IEnumerable<ReportsModel> Employeemodel= dtWorkShopManPowerReport.AsEnumerable().Select(x => new ReportsModel { EmployeeName = x.Field<string>("employeename"), WorkShopName = x.Field<string>("workshop_name"), Designation = x.Field<string>("Designation"), Experience = x.Field<string>("Experience"), DateOfJoining = x.Field<string>("DOJ") });
            return PartialView("WorkShopManPowerReport",Employeemodel);
        }
        public ActionResult InfraStructureReport()
        {

            DataTable dtInfraReport = _helper.ExecuteSelectStmt("sp_InfraStructure_report");
            IEnumerable<ReportsModel> Inframodel = dtInfraReport.AsEnumerable().Select(x => new ReportsModel { Infrastructure = x.Field<string>("infra_name"), Category = x.Field<string>("category_name"), Quantity = x.Field<int>("Quantity") });
            return PartialView("InfraStructureReport",Inframodel);
        }
        public ActionResult WorkshopWiseSparepartsReport()
        {
            DataTable dtworkshopwisespareparts = _helper.ExecuteSelectStmt("sp_workshopwisespareparts_report");
            IEnumerable<ReportsModel> sparePartsmodel = dtworkshopwisespareparts.AsEnumerable().Select(x => new ReportsModel { WorkShopName = x.Field<string>("workshop_name"), SparePart = x.Field<string>("PartName"), Quantity = x.Field<int>("Quantity"), Manufacturer = x.Field<string>("ManufacturerName") });
            return PartialView("WorkshopWiseSparepartsReport",sparePartsmodel);
        }
        public ActionResult WorkshopWiseLubesReport()
        {
            DataTable dtworkshopwiseLubes = _helper.ExecuteSelectStmt("sp_workshopwiselubes_report");
            IEnumerable<ReportsModel> Lubesmodel = dtworkshopwiseLubes.AsEnumerable().Select(x => new ReportsModel { WorkShopName = x.Field<string>("workshop_name"), Lubricant = x.Field<string>("OilName"), Quantity = x.Field<int>("Quantity"), Manufacturer = x.Field<string>("ManufacturerName") });
            return PartialView("WorkshopWiseLubesReport",Lubesmodel);
        }
    }
}