using Fleet_WorkShop.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Fleet_WorkShop.Controllers
{
    public class InventoryController : Controller
    {
        private readonly EmployeeHelper _helper = new EmployeeHelper();
        private readonly InventoryModel _inventoryModel = new InventoryModel();
        // GET: Inventory

            public ActionResult SparePartsMaster()
        {
            IEnumerable<SparePartsModel> sparemodel;
            string query = "select * from m_VehicleManufacturer";
           DataTable dtSpares= _helper.ExecuteSelectStmt(query);
            Session["Manufacturer"] = dtSpares;
            ViewBag.Manufacturers= new SelectList(dtSpares.AsDataView(), "Id", "ManufacturerName");
            DataTable dtSpareParts=_helper.ExecuteSelectStmtusingSP("spGetSpares");
            Session["getSpares"] = dtSpareParts;
            sparemodel = dtSpareParts.AsEnumerable().ToList().Select(x => new SparePartsModel {Id=x.Field<int>("Id"), ManufacturerName = x.Field<string>("ManufacturerName"), PartName = x.Field<string>("PartName"), Cost = x.Field<decimal>("Cost") });
            return View(sparemodel);
        }
        [HttpPost]
        public ActionResult SparePartsMaster(SparePartsModel spareModel)
        {
            int returnVal=_helper.ExecuteInsertSparePartsMasterDetails("spSparePartsMaster", spareModel.ManufacturerId, spareModel.PartName, spareModel.PartNumber, spareModel.Cost);
            if (returnVal == 1)
                return Json("Hello", JsonRequestBehavior.AllowGet);
            return RedirectToAction("SparePartsMaster");
        }
        [HttpGet]
        public ActionResult EditSpares(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("SparePartsMaster");
            }
            SparePartsModel spmodel = new SparePartsModel();
            DataTable dtGetSpareDetails = Session["getSpares"] as DataTable;
            DataRow row = dtGetSpareDetails.AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
            DataTable dtManufacturers = Session["Manufacturer"] as DataTable;
            spmodel.Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName");
            spmodel.ManufacturerId = Convert.ToInt32(row["ManufacturerId"]);
            spmodel.PartName = row["PartName"].ToString();
            spmodel.Cost = Convert.ToDecimal(row["Cost"]);
            return View(spmodel);
        }

        [HttpPost]
        public ActionResult EditSpares(SparePartsModel spmodel)
        {
            return RedirectToAction("SparePartsMaster");
        }
        public ActionResult LubesMaster()
        {
            string query = "select * from m_VehicleManufacturer";
            DataTable dtLubes = _helper.ExecuteSelectStmt(query);
            ViewBag.Manufacturers = new SelectList(dtLubes.AsDataView(), "Id", "ManufacturerName");
            return View();
        }

        public ActionResult GetCostDetails(int id)
        {
            string query = "select cost from m_spareparts where Id='" + id + "'";
            DataTable dtCost=_helper.ExecuteSelectStmt(query);
           decimal cost= dtCost.AsEnumerable().ToList().Select(x => x.Field<decimal>("Cost")).First();
            if (cost != 1)
                return Json(cost, JsonRequestBehavior.AllowGet);
            return View();
        }
        [HttpPost]
        public ActionResult GetLubesCostDetails(int id)
        {
            string query = "select CostPerLitre from m_lubes where Id='" + id + "'";
            DataTable dtCost = _helper.ExecuteSelectStmt(query);
            decimal cost = dtCost.AsEnumerable().ToList().Select(x => x.Field<decimal>("CostPerLitre")).First();
            if (cost != 1)
                return Json(cost, JsonRequestBehavior.AllowGet);
            return View();
        }
        [HttpPost]
        public ActionResult GetSpareCostDetails(int id)
        {
            string query = "select cost from m_spareparts where Id='" + id + "'";
            DataTable dtCost = _helper.ExecuteSelectStmt(query);
            decimal cost = dtCost.AsEnumerable().ToList().Select(x => x.Field<decimal>("cost")).First();
            if (cost != 1)
                return Json(cost, JsonRequestBehavior.AllowGet);
            return View();
        }
        [HttpPost]
        public ActionResult LubesMaster(LubesModel lubesModel)
        {
           int returnVal= _helper.ExecuteInsertLubesMasterDetails("spLubesMaster", lubesModel.ManufacturerId, lubesModel.OilName, lubesModel.CostPerLitre);
            if (returnVal == 1)
                return Json("Hello", JsonRequestBehavior.AllowGet);
            return RedirectToAction("LubesMaster");
        }
        public ActionResult getSparePartsDetails()
        {
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");
            return RedirectToAction("SaveInventoryDetails");
           
        }
        public ActionResult SaveInventoryDetails()
        {
            if (Session["WorkshopId"] == null)
                return RedirectToAction("Login", "Account");
            IEnumerable<InventoryModel> invModel = null;
            DataSet dsGetManufacturerVendor= _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id", "ManufacturerName");
            ViewBag.Vendors = new SelectList(dsGetManufacturerVendor.Tables[1].AsDataView(), "id", "vendor_name");
            ViewBag.Spares = new SelectList(dsGetManufacturerVendor.Tables[2].AsDataView(), "Id", "PartName");
            DataSet dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetReceiptDetails");
            invModel = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel {Id= x.Field<int>("id"), BillNo = x.Field<string>("BillNumber"), ManName = x.Field<string>("ManufacturerName"),BillDate=x.Field<DateTime>("BillDate"),BillAmount= x.Field<decimal>("BillAmount"), PartName = x.Field<string>("PartName"), PartNumber = x.Field<string>("PartNumber"), Uprice = x.Field<decimal>("UnitPrice"), Qty = x.Field<int>("Quantity"), Amt = x.Field<decimal>("Amount")});
            invModel.ToList();
            return View(invModel);
        }
        [HttpPost]
        public ActionResult SaveInventoryDetails(InventoryModel model)
        {
            
            InventoryModel billDetails = new InventoryModel { BillNo =model.BillNo, BillDate = model.BillDate, BillAmount = model.BillAmount,VendorName=model.VendorName,VendorId=model.VendorId,PoNumber=model.PoNumber,PoDate=model.PoDate, WorkShopId = Convert.ToInt32(Session["WorkshopId"]) };
            _helper.ExecuteBillDetails("spInsertBillDetails", billDetails.BillNo, billDetails.BillDate, billDetails.BillAmount, billDetails.VendorId,billDetails.PoNumber,billDetails.PoDate,billDetails.WorkShopId);
            foreach(var items in model.itemmodel)
            {

                //var insertgridDetails = billDetails.itemmodel.Select((x) => new {ManufacturerId=x.ManufacturerId, ManufacturerName = x.ManufacturerName,SparePartId=x.SparePartId, SparePartName = x.SparePartName, Quantity = x.Quantity, Amount = x.Amount, UnitPrice = x.UnitPrice });
                _helper.ExecuteInsertInventoryDetails("spInsertInventoryDetails",model.BillNo, items.ManufacturerId, items.SparePartId, items.UnitPrice, items.Quantity, items.Amount,model.VendorId);
                
            }
            
            CommonMethod(model);


            return RedirectToAction("SaveInventoryDetails");
        }
        public void CommonMethod(InventoryModel model)
        {
            string query = "select workshopid,receipt_id from t_receipts where billnumber='" + model.BillNo + "'and vendorid=" + model.VendorId + "";
            DataTable dtWorshopId = _helper.ExecuteSelectStmt(query);
            int workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
            var receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            foreach (var item in model.itemmodel)
            {

                //var insertgridDetails = billDetails.itemmodel.Select((x) => new {ManufacturerId=x.ManufacturerId, ManufacturerName = x.ManufacturerName,SparePartId=x.SparePartId, SparePartName = x.SparePartName, Quantity = x.Quantity, Amount = x.Amount, UnitPrice = x.UnitPrice });
                _helper.ExecuteInsertStockDetails("spInsertInventoryStockDetails", workShopId, item.ManufacturerId, item.SparePartId, item.UnitPrice, item.Quantity, receiptId,model.BillNo,model.VendorId);

            }
        }

        public void CommonMethodLubes(InventoryModel model)
        {
            string query = "select workshopid,receipt_id from t_lubesreceipts where billnumber='" + model.BillNo + "'and vendorid=" + model.VendorId + "";
            DataTable dtWorshopId = _helper.ExecuteSelectStmt(query);
            int workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
            var receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            foreach (var item in model.itemmodel)
            {

                //var insertgridDetails = billDetails.itemmodel.Select((x) => new {ManufacturerId=x.ManufacturerId, ManufacturerName = x.ManufacturerName,SparePartId=x.SparePartId, SparePartName = x.SparePartName, Quantity = x.Quantity, Amount = x.Amount, UnitPrice = x.UnitPrice });
                _helper.ExecuteInsertLubeStockDetails("spInsertLubesStockDetails", workShopId, item.ManufacturerId, item.LubricantId, item.UnitPrice, item.Quantity, receiptId, model.BillNo, model.VendorId);

            }
            //string query = "select workshopid from t_lubesreceipts where billnumber='" + model.BillNo + "'and vendorid=" + model.VendorId + "";
            //DataTable dtWorshopId = _helper.ExecuteSelectStmt(query);
            //int workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("WorkShopId")).FirstOrDefault();
            //DataTable dtGetTotalAmount = _helper.ExecuteSelectStmtusingSP("spGetTotalBillAmountLubes", "@vendorid", model.VendorId.ToString(), "@workshopid", workShopId.ToString());
            //if (dtGetTotalAmount.Rows.Count > 0)
            //{
            //    _helper.UpdateTotalBillDetailsLubes(dtGetTotalAmount);
            //}
            //DataTable dtQuantity = _helper.ExecuteSelectStmtusingSP("UpdateLubesQuantityOnWorkShopID", "@workshopid", workShopId.ToString());
            //string workshopstocks = "select WorkShopId from t_lubes_stock";
            //DataTable dtworkshopstocks = _helper.ExecuteSelectStmt(workshopstocks);
            //if (dtworkshopstocks.Rows.Count > 0)
            //{
            //    var dtstoreRecords = dtworkshopstocks.AsEnumerable().Select(x => x/*new { WorkShopId=x.Field<int>("WorkShopId"), ManufacturerId = x.Field<int>("ManufacturerId"), SparePartId = x.Field<int>("SparePartId") }*/);
            //    if (dtstoreRecords.AsEnumerable().Where(c => c.Field<int>("WorkShopId").Equals(workShopId)).Count() > 0)
            //    {
            //        _helper.InsertLubricantStockDetails(dtQuantity, "1");

            //    }
            //    else
            //    {
            //        _helper.InsertLubricantStockDetails(dtQuantity);

            //    }
            //}
            //else
            //{
            //    _helper.InsertLubricantStockDetails(dtQuantity);
            //}
        }
        [HttpGet]
        public ActionResult Edit(int? Id = null)
        {
            if (Id == null)
            {
                return RedirectToAction("SaveInventoryDetails");
            }
            DataSet dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            
            DataSet dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetReceiptDetails");
            DataRow row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == Id);
            InventoryModel model = new InventoryModel() {
                BillNo = row["BillNumber"].ToString(),
                ManName = row["ManufacturerName"].ToString(),
                PartName = row["PartName"].ToString(),
                Uprice = Convert.ToDecimal(row["UnitPrice"]),
                Qty = Convert.ToInt32(row["Quantity"]),
                Amt = Convert.ToDecimal(row["Amount"]),
                BillAmount = Convert.ToDecimal(row["BillAmount"]),
                BillDate=DateTime.Parse(row["BillDate"].ToString()),
                Manufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id", "ManufacturerName"),
                SpareParts = new SelectList(dsGetManufacturerVendor.Tables[2].AsDataView(), "Id", "PartName"),
                VendorId= Convert.ToInt32(row["vendorid"])
            };
            Session["BillAmount"] = model.BillAmount;
            Session["Amt"] = model.Amt;
            Session["Bill"] = model.BillNo;
            Session["VendorId"] = model.VendorId;
                                  
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(InventoryModel postInventory)
        {
            DataSet dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetReceiptDetails");
            DataRow row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == postInventory.Id);
       postInventory.VendorId = Convert.ToInt32(row["vendorId"]);
            //string vendorQuery="select vendorid "
            postInventory.BillAmount = decimal.Parse(Session["BillAmount"].ToString());
            if (row["BillAmount"].ToString() != null)
            {
                if (postInventory.BillAmount != Decimal.Parse(row["BillAmount"].ToString()))
                {
                    postInventory.BillAmount = postInventory.BillAmount + Decimal.Parse(row["BillAmount"].ToString());
                    _inventoryModel.Amt = Convert.ToDecimal(Session["Amt"].ToString());
                    var bill = Decimal.Parse(Session["BillAmount"].ToString());
                    postInventory.BillAmount = postInventory.BillAmount - (_inventoryModel.Amt - postInventory.Amt);
                    _helper.ExecuteUpdateInventoryStatement(postInventory.ManufacturerId, "spEditInventory", postInventory.SparePartId, postInventory.Uprice, postInventory.Qty, postInventory.Amt, postInventory.BillAmount, postInventory.BillNo, postInventory.Id, postInventory.VendorId);
                    string query = "delete from t_receipts where billnumber='" + Session["Bill"].ToString() + "'";
                    _helper.ExecuteDeleteInvBillNumberStatement(query);
                }
                else
                {
                    _inventoryModel.Amt = Convert.ToDecimal(Session["Amt"].ToString());
                    var bill = Decimal.Parse(Session["BillAmount"].ToString());
                    postInventory.BillAmount = postInventory.BillAmount - (_inventoryModel.Amt - postInventory.Amt);
                    _helper.ExecuteUpdateInventoryStatement(postInventory.ManufacturerId, "spEditInventory", postInventory.SparePartId, postInventory.Uprice, postInventory.Qty, postInventory.Amt, postInventory.BillAmount, postInventory.BillNo, postInventory.Id, postInventory.VendorId);
                }
            }
            CommonEditSparesMethod(postInventory);
            return RedirectToAction("SaveInventoryDetails");
        }

        private void CommonEditSparesMethod(InventoryModel model)
        {
            string query = "select workshopid,receipt_id from t_receipts where billnumber='" + model.BillNo + "'and vendorid=" + model.VendorId + "";
            DataTable dtWorshopId = _helper.ExecuteSelectStmt(query);
            int workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
            var receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            _helper.ExecuteUpdateInventoryStocksStatement(workShopId, model.ManufacturerId, "spEditSParepartsInventory", model.SparePartId, model.Uprice, model.Qty, receiptId,model.BillNo,model.VendorId);
        }

        public ActionResult SaveLubesInventoryDetails()
        {
            if (Session["WorkshopId"] == null)
                return RedirectToAction("Login", "Account");
            IEnumerable<InventoryModel> invModel = null;
            DataSet dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id", "ManufacturerName");
            ViewBag.Vendors = new SelectList(dsGetManufacturerVendor.Tables[1].AsDataView(), "id", "vendor_name");
            ViewBag.Lubes = new SelectList(dsGetManufacturerVendor.Tables[3].AsDataView(), "Id", "OilName");
            DataSet dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails");
            invModel = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel { Id = x.Field<int>("id"), BillNo = x.Field<string>("BillNumber"), ManName = x.Field<string>("ManufacturerName"), BillDate = x.Field<DateTime>("BillDate"), BillAmount = x.Field<decimal>("BillAmount"), LubricantName = x.Field<string>("OilName"), Uprice = x.Field<decimal>("UnitPrice"), Qty = x.Field<int>("Quantity"), Amt = x.Field<decimal>("Amount") });
            invModel.ToList();
            return View(invModel);
       
        }
        [HttpPost]
        public ActionResult SaveLubesInventoryDetails(InventoryModel model)
        {
            InventoryModel billDetails = new InventoryModel { BillNo = model.BillNo, BillDate = model.BillDate, BillAmount = model.BillAmount, VendorName = model.VendorName, VendorId = model.VendorId, PoNumber = model.PoNumber, PoDate = model.PoDate, WorkShopId = Convert.ToInt32(Session["WorkshopId"]) };
            _helper.ExecuteBillDetails("spInsertLubeBillDetails", billDetails.BillNo, billDetails.BillDate, billDetails.BillAmount, billDetails.VendorId,billDetails.PoNumber,billDetails.PoDate,billDetails.WorkShopId);
            foreach (var items in model.itemmodel)
            {

                //var insertgridDetails = billDetails.itemmodel.Select((x) => new {ManufacturerId=x.ManufacturerId, ManufacturerName = x.ManufacturerName,SparePartId=x.SparePartId, SparePartName = x.SparePartName, Quantity = x.Quantity, Amount = x.Amount, UnitPrice = x.UnitPrice });
                _helper.ExecuteInsertLubesDetails("spInsertLubricantDetails", model.BillNo, items.ManufacturerId, items.LubricantId, items.UnitPrice, items.Quantity, items.Amount,billDetails.VendorId);

            }
            CommonMethodLubes(model);
            return RedirectToAction("SaveInventoryDetails");
        }

        public ActionResult EditLubeDetails(int? Id = null)
        {
            if (Id == null)
            {
                return RedirectToAction("SaveLubesInventoryDetails");
            }
            DataSet dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");

            DataSet dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails");
            DataRow row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == Id);
            InventoryModel model = new InventoryModel()
            {
                BillNo = row["BillNumber"].ToString(),
                ManName = row["ManufacturerName"].ToString(),
                LubricantName = row["OilName"].ToString(),
                Uprice = Convert.ToDecimal(row["UnitPrice"]),
                Qty = Convert.ToInt32(row["Quantity"]),
                Amt = Convert.ToDecimal(row["Amount"]),
                BillAmount = Convert.ToDecimal(row["BillAmount"]),
                BillDate = DateTime.Parse(row["BillDate"].ToString()),
                Manufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id", "ManufacturerName"),
                Lubricant = new SelectList(dsGetManufacturerVendor.Tables[3].AsDataView(), "Id", "OilName"),
                VendorId = Convert.ToInt32(row["vendorid"])
            };
            Session["BillAmount"] = model.BillAmount;
            Session["Amt"] = model.Amt;
            Session["Bill"] = model.BillNo;
            Session["VendorId"] = model.VendorId;

            return View(model);
     
        }


        [HttpPost]
        public ActionResult EditLubeDetails(InventoryModel postInventory)
        {
            DataSet dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails");
            DataRow row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == postInventory.Id);
            postInventory.VendorId = Convert.ToInt32(row["vendorid"]);
            postInventory.BillAmount = decimal.Parse(Session["BillAmount"].ToString());
            if (row["BillAmount"].ToString() != null)
            {
                if (postInventory.BillAmount != Decimal.Parse(row["BillAmount"].ToString()))
                {
                    postInventory.BillAmount = postInventory.BillAmount + Decimal.Parse(row["BillAmount"].ToString());
                    _inventoryModel.Amt = Convert.ToDecimal(Session["Amt"].ToString());
                    var bill = Decimal.Parse(Session["BillAmount"].ToString());
                    postInventory.BillAmount = postInventory.BillAmount - (_inventoryModel.Amt - postInventory.Amt);
                    _helper.ExecuteUpdateLubesStatement(postInventory.ManufacturerId, "spEditLubes", postInventory.LubricantId, postInventory.Uprice, postInventory.Qty, postInventory.Amt, postInventory.BillAmount, postInventory.BillNo, postInventory.Id,postInventory.BillDate,postInventory.VendorId);
                    string query = "delete from t_Lubesreceipts where billnumber='" + Session["Bill"].ToString() + "'";
                    _helper.ExecuteDeleteInvBillNumberStatement(query);
                }
                else
                {
                    _inventoryModel.Amt = Convert.ToDecimal(Session["Amt"].ToString());
                    var bill = Decimal.Parse(Session["BillAmount"].ToString());
                    postInventory.BillAmount = postInventory.BillAmount - (_inventoryModel.Amt - postInventory.Amt);
                    _helper.ExecuteUpdateLubesStatement(postInventory.ManufacturerId, "spEditLubes", postInventory.LubricantId, postInventory.Uprice, postInventory.Qty, postInventory.Amt, postInventory.BillAmount, postInventory.BillNo, postInventory.Id, postInventory.BillDate,postInventory.VendorId);
                }
            }
            CommonEditLubesMethod(postInventory);
            return RedirectToAction("saveLubesInventoryDetails");
        }
        private void CommonEditLubesMethod(InventoryModel model)
        {
            string query = "select workshopid,receipt_id from t_lubesreceipts where billnumber='" + model.BillNo + "'and vendorid=" + model.VendorId + "";
            DataTable dtWorshopId = _helper.ExecuteSelectStmt(query);
            int workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
            var receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            _helper.ExecuteUpdateLubesStatement(workShopId, model.ManufacturerId, "spEditLubesInventory", model.LubricantId, model.Uprice, model.Qty, receiptId, model.BillNo, model.VendorId);
        }
    }
}