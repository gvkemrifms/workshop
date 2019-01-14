using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Fleet_WorkShop.Models;
using Newtonsoft.Json;

namespace Fleet_WorkShop.Controllers
{
    public class InventoryController : Controller
    {
        private readonly EmployeeHelper _helper = new EmployeeHelper();

        private readonly InventoryModel _inventoryModel = new InventoryModel();
        // GET: Inventory

        public ActionResult SparePartsMaster()
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            var spModel = new SparePartsModel();
            var lspares = new List<SparePartsModel>();
            const string queryScrap = "select * from m_ScrapBin";
            const string gstQuery = "select * from m_gst";

            using (var dtScrap = _helper.ExecuteSelectStmt(queryScrap))
            {
                ViewBag.ScrapBin = new SelectList(dtScrap.AsDataView(), "ScrapBinId", "ScrapBinName");
            }

            using (var dtGst = _helper.ExecuteSelectStmt(gstQuery))
            {
                ViewBag.GST = new SelectList(dtGst.AsDataView(), "Id", "Percentage");
            }

            const string query = "select * from m_VehicleManufacturer";

            using (var dtSpares = _helper.ExecuteSelectStmt(query))
            {
                if (dtSpares == null) return null;
                Session["Manufacturer"] = dtSpares;
                ViewBag.Manufacturers = new SelectList(dtSpares.AsDataView(), "Id", "ManufacturerName");
            }

            var getLastIdentitySpars = "SELECT top 1 * FROM m_spareparts ORDER BY ID DESC";

            using (var dtgetLastSpare = _helper.ExecuteSelectStmt(getLastIdentitySpars))
            {
                var manufacturerId = dtgetLastSpare.AsEnumerable().Select(x => x.Field<int>("ManufacturerId")).FirstOrDefault();
                var ManufacturerName = "select manufacturername from m_VehicleManufacturer where id=" + manufacturerId + "";
                var dtManName = _helper.ExecuteSelectStmt(ManufacturerName);
                spModel.ManufacturerName = dtManName.AsEnumerable().Select(x => x.Field<string>("manufacturername")).FirstOrDefault();
                spModel.PartName = dtgetLastSpare.AsEnumerable().Select(x => x.Field<string>("PartName")).FirstOrDefault();
                spModel.Cost = dtgetLastSpare.AsEnumerable().Select(x => x.Field<decimal>("Cost")).FirstOrDefault();
                spModel.PartNumber = dtgetLastSpare.AsEnumerable().Select(x => x.Field<string>("PartNumber")).FirstOrDefault();
            }

            lspares.Add(spModel);
            return View(lspares);
        }

