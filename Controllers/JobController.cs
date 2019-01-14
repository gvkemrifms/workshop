using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
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
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            return RedirectToAction("SaveJobCardDetails");
        }

        public ActionResult getDistancelatlong(int? vehicleid)
        {
            var dp = new GetDistanceAndProject();
            if (vehicleid == null) return null;
            double originLatitude = 0;
            double originLongitude = 0;
            double destLatitude = 0;
            double destLongitude = 0;

            using (var dtgetDistance = _helper.ExecuteSelectStmtusingSP("distanceCalculator", "@vehicleid", vehicleid.ToString()))
            {
                foreach (DataRow row in dtgetDistance.Rows)
                    if (row != null)
                    {
                        originLatitude = Convert.ToDouble(row["olatitude"]);
                        originLongitude = Convert.ToDouble(row["olongitude"]);
                        destLatitude = Convert.ToDouble(row["destlatitude"]);
                        destLongitude = Convert.ToDouble(row["destlongitude"]);
                    }
            }

            //string url = String.Format("https://maps.googleapis.com/maps/api/distancematrix/xml?units=imperial&origins={0},{1}&destinations={2},{3}&key=AIzaSyCqNE77AmnSEywlJ63QTJsboi7E7wFPmMQ", originLatitude, originLongitude, destLatitude, destLongitude);
            //string url = String.Format("https://maps.googleapis.com/maps/api/distancematrix/xml?units=imperial&origins={0},{1}&destinations={2},{3}&key=d1CkkvZsTH1jgTgvsZvvYRaSDSc=", originLatitude, originLongitude, destLatitude, destLongitude);
            //string url= "https://maps.googleapis.com/maps/api/directions/xml?units=imperial&origins={0},{1}&destinations={2},{3}&key=d1CkkvZsTH1jgTgvsZvvYRaSDSc=";
            //WebRequest request = WebRequest.Create(url);
            //WebResponse response = request.GetResponse();
            //XDocument xdoc = XDocument.Load(response.GetResponseStream());

            //XElement result = xdoc.Element("DistanceMatrixResponse").Element("row").Element("element").Element("distance").Element("text");
            //var locationElement = result.Value;
            //locationElement = locationElement.Replace(",", "");
            //string resultString = Regex.Match(locationElement, @"\d+").Value;
            //var distanceinKm = Convert.ToInt32(resultString) * 1.6;
            //var res = distanceinKm;



            var distanceinKm = _helper.GetDistanceFromLatLonInKm(originLatitude, originLongitude, destLatitude, destLongitude);

            dp.distance = distanceinKm;
            var getProject = "select project from m_GetVehicleDetails where Id=" + vehicleid + "";
            var dtGetProject = _helper.ExecuteSelectStmt(getProject);
            dp.project = dtGetProject.AsEnumerable().Select(x => x.Field<string>("project")).FirstOrDefault();
           
            return Json(dp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateDateOfReceiving(string Dor, string Dod)
        {
            var result = "0";
            var receivingDate = Convert.ToDateTime(Dor);
            var delivaryDate = Convert.ToDateTime(Dod);

            if (delivaryDate < receivingDate)
                result = receivingDate.ToString();
            else
                result = "0";
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveJobCardDetails()
        {
            if (Session["WorkshopId"] == null) return RedirectToAction("Login", "Account");
            var dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers");
            Session["VehicleDistrictDetails"] = dsVehicleDistrictDetails;
            ViewBag.Districts = new SelectList(dsVehicleDistrictDetails.Tables[0].AsDataView(), "Id", "District");
            ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName");
            ViewBag.NatureOfComplaint = new SelectList(dsVehicleDistrictDetails.Tables[2].AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");
            ViewBag.AllotedMechanic = new SelectList(dsVehicleDistrictDetails.Tables[3].AsDataView(), "empId", "name");
            ViewBag.ServiceEngineer = new SelectList(dsVehicleDistrictDetails.Tables[4].AsDataView(), "empId", "name");
            ViewBag.Helper = new SelectList(dsVehicleDistrictDetails.Tables[5].AsDataView(), "empId", "name");
            ViewBag.MaintenanceType = new SelectList(dsVehicleDistrictDetails.Tables[6].AsDataView(), "Id", "MaintenanceType");
            var list = new List<SelectListItem> { new SelectListItem { Text = "SS1", Value = "1" }, new SelectListItem { Text = "SS2", Value = "2" }, new SelectListItem { Text = "SS3", Value = "3" }, new SelectListItem { Text = "SS4", Value = "4" }, new SelectListItem { Text = "SS5", Value = "5" } };
            IEnumerable<SelectListItem> myCollection = list.AsEnumerable();
            ViewBag.SS = new SelectList(myCollection, "Value", "Text");
            _vehModel.DistrictId = Convert.ToInt32(Session["Id"]);
            IEnumerable<VehicleModel> model;

            using (var dsGetJobCardDetails = _helper.FillDropDownHelperMethodWithSp("spGetJobCardDetails", Convert.ToInt32(Session["WorkshopId"])))
            {
                Session["GetJobCardDetails"] = dsGetJobCardDetails;
                model = dsGetJobCardDetails.Tables[0].AsEnumerable().ToList().Select(x => new VehicleModel {Id = x.Field<int>("JobCardNumber"), VehId = x.Field<int>("VehicleId"), DistrictName = x.Field<string>("District"), NatureOfComplaint = x.Field<int>("NatureOfComplaint"), VehicleId = x.Field<string>("vehicleNumber"), DateOfDelivery = x.Field<DateTime>("Dor"), ModelNumber = x.Field<int>("Model"), Odometer = x.Field<int>("Odometer"), PilotName = x.Field<string>("PilotName"), ApproximateCost = x.Field<int>("ApproxCost"), AllotedMechanicName = x.Field<string>("Name")});
            }

            return View(model);
        }

        [HttpPost] public ActionResult SaveJobCardDetails(VehicleModel model, string vehicleNumber, int? helperId, int? MaintenanceId, int? SSId, string distanceTravelled = null,int? OffroadId=0)
        {
            var result = 0;
            int? distance = int.Parse(distanceTravelled);
            var districtId = model.jobcarditems.Select(x => x.DistrictId).FirstOrDefault();

            foreach (var item in model.jobcarditems)
            {
                if (item.ServiceEngineer == 0 || item.AllotedMechanic == 0 || item.DateOfDelivery == null) return Json(result, JsonRequestBehavior.AllowGet);
                var vehDetails = new VehicleJobCardModel {WorkShopId = Convert.ToInt32(Session["WorkshopId"]), DistrictId = item.DistrictId, VehId = item.VehId, DateOfDelivery = item.DateOfDelivery, ApproximateCost = item.ApproximateCost, ModelNumber = item.ModelNumber, NatureOfComplaint = item.NatureOfComplaint, Odometer = item.Odometer, PilotId = item.PilotId, PilotName = item.PilotName, ReceivedLocation = item.ReceivedLocation, DateOfRepair = item.DateOfRepair, AllotedMechanic = item.AllotedMechanic, ServiceEngineer = item.ServiceEngineer, CategoryIdd = item.CategoryIdd, SubCat = item.SubCat, ManufacturerId = item.ManufacturerId,EndOdo = item.EndOdo};
                var dtGetEmpmre = _helper.ExecuteSelectStmtusingSP("spGetEMEPMRM", null, null, null, null, "@vehiclenumber", vehicleNumber);
                var emt = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "EMT").Select(x => x.Field<int>("empid")).FirstOrDefault();
                var pm = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "PM").Select(x => x.Field<int>("empid")).FirstOrDefault();
                var rm = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "RM").Select(x => x.Field<int>("empid")).FirstOrDefault();
                if (helperId == null) helperId = 0;
                if (SSId == null) SSId = 0;
                result = _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", Convert.ToInt32(vehDetails.DistrictId), Convert.ToInt32(vehDetails.VehId), Convert.ToDateTime(vehDetails.DateOfRepair), Convert.ToInt32(vehDetails.ModelNumber), Convert.ToInt32(vehDetails.Odometer), vehDetails.ReceivedLocation, vehDetails.PilotId, vehDetails.PilotName, Convert.ToDateTime(vehDetails.DateOfDelivery), Convert.ToInt32(vehDetails.NatureOfComplaint), Convert.ToInt32(vehDetails.ApproximateCost), Convert.ToInt32(vehDetails.AllotedMechanic), Convert.ToInt32(vehDetails.WorkShopId), Convert.ToInt32(vehDetails.ServiceEngineer), Convert.ToInt32(vehDetails.CategoryIdd), Convert.ToInt32(item.SubCat), Convert.ToInt32(item.ManufacturerId), Convert.ToInt32(rm), Convert.ToInt32(pm), Convert.ToInt32(emt), Convert.ToInt32(helperId), distance,Convert.ToInt32(MaintenanceId),Convert.ToInt32(SSId), OffroadId);
            }

            //return RedirectToAction("SaveJobCardDetails");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult SaveScheduledServiceDetails(SS model, string vehicleNumber, int? helperId, int? MaintenanceId, int? SSId, string distanceTravelled = null,int OffroadId=0)
        {
            var result = 0;

            if (distanceTravelled != null)
            {
                int? distance = int.Parse(distanceTravelled);
                if (model.ServiceEngineer == 0 || model.AllotedMechanic == 0) return Json(result, JsonRequestBehavior.AllowGet);
                var vehDetails = new VehicleJobCardModel { WorkShopId = Convert.ToInt32(Session["WorkshopId"]), DistrictId = model.DistrictId, VehId = model.VehId, DateOfDelivery = model.DateOfDelivery, ApproximateCost = model.ApproximateCost, ModelNumber = model.ModelNumber, NatureOfComplaint = 0, Odometer = model.Odometer, PilotId = model.PilotId.ToString(), PilotName = model.PilotName, ReceivedLocation = model.ReceivedLocation, DateOfRepair = model.DateOfRepair, AllotedMechanic = model.AllotedMechanic, ServiceEngineer = model.ServiceEngineer, CategoryIdd = 0, SubCat = 0, ManufacturerId = model.ManufacturerId };
                var dtGetEmpmre = _helper.ExecuteSelectStmtusingSP("spGetEMEPMRM", null, null, null, null, "@vehiclenumber", vehicleNumber);
                var emt = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "EMT").Select(x => x.Field<int>("empid")).FirstOrDefault();
                var pm = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "PM").Select(x => x.Field<int>("empid")).FirstOrDefault();
                var rm = dtGetEmpmre.AsEnumerable().Where(x => x.Field<string>("Designation") == "RM").Select(x => x.Field<int>("empid")).FirstOrDefault();
                if (helperId == null) helperId = 0;
                if (SSId == null) SSId = 0;
                result = _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", Convert.ToInt32(vehDetails.DistrictId), Convert.ToInt32(vehDetails.VehId), Convert.ToDateTime(vehDetails.DateOfRepair), Convert.ToInt32(vehDetails.ModelNumber), Convert.ToInt32(vehDetails.Odometer), vehDetails.ReceivedLocation, vehDetails.PilotId, vehDetails.PilotName, Convert.ToDateTime(vehDetails.DateOfDelivery), Convert.ToInt32(vehDetails.NatureOfComplaint), Convert.ToInt32(vehDetails.ApproximateCost), Convert.ToInt32(vehDetails.AllotedMechanic), Convert.ToInt32(vehDetails.WorkShopId), Convert.ToInt32(vehDetails.ServiceEngineer), Convert.ToInt32(vehDetails.CategoryIdd), Convert.ToInt32(vehDetails.SubCat), Convert.ToInt32(vehDetails.ManufacturerId), Convert.ToInt32(rm), Convert.ToInt32(pm), Convert.ToInt32(emt), Convert.ToInt32(helperId), distance, Convert.ToInt32(MaintenanceId), Convert.ToInt32(SSId),Convert.ToInt32(OffroadId));
            }

            return Json(result,JsonRequestBehavior.AllowGet); 

        }

        
        [HttpPost] public ActionResult GetDistrictIds(string districtId)
        {
            if (districtId == null) throw new ArgumentNullException(nameof(districtId));
            var list = string.Empty;
            if (!ModelState.IsValid) return Content(list, "application/json");
            _vehModel.DistrictId = int.Parse(districtId);
            Session["Id"] = _vehModel.DistrictId;
            List<DataRow> data;

            using (var dsFillVehiclesByDistrict = _helper.FillDropDownHelperMethodWithSp("spFillVehiclePendingStatusDetails", _vehModel.DistrictId))
            {
                data = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().ToList();
            }

            var names = new List<string>();

            foreach (var row in data)
            {
                _vehModel.VehicleNumberString = row["vehiclenumber"].ToString();
                _vehModel.VehicleId = row["Id"].ToString();
                names.Add(_vehModel.VehicleNumberString + "-" + _vehModel.VehicleId);
            }

            list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        [HttpPost] public ActionResult GetAggregatesBasedOnManufacturers(int manufacturerId)
        {
            if (!ModelState.IsValid) return null;
            _vehModel.ManufacturerId = Convert.ToInt32(manufacturerId);
            Session["ManufacturerId"] = _vehModel.ManufacturerId;
            List<DataRow> data;

            using (var dsFillManufacturerBasedOnAggregates = _helper.FillDropDownHelperMethodWithSp("getAggeregatesByManufacturers", _vehModel.ManufacturerId))
            {
                data = dsFillManufacturerBasedOnAggregates.Tables[0].AsEnumerable().ToList();
            }

            var names = new List<string>();

            foreach (var row in data)
            {
                _vehModel.AggregateId = Convert.ToInt32(row["ServiceGroup_Id"]);
                _vehModel.AggregateName = row["ServiceGroup_Name"].ToString();

                names.Add(_vehModel.AggregateName + "-" + _vehModel.AggregateId);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        [HttpPost] public ActionResult GetSubCategoryIds(string categoryId)
        {
            if (categoryId == null) throw new ArgumentNullException(nameof(categoryId));
            if (!ModelState.IsValid) return null;
            _vehModel.SubCategory = int.Parse(categoryId);
            Session["SubCategoryId"] = _vehModel.SubCategory;
            List<DataRow> data;

            using (var dsFillVehiclesByDistrict = _helper.FillDropDownHelperMethodWithSpCategory("spGetSubCategoryDetails", _vehModel.SubCategory))
            {
                data = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().ToList();
            }

            var names = new List<string>();

            foreach (var row in data)
            {
                _vehModel.SubCategoryName = row["SubCategories"].ToString();
                _vehModel.SubCategoryId = row["SubCategory_Id"].ToString();

                names.Add(_vehModel.SubCategoryName + "-" + _vehModel.SubCategoryId);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        [HttpPost] public ActionResult GetCategoryIds(string aggregatedId)
        {
            if (aggregatedId == null) throw new ArgumentNullException(nameof(aggregatedId));
            if (!ModelState.IsValid) return null;
            _vehModel.AggregateId = int.Parse(aggregatedId);
            Session["AggregateId"] = _vehModel.AggregateId;
            List<DataRow> data;

            using (var dsFillCategoryByAggregate = _helper.FillDropDownHelperMethodWithSp("spGetCategory", _vehModel.AggregateId))
            {
                data = dsFillCategoryByAggregate.Tables[0].AsEnumerable().ToList();
            }

            var names = new List<string>();

            foreach (var row in data)
            {
                _vehModel.CategoryName = row["Categories"].ToString();
                _vehModel.CategoryId = row["Category_Id"].ToString();

                names.Add(_vehModel.CategoryName + "-" + _vehModel.CategoryId);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        [HttpPost] public ActionResult GetEstimatedCostDetails(string categoryId)
        {
            if (!ModelState.IsValid) return null;
            _vehModel.EstimatedCost = int.Parse(categoryId);
            decimal list;

            using (var dsgetCostBySubCategory = _helper.getCost("spGetCostDetails", _vehModel.EstimatedCost))
            {
                list = dsgetCostBySubCategory.Tables[0].AsEnumerable().Select(x => x.Field<int>("EstimatedCost")).FirstOrDefault();
            }

            return Content(list.ToString(CultureInfo.InvariantCulture), "application/json");
        }

        [HttpPost] public ActionResult GetModelNumber(string vehicleid)
        {
            if (vehicleid == null) return null;
            var list = 0;
            if (!ModelState.IsValid) return null;
            _vehModel.VehId = int.Parse(vehicleid);
            if (_vehModel.VehId == 0) return null;

            using (var dsFillVehiclesByDistrict = _helper.FillModelNumbers("spGetVehicleModelNumber", _vehModel.VehId))
            {
                list = dsFillVehiclesByDistrict.Tables[0].AsEnumerable().Select(x => x.Field<int>("model")).FirstOrDefault();
            }

            return Content(list.ToString(), "application/json");
        }

        //public ActionResult CheckPilotId(int? PilotId)
        //{
        //    if (PilotId == null) return null;
        //    int count;

        //    using (var dtCheckPilotId = _helper.ExecuteSelectStmtusingSP("checkPilotIdOnEmailScheduling", "@empid", PilotId.ToString()))
        //        //using (var dtCheckPilotId = _helper.ExecuteSelectStmtusingSP("checkPilotId", "@empid", PilotId.ToString()))
        //    {
        //        count = dtCheckPilotId.Rows.Count;
        //    }

        //    return Json(count, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult CheckPilotId(int? PilotId)
        {
            if (PilotId == null) return null;

            int count=0;
            using (var dtCheckPilotId = _helper.ExecuteSelectStmtusingSP("checkPilotIdOnEmailScheduling", "@empid", PilotId.ToString()))
                //using (var dtCheckPilotId = _helper.ExecuteSelectStmtusingSP("checkPilotId", "@empid", PilotId.ToString()))
            {
                if (dtCheckPilotId.Rows.Count == 0)
                {
                    return Json(count, JsonRequestBehavior.AllowGet);
                }

                string Name = dtCheckPilotId.AsEnumerable().Select(x => x.Field<string>("Name")).FirstOrDefault();
                return Json(Name, "application/json");
            }
        }
        public ActionResult Edit(int? id = null)
        {
            if (id == null) return RedirectToAction("SaveJobCardDetails");
            var dsGetVehicleManufacturers = Session["VehicleDistrictDetails"] as DataSet;

            DataRow row;
            if (Session["GetJobCardDetails"] == null) return RedirectToAction("SaveJobCardDetails");

            using (var dsGetJobCardDetails = Session["GetJobCardDetails"] as DataSet)
            {
                row = dsGetJobCardDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("JobCardNumber") == id);
            }

            Session["JobCardId"] = id;
            Session["Drow"] = row;

            var query = "select v.id, v.vehiclenumber,v.districtId from m_GetVehicleDetails v join m_districts d on v.districtId=d.Id where v.vehiclenumber = '" + row["vehicleNumber"] + "'";
            VehicleModel model;
            var categories = "select category_id, Categories from M_FMS_Categories";
            var dtCat = _helper.ExecuteSelectStmt(categories);
            ViewBag.Categories = new SelectList(dtCat.AsDataView(), "category_id", "Categories");
            var subCategories = "select Service_Id, Service_Name from M_FMS_MaintenanceWorksMasterDetails";
            var dtSubCat = _helper.ExecuteSelectStmt(subCategories);
            ViewBag.SubCategories = new SelectList(dtSubCat.AsDataView(), "Service_Id", "Service_Name");

            using (var dtVehicle = _helper.ExecuteSelectStmt(query))
            {
                var dtEditJob = _helper.ExecuteSelectStmtusingSP("spGetJobCardDetailsEdit", "@vehicleId", Convert.ToString(dtVehicle.Rows[0][0]));
                if(dtEditJob.Rows.Count>0)
                ViewBag.CartItems = dtEditJob;
                var fillVehicleDropQuery = "select * from m_GetVehicleDetails where districtId=" + dtVehicle.Rows[0][2] + "";
                Session["VehicleId"] = dtVehicle;

                using (var dtVehicledet = _helper.ExecuteSelectStmt(fillVehicleDropQuery))
                {
                    var complaintsQuery = "select distinct  mg.ServiceGroup_Id,mg.ServiceGroup_Name,Categories from M_FMS_MaintenanceWorksServiceGroupDetails mg join t_vehiclejobcarddetails vd on vd.NatureOfComplaint=mg.ServiceGroup_Id where vd.VehicleId=" + dtVehicle.Rows[0][0] + "";
                    var dtComplaintsQuery = _helper.ExecuteSelectStmt(complaintsQuery);

                    model = new VehicleModel {District = new SelectList(dsGetVehicleManufacturers.Tables[0].AsDataView(), "Id", "District"), ComplaintsNature = new SelectList(dtComplaintsQuery.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name"), DistrictName = row["District"].ToString(), NatureOfComplaint = Convert.ToInt32(row["NatureOfComplaint"]), Vehicle = new SelectList(dtVehicledet.AsDataView(), "Id", "VehicleNumber"), VehicleNumberString = row["VehicleNumber"].ToString(), VehId = Convert.ToInt32(dtVehicle.Rows[0][0].ToString()), DistrictId = Convert.ToInt32(dtVehicle.Rows[0][2].ToString()), DateOfRepair = Convert.ToDateTime(row["Dor"]), ModelYear = Convert.ToInt32(row["Model"]), Odometer = Convert.ToInt32(row["Odometer"]), ApproximateCost = Convert.ToInt32(row["ApproxCost"])};
                }
            }

            return View(model);
        }

        public ActionResult GetOffroadVehicleDetailsALS108(int? offroadid)
        {
            var client = new HttpClient();
            var messge = client.GetAsync("http://up108.emri.in/vehicleoffroaddetails/api/Vehicle?offroadId=" + offroadid + "").Result;

            if (!messge.IsSuccessStatusCode) return Json(0, JsonRequestBehavior.AllowGet);
            var result = messge.Content.ReadAsStringAsync().Result;
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetOffroadVehicleDetails102(int? offroadid)
        {
            var client = new HttpClient();
            var messge = client.GetAsync("http://up102.emri.in/vehicleoffroaddetails/api/Vehicle?offroadid=" + offroadid + "").Result;

            if (!messge.IsSuccessStatusCode) return Json(0, JsonRequestBehavior.AllowGet);
            var result = messge.Content.ReadAsStringAsync().Result;
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public ActionResult DeleteFromJobCard(int? id, int? vehicleId)
        {
            var result = _helper.ExecuteInsertStmtusingSp("SpdeleteJobCardIsuueItems", "@jobcardid", id.ToString(), "@vehicleid", vehicleId.ToString());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadEditCategories(int AggregateId)
        {
            var dtVehId = Session["VehicleId"] as DataTable;
            var catQuery = "select distinct  mg.ServiceGroup_Id,mg.ServiceGroup_Name,vd.Categories,cat.Categories as CategoryName from M_FMS_MaintenanceWorksServiceGroupDetails mg join t_vehiclejobcarddetails vd on vd.NatureOfComplaint=mg.ServiceGroup_Id join M_FMS_Categories cat on cat.Category_Id=vd.Categories where vd.VehicleId=" + dtVehId.Rows[0][0] + " and mg.ServiceGroup_Id=" + AggregateId + "";
            var dtCategories = _helper.ExecuteSelectStmt(catQuery);
            var data = dtCategories.AsEnumerable().ToList();
            var names = new List<string>();

            foreach (var row in data)
            {
                _jobModel.IdCategory = Convert.ToInt32(row["Categories"]);
                _jobModel.CategoryName = row["CategoryName"].ToString();
                names.Add(_jobModel.CategoryName + "-" + _jobModel.IdCategory);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        public ActionResult LoadEditSubCategories(int CategoryId)
        {
            var subCatQuery = "select* from M_FMS_MaintenanceWorksMasterDetails mw  join M_FMS_Categories cat on  cat.Aggregate_ID = mw.ServiceGroup_Id and cat.ManufacturerId=mw.ManufacturerId  where cat.Category_Id =  " + CategoryId + " ";
            var dtSubCategories = _helper.ExecuteSelectStmt(subCatQuery);
            var data = dtSubCategories.AsEnumerable().ToList();
            var names = new List<string>();

            foreach (var row in data)
            {
                _jobModel.SubCategory = Convert.ToInt32(row["Service_Id"]);
                _jobModel.SubCategoryName = row["Service_Name"].ToString();
                names.Add(_jobModel.SubCategoryName + "-" + _jobModel.SubCategory);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        [HttpPost] public ActionResult Edit(VehicleModel postVehicle, int SubCategory, int IdCategory, int districtId, int vehicleId, int Model, int Odometer, int ApproxCost, int AggregateId, DateTime Dor)
        {
            var row = Session["Drow"] as DataRow;
            var returnVal = _helper.ExecuteInsertJobCardDetails("SpVehicleJobCardDetails", districtId, vehicleId, Dor, Model, Odometer, Convert.ToString(row["ReceivedLoc"]), Convert.ToString(row["pilotid"]), Convert.ToString(row["PilotName"]), Convert.ToDateTime(row["DateOfDelivery"]), AggregateId, ApproxCost, Convert.ToInt32(row["AllotedMechanic"]), Convert.ToInt32(Session["WorkshopId"]), Convert.ToInt32(row["ServiceIncharge"]), IdCategory, SubCategory, Convert.ToInt32(row["ManufacturerId"]), 0, 0, 0, 0, 0,0,0,0);
            //_helper.ExecuteJobUpdateStatement(districtId, "spEditJobcardDetails", vehicleId, Model, Odometer, ApproxCost, Dor, AggregateId, Convert.ToInt32(Session["JobCardId"]));
            return Json(returnVal, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete] public ActionResult Delete(int? Id)
        {
            _helper.ExecuteDeleteStatement("spDeleteJobCardDetails", Id);
            return RedirectToAction("SaveJobCardDetails");
        }

        [HttpPost] public JsonResult AutoComplete(string prefix, int Manufacturer)
        {
            var query = "select * from M_FMS_MaintenanceWorksServiceGroupDetails where manufacturer_id=" + Manufacturer + "";
            var dtAggregates = _helper.ExecuteSelectStmt(query);
            var results = dtAggregates.AsEnumerable().Where(x => x.Field<string>("ServiceGroup_Name").StartsWith(prefix.ToUpper())).Select(x => new {Name = x.Field<string>("ServiceGroup_Name").ToUpper(), Value = x.Field<int>("ServiceGroup_Id")}).ToList();
            return Json(results);
        }

        [HttpPost] public JsonResult AutoCompleteCategory(string prefix, int Manufacturer, int Aggregates)
        {
            var query = "select * from m_fms_categories where aggregate_id=" + Aggregates + " and manufacturerid=" + Manufacturer + "";
            var dtCategories = _helper.ExecuteSelectStmt(query);
            var results = dtCategories.AsEnumerable().Where(x => x.Field<string>("Categories").StartsWith(prefix.ToUpper())).Select(x => new {Name = x.Field<string>("Categories").ToUpper(), Value = x.Field<int>("Category_Id")}).ToList();
            return Json(results);
        }

        [HttpPost] public JsonResult AutoCompleteSubCategory(string prefix, int Manufacturer, int Aggregates, int Categories)
        {
            var query = "select mw.Service_Name,mw.Service_Id from M_FMS_MaintenanceWorksMasterDetails mw join m_fms_categories c on c.Aggregate_id=mw.ServiceGroup_Id  where mw.ServiceGroup_id=" + Aggregates + " and mw.manufacturerid=" + Manufacturer + "";
            var dtSubCategories = _helper.ExecuteSelectStmt(query);
            var results = dtSubCategories.AsEnumerable().Where(x => x.Field<string>("Service_Name").StartsWith(prefix.ToUpper())).Select(x => new {Name = x.Field<string>("Service_Name").ToUpper(), Value = x.Field<int>("Service_Id")}).ToList();
            return Json(results);
        }

        public ActionResult ServiceGroup()
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            EnumerableRowCollection<Aggregates> aggregates;

            using (var dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers"))
            {
                ViewBag.VehicleManufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName");
                aggregates = dsVehicleDistrictDetails.Tables[2].AsEnumerable().Select(x => new Aggregates {AggregateId = x.Field<int>("ServiceGroup_Id"), AggregateName = x.Field<string>("ServiceGroup_Name"), ManufacturerName = x.Field<string>("ManufacturerName")});
            }

            return View(aggregates);
        }

        [HttpPost] public ActionResult ServiceGroup(Aggregates aggregates)
        {
            var result = _helper.InsertJobAggregateDetails("spInsertJobAggregateDetails", aggregates.ManufacturerId, aggregates.AggregateName, Convert.ToInt32(Session["Employee_Id"]));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditAggregates(int? id)
        {
            if (id == null) return RedirectToAction("ServiceGroup");
            Aggregates aggregates;

            using (var dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers"))
            {
                var row = dsVehicleDistrictDetails.Tables[2].AsEnumerable().ToList().Single(x => x.Field<int>("ServiceGroup_Id") == id);
                aggregates = new Aggregates {AggregateList = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName"), AggregateName = row["ServiceGroup_Name"].ToString(), ManufacturerId = Convert.ToInt32(row["Manufacturer_Id"]), AggregateId = Convert.ToInt32(row["ServiceGroup_Id"])};
            }

            Session["Id"] = aggregates.AggregateId;
            return View(aggregates);
        }

        [HttpPost] public ActionResult EditAggregates(Aggregates aggregates)
        {
            if (aggregates == null) return null;
            _helper.ExecuteJobAggregateUpdateStatement("spEditJobAggregateDetails", aggregates.ManufacturerId, aggregates.AggregateName, Convert.ToInt32(Session["Id"]));
            return RedirectToAction("ServiceGroup");
        }

        [HttpDelete] public ActionResult DeleteAggregates(int? id)
        {
            if (id == null) return null;
            _helper.ExecuteDeleteStatement("spDeleteAggregates", id);
            return RedirectToAction("ServiceGroup");
        }

        public ActionResult CategoryGroup()
        {
            var manufacturerQuery = "select * from m_VehicleManufacturer";

            using (var dtmanufacturers = _helper.ExecuteSelectStmt(manufacturerQuery))
            {
                ViewBag.Manufacturers = new SelectList(dtmanufacturers.AsDataView(), "Id", "ManufacturerName");
            }

            var queryCategories = "select * from M_FMS_Categories c join M_FMS_MaintenanceWorksServiceGroupDetails m on m.ServiceGroup_Id=c.Aggregate_ID";
            var Agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            var dtAgregates = _helper.ExecuteSelectStmt(Agrregates);
            ViewBag.AggregatesDrop = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name");
            EnumerableRowCollection<Aggregates> categories;

            using (var dtCategories = _helper.ExecuteSelectStmt(queryCategories))
            {
                Session["Categories"] = queryCategories;
                categories = dtCategories.AsEnumerable().Select(x => new Aggregates {IdCategory = x.Field<int>("Category_Id"), AggregateName = x.Field<string>("ServiceGroup_Name"), CategoryName = x.Field<string>("Categories")});
            }

            return View(categories);
        }

        [HttpPost] public ActionResult CategoryGroup(string ManufacturerId, string aggregateId, string categoryName)
        {
            var result = 0;
            if (ManufacturerId != "" && aggregateId != "" && categoryName != "") result = _helper.InsertJobCategoryDetails("spInsertJobCategoryDetails", Convert.ToInt32(ManufacturerId), Convert.ToInt32(aggregateId), categoryName);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditCategories(int? id)
        {
            if (id == null) return RedirectToAction("CategoryGroup");
            const string queryCategories = "select * from M_FMS_Categories";
            const string agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            Aggregates aggregates;

            using (var dtAgregates = _helper.ExecuteSelectStmt(agrregates))
            {
                DataRow row;

                using (var dtCategories = _helper.ExecuteSelectStmt(queryCategories))
                {
                    row = dtCategories.AsEnumerable().ToList().Single(x => x.Field<int>("Category_Id") == id);
                }

                aggregates = new Aggregates {AggregateList = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name"), CategoryName = row["Categories"].ToString(), AggregateId = Convert.ToInt32(row["Aggregate_ID"])};
            }

            return View(aggregates);
        }

        [HttpPost] public ActionResult EditCategories(Aggregates aggregates)
        {
            if (aggregates == null) return null;
            _helper.ExecuteJobAggregateUpdateStatement("spEditJobAggregateDetails", aggregates.ManufacturerId, aggregates.AggregateName, Convert.ToInt32(Session["Id"]));
            return RedirectToAction("ServiceGroup");
        }

        [HttpDelete] public ActionResult DeleteCategories(int? id)
        {
            if (id == null) return RedirectToAction("ServiceGroup");
            _helper.ExecuteDeleteStatement("spDeleteCategories", id);
            return RedirectToAction("ServiceGroup");
        }

        [HttpDelete] public ActionResult DeleteSubCategories(int? id)
        {
            if (id == null) return RedirectToAction("SubCategoryGroup");
            _helper.ExecuteDeleteStatement("spDeleteSubCategories", id);
            return RedirectToAction("SubCategoryGroup");
        }

        public ActionResult SubCategoryGroup()
        {
            var manufacturerQuery = "select * from m_VehicleManufacturer";

            using (var dtmanufacturers = _helper.ExecuteSelectStmt(manufacturerQuery))
            {
                ViewBag.Manufacturers = new SelectList(dtmanufacturers.AsDataView(), "Id", "ManufacturerName");
            }

            var querySubCategories = "select * from M_FMS_MaintenanceWorksMasterDetails m join m_VehicleManufacturer v on m.ManufacturerId=v.Id ";
            var categories = "select * from M_FMS_Categories";

            using (var dtCategories = _helper.ExecuteSelectStmt(categories))
            {
                ViewBag.CategoryDrop = new SelectList(dtCategories.AsDataView(), "Category_id", "Categories");
            }

            EnumerableRowCollection<Aggregates> subCategories;

            using (var dtSubCategories = _helper.ExecuteSelectStmt(querySubCategories))
            {
                Session["SubCategories"] = dtSubCategories;
                subCategories = dtSubCategories.AsEnumerable().Select(x => new Aggregates {ServiceId = x.Field<int>("Service_Id"), ServiceName = x.Field<string>("Service_Name"), ManufacturerName = x.Field<string>("ManufacturerName")});
            }

            return View(subCategories);
        }

        [HttpPost] public ActionResult SubCategoryGroup(Aggregates aggregates)
        {
            var result = 0;

            if (Session["Employee_Id"] != null)
                if (ModelState.IsValid)
                    result = _helper.InsertJobSubCategoriesDetails("spInsertSubCategoryMasterDetails", aggregates.ManufacturerId, aggregates.ServiceGroupId, aggregates.ServiceName, aggregates.timeTaken, Convert.ToInt32(Session["Employee_Id"]), Convert.ToInt32(aggregates.ApproxCost));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditSubCategories(int? id)
        {
            if (id == null) return RedirectToAction("SubCategoryGroup");
            var queryCategories = "select * from M_FMS_Categories";
            var agrregates = "select * from M_FMS_MaintenanceWorksServiceGroupDetails";
            var subcategories = "select * from M_FMS_MaintenanceWorksMasterDetails";
            Aggregates aggregates;

            using (var dsVehicleDistrictDetails = _helper.FillDropDownHelperMethodWithSp("GetDistrictsAndVehicleManufacturers"))
            {
                using (var dtAgregates = _helper.ExecuteSelectStmt(agrregates))
                {
                    using (var dtCategories = _helper.ExecuteSelectStmt(queryCategories))
                    {
                        DataRow row;

                        using (var dtSubCategories = _helper.ExecuteSelectStmt(subcategories))
                        {
                            row = dtSubCategories.AsEnumerable().ToList().Single(x => x.Field<int>("Service_id") == id);
                        }

                        var r1 = dtCategories.AsEnumerable().FirstOrDefault(x => x.Field<int>("Aggregate_ID") == Convert.ToInt32(row["ServiceGroup_Id"]));
                        if (r1 == null) return RedirectToAction("SubCategoryGroup");
                        Session["Id"] = id;
                        aggregates = new Aggregates {Categories = new SelectList(dtCategories.AsDataView(), "Category_Id", "Categories"), AggregateList = new SelectList(dtAgregates.AsDataView(), "ServiceGroup_Id", "ServiceGroup_Name"), ServiceGroupId = Convert.ToInt32(row["ServiceGroup_Id"]), IdCategory = Convert.ToInt32(r1["Aggregate_id"]), Manufacturer = new SelectList(dsVehicleDistrictDetails.Tables[1].AsDataView(), "Id", "ManufacturerName"), ManufacturerId = Convert.ToInt32(row["ManufacturerId"]), ServiceName = row["Service_Name"].ToString(), timeTaken = Convert.ToInt32(row["Time_Taken"]), ApproxCost = Convert.ToInt32(row["CostFor_A_Grade"])};
                    }
                }
            }

            return View(aggregates);
        }

        [HttpPost] public ActionResult EditSubCategories(Aggregates aggregates)
        {
            if (aggregates == null) throw new ArgumentNullException(nameof(aggregates));
            _helper.ExecuteJobSubCategoryUpdateStatement("spEditJobSubCategoryDetails", aggregates.ManufacturerId, aggregates.ServiceGroupId, aggregates.ServiceName, aggregates.timeTaken, Convert.ToInt32(Session["Id"]), Convert.ToInt32(aggregates.ApproxCost));
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
            var dtLubesNumber = _helper.ExecuteSelectStmtusingSP("getOdoReading", "@vehicleid", vehicleid.ToString());
            var list = dtLubesNumber.AsEnumerable().FirstOrDefault(x => x.Field<int>("odometer") >= odo);
            return list == null ? null : Content(list[0].ToString(), "application/Json");
        }
        public ActionResult CheckEndingOdoReading(string vehicleNumber, int? endOdo)
        {
            string getVehIdQuery = "select Id from m_GetVehicleDetails where VehicleNumber='" + vehicleNumber + "'";
            DataTable dtgetVehicleId = _helper.ExecuteSelectStmt(getVehIdQuery);
            int vehId = dtgetVehicleId.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
            var dtLubesNumber = _helper.ExecuteSelectStmtusingSP("getOdoReading", "@vehicleid", vehId.ToString());
            int currOdo = dtLubesNumber.AsEnumerable().Select(x => x.Field<int>("odometer")).FirstOrDefault();

            if (currOdo > endOdo)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            
        }

        [HttpPost] public ActionResult GetManufacturerNameForAggregates(string manufacturerId)
        {
            _jobModel.ManufacturerId = int.Parse(manufacturerId);
            Session["ManufacturerId"] = _jobModel.ManufacturerId;
            List<DataRow> data;

            using (var dsFillAggregatesByManufacturers = _helper.FillDropDownHelperMethodWithSp2("spGetAggregatesForManufacturers", _jobModel.ManufacturerId))
            {
                data = dsFillAggregatesByManufacturers.Tables[0].AsEnumerable().ToList();
            }

            var names = new List<string>();

            foreach (var row in data)
            {
                _jobModel.ServiceName = row["ServiceGroup_Name"].ToString();
                _jobModel.ServiceGroupId = Convert.ToInt32(row["ServiceGroup_Id"]);

                names.Add(_jobModel.ServiceName + "-" + _jobModel.ServiceGroupId);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        // GetCategoriesForAggregates
        public ActionResult GetCategoriesForAggregates(string aggregateId)
        {
            _jobModel.AggregateId = int.Parse(aggregateId);
            Session["AggregateId"] = _jobModel.AggregateId;
            List<DataRow> data;

            using (var dsFillCategoriesForAggregates = _helper.FillDropDownHelperMethodWithSp3("getCategoriesForAggregates", _jobModel.AggregateId))
            {
                data = dsFillCategoriesForAggregates.Tables[0].AsEnumerable().ToList();
            }

            var names = new List<string>();

            foreach (var row in data)
            {
                _jobModel.CategoryName = row["Categories"].ToString();
                _jobModel.IdCategory = Convert.ToInt32(row["Category_Id"]);

                names.Add(_jobModel.CategoryName + "-" + _jobModel.IdCategory);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        [HttpPost] public ActionResult GetCategorySubCategoryCostDetails(int aggregateid, int manufacturerid)
        {
            using (var dtDetails = _helper.ExecuteSelectStmtusingSP("getAggregateCategoryDetails", "@aggregateid", aggregateid.ToString(), "@manufacturerid", manufacturerid.ToString()))
            {
                dtDetails.AsEnumerable().Select(x => new Aggregates {CategoryName = x.Field<string>("categories"), SubCategoryName = x.Field<string>("subcategories"), ApproxCost = x.Field<int>("ApproxCost")});
                Session["Aggregates"] = dtDetails;
            }

            return RedirectToAction("GetManufacturerAggregatesDetails");
        }

        public ActionResult GetManufacturerAggregatesDetails()
        {
            var queryManufacturers = "select * from m_VehicleManufacturer";

            using (var dtManufacturers = _helper.ExecuteSelectStmt(queryManufacturers))
            {
                ViewBag.Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName");
            }

            _helper.ExecuteSelectStmtusingSP("getAggregateCategoryDetails");
            if (Session["Aggregates"] == null) return null;
            EnumerableRowCollection<Aggregates> aggregates;

            using (var dtDetails = Session["Aggregates"] as DataTable)
            {
                aggregates = dtDetails.AsEnumerable().Select(x => new Aggregates {IdCategory = x.Field<int>("Category_Id"), CategoryName = x.Field<string>("categories"), SubCategoryName = x.Field<string>("SubCategories"), ApproxCost = x.Field<int?>("ApproxCost"), ServiceId = x.Field<int>("Service_Id")});
            }

            return View(aggregates);
        }

        public ActionResult EditSubCategoryCostDetails(int? id = 0)
        {
            if (id == null) return RedirectToAction("GetManufacturerAggregatesDetails");
            var categorystr = "select * from [M_FMS_Categories]";
            var subCategorystr = "select Service_Id,Service_Name from [dbo].[M_FMS_MaintenanceWorksMasterDetails] ";
            DataTable dtSubcatStr;
            DataRow categoryIdd;
            DataTable dtSubCostDetails;

            using (var dtCategories = _helper.ExecuteSelectStmt(categorystr))
            {
                dtSubcatStr = _helper.ExecuteSelectStmt(subCategorystr);
                var dtcategoryIdd = Session["Aggregates"] as DataTable;
                if (dtcategoryIdd == null) return null;
                categoryIdd = dtcategoryIdd.AsEnumerable().FirstOrDefault(x => x.Field<int>("Service_Id") == id);
                dtSubCostDetails = _helper.ExecuteSelectStmtusingSP("getJobCardVehicleDetails", "@jobcardnumber", id.ToString());
                _jobModel.Categories = new SelectList(dtCategories.AsDataView(), "Category_Id", "Categories");
            }

            _jobModel.SubCategories = new SelectList(dtSubcatStr.AsDataView(), "Service_Id", "Service_Name");
            if (categoryIdd != null) _jobModel.IdCategory = Convert.ToInt32(categoryIdd["Category_Id"]);
            _jobModel.SubCategory = dtSubCostDetails.AsEnumerable().Select(x => x.Field<int>("Service_Id")).FirstOrDefault();
            _jobModel.ApproxCost = dtSubCostDetails.AsEnumerable().Select(x => x.Field<int?>("CostFor_A_Grade")).FirstOrDefault();
            return View(_jobModel);
        }

        [HttpPost] public ActionResult EditSubCategoryCostDetails(Aggregates aggregates)
        {
            _helper.ExecuteJobSubCategoryUpdateCost("spEditJobSubCategoryCostDetails", aggregates.ApproxCost, aggregates.SubCategory);
            return RedirectToAction("SubCategoryGroup");
        }
    }

    internal class WebApiModel
    {
        public int offroadId { get; internal set; }
        public int VehId { get; internal set; }
    }
}