        [HttpPost] public ActionResult SparePartsMaster(SparePartsModel spareModel)
        {
            if (spareModel == null) return RedirectToAction("SparePartsMaster");
            if (spareModel.Cost == 0 || spareModel.PartName == null || spareModel.ManufacturerId == 0 || spareModel.PartNumber == null || spareModel.GroupId == 0 || spareModel.GroupName == null) return null;
            var returnVal = _helper.ExecuteInsertStmtusingSp("spSparePartsMaster", "@manufacturerid", spareModel.ManufacturerId.ToString(), "@scrapbinid", spareModel.ScrapBinId.ToString(), "@partname", spareModel.PartName, "@groupid", spareModel.GroupId.ToString(), null, null, null, null, null, null, "@partnumber", spareModel.PartNumber, "@cost", spareModel.Cost.ToString(CultureInfo.CurrentCulture), null, null, "@groupname", spareModel.GroupName);
            return Json(returnVal, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public JsonResult AutoComplete(string prefix)
        {
            var query = "select * from m_spareparts";
            var dtSpares = _helper.ExecuteSelectStmt(query);
            var results = dtSpares.AsEnumerable().Where(x => x.Field<string>("partNumber").EndsWith(prefix.ToUpper())).Select(x => new {Name = x.Field<string>("partNumber"), Value = x.Field<int>("Id")}).ToList();
            return Json(results);
        }

        public ActionResult DisplaySparePartsDetails(string search)
        {
            IEnumerable<SparePartsModel> spareModel;

            using (var dtSpareParts = _helper.ExecuteSelectStmtusingSP("spGetSparesForPartNumber", null, null, null, null, "@partnumber", search))
            {
                spareModel = dtSpareParts.AsEnumerable().ToList().Select(x => new SparePartsModel {Id = x.Field<int>("Id"), ManufacturerName = x.Field<string>("ManufacturerName"), PartName = x.Field<string>("PartName"), Cost = x.Field<decimal>("Cost")});
            }

            return Json(spareModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet] public ActionResult EditSpares(int? id)
        {
            if (id == null) return RedirectToAction("SparePartsMaster");
            var spModel = new SparePartsModel();
            DataRow row;

            using (var dtGetSpareDetails = Session["getSpares"] as DataTable)
            {
                if (dtGetSpareDetails == null) return null;
                row = dtGetSpareDetails.AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
            }

            Session["SparesMasterId"] = row["Id"];

            using (var dtManufacturers = Session["Manufacturer"] as DataTable)
            {
                if (dtManufacturers != null) spModel.Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName");
                spModel.ManufacturerId = Convert.ToInt32(row["ManufacturerId"]);
                spModel.PartName = row["PartName"].ToString();
                spModel.Cost = Convert.ToDecimal(row["Cost"]);
            }

            return View(spModel);
        }

        [HttpPost] public ActionResult EditSpares(SparePartsModel spModel)
        {
            if (spModel == null) return null;
            var sparesId = Convert.ToInt32(Session["SparesMasterId"]);
            _helper.ExecuteInsertStmtusingSp("spEditSparePartsMaster", "@id", sparesId.ToString(), "@manufacturerid", spModel.ManufacturerId.ToString(), "@partname", spModel.PartName, null, null, null, null, null, null, null, null, null, null, "@cost", spModel.Cost.ToString(CultureInfo.CurrentCulture));
            return RedirectToAction("SparePartsMaster");
        }

        public ActionResult LubesMaster()
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            const string gstQuery = "select * from m_gst";
            const string query = "select * from m_LubesManufactures";
            var dtLubes = _helper.ExecuteSelectStmt(query);
            ViewBag.Manufacturers = new SelectList(dtLubes.AsDataView(), "Id", "ManufacturerName");
            const string lubesQuery = "select * from m_lubes";
            IEnumerable<LubesModel> lubesModel;

            using (var dtGst = _helper.ExecuteSelectStmt(gstQuery))
            {
                ViewBag.GST = new SelectList(dtGst.AsDataView(), "Id", "Percentage");
            }

            using (var dtLubesData = _helper.ExecuteSelectStmt(lubesQuery))
            {
                Session["LubesData"] = dtLubesData;
                lubesModel = dtLubesData.AsEnumerable().ToList().Select(x => new LubesModel {Id = x.Field<int>("Id"), ManufacturerId = x.Field<int>("ManufacturerId"), OilName = x.Field<string>("OilName"), CostPerLitre = x.Field<decimal>("CostPerLitre"), LubricantNumber = x.Field<string>("LubricantNumber")});
            }

            return View(lubesModel);
        }

        [HttpPost] public ActionResult GetSparePartsDetailsForManufacturer(string manufacturerId)
        {
            if (!ModelState.IsValid) return null;
            _inventoryModel.ManufacturerId = int.Parse(manufacturerId);
            Session["Id"] = _inventoryModel.ManufacturerId;
            int manufacturerid = Convert.ToInt32(manufacturerId);
            DataSet dsFillSparesOfManufacturers;
            dsFillSparesOfManufacturers = manufacturerid != 3 ? _helper.FillDropDownHelperMethodWithSp("spGetSparesForManufacturer", _inventoryModel.ManufacturerId) : _helper.FillDropDownHelperMethodWithSp("getSparesForLocalVendor");
            var data = dsFillSparesOfManufacturers.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();

            foreach (var row in data)
            {
                _inventoryModel.SparePartId = Convert.ToInt32(row["Id"]);
                _inventoryModel.SpareName = row["PartName"].ToString();
                names.Add(_inventoryModel.SpareName + "-" + _inventoryModel.SparePartId);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        public ActionResult GetLubesDetailsForManufacturer(string manufacturerId)
        {
            if (!ModelState.IsValid) return null;
            _inventoryModel.ManufacturerId = int.Parse(manufacturerId);
            Session["Id"] = _inventoryModel.ManufacturerId;
            List<DataRow> data;

            using (var dsFillLubesOfManufacturers = _helper.FillDropDownHelperMethodWithSp("spGetLubesForManufacturer", _inventoryModel.ManufacturerId))
            {
                data = dsFillLubesOfManufacturers.Tables[0].AsEnumerable().ToList();
            }

            var names = new List<string>();

            foreach (var row in data)
            {
                _inventoryModel.LubricantId = Convert.ToInt32(row["Id"]);
                _inventoryModel.LubricantName = row["OilName"].ToString();
                names.Add(_inventoryModel.LubricantName + "-" + _inventoryModel.LubricantId);
            }

            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            return Content(list, "application/json");
        }

        public ActionResult GetCostDetails(int sparePartId)
        {
            if (sparePartId == 0) return null;
            var query = "select cost,partNumber from m_spareparts where Id='" + sparePartId + "'";
            var dtCost = _helper.ExecuteSelectStmt(query);
            var cost = dtCost.AsEnumerable().ToList().Select(x => new {Cost = x.Field<decimal>("Cost"), PartNumber = x.Field<string>("partNumber")}).FirstOrDefault();
            return cost == null ? null : Json(cost, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult GetLubesCostDetails(int id)
        {
            if (id == 0) return null;
            var query = "select CostPerLitre,LubricantNumber from m_lubes where Id='" + id + "'";
            var dtCost = _helper.ExecuteSelectStmt(query);
            var cost = dtCost.AsEnumerable().ToList().Select(x => new {Cost = x.Field<decimal>("CostPerLitre"), LubricantNumber = x.Field<string>("LubricantNumber")}).FirstOrDefault();
            return cost == null ? null : Json(cost.Cost, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult GetSpareCostDetails(int id)
        {
            var query = "select cost from m_spareparts where Id='" + id + "'";
            decimal cost;

            using (var dtCost = _helper.ExecuteSelectStmt(query))
            {
                cost = dtCost.AsEnumerable().ToList().Select(x => x.Field<decimal>("cost")).First();
            }

            return cost != 1 ? Json(cost, JsonRequestBehavior.AllowGet) : null;
        }

        [HttpPost] public ActionResult LubesMaster(LubesModel lubesModel)
        {
            var returnVal = 0;
            if (ModelState.IsValid) returnVal = _helper.ExecuteInsertStmtusingSp("spLubesMaster", "@manufacturerid", lubesModel.ManufacturerId.ToString(), "@isactive", "1", "@oilname", lubesModel.OilName, null, null, null, null, null, null, null, null, "@lubricantnumber", lubesModel.LubricantNumber, "@costperlitre", lubesModel.CostPerLitre.ToString(CultureInfo.CurrentCulture));

            if (returnVal == 1) return Json("Hello", JsonRequestBehavior.AllowGet);
            return RedirectToAction("LubesMaster");
        }

        [HttpGet] public ActionResult LubesMasterEdit(int? id)
        {
            if (id == null) return RedirectToAction("LubesMaster");

            var dtGetSpareDetails = Session["LubesData"] as DataTable;
            if (dtGetSpareDetails == null) return null;
            var row = dtGetSpareDetails.AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
            var spModel = new LubesModel {Id = Convert.ToInt32(row["Id"])};
            Session["Id"] = spModel.Id;
            const string query = "select * from m_VehicleManufacturer";

            using (var dtLubes = _helper.ExecuteSelectStmt(query))
            {
                spModel.Manufacturer = new SelectList(dtLubes.AsDataView(), "Id", "ManufacturerName");
                spModel.ManufacturerId = Convert.ToInt32(row["ManufacturerId"]);
                spModel.OilName = row["OilName"].ToString();
                spModel.CostPerLitre = Convert.ToDecimal(row["CostPerLitre"]);
            }

            return View(spModel);
        }

        [HttpPost] public ActionResult LubesMasterEdit(LubesModel lubesModel)
        {
            if (lubesModel == null) throw new ArgumentNullException(nameof(lubesModel));
            var lubesId = Convert.ToInt32(Session["Id"]);
            _helper.ExecuteInsertStmtusingSp("spEditLubesMaster", "@manufacturerid", lubesModel.ManufacturerId.ToString(), "@id", lubesId.ToString(), "@oilname", lubesModel.OilName, null, null, null, null, null, null, null, null, null, null, "@costPerlitre", lubesModel.CostPerLitre.ToString(CultureInfo.CurrentCulture));
            return RedirectToAction("LubesMaster");
        }

        public ActionResult DeleteLubesMaster(int? id)
        {
            _helper.ExecuteDeleteStatement("spDeleteLubesMaster", id);
            return RedirectToAction("LubesMaster");
        }

        public ActionResult GetSparePartsDetails() { return Session["Employee_Id"] == null ? RedirectToAction("Login", "Account") : RedirectToAction("SaveInventoryDetails"); }

        public ActionResult SavePODetails()
        {
            if (Session["WorkshopId"] == null) return RedirectToAction("Login", "Account");

            using (var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor"))
            {
                ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id", "ManufacturerName");
                ViewBag.Spares = new SelectList(dsGetManufacturerVendor.Tables[2].AsDataView(), "Id", "PartName");
            }

            var roleId = Convert.ToInt32(Session["RoleId"]) + 1;

            if (roleId != 4 || roleId != 5)
            {
                var query = "select employeeid,employeename,roleid from m_employees where roleid=" + roleId + " and workshop_id=" + Session["WorkshopId"] + "";

                using (var dtRole = _helper.ExecuteSelectStmt(query))
                {
                    ViewBag.Roles = new SelectList(dtRole.AsDataView(), "employeeid", "employeename");
                }
            }
            else
            {
                var query = "select employeeid,employeename,roleid from m_employees where roleid in (4,5) and workshop_id=" + Session["WorkshopId"] + "";

                using (var dtRole = _helper.ExecuteSelectStmt(query))
                {
                    ViewBag.Roles = new SelectList(dtRole.AsDataView(), "employeeid", "employeename");
                }
            }

            IEnumerable<InventoryModel> invModel;

            using (var GetSparesPODetailList = _helper.FillDropDownHelperMethodWithSp("spGetSparesPODetailList"))
            {
                invModel = GetSparesPODetailList.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel {Id = x.Field<int>("id"), PoNumber = x.Field<string>("PoNumber"), ManName = x.Field<string>("ManufacturerName"), PoDate = x.Field<DateTime>("PoDate"), PartName = x.Field<string>("PartName"), PartNumber = x.Field<string>("PartNumber"), Uprice = x.Field<decimal>("UnitPrice"), Qty = x.Field<decimal>("Quantity"), Amt = x.Field<decimal>("Amount")});
            }

            return View(invModel);
        }

        [HttpPost] public ActionResult SavePODetails(InventoryModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var employee = Session["Employee_Id"].ToString();
            var billDetails = new InventoryModel {PoNumber = model.PoNumber, PoDate = model.PoDate, EmployeeId = Convert.ToInt32(employee)};
            var result = _helper.ExecutePODetails("spInsertPODetails", billDetails.PoNumber, billDetails.PoDate, billDetails.EmployeeId);
            foreach (var items in model.itemmodel) _helper.ExecuteInsertPOManufacturerDetails("spInsertSparePODetails", billDetails.PoNumber, Convert.ToInt32(items.ManufacturerId), Convert.ToInt32(items.SparePartId), Convert.ToDecimal(items.UnitPrice), Convert.ToInt32(items.Quantity), Convert.ToDecimal(items.Amount), 1);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult ApprovePODetails(InventoryModel model, int EmployeeId1 = 0)
        {
            if (Session["WorkshopId"] == null) return RedirectToAction("Login", "Account");
            if (model == null) throw new ArgumentNullException(nameof(model));
            var employee = Session["Employee_Id"].ToString();
            var workshopId = Convert.ToInt32(Session["WorkshopId"]);
            var billDetails = new InventoryModel {PoNumber = model.PoNumber, PoDate = model.PoDate, EmployeeId = Convert.ToInt32(employee), Status = "Pending", Remarks = model.Remarks, RoleId = Convert.ToInt32(Session["RoleId"]), WorkShopId = workshopId};
            if (billDetails.Remarks == null) return Json("", JsonRequestBehavior.AllowGet);
            var result = _helper.ExecuteTemporaryPODetails("spPurchaseOrderTemp", billDetails.PoNumber, billDetails.PoDate, billDetails.EmployeeId, billDetails.RoleId, billDetails.Status, billDetails.Remarks, billDetails.WorkShopId, 1, EmployeeId1);
            foreach (var items in model.itemmodel) _helper.ExecuteInsertPOManufacturerDetails("spPurchaseorderItemsTemp", billDetails.PoNumber, Convert.ToInt32(items.ManufacturerId), Convert.ToInt32(items.SparePartId), Convert.ToDecimal(items.UnitPrice), Convert.ToInt32(items.Quantity), Convert.ToDecimal(items.Amount), 1);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult ApproveLubesPODetails(InventoryModel model, int EmployeeId1 = 0)
        {
            if (Session["WorkshopId"] == null) return RedirectToAction("Login", "Account");
            if (model == null) throw new ArgumentNullException(nameof(model));
            var employee = Session["Employee_Id"].ToString();
            var workshopId = Convert.ToInt32(Session["WorkshopId"]);
            var billDetails = new InventoryModel {PoNumber = model.PoNumber, PoDate = model.PoDate, EmployeeId = Convert.ToInt32(employee), Status = "Pending", Remarks = model.Remarks, RoleId = Convert.ToInt32(Session["RoleId"]), WorkShopId = workshopId};
            if (billDetails.Remarks == null) return Json("", JsonRequestBehavior.AllowGet);
            var result = _helper.ExecuteTemporaryPODetails("spPurchaseOrderTemp", billDetails.PoNumber, billDetails.PoDate, billDetails.EmployeeId, billDetails.RoleId, billDetails.Status, billDetails.Remarks, billDetails.WorkShopId, 2, EmployeeId1);
            foreach (var items in model.itemmodel) _helper.ExecuteInsertPOManufacturerDetails("spPurchaseorderItemsTemp", billDetails.PoNumber, Convert.ToInt32(items.ManufacturerId), Convert.ToInt32(items.LubricantId), Convert.ToDecimal(items.UnitPrice), Convert.ToDecimal(items.LubesQty), Convert.ToDecimal(items.Amount), 2);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveInventoryDetails()
        {
            if (Session["WorkshopId"] == null) return RedirectToAction("Login", "Account");

            using (var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor"))
            {
                ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id", "ManufacturerName");
                ViewBag.Vendors = new SelectList(dsGetManufacturerVendor.Tables[1].AsDataView(), "id", "vendor_name");
                ViewBag.Spares = new SelectList(dsGetManufacturerVendor.Tables[2].AsDataView(), "Id", "PartName");
            }

            var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetReceiptDetails",Convert.ToInt32(Session["WorkshopId"]));
            var invModel = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel {Id = x.Field<int>("id"), BillNo = x.Field<string>("BillNumber"), ManName = x.Field<string>("ManufacturerName"), BillDate = x.Field<DateTime>("BillDate"), BillAmount = x.Field<decimal>("BillAmount"), PartName = x.Field<string>("PartName"), PartNumber = x.Field<string>("PartNumber"), Uprice = x.Field<decimal>("UnitPrice"), Qty = x.Field<int>("Quantity"), Amt = x.Field<decimal>("Amount")});
            return View(invModel);
        }

        public ActionResult checkForValidUnitPrice(int uPrice)
        {
            var ItemValue = 0;
            var roleId = Convert.ToString(Session["RoleId"]);

            using (var dtItemVal = _helper.ExecuteSelectStmtusingSP("getPOValueDetails", "@roleid", roleId))
            {
                if (uPrice == 0)
                {
                    var lstItemVal = dtItemVal.AsEnumerable().Select(x => new {ItemValue = x.Field<int>("ItemValue"), POValue = x.Field<int>("POValue")});
                    return Json(lstItemVal, JsonRequestBehavior.AllowGet);
                }

                ItemValue = dtItemVal.AsEnumerable().Select(x => x.Field<int>("ItemValue")).FirstOrDefault();
            }

            return Json(ItemValue, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult SaveInventoryDetails(InventoryModel model)
        {
            var result = 0;

            if (model.BillNo == null || model.BillDate == null || model.VendorId == 0) return Json(result, JsonRequestBehavior.AllowGet);
            var poQuantitySpares = Session["PoQuantitySpares"] as IEnumerable<GetPODetailsSpareParts>;
            var billDetails = new InventoryModel {BillNo = model.BillNo, BillDate = model.BillDate, BillAmount = model.BillAmount, VendorName = model.VendorName, VendorId = model.VendorId, PoNumber = model.PoNumber, PoDate = model.PoDate, WorkShopId = Convert.ToInt32(Session["WorkshopId"])};
            _helper.ExecuteBillDetails("spInsertBillDetails", billDetails.BillNo, billDetails.BillDate, billDetails.BillAmount, billDetails.VendorId, billDetails.PoNumber, billDetails.PoDate, billDetails.WorkShopId);

            foreach (var items in model.itemmodel)
            {
                _helper.ExecuteInsertInventoryDetails("spInsertInventoryDetails", model.BillNo, items.ManufacturerId, items.SparePartId, items.UnitPrice, items.Quantity, items.Amount, model.VendorId, billDetails.PoNumber);

                foreach (var poitem in poQuantitySpares)
                    if (poitem.SparePartId == items.SparePartId)
                        result = _helper.UpdateSparePartsPoDetails("UpdateSparePartsPODetails", poitem.ReceivedQuantity + items.Quantity, model.BillDate, items.ManufacturerId, items.SparePartId, model.PoNumber);
            }

            CommonMethod(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult SaveLocalInventoryDetails(InventoryModel model)
        {
            var result = 0;
            model.PoDate = DateTime.Now;
            model.PoNumber = "LocalPurchase";

            if (model.BillNo == null || model.BillDate == null || model.VendorId == 0) return Json(result, JsonRequestBehavior.AllowGet);
            var billDetails = new InventoryModel {BillNo = model.BillNo, BillDate = model.BillDate, BillAmount = model.BillAmount, VendorName = model.VendorName, VendorId = model.VendorId, PoNumber = model.PoNumber, PoDate = model.PoDate, WorkShopId = Convert.ToInt32(Session["WorkshopId"])};
            _helper.ExecuteBillDetails("spInsertBillDetails", billDetails.BillNo, billDetails.BillDate, billDetails.BillAmount, billDetails.VendorId, billDetails.PoNumber, billDetails.PoDate, billDetails.WorkShopId);

            foreach (var items in model.itemmodel) result = _helper.ExecuteInsertInventoryDetails("spInsertInventoryDetails", model.BillNo, items.ManufacturerId, items.SparePartId, items.UnitPrice, items.Quantity, items.Amount, model.VendorId, billDetails.PoNumber);

            CommonMethod(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void CommonMethod(InventoryModel model)
        {
            var query = "select workshopid,receipt_id from t_receipts where billnumber='" + model.BillNo + "'and vendorid=" + model.VendorId + "";
            int workShopId;
            long receiptId;

            using (var dtWorshopId = _helper.ExecuteSelectStmt(query))
            {
                workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
                receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            }

            foreach (var item in model.itemmodel) _helper.ExecuteInsertStockDetails("spInsertInventoryStockDetails", workShopId, item.ManufacturerId, item.SparePartId, item.UnitPrice, item.Quantity, receiptId, model.BillNo, model.VendorId);
        }

        public void CommonMethodLubes(InventoryModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var query = "select workshopid,receipt_id from t_lubesreceipts where billnumber='" + model.BillNo + "'and vendorid=" + model.VendorId + "";
            int workShopId;
            long receiptId;

            using (var dtWorshopId = _helper.ExecuteSelectStmt(query))
            {
                workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
                receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            }

            foreach (var item in model.itemmodel) _helper.ExecuteInsertLubeStockDetails("spInsertLubesStockDetails", workShopId, item.ManufacturerId, item.LubricantId, item.UnitPrice, item.LubesQty, receiptId, model.BillNo, model.VendorId);
        }

        [HttpGet] public ActionResult Edit(int? id = null)
        {
            if (id == null) return RedirectToAction("SaveInventoryDetails");
            DataRow row;

            using (var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetReceiptDetails",Convert.ToInt32(Session["WorkShopId"])))
                
            {
                if (dsGetReceiptsDetails.Tables[0].Rows.Count <= 0) return RedirectToAction("SaveInventoryDetails");
                row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int?>("Id") == id);
            }

            var manufacturerId = Convert.ToInt32(row["ManufacturerId"]);
            var manufacturerQuery = "select * from m_VehicleManufacturer where Id=" + manufacturerId + " ";
            InventoryModel model;

            using (var dtManufacturers = _helper.ExecuteSelectStmt(manufacturerQuery))
            {
                var dtGetReceiptsDetailsonSpares = _helper.ExecuteSelectStmtusingSP("spGetReceiptDetailsonBillNumber", null, null, null, null, "@billnumber", row["BillNumber"].ToString());
                model = new InventoryModel {BillNo = row["BillNumber"].ToString(), ManName = row["ManufacturerName"].ToString(), PartName = row["PartName"].ToString(), Uprice = Convert.ToDecimal(row["UnitPrice"]), Qty = Convert.ToInt32(row["Quantity"]), Amt = Convert.ToDecimal(row["Amount"]), BillAmount = Convert.ToDecimal(row["BillAmount"]), BillDate = DateTime.Parse(row["BillDate"].ToString()), ManufacturerId = Convert.ToInt32(row["ManufacturerId"]), Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName"), SpareParts = new SelectList(dtGetReceiptsDetailsonSpares.AsDataView(), "SparePartId", "PartName"), VendorId = Convert.ToInt32(row["vendorid"]), SparePartId = Convert.ToInt32(row["SparePartId"])};
            }

            using (var dtgetAllSpares = _helper.ExecuteSelectStmtusingSP("getSparesForBillNumberAndVendors", "@vendorid", model.VendorId.ToString(), null, null, "@billnumber", model.BillNo))
            {
                ViewBag.CartItems = dtgetAllSpares;
            }

            var getPo = "select top 1 r.PoNumber,billamount from t_receipts r join t_receiptData rd on r.BillNumber=rd.BillNumber where r.BillNumber='" + model.BillNo + "'";
            string getPoNumber;

            using (var dtGetPoNum = _helper.ExecuteSelectStmt(getPo))
            {
                getPoNumber = dtGetPoNum.AsEnumerable().Select(x => x.Field<string>("PoNumber")).FirstOrDefault();
            }

            ViewBag.BillAmountss = model.BillAmount;

            using (var dtGetPoNumber = _helper.ExecuteSelectStmtusingSP("spSparePartsEditPODetails", null, null, null, null, "@ponumber", getPoNumber))
            {
                ViewBag.SparesQty = dtGetPoNumber;
            }

            Session["BillAmount"] = model.BillAmount;
            Session["Amt"] = model.Amt;
            Session["Bill"] = model.BillNo;
            Session["VendorId"] = model.VendorId;
            Session["PONumber"] = getPoNumber;

            return View(model);
        }

        [HttpGet] public ActionResult EditPurchaseOrderSparesDetaiList(int? id = null)
        {
            if (id == null) return RedirectToAction("SavePODetails");
            DataRow row;

            using (var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetSparesPODetailList"))
            {
                if (dsGetReceiptsDetails.Tables[0].Rows.Count <= 0) return RedirectToAction("SavePODetails");
                row = dsGetReceiptsDetails.Tables[0].AsEnumerable().Where(x => x.Field<int?>("Id") == id).FirstOrDefault();
            }

            if (row == null) return null;
            var manufacturerId = Convert.ToInt32(row["ManufacturerId"]);
            var manufacturerQuery = "select * from m_VehicleManufacturer where Id=" + manufacturerId + " ";
            var dtManufacturers = _helper.ExecuteSelectStmt(manufacturerQuery);
            var dtGetReceiptsDetailsonSpares = _helper.ExecuteSelectStmtusingSP("spGetReceiptDetailsOnPONumber", null, null, null, null, "@billnumber", row["PoNumber"].ToString());
            var model = new InventoryModel {PoNumber = row["PoNumber"].ToString(), ManName = row["ManufacturerName"].ToString(), PartName = row["PartName"].ToString(), Uprice = Convert.ToDecimal(row["UnitPrice"]), Qty = Convert.ToInt32(row["Quantity"]), Amt = Convert.ToDecimal(row["Amount"]), PoDate = DateTime.Parse(row["PoDate"].ToString()), ManufacturerId = Convert.ToInt32(row["ManufacturerId"]), Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName"), SpareParts = new SelectList(dtGetReceiptsDetailsonSpares.AsDataView(), "SparePartId", "PartName"), SparePartId = Convert.ToInt32(row["SparePartId"])};
            var dtgetAllSpares = _helper.ExecuteSelectStmtusingSP("getSparesForPoNumber", null, null, null, null, "@billnumber", model.PoNumber);
            ViewBag.CartItems = dtgetAllSpares;
            return View(model);
        }

        [HttpGet] public ActionResult EditPurchaseOrderLubesDetaiList(int? id = null)
        {
            if (id == null) return RedirectToAction("SaveLubesInventoryPODetails");
            DataRow row;

            using (var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesPODetailList"))
            {
                if (dsGetReceiptsDetails.Tables[0].Rows.Count <= 0) return RedirectToAction("SaveLubesInventoryPODetails");
                row = dsGetReceiptsDetails.Tables[0].AsEnumerable().Where(x => x.Field<int?>("Id") == id).FirstOrDefault();
            }

            if (row == null) return null;
            var manufacturerId = Convert.ToInt32(row["ManufacturerId"]);
            var manufacturerQuery = "select * from m_VehicleManufacturer where Id=" + manufacturerId + " ";
            var dtManufacturers = _helper.ExecuteSelectStmt(manufacturerQuery);
            InventoryModel model;

            using (var dtGetReceiptsDetailsonSpares = _helper.ExecuteSelectStmtusingSP("spGetLubesReceiptDetailsOnPONumber", null, null, null, null, "@billnumber", row["PoNumber"].ToString()))
            {
                model = new InventoryModel {PoNumber = row["PoNumber"].ToString(), ManName = row["ManufacturerName"].ToString(), LubricantName = row["OilName"].ToString(), Uprice = Convert.ToDecimal(row["UnitPrice"]), Qty = Convert.ToDecimal(row["Quantity"]), Amt = Convert.ToDecimal(row["Amount"]), PoDate = DateTime.Parse(row["PoDate"].ToString()), ManufacturerId = Convert.ToInt32(row["ManufacturerId"]), Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName"), Lubricant = new SelectList(dtGetReceiptsDetailsonSpares.AsDataView(), "LubricantId", "OilName"), LubricantId = Convert.ToInt32(row["LubricantId"])};
            }

            using (var dtgetAllSpares = _helper.ExecuteSelectStmtusingSP("getLubesForPoNumber", null, null, null, null, "@billnumber", model.PoNumber))
            {
                ViewBag.CartItems = dtgetAllSpares;
            }

            return View(model);
        }

        public ActionResult DeleteStockDetails(int? id, string bill, string ponumber, decimal? quantity)
        {
            if (quantity != null) quantity = Convert.ToInt32(quantity);
            var result = _helper.ExecuteInsertStmtusingSp("Spdeletestocks", "@sparepartid", id.ToString(), null, null, "@billnumber", bill, "@quantity", quantity.ToString(), null, null, null, null, null, null, "@ponumber", ponumber);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteLubesDetails(int? id, string bill, string ponumber, decimal? quantity)
        {
            if (quantity != null) quantity = Convert.ToInt32(quantity);
            var result = _helper.ExecuteInsertStmtusingSp("SpdeleteLubes", "@sparepartid", id.ToString(), null, null, "@billnumber", bill, "@quantity", quantity.ToString(), null, null, null, null, null, null, "@ponumber", ponumber);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePOSparesDetailList(int? id, string ponumber, decimal? quantity)
        {
            if (quantity != null) quantity = Convert.ToInt32(quantity);
            var checkReceivedQtyQty = "select receivedquantity from t_sparepartspodetails where ponumber='" + ponumber + "' and sparepartid=" + id + "";
            int receivedQty;

            using (var dtReceivedQty = _helper.ExecuteSelectStmt(checkReceivedQtyQty))
            {
                receivedQty = dtReceivedQty.AsEnumerable().Select(x => x.Field<int>("receivedquantity")).FirstOrDefault();
            }

            if (receivedQty != 0) return Json(0, JsonRequestBehavior.AllowGet);
            var result = _helper.ExecuteInsertStmtusingSp("SpdeletePoDetailsList", "@sparepartid", id.ToString(), null, null, null, null, "@quantity", quantity.ToString(), null, null, null, null, null, null, "@ponumber", ponumber);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePOLubesDetailList(int? id, string ponumber, decimal? quantity)
        {
            if (quantity != null) quantity = Convert.ToInt32(quantity);
            var checkReceivedQtyQty = "select receivedquantity from t_LubesPodetails where ponumber='" + ponumber + "' and lubricantid=" + id + "";
            decimal receivedQty;

            using (var dtReceivedQty = _helper.ExecuteSelectStmt(checkReceivedQtyQty))
            {
                receivedQty = dtReceivedQty.AsEnumerable().Select(x => x.Field<decimal>("receivedquantity")).FirstOrDefault();
            }

            if (receivedQty != 0) return Json(0, JsonRequestBehavior.AllowGet);
            var result = _helper.ExecuteInsertStmtusingSp("SpdeleteLubesPoDetailsList", "@sparepartid", id.ToString(), null, null, null, null, "@quantity", quantity.ToString(), null, null, null, null, null, null, "@ponumber", ponumber);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult EditOrderItemsInventoryDetails(InventoryModel postInventory)
        {
            var result = 0;
            var totBill = "";
            var differenceInQuantity = 0;

            foreach (var item in postInventory.itemmodel)
            {
                result = _helper.ExecuteInsertStmtusingSp("editSparequantityinpurchaseorder", null, null, "@sparepartid", item.SparePartId.ToString(), "@ponumber", Session["PONumber"].ToString(), null, null, null, null, null, null, null, null, null, null, "@quantity", item.Quantity.ToString());
                _helper.ExecuteInsertStmtusingSp("editSparequantitywithbillnumber", null, null, null, null, "@billnumber", postInventory.BillNo, null, null, "@sparepartid", item.SparePartId.ToString(), null, null, null, null, null, null, "@amount", item.Amount.ToString(CultureInfo.CurrentCulture), "@quantity", item.Quantity.ToString());
            }

            totBill = "select sum(Amount) as TotalAmount from t_receiptData where BillNumber='" + postInventory.BillNo + "' and ponumber='" + Session["PONumber"] + "'";
            decimal totalAmount;

            using (var dtTotal = _helper.ExecuteSelectStmt(totBill))
            {
                totalAmount = dtTotal.AsEnumerable().Select(x => x.Field<decimal>("TotalAmount")).FirstOrDefault();
            }

            var updateReceipt = "update t_receipts set billamount=" + Convert.ToDecimal(totalAmount) + "";
            _helper.ExecuteSelectStmt(updateReceipt);
            if (result != 0) return Json(totalAmount, JsonRequestBehavior.AllowGet);
            return null;
        }

        public ActionResult EditOrderItemsInventoryPODetails(InventoryModel postInventory, string PONumber)
        {
            var result = 0;
            foreach (var item in postInventory.itemmodel) result = _helper.ExecuteInsertStmtusingSp("spUpdateSparesPoDetailsList", "@sparepartid", Convert.ToString(item.SparePartId), "@quantity", Convert.ToString(item.Quantity), "@ponumber", PONumber, null, null, null, null, null, null, null, null, null, null, "@unitprice", Convert.ToString(item.UnitPrice), "@amount", Convert.ToString(item.Amount));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditOrderItemsLubesInventoryPODetails(InventoryModel postInventory, string PONumber)
        {
            var result = 0;
            foreach (var item in postInventory.itemmodel) result = _helper.ExecuteInsertStmtusingSp("spUpdateLubesPoDetailsList", "@lubricantid", Convert.ToString(item.LubricantId), null, null, "@ponumber", PONumber, null, null, null, null, null, null, null, null, null, null, "@unitprice", Convert.ToString(item.UnitPrice), "@amount", Convert.ToString(item.Amount), "@quantity", Convert.ToString(item.LubesQty));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public ActionResult EditOrderItemsLubesInventoryDetails(InventoryModel postInventory)
        {
            var result = 0;
            var totBill = "";
            decimal differenceInQuantity = 0;

            foreach (var item in postInventory.itemmodel)
            {
                result = _helper.ExecuteInsertStmtusingSp("editLubesquantityinpurchaseorder", null, null, "@lubricantid", item.SparePartId.ToString(), "@ponumber", Session["PONumber"].ToString(), null, null, null, null, null, null, null, null, null, null, "@quantity", item.LubesQty.ToString());
                _helper.ExecuteInsertStmtusingSp("editLubesquantitywithbillnumber", null, null, null, null, "@billnumber", postInventory.BillNo, null, null, "@lubricantid", item.SparePartId.ToString(), null, null, null, null, null, null, "@amount", item.Amount.ToString(CultureInfo.CurrentCulture), "@quantity", item.LubesQty.ToString());
            }

            totBill = "select sum(Amount) as TotalAmount from t_LubricantreceiptData where BillNumber='" + postInventory.BillNo + "'";
            var dtTotal = _helper.ExecuteSelectStmt(totBill);
            var totalAmount = dtTotal.AsEnumerable().Select(x => x.Field<decimal>("TotalAmount")).FirstOrDefault();
            var updateReceipt = "update t_Lubesreceipts set billamount=" + Convert.ToDecimal(totalAmount) + "";
            _helper.ExecuteSelectStmt(updateReceipt);
            return result != 0 ? Json(totalAmount, JsonRequestBehavior.AllowGet) : null;
        }

        public ActionResult SaveLubesInventoryPODetails()
        {
            if (Session["WorkshopId"] == null) return RedirectToAction("Login", "Account");

            using (var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor"))
            {
                ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[4].AsDataView(), "Id", "ManufacturerName");

                ViewBag.Lubes = new SelectList(dsGetManufacturerVendor.Tables[3].AsDataView(), "Id", "OilName");
            }

            IEnumerable<InventoryModel> invModel;

            using (var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesPODetailsList"))
            {
                var roleId = Convert.ToInt32(Session["RoleId"]) + 1;
                var query = "select employeeid,employeename,roleid from m_employees where roleid=" + roleId + " and workshop_id=" + Session["WorkshopId"] + "";
                var dtRole = _helper.ExecuteSelectStmt(query);
                ViewBag.Roles = new SelectList(dtRole.AsDataView(), "employeeid", "employeename");
                invModel = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel {Id = x.Field<int>("id"), PoNumber = x.Field<string>("PoNumber"), ManName = x.Field<string>("ManufacturerName"), PoDate = x.Field<DateTime>("PoDate"), LubricantName = x.Field<string>("OilName"), Uprice = x.Field<decimal>("UnitPrice"), Qty = x.Field<decimal>("Quantity"), Amt = x.Field<decimal>("Amount")});
            }

            return View(invModel);
        }

        [HttpPost] public ActionResult SaveLubesInventoryPODetails(InventoryModel model)
        {
            var result = 0;

            if (model.PoNumber == null || model.PoDate == null) return Json(result, JsonRequestBehavior.AllowGet);
            var employee = Session["Employee_Id"].ToString();
            var billDetails = new InventoryModel {PoNumber = model.PoNumber, PoDate = model.PoDate, EmployeeId = Convert.ToInt32(employee)};
            result = _helper.ExecutePODetails("spInsertLubesPODetails", billDetails.PoNumber, billDetails.PoDate, billDetails.EmployeeId);
            foreach (var items in model.itemmodel) _helper.ExecuteInsertPOManufacturerDetails("spInsertLubePODetails", billDetails.PoNumber, Convert.ToInt32(items.ManufacturerId), Convert.ToInt32(items.LubricantId), Convert.ToDecimal(items.UnitPrice), Convert.ToDecimal(items.LubesQty), Convert.ToDecimal(items.Amount), 2);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveLubesInventoryDetails()
        {
            if (Session["WorkshopId"] == null) return RedirectToAction("Login", "Account");

            using (var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor"))
            {
                ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[4].AsDataView(), "Id", "ManufacturerName");
                ViewBag.Vendors = new SelectList(dsGetManufacturerVendor.Tables[1].AsDataView(), "id", "vendor_name");
                ViewBag.Lubes = new SelectList(dsGetManufacturerVendor.Tables[3].AsDataView(), "Id", "OilName");
            }

            IEnumerable<InventoryModel> invModel;

            using (var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails",Convert.ToInt32(Session["WorkshopId"])))
            {
                invModel = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel {Id = x.Field<int>("id"), BillNo = x.Field<string>("BillNumber"), ManName = x.Field<string>("ManufacturerName"), BillDate = x.Field<DateTime>("BillDate"), BillAmount = x.Field<decimal>("BillAmount"), LubricantName = x.Field<string>("OilName"), Uprice = x.Field<decimal>("UnitPrice"), Qty = x.Field<decimal>("Quantity"), Amt = x.Field<decimal>("Amount")});
            }

            return View(invModel);
        }

        [HttpPost] public ActionResult SaveLubesInventoryDetails(InventoryModel model)
        {
            var result = 0;

            if (model.BillNo == null || model.BillDate == DateTime.MinValue || model.VendorId == 0) return Json(result, JsonRequestBehavior.AllowGet);
            var poQuantityLubes = Session["PoQuantityLubes"] as IEnumerable<GetPODetailsSpareParts>;
            var billDetails = new InventoryModel {BillNo = model.BillNo, BillDate = model.BillDate, BillAmount = model.BillAmount, VendorName = model.VendorName, VendorId = model.VendorId, PoNumber = model.PoNumber, PoDate = model.PoDate, WorkShopId = Convert.ToInt32(Session["WorkshopId"])};
            _helper.ExecuteBillDetails("spInsertLubeBillDetails", billDetails.BillNo, billDetails.BillDate, billDetails.BillAmount, billDetails.VendorId, billDetails.PoNumber, billDetails.PoDate, billDetails.WorkShopId);

            foreach (var items in model.itemmodel)
            {
                _helper.ExecuteInsertLubesDetails("spInsertLubricantDetails", model.BillNo, items.ManufacturerId, items.LubricantId, items.UnitPrice, items.LubesQty, items.Amount, billDetails.VendorId);

                foreach (var poitem in poQuantityLubes)
                    if (poitem.SparePartId == items.LubricantId)
                        result = _helper.UpdateSparePartsPoDetails("UpdateLubesPODetails", poitem.LubesReceivedQuantity + items.LubesQty, model.BillDate, items.ManufacturerId, items.LubricantId, model.PoNumber);
            }

            CommonMethodLubes(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditLubeDetails(int? id = null)
        {
            if (id == null) return RedirectToAction("SaveLubesInventoryDetails");
            var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            DataRow row;

            using (var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails",Convert.ToInt32(Session["WorkshopId"])))
            {
                row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int?>("Id") == id);
            }

            var model = new InventoryModel {BillNo = row["BillNumber"].ToString(), ManName = row["ManufacturerName"].ToString(), LubricantName = row["OilName"].ToString(), Uprice = Convert.ToDecimal(row["UnitPrice"]), Qty = Convert.ToInt32(row["Quantity"]), Amt = Convert.ToDecimal(row["Amount"]), BillAmount = Convert.ToDecimal(row["BillAmount"]), BillDate = DateTime.Parse(row["BillDate"].ToString()), Manufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id", "ManufacturerName"), Lubricant = new SelectList(dsGetManufacturerVendor.Tables[3].AsDataView(), "Id", "OilName"), VendorId = Convert.ToInt32(row["vendorid"]), ManufacturerId = Convert.ToInt32(row["ManufacturerId"]), LubricantId = Convert.ToInt32(row["LubricantId"])};

            using (var dtgetAllSpares = _helper.ExecuteSelectStmtusingSP("getLubesForBillNumberAndVendors", "@vendorid", model.VendorId.ToString(), null, null, "@billnumber", model.BillNo))
            {
                ViewBag.CartItems = dtgetAllSpares;
            }

            var getPo = "select top 1 r.PoNumber,billamount from t_Lubesreceipts r join t_LubricantreceiptData rd on r.BillNumber=rd.BillNumber where r.BillNumber='" + model.BillNo + "'";
            string getPoNumber;

            using (var dtGetPoNum = _helper.ExecuteSelectStmt(getPo))
            {
                getPoNumber = dtGetPoNum.AsEnumerable().Select(x => x.Field<string>("PoNumber")).FirstOrDefault();
            }

            ViewBag.BillAmountss = model.BillAmount;

            using (var dtGetPoNumber = _helper.ExecuteSelectStmtusingSP("spLubessEditPODetails", null, null, null, null, "@ponumber", getPoNumber))
            {
                ViewBag.SparesQty = dtGetPoNumber;
            }

            Session["BillAmount"] = model.BillAmount;
            Session["Amt"] = model.Amt;
            Session["Bill"] = model.BillNo;
            Session["VendorId"] = model.VendorId;
            Session["PONumber"] = getPoNumber;
            return View(model);
        }

        public ActionResult DeleteSpares(int? id)
        {
            _helper.ExecuteDeleteStatement("spDeleteSpares", id);
            return RedirectToAction("SparePartsMaster");
        }

        public ActionResult CheckSparePartsNumber(string partNumber)
        {
            if (partNumber == null) throw new ArgumentNullException(nameof(partNumber));
            if (!ModelState.IsValid) return null;
            var dtPartNumber = _helper.ExecuteSelectStmtusingSP("getPartNumber", null, null, null, null, "@partnumber", partNumber);
            var list = dtPartNumber.AsEnumerable().Select(x => x.Field<string>("partNumber")).FirstOrDefault();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckQuantity(int qty, int sparesid, string poNumber)
        {
            if (qty <= 0) return Json(qty, JsonRequestBehavior.AllowGet);
            var quantitycheck = "select (Quantity-sum(ReceivedQuantity)) as tquantity from t_sparepartspodetails where sparepartid=" + sparesid + " and ponumber='" + poNumber + "' group by quantity";
            decimal quantity;

            using (var dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck))
            {
                quantity = dtCheckSpares.AsEnumerable().Where(x => x.Field<decimal>("tquantity") >= qty).Select(x => x.Field<decimal>("tquantity")).FirstOrDefault();
            }

            return Json(quantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckQuantitySparesEdit(int qty, int sparesid)
        {
            if (qty <= 0) return Json(0, JsonRequestBehavior.AllowGet);
            var quantitycheck = "select (Quantity) as tquantity from t_sparepartspodetails where sparepartid=" + sparesid + "and ponumber='" + Session["PONumber"] + "' group by quantity";
            decimal quantity;

            using (var dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck))
            {
                quantity = dtCheckSpares.AsEnumerable().Where(x => x.Field<decimal>("tquantity") >= qty).Select(x => x.Field<decimal>("tquantity")).FirstOrDefault();
            }

            return Json(quantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckQuantitySparesEditPODetails(int qty, int sparesid, string ponumber)
        {
            if (qty <= 0) return Json(0, JsonRequestBehavior.AllowGet);
            decimal updatedQuantity = 0;
            var quantitycheck = "select (Quantity) as tquantity,ReceivedQuantity from t_sparepartspodetails where sparepartid=" + sparesid + "and ponumber='" + ponumber + "' group by quantity,ReceivedQuantity";
            decimal Totalquantity;
            int Receivedquantity;

            using (var dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck))
            {
                Totalquantity = dtCheckSpares.AsEnumerable().Select(x => x.Field<decimal>("tquantity")).FirstOrDefault();
                Receivedquantity = dtCheckSpares.AsEnumerable().Select(x => x.Field<int>("ReceivedQuantity")).FirstOrDefault();
            }

            if (Receivedquantity == 0)
            {
                updatedQuantity = qty;
                return Json(updatedQuantity, JsonRequestBehavior.AllowGet);
            }

            updatedQuantity = qty > Totalquantity ? qty : Totalquantity;
            return Json(updatedQuantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckQuantityLubesEditPODetails(decimal qty, int lubesid, string ponumber)
        {
            if (qty <= 0) return Json(0, JsonRequestBehavior.AllowGet);
            decimal updatedQuantity = 0;
            var quantitycheck = "select (Quantity) as tquantity,ReceivedQuantity from t_LubesPodetails where lubricantid=" + lubesid + "and ponumber='" + ponumber + "' group by quantity,ReceivedQuantity";
            decimal totalquantity;
            decimal receivedquantity;

            using (var dtCheckLubes = _helper.ExecuteSelectStmt(quantitycheck))
            {
                totalquantity = dtCheckLubes.AsEnumerable().Select(x => x.Field<decimal>("tquantity")).FirstOrDefault();
                receivedquantity = dtCheckLubes.AsEnumerable().Select(x => x.Field<decimal>("ReceivedQuantity")).FirstOrDefault();
            }

            if (receivedquantity == 0)
            {
                updatedQuantity = qty;
                return Json(updatedQuantity, JsonRequestBehavior.AllowGet);
            }

            updatedQuantity = qty > totalquantity ? qty : totalquantity;
            return Json(updatedQuantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckQuantityLubesEdit(decimal qty, int sparesid)
        {
            if (qty <= 0) return Json(0, JsonRequestBehavior.AllowGet);
            var quantitycheck = "select (Quantity) as tquantity from t_LubesPodetails where LubricantId=" + sparesid + "and ponumber='" + Session["PONumber"] + "' group by quantity";
            decimal quantity;

            using (var dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck))
            {
                quantity = dtCheckSpares.AsEnumerable().Where(x => x.Field<decimal>("tquantity") >= qty).Select(x => x.Field<decimal>("tquantity")).FirstOrDefault();
            }

            return Json(quantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckLubesAvailableQuantity(decimal qty, int sparesid, string poNumber)
        {
            if (qty <= 0) return Json(qty, JsonRequestBehavior.AllowGet);
            var quantitycheck = "select (Quantity-sum(ReceivedQuantity)) as tquantity from t_LubesPodetails where lubricantid=" + sparesid + " and ponumber='" + poNumber + "' group by quantity";
            decimal quantity;

            using (var dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck))
            {
                quantity = dtCheckSpares.AsEnumerable().Where(x => x.Field<decimal>("tquantity") >= qty).Select(x => x.Field<decimal>("tquantity")).FirstOrDefault();
            }

            return Json(quantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckLubesQuantity(int qty, int lubesid)
        {
            if (qty <= 0) return Json(qty, JsonRequestBehavior.AllowGet);
            var quantitycheck = "select (Quantity-sum(ReceivedQuantity)) as tquantity from t_LubesPodetails where LubricantId=" + lubesid + " group by quantity";
            int quantity;

            using (var dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck))
            {
                quantity = dtCheckSpares.AsEnumerable().Where(x => x.Field<int>("tquantity") >= qty).Select(x => x.Field<int>("tquantity")).FirstOrDefault();
            }

            return Json(quantity, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckLubricantNumber(string lubricantNumber)
        {
            if (!ModelState.IsValid) return null;
            string list;

            using (var dtLubesNumber = _helper.ExecuteSelectStmtusingSP("getLubricantNumber", null, null, null, null, "@lubricantnumber", lubricantNumber))
            {
                list = dtLubesNumber.AsEnumerable().Select(x => x.Field<string>("LubricantNumber")).FirstOrDefault();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckPoNumber(string poNumber)
        {
            var poSparesQuery = "select * from SparePartsPODetails where PoNumber='" + poNumber + "'";
            var dtPoDetails = _helper.ExecuteSelectStmt(poSparesQuery);

            switch (dtPoDetails.Rows.Count)
            {
                case 0:
                    var result = CheckTempTableForPoExistance(poNumber);
                    return Json(result, JsonRequestBehavior.AllowGet);
                default:
                    var po = dtPoDetails.AsEnumerable().Select(x => x.Field<int?>("Id")).FirstOrDefault();
                    return Json(po, JsonRequestBehavior.AllowGet);
            }
        }

        public int? CheckTempTableForPoExistance(string poNumber)
        {
            var poTemp = "select * from purchaseOrderTemp where ponumber='" + poNumber + "'";
            int? poTempdetails;

            using (var dtPoTempDetails = _helper.ExecuteSelectStmt(poTemp))
            {
                if (dtPoTempDetails == null) return null;
                poTempdetails = dtPoTempDetails.AsEnumerable().Select(x => x.Field<int?>("Id")).FirstOrDefault();
            }

            return poTempdetails;
        }

        public ActionResult PendingPOApprovals()
        {
            var dtPendingPos = new DataTable();
            var dtPendingPos1 = new DataTable();
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            var empId = Convert.ToInt32(Session["Employee_Id"]);
            var role = "select roleid from m_employees where employeeid=" + empId + "";
            int roleid;

            using (var dtRole = _helper.ExecuteSelectStmt(role))
            {
                roleid = dtRole.AsEnumerable().Select(x => x.Field<int>("roleid")).FirstOrDefault();
            }

            switch (roleid)
            {
                case 2:
                    dtPendingPos = _helper.ExecuteSelectStmtusingSP("getPendingPODetailsForApproval", "@sentto", empId.ToString());
                    dtPendingPos1 = _helper.ExecuteSelectStmtusingSP("getPendingPODetailsForApproval", "@sentto", empId.ToString());
                    Session["PendingApprovalsDetailsList"] = dtPendingPos1;
                    break;
                default:
                    dtPendingPos = _helper.ExecuteSelectStmtusingSP("getPendingPODetailsForApproval1", "@sentto", empId.ToString());
                    dtPendingPos1 = _helper.ExecuteSelectStmtusingSP("getPendingPODetailsForApproval1", "@sentto", empId.ToString());
                    Session["PendingApprovalsDetailsList"] = dtPendingPos1;
                    break;
            }

            dtPendingPos.Columns.Remove("filter");
            dtPendingPos.Columns.Remove("workshopid");
            dtPendingPos.Columns.Remove("roleid");
            return View(dtPendingPos);
        }

        public ActionResult RejectedPOs()
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");

            using (var dtRejectedPos = _helper.ExecuteSelectStmtusingSP("getRejectedPODetails"))
            {
                Session["RejectedPODetailsList"] = dtRejectedPos;
                dtRejectedPos.Columns.Remove("filter");
                return View(dtRejectedPos);
            }
        }

        public ActionResult CheckLubesPoNumber(string poNumber)
        {
            var poLubesQuery = "select * from LubesPO where PONumber='" + poNumber + "'";
            int? po;

            using (var dtPoDetails = _helper.ExecuteSelectStmt(poLubesQuery))
            {
                if (dtPoDetails == null) return null;
                po = dtPoDetails.AsEnumerable().Select(x => x.Field<int?>("Id")).FirstOrDefault();
            }

            return Json(po, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSparepartsForLocal(int? localManId)
        {
            var getspares = "select Id,partname,partnumber from m_spareparts where ManufacturerId=" + localManId + "";
            IEnumerable<GetPODetailsSpareParts> podatailsSpares;

            using (var dtSpares = _helper.ExecuteSelectStmt(getspares))
            {
                podatailsSpares = dtSpares.AsEnumerable().Select(x => new GetPODetailsSpareParts {PartName = x.Field<string>("partname"), PartNumber = x.Field<string>("partnumber"), SparePartId = x.Field<int>("Id")});
            }

            return Json(podatailsSpares, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSparePoDetails(string ponumber)
        {
            const int response = 0;
            if (ponumber == null) return null;
            var dtSparePartsPoDatails = _helper.ExecuteSelectStmtusingSP("spSparePartsPODetails", null, null, null, null, "@ponumber", ponumber);
            if (dtSparePartsPoDatails == null || dtSparePartsPoDatails.Rows.Count <= 0) return Json(response, JsonRequestBehavior.AllowGet);
            var manufactureId = dtSparePartsPoDatails.AsEnumerable().Select(x => x.Field<int>("ManufacturerId")).FirstOrDefault();
            var manufacturerQuery = "select * from [dbo].[m_VehicleManufacturer] where Id=" + manufactureId + "";
            int id;
            string manufacturerName;

            using (var dtManufacturerOnPo = _helper.ExecuteSelectStmt(manufacturerQuery))
            {
                id = dtManufacturerOnPo.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
                manufacturerName = dtManufacturerOnPo.AsEnumerable().Select(x => x.Field<string>("ManufacturerName")).FirstOrDefault();
            }

            IEnumerable<GetPODetailsSpareParts> podatailsSpares = dtSparePartsPoDatails.AsEnumerable().Select(x => new GetPODetailsSpareParts {PoDate = x.Field<DateTime>("PoDate").ToShortDateString(), PartName = x.Field<string>("partname"), PartNumber = x.Field<string>("partnumber"), PoQuantity = x.Field<decimal>("poquantity"), ReceivedQuantity = x.Field<int>("ReceivedQuantity"), LastReceivedDate = x.Field<DateTime?>("lastreceiveddate"), PendingQuantity = x.Field<decimal>("poquantity") - x.Field<int>("ReceivedQuantity"), ManufacturerId = id, ManufacturerName = manufacturerName, SparePartId = x.Field<int>("sparepartId"), GetLastReceivedDate = x.Field<DateTime?>("lastreceiveddate").ToString()});
            Session["PoQuantitySpares"] = podatailsSpares;
            return Json(podatailsSpares, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLubesPoDetails(string ponumber)
        {
            const int response = 0;
            if (ponumber == null) return null;
            var dtLubesPoDatails = _helper.ExecuteSelectStmtusingSP("spLubesPODetails", null, null, null, null, "@ponumber", ponumber);
            if (dtLubesPoDatails == null || dtLubesPoDatails.Rows.Count <= 0) return Json(response, JsonRequestBehavior.AllowGet);
            var manufactureId = dtLubesPoDatails.AsEnumerable().Select(x => x.Field<int>("ManufacturerId")).FirstOrDefault();
            var manufacturerQuery = "select * from [dbo].[m_LubesManufactures] where Id=" + manufactureId + "";
            int id;
            string manufacturerName;

            using (var dtManufacturerOnPo = _helper.ExecuteSelectStmt(manufacturerQuery))
            {
                id = dtManufacturerOnPo.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
                manufacturerName = dtManufacturerOnPo.AsEnumerable().Select(x => x.Field<string>("ManufacturerName")).FirstOrDefault();
            }

            IEnumerable<GetPODetailsSpareParts> podatailsLubes = dtLubesPoDatails.AsEnumerable().Select(x => new GetPODetailsSpareParts {PoDate = x.Field<DateTime>("PoDate").ToShortDateString(), LubricantName = x.Field<string>("OilName"), LubricantNumber = x.Field<string>("LubricantNumber"), PoQuantity = x.Field<decimal>("poquantity"), LubesReceivedQuantity = x.Field<decimal>("ReceivedQuantity"), LastReceivedDate = x.Field<DateTime?>("lastreceiveddate"), PendingQuantity = x.Field<decimal>("poquantity") - x.Field<decimal>("ReceivedQuantity"), ManufacturerId = id, ManufacturerName = manufacturerName, SparePartId = x.Field<int>("LubricantId"), GetLastReceivedDate = x.Field<DateTime?>("lastreceiveddate").ToString()});
            Session["PoQuantityLubes"] = podatailsLubes;
            return Json(podatailsLubes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetScrapDetails(int? scrapId)
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            var scrapList = new List<ScrapBinModel>();
            const string queryScrap = "select * from m_ScrapBin";
            var dtScrap = _helper.ExecuteSelectStmt(queryScrap);
            ViewBag.ScrapBin = new SelectList(dtScrap.AsDataView(), "ScrapBinId", "ScrapBinName");
            if (scrapId == null) return View();
            var dtGetScrapDetails = _helper.ExecuteSelectStmtusingSP("ScrapDetails", "@scrapbinid", scrapId.ToString());
            Session["Scraps"] = dtGetScrapDetails;

            foreach (DataRow row in dtGetScrapDetails.Rows)
            {
                var scrapBin = new ScrapBinModel {ScrapBinId = Convert.ToInt32(row["ScrapBinId"]), PartName = row["PartName"].ToString(), PartNumber = row["PartNumber"].ToString(), Quantity = Convert.ToInt32(row["Quantity"])};
                scrapList.Add(scrapBin);
            }

            return Json(scrapList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InsertScrapDetails(int? netWeight, int? actualCount)
        {
            var result = 0;
            var employee = Session["Employee_Id"].ToString();
            var dtGetScrapDetails = Session["Scraps"] as DataTable;
            if (dtGetScrapDetails == null) return Json(result, JsonRequestBehavior.AllowGet);
            var count = dtGetScrapDetails.AsEnumerable().Sum(x => x.Field<long>("rownumber"));

            foreach (DataRow row in dtGetScrapDetails.Rows)
            {
                result = _helper.ExecuteInsertStmtusingSp("SpinsertScrapDetails", "@scrapid", row["ScrapBinId"].ToString(), "@sparecount", count.ToString(), null, null, "@sparequantity", row["Quantity"].ToString(), "@netweight", netWeight.ToString(), "@actualquantity", actualCount.ToString(), "@enterdby", employee);
                if (result > 0) break;
            }

            foreach (DataRow row in dtGetScrapDetails.Rows) _helper.ExecuteInsertStmtusingSp("SpUpdateScrapDetails", "@scrapid", row["ScrapBinId"].ToString(), "@sparepartid", row["Id"].ToString());

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetApprovalList(string id)
        {
            Session["PoNUmberId"] = id;
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            IEnumerable<InventoryModel> list = null;
            var dtApprovalDialog = _helper.ExecuteSelectStmtusingSP("spgetPendingPOApprovalDetailList", null, null, null, null, "@ponumber", id);
            var dtLubesApprovalDialog = _helper.ExecuteSelectStmtusingSP("spgetPendingLubesPOApprovalDetailList", null, null, null, null, "@ponumber", id);
            Session["DataForApproval"] = dtApprovalDialog;
            var filter = dtApprovalDialog.AsEnumerable().Select(x => x.Field<int>("filterCode")).FirstOrDefault();
            ViewBag.Manufacturer = dtApprovalDialog.AsEnumerable().Select(x => x.Field<string>("ManufacturerName")).FirstOrDefault();
            ViewBag.WorkShop = dtApprovalDialog.AsEnumerable().Select(x => x.Field<string>("workshop_name")).FirstOrDefault();
            ViewBag.PONumber = dtApprovalDialog.AsEnumerable().Select(x => x.Field<string>("ponumber")).FirstOrDefault();
            ViewBag.PODate = dtApprovalDialog.AsEnumerable().Select(x => x.Field<DateTime>("podate")).FirstOrDefault();
            ViewBag.SentBy = dtApprovalDialog.AsEnumerable().Select(x => x.Field<string>("Name")).FirstOrDefault();
            ViewBag.SentOn = dtApprovalDialog.AsEnumerable().Select(x => x.Field<DateTime>("senton")).FirstOrDefault();
            var roles = "select roleid,employeeid,employeename from m_employees where employeeid=" + Convert.ToInt32(Session["Employee_Id"]) + "";
            var dtRoles = _helper.ExecuteSelectStmt(roles);
            var roleid = dtRoles.AsEnumerable().Select(x => x.Field<int>("roleid")).FirstOrDefault();
            ViewBag.RoleId = roleid;
            Session["RoleIds"] = roleid;

            if (roleid != 6)
            {
                if (roleid != 3 && roleid != 4)
                {
                    var rolid = roleid + 1;
                    var query = "select employeeid,employeename,roleid from m_employees where roleid=" + rolid + "";
                    var dtRole = _helper.ExecuteSelectStmt(query);
                    ViewBag.Roles = new SelectList(dtRole.AsDataView(), "employeeid", "employeename");
                }
                else if (roleid == 4)
                {
                    var query = "select employeeid,employeename,roleid from m_employees where roleid in (6)";
                    var dtRole = _helper.ExecuteSelectStmt(query);
                    ViewBag.Roles = new SelectList(dtRole.AsDataView(), "employeeid", "employeename");
                }
                else
                {
                    var query = "select employeeid,employeename,roleid from m_employees where roleid in (4,5)";
                    var dtRole = _helper.ExecuteSelectStmt(query);
                    ViewBag.Roles = new SelectList(dtRole.AsDataView(), "employeeid", "employeename");
                }
            }
            else
            {
                ViewBag.Roles = null;
            }

            var poquery = "select sum(amount) as totalPOAmount from purchaseOrderitemsTemp   where ponumber='" + id + "' and filterCode=" + filter + "";
            var dtpoquery = _helper.ExecuteSelectStmt(poquery);
            var totalAmount = dtpoquery.AsEnumerable().Select(x => x.Field<decimal>("totalPOAmount")).FirstOrDefault();
            var potally = "select povalue,itemvalue from [dbo].[purchaseordervalue_roles] where roleId=" + roleid + "";
            var dtpotally = _helper.ExecuteSelectStmt(potally);
            var poValue = dtpotally.AsEnumerable().Select(x => x.Field<int>("povalue")).FirstOrDefault();

            if (totalAmount > poValue)
                ViewBag.TotalAmount = roleid != 6 ? totalAmount : 0;
            else
                ViewBag.TotalAmount = 0;

            switch (filter)
            {
                case 1:
                    list = dtApprovalDialog.AsEnumerable().Select(x => new InventoryModel {ManName = x.Field<string>("ManufacturerName"), Id = x.Field<int>("SparePartId"), Name = x.Field<string>("PartName"), WorkShopName = x.Field<string>("workshop_name"), PoNumber = x.Field<string>("ponumber"), PoDate = x.Field<DateTime>("podate"), BillAmount = x.Field<decimal>("Amount"), Quantity = x.Field<decimal>("quantity"), SentBy = x.Field<string>("Name"), SentOn = x.Field<DateTime>("senton")});
                    return View("GetApprovalList", list);
                default:
                    list = dtLubesApprovalDialog.AsEnumerable().Select(x => new InventoryModel {ManName = x.Field<string>("ManufacturerName"), Id = x.Field<int>("SparePartId"), Name = x.Field<string>("OilName"), WorkShopName = x.Field<string>("workshop_name"), PoNumber = x.Field<string>("ponumber"), PoDate = x.Field<DateTime>("podate"), BillAmount = x.Field<decimal>("Amount"), Quantity = x.Field<decimal>("quantity"), SentBy = x.Field<string>("Name"), SentOn = x.Field<DateTime>("senton")});
                    return View("GetApprovalList", list);
            }
        }

        public ActionResult GetRejectedList(string id)
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            IEnumerable<InventoryModel> list = null;
            var dtApprovalDialog = _helper.ExecuteSelectStmtusingSP("spgetRejectedPOApprovalDetailList", null, null, null, null, "@ponumber", id);
            Session["DataForRejection"] = dtApprovalDialog;
            var filter = dtApprovalDialog.AsEnumerable().Select(x => x.Field<int>("filterCode")).FirstOrDefault();
            ViewBag.Manufacturer = dtApprovalDialog.AsEnumerable().Select(x => x.Field<string>("ManufacturerName")).FirstOrDefault();
            ViewBag.WorkShop = dtApprovalDialog.AsEnumerable().Select(x => x.Field<string>("workshop_name")).FirstOrDefault();
            ViewBag.PONumber = dtApprovalDialog.AsEnumerable().Select(x => x.Field<string>("ponumber")).FirstOrDefault();
            ViewBag.PODate = dtApprovalDialog.AsEnumerable().Select(x => x.Field<DateTime>("podate")).FirstOrDefault();
            ViewBag.SentBy = dtApprovalDialog.AsEnumerable().Select(x => x.Field<string>("Name")).FirstOrDefault();
            ViewBag.SentOn = dtApprovalDialog.AsEnumerable().Select(x => x.Field<DateTime>("senton")).FirstOrDefault();
            list = dtApprovalDialog.AsEnumerable().Select(x => new InventoryModel {ManName = x.Field<string>("ManufacturerName"), SparePartId = x.Field<int>("SparePartId"), PartName = x.Field<string>("PartName"), WorkShopName = x.Field<string>("workshop_name"), PoNumber = x.Field<string>("ponumber"), PoDate = x.Field<DateTime>("podate"), BillAmount = x.Field<decimal>("Amount"), Quantity = x.Field<int>("quantity"), SentBy = x.Field<string>("Name"), SentOn = x.Field<DateTime>("senton")});
            return View("GetRejectedList", list);
        }

        public ActionResult UpdateApprovalList(int? Id)
        {
            var status = 0;

            var dtApprovalPendingDetails = Session["DataForApproval"] as DataTable;
            var row1 = dtApprovalPendingDetails.AsEnumerable().Select(x => new {enteredBy = x.Field<int>("sentby"), poNumber = x.Field<string>("ponumber"), poDate = x.Field<DateTime>("podate"), sentOn = x.Field<DateTime>("senton"), filter = x.Field<int>("filterCode")}).FirstOrDefault();

            status = _helper.ExecuteInsertPOPendingDetails(row1.filter == 1 ? "spInsertSparepartspoDetails" : "spInsertLubesspoDetails", row1.enteredBy, row1.poNumber, row1.poDate, row1.sentOn);

            var result = 0;

            if (status > 0)
            {
                var deletePo = "delete from purchaseOrderTemp where ponumber='" + row1.poNumber + "'";
                var deletePoItems = "delete from PurchaseorderItemsTemp where ponumber='" + row1.poNumber + "'";
                _helper.ExecuteSelectStmt(deletePo);
                _helper.ExecuteSelectStmt(deletePoItems);
            }

            foreach (DataRow row in dtApprovalPendingDetails.Rows)
            {
                var manufacturerId = row["ManufacturerId"].ToString();
                var sparePartId = row["SparePartId"].ToString();
                var quantity = row["Quantity"].ToString();
                var unitPrice = row["UnitPrice"].ToString();
                var amount = row["Amount"].ToString();
                var poNumber = row["ponumber"].ToString();

                result = _helper.ExecuteInsertPOPendingDetailsList(row1.filter == 1 ? "ExecuteInsertPOPendingDetailsList" : "ExecuteInsertPOLubesPendingDetailsList", Convert.ToInt32(manufacturerId), Convert.ToInt32(sparePartId), Convert.ToDecimal(quantity), Convert.ToDecimal(unitPrice), Convert.ToDecimal(amount), Convert.ToString(poNumber));
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteApprovalList(int? Id)
        {
            var dtDeletePendingDetails = Session["DataForApproval"] as DataTable;
            var row1 = dtDeletePendingDetails.AsEnumerable().Select(x => new {poNumber = x.Field<string>("ponumber")}).FirstOrDefault();
            var result = _helper.ExecuteDeleteStatementString("spupdatepodetailsquery", row1.poNumber);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FarwordApprovalList(int? Id)
        {
            if (Session["PoNUmberId"] == null) return RedirectToAction("GetApprovalList", "Inventory");
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            var result = 0;
            var roleIdQuery = "select roleid from m_employees where employeeid=" + Id + "";
            var dtroleIdQuery = _helper.ExecuteSelectStmt(roleIdQuery);
            var roleid = dtroleIdQuery.AsEnumerable().Select(x => x.Field<int>("roleid")).FirstOrDefault();
            var dtPendingApprovalsDetailsList = Session["PendingApprovalsDetailsList"] as DataTable;

            foreach (DataRow row in dtPendingApprovalsDetailsList.Rows)
            {
                if (Session["PoNUmberId"].ToString() == row["PONumber"].ToString())
                {
                    result = _helper.InsertFarwordedDetails("spFarwordedApprovalDetails", Convert.ToInt32(row["workshopid"]), Convert.ToString(row["PONumber"]), Convert.ToDateTime(row["PODate"]), Convert.ToDateTime(row["SentON"]), Convert.ToInt32(row["employeeid"]), Convert.ToInt32(row["roleid"]));

                    if (result == 1) result = _helper.ExecuteUpdateFarwodedApprovals("spUpdateApprovalFarwodedDetails", Convert.ToInt32(Session["Employee_Id"]), DateTime.Now, roleid, Convert.ToInt32(row["workshopid"]), Id, Convert.ToString(row["PONumber"]));
                }

                sendMailMessage(row, roleid);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void sendMailMessage(DataRow row, int roleid)
        {
            var previousroleid = roleid - 1;
            var sentTo = "select employeeEmail from m_employees where roleid=" + roleid + "";
            var dtSentTo = _helper.ExecuteSelectStmt(sentTo);
            var to = dtSentTo.AsEnumerable().Select(x => x.Field<string>("employeeEmail")).FirstOrDefault();
            var sentFrom = "select employeeEmail from m_employees where roleid=" + previousroleid + "";
            var dtsentFrom = _helper.ExecuteSelectStmt(sentFrom);
            var from = dtsentFrom.AsEnumerable().Select(x => x.Field<string>("employeeEmail")).FirstOrDefault();
            var dtgetDetails = Session["DataForApproval"] as DataTable;
            var test = dtgetDetails.AsEnumerable().Select(x => new MailSend {Id = x.Field<int>("sparepartid"), Name = x.Field<string>("partname"), Quantity = x.Field<decimal>("quantity"), TotalAmount = x.Field<decimal>("amount")});
            _helper.SendMailMessage("ts_102@emri.in", to, "UP-'" + row["WorkShop"] + "'-POApproval", "", ConfigurationManager.AppSettings["hostname"], "tsemri@102$", Convert.ToString(row["PONumber"]), Convert.ToDateTime(row["PODate"]).ToString(), test);
        }

        public ActionResult RevertApprovalList(int? Id)
        {
            var dtDeletePendingDetails = Session["DataForRejection"] as DataTable;
            var row1 = dtDeletePendingDetails.AsEnumerable().Select(x => new {poNumber = x.Field<string>("ponumber")}).FirstOrDefault();
            var result = _helper.ExecuteDeleteStatementString("spRevertpodetailsquery", row1.poNumber);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public JsonResult CheckAggregates(string aggVal, int Manufacturer)
        {
            var query = "select * from M_FMS_MaintenanceWorksServiceGroupDetails where ServiceGroup_Name='" + aggVal.ToUpper() + "' and manufacturer_id=" + Manufacturer + "";
            var dtAggregates = _helper.ExecuteSelectStmt(query);
            var results = dtAggregates.AsEnumerable().Where(x => x.Field<string>("ServiceGroup_Name") == aggVal.ToUpper()).Select(x => x.Field<string>("ServiceGroup_Name")).FirstOrDefault();
            return dtAggregates.Rows.Count > 0 ? Json(results) : Json(0);
        }

        [HttpPost] public JsonResult CheckCategories(string catVal, int Manufacturer, int AggregateVal)
        {
            var query = "select * from M_FMS_Categories where Categories='" + catVal.ToUpper() + "' and manufacturerid=" + Manufacturer + " and aggregate_id=" + AggregateVal + " ";
            var dtCategories = _helper.ExecuteSelectStmt(query);
            var results = dtCategories.AsEnumerable().Where(x => x.Field<string>("Categories") == catVal.ToUpper()).Select(x => x.Field<string>("Categories")).FirstOrDefault();
            return dtCategories.Rows.Count > 0 ? Json(results) : Json(0);
        }

        [HttpPost] public JsonResult CheckSubCategories(int catVal, int Manufacturer, int AggregateVal, string SubCatVal)
        {
            var query = "select mw.Service_Name,mw.Service_Id from M_FMS_MaintenanceWorksMasterDetails mw join m_fms_categories c on c.Aggregate_id=mw.ServiceGroup_Id  where mw.ServiceGroup_id=" + AggregateVal + " and mw.manufacturerid=" + Manufacturer + "";
            var dtSubCategories = _helper.ExecuteSelectStmt(query);
            var results = dtSubCategories.AsEnumerable().Where(x => x.Field<string>("Service_Name").ToUpper() == SubCatVal.ToUpper()).Select(x => x.Field<string>("Service_Name")).FirstOrDefault();
            return results != null ? Json(results) : Json(0);
        }
    }

    internal class MailSend
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}