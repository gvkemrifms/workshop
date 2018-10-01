using System;
using System.Collections.Generic;
using System.Data;
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
            const string queryScrap = "select * from m_ScrapBin";
            var dtScrap = _helper.ExecuteSelectStmt(queryScrap);
            ViewBag.ScrapBin = new SelectList(dtScrap.AsDataView(), "ScrapBinId", "ScrapBinName");
            const string query = "select * from m_VehicleManufacturer";
            var dtSpares = _helper.ExecuteSelectStmt(query);
            if (dtSpares == null) return null;
            Session["Manufacturer"] = dtSpares;
            ViewBag.Manufacturers = new SelectList(dtSpares.AsDataView(), "Id", "ManufacturerName");
            return View();
        }

        [HttpPost]
        public ActionResult SparePartsMaster(SparePartsModel spareModel)
        {
            
                if (spareModel == null) throw new ArgumentNullException(nameof(spareModel));
                var returnVal = _helper.ExecuteInsertSparePartsMasterDetails("spSparePartsMaster",
                    spareModel.ManufacturerId, spareModel.PartName, spareModel.PartNumber, spareModel.Cost,
                    spareModel.ScrapBinId);                
                    return Json(returnVal, JsonRequestBehavior.AllowGet);                    

        }

        public ActionResult DisplaySparePartsDetails(string search)
        {
            var dtSpareParts = _helper.ExecuteSelectStmtusingSP("spGetSparesForPartNumber", null, null, null, null,
                "@partnumber", search);
            var sparemodel = dtSpareParts.AsEnumerable().ToList().Select(x => new SparePartsModel
            {
                Id = x.Field<int>("Id"),
                ManufacturerName = x.Field<string>("ManufacturerName"),
                PartName = x.Field<string>("PartName"),
                Cost = x.Field<decimal>("Cost")
            });
            return Json(sparemodel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditSpares(int? id)
        {
            if (id == null)
                return RedirectToAction("SparePartsMaster");
            var spmodel = new SparePartsModel();
            var dtGetSpareDetails = Session["getSpares"] as DataTable;
            if (dtGetSpareDetails == null) return null;
            var row = dtGetSpareDetails.AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
            Session["SparesMasterId"] = row["Id"];
            var dtManufacturers = Session["Manufacturer"] as DataTable;
            if (dtManufacturers != null)
                spmodel.Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName");
            spmodel.ManufacturerId = Convert.ToInt32(row["ManufacturerId"]);
            spmodel.PartName = row["PartName"].ToString();
            spmodel.Cost = Convert.ToDecimal(row["Cost"]);
            return View(spmodel);
        }

        [HttpPost]
        public ActionResult EditSpares(SparePartsModel spmodel)
        {
            if (spmodel == null) throw new ArgumentNullException(nameof(spmodel));
            var sparesId = Convert.ToInt32(Session["SparesMasterId"]);
            _helper.ExecuteUpdateSparesMaster(sparesId, "spEditSparePartsMaster", spmodel.ManufacturerId,
                spmodel.PartName, spmodel.Cost);
            return RedirectToAction("SparePartsMaster");
        }

        public ActionResult LubesMaster()
        {
            const string query = "select * from m_LubesManufactures";
            var dtLubes = _helper.ExecuteSelectStmt(query);
            ViewBag.Manufacturers = new SelectList(dtLubes.AsDataView(), "Id", "ManufacturerName");
            const string lubesQuery = "select * from m_lubes";
            var dtLubesData = _helper.ExecuteSelectStmt(lubesQuery);
            Session["LubesData"] = dtLubesData;
            var lubesModel = dtLubesData.AsEnumerable().ToList().Select(x => new LubesModel
            {
                Id = x.Field<int>("Id"),
                ManufacturerId = x.Field<int>("ManufacturerId"),
                OilName = x.Field<string>("OilName"),
                CostPerLitre = x.Field<decimal>("CostPerLitre"),
                LubricantNumber = x.Field<string>("LubricantNumber")
            });
            return View(lubesModel);
        }

        [HttpPost]
        public ActionResult GetSparePartsDetailsForManufacturer(string manufacturerId)
        {
            if (!ModelState.IsValid) return null;
            _inventoryModel.ManufacturerId = int.Parse(manufacturerId);
            Session["Id"] = _inventoryModel.ManufacturerId;
            var dsFillSparesOfManufacturers =
                _helper.FillDropDownHelperMethodWithSp("spGetSparesForManufacturer",
                    _inventoryModel.ManufacturerId);
            var data = dsFillSparesOfManufacturers.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();
            foreach (var row in data)
            {
                _inventoryModel.SparePartId = Convert.ToInt32(row["Id"]);
                _inventoryModel.SpareName = row["PartName"].ToString();
                names.Add(_inventoryModel.SpareName + "-" + _inventoryModel.SparePartId);
            }
            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(list, "application/json");
        }

        public ActionResult GetLubesDetailsForManufacturer(string manufacturerId)
        {
            if (!ModelState.IsValid) return null;
            _inventoryModel.ManufacturerId = int.Parse(manufacturerId);
            Session["Id"] = _inventoryModel.ManufacturerId;
            var dsFillLubesOfManufacturers =
                _helper.FillDropDownHelperMethodWithSp("spGetLubesForManufacturer", _inventoryModel.ManufacturerId);
            var data = dsFillLubesOfManufacturers.Tables[0].AsEnumerable().ToList();
            var names = new List<string>();
            foreach (var row in data)
            {
                _inventoryModel.LubricantId = Convert.ToInt32(row["Id"]);
                _inventoryModel.LubricantName = row["OilName"].ToString();

                names.Add(_inventoryModel.LubricantName + "-" + _inventoryModel.LubricantId);
            }
            var list = JsonConvert.SerializeObject(names, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(list, "application/json");
        }

        public ActionResult GetCostDetails(int id)
        {
            if (id == 0) return null;
            var query = "select cost,partNumber from m_spareparts where Id='" + id + "'";
            var dtCost = _helper.ExecuteSelectStmt(query);
            var cost = dtCost.AsEnumerable().ToList()
                .Select(x => new {Cost = x.Field<decimal>("Cost"), PartNumber = x.Field<string>("partNumber")})
                .FirstOrDefault();
            if (cost == null) return null;
            return Json(cost, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLubesCostDetails(int id)
        {
            if (id == 0) return null;
            var query = "select CostPerLitre,LubricantNumber from m_lubes where Id='" + id + "'";
            var dtCost = _helper.ExecuteSelectStmt(query);
            var cost = dtCost.AsEnumerable().ToList()
                .Select(x => new
                {
                    Cost = x.Field<decimal>("CostPerLitre"),
                    LubricantNumber = x.Field<string>("LubricantNumber")
                }).FirstOrDefault();
            return cost == null ? null : Json(cost, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetSpareCostDetails(int id)
        {
            var query = "select cost from m_spareparts where Id='" + id + "'";
            var dtCost = _helper.ExecuteSelectStmt(query);
            var cost = dtCost.AsEnumerable().ToList().Select(x => x.Field<decimal>("cost")).First();
            return cost != 1 ? Json(cost, JsonRequestBehavior.AllowGet) : null;
        }

        [HttpPost]
        public ActionResult LubesMaster(LubesModel lubesModel)
        {
            var returnVal = _helper.ExecuteInsertLubesMasterDetails("spLubesMaster", lubesModel.ManufacturerId,
                lubesModel.OilName, lubesModel.CostPerLitre, lubesModel.LubricantNumber, 1);
            if (returnVal == 1)
                return Json("Hello", JsonRequestBehavior.AllowGet);
            return RedirectToAction("LubesMaster");
        }

        [HttpGet]
        public ActionResult LubesMasterEdit(int? id)
        {
            if (id == null)
                return RedirectToAction("LubesMaster");
            var spmodel = new LubesModel();
            var dtGetSpareDetails = Session["LubesData"] as DataTable;
            if (dtGetSpareDetails == null) return null;
            var row = dtGetSpareDetails.AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
            spmodel.Id = Convert.ToInt32(row["Id"]);
            Session["Id"] = spmodel.Id;
            const string query = "select * from m_VehicleManufacturer";
            var dtLubes = _helper.ExecuteSelectStmt(query);
            spmodel.Manufacturer = new SelectList(dtLubes.AsDataView(), "Id", "ManufacturerName");
            spmodel.ManufacturerId = Convert.ToInt32(row["ManufacturerId"]);
            spmodel.OilName = row["OilName"].ToString();
            spmodel.CostPerLitre = Convert.ToDecimal(row["CostPerLitre"]);
            return View(spmodel);
        }

        [HttpPost]
        public ActionResult LubesMasterEdit(LubesModel lubesModel)
        {
            if (lubesModel == null) throw new ArgumentNullException(nameof(lubesModel));
            var lubesId = Convert.ToInt32(Session["Id"]);
            _helper.ExecuteUpdateLubesMaster(lubesId, "spEditLubesMaster", lubesModel.ManufacturerId,
                lubesModel.OilName, lubesModel.CostPerLitre);
            return RedirectToAction("LubesMaster");
        }

        public ActionResult DeleteLubesMaster(int? id)
        {
            _helper.ExecuteDeleteStatement("spDeleteLubesMaster", id);
            return RedirectToAction("LubesMaster");
        }

        public ActionResult GetSparePartsDetails()
        {
            return Session["Employee_Id"] == null
                ? RedirectToAction("Login", "Account")
                : RedirectToAction("SaveInventoryDetails");
        }

        public ActionResult SavePODetails()
        {
            if (Session["WorkshopId"] == null)
                return RedirectToAction("Login", "Account");
            var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id",
                "ManufacturerName");
            ViewBag.Spares = new SelectList(dsGetManufacturerVendor.Tables[2].AsDataView(), "Id", "PartName");
            return View();
        }

        [HttpPost]
        public ActionResult SavePODetails(InventoryModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var employee = Session["Employee_Id"].ToString();
            var billDetails = new InventoryModel
            {
                PoNumber = model.PoNumber,
                PoDate = model.PoDate,
                EmployeeId = Convert.ToInt32(employee)
            };
            var result = _helper.ExecutePODetails("spInsertPODetails", billDetails.PoNumber, billDetails.PoDate,
                billDetails.EmployeeId);
            foreach (var items in model.itemmodel)
                _helper.ExecuteInsertPOManufacturerDetails("spInsertSparePODetails", billDetails.PoNumber,
                    Convert.ToInt32(items.ManufacturerId), Convert.ToInt32(items.SparePartId),
                    Convert.ToDecimal(items.UnitPrice), Convert.ToInt32(items.Quantity),
                    Convert.ToDecimal(items.Amount));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveInventoryDetails()
        {
            if (Session["WorkshopId"] == null)
                return RedirectToAction("Login", "Account");
            var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[0].AsDataView(), "Id",
                "ManufacturerName");
            ViewBag.Vendors = new SelectList(dsGetManufacturerVendor.Tables[1].AsDataView(), "id", "vendor_name");
            ViewBag.Spares = new SelectList(dsGetManufacturerVendor.Tables[2].AsDataView(), "Id", "PartName");
            var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetReceiptDetails");
            var invModel = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel
            {
                Id = x.Field<int>("id"),
                BillNo = x.Field<string>("BillNumber"),
                ManName = x.Field<string>("ManufacturerName"),
                BillDate = x.Field<DateTime>("BillDate"),
                BillAmount = x.Field<decimal>("BillAmount"),
                PartName = x.Field<string>("PartName"),
                PartNumber = x.Field<string>("PartNumber"),
                Uprice = x.Field<decimal>("UnitPrice"),
                Qty = x.Field<int>("Quantity"),
                Amt = x.Field<decimal>("Amount")
            });
            return View(invModel);
        }

        [HttpPost]
        public ActionResult SaveInventoryDetails(InventoryModel model)
        {
            var result = 0;
            var poQuantitySpares = Session["PoQuantitySpares"] as IEnumerable<GetPODetailsSpareParts>;

            if (model == null) throw new ArgumentNullException(nameof(model));
            var billDetails = new InventoryModel
            {
                BillNo = model.BillNo,
                BillDate = model.BillDate,
                BillAmount = model.BillAmount,
                VendorName = model.VendorName,
                VendorId = model.VendorId,
                PoNumber = model.PoNumber,
                PoDate = model.PoDate,
                WorkShopId = Convert.ToInt32(Session["WorkshopId"])
            };
            _helper.ExecuteBillDetails("spInsertBillDetails", billDetails.BillNo, billDetails.BillDate,
                billDetails.BillAmount, billDetails.VendorId, billDetails.PoNumber, billDetails.PoDate,
                billDetails.WorkShopId);
            foreach (var items in model.itemmodel)
            {
                _helper.ExecuteInsertInventoryDetails("spInsertInventoryDetails", model.BillNo, items.ManufacturerId,
                    items.SparePartId, items.UnitPrice, items.Quantity, items.Amount, model.VendorId, billDetails.PoNumber);
                foreach (var poitem in poQuantitySpares)
                    if (poitem.SparePartId == items.SparePartId)
                        result = _helper.UpdateSparePartsPoDetails("UpdateSparePartsPODetails",
                            poitem.ReceivedQuantity + items.Quantity, model.BillDate, items.ManufacturerId,
                            items.SparePartId, model.PoNumber);
            }
            CommonMethod(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void CommonMethod(InventoryModel model)
        {
            var query = "select workshopid,receipt_id from t_receipts where billnumber='" + model.BillNo +
                        "'and vendorid=" + model.VendorId + "";
            var dtWorshopId = _helper.ExecuteSelectStmt(query);
            var workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
            var receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            foreach (var item in model.itemmodel)
                _helper.ExecuteInsertStockDetails("spInsertInventoryStockDetails", workShopId, item.ManufacturerId,
                    item.SparePartId, item.UnitPrice, item.Quantity, receiptId, model.BillNo, model.VendorId);
        }

        public void CommonMethodLubes(InventoryModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var query = "select workshopid,receipt_id from t_lubesreceipts where billnumber='" + model.BillNo +
                        "'and vendorid=" + model.VendorId + "";
            var dtWorshopId = _helper.ExecuteSelectStmt(query);
            var workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
            var receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            foreach (var item in model.itemmodel)
                _helper.ExecuteInsertLubeStockDetails("spInsertLubesStockDetails", workShopId, item.ManufacturerId,
                    item.LubricantId, item.UnitPrice, item.Quantity, receiptId, model.BillNo, model.VendorId);
        }

        [HttpGet]
        public ActionResult Edit(int? id = null)
        {
    
            if (id == null)
                return RedirectToAction("SaveInventoryDetails");
            //var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetReceiptDetails");
            if (dsGetReceiptsDetails.Tables[0].Rows.Count <= 0) return RedirectToAction("SaveInventoryDetails");
            var row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int?>("Id") == id);
                var manufacturerId = Convert.ToInt32(row["ManufacturerId"]);
                var manufacturerQuery = "select * from m_VehicleManufacturer where Id=" + manufacturerId + " ";
                var dtManufacturers = _helper.ExecuteSelectStmt(manufacturerQuery);
                var dtGetReceiptsDetailsonSpares = _helper.ExecuteSelectStmtusingSP("spGetReceiptDetailsonBillNumber",
                    null,
                    null, null, null, "@billnumber", row["BillNumber"].ToString());
                //var sparesList= dsGetReceiptsDetailsonSpares.AsEnumerable().Select(x=>x).Where(x => x.Field<string>("BillNumber") == row["BillNumber"].ToString());
                //var sparePartId= dsGetReceiptsDetailsonSpares.Tables[0].AsEnumerable().Where(x => x.Field<string>("BillNumber") == row["BillNumber"].ToString()).Select(x => x.Field<int>("SparePartId")).FirstOrDefault();
                var model = new InventoryModel
                {
                    BillNo = row["BillNumber"].ToString(),
                    ManName = row["ManufacturerName"].ToString(),
                    PartName = row["PartName"].ToString(),
                    Uprice = Convert.ToDecimal(row["UnitPrice"]),
                    Qty = Convert.ToInt32(row["Quantity"]),
                    Amt = Convert.ToDecimal(row["Amount"]),
                    BillAmount = Convert.ToDecimal(row["BillAmount"]),
                    BillDate = DateTime.Parse(row["BillDate"].ToString()),
                    ManufacturerId = Convert.ToInt32(row["ManufacturerId"]),
                    Manufacturer = new SelectList(dtManufacturers.AsDataView(), "Id", "ManufacturerName"),
                    //SpareParts = new SelectList(dsGetManufacturerVendor.Tables[2].AsDataView(), "Id", "PartName"),
                    SpareParts = new SelectList(dtGetReceiptsDetailsonSpares.AsDataView(), "SparePartId", "PartName"),
                    VendorId = Convert.ToInt32(row["vendorid"]),
                    SparePartId = Convert.ToInt32(row["SparePartId"])
                };
            

            var dtgetAllSpares = _helper.ExecuteSelectStmtusingSP("getSparesForBillNumberAndVendors", "@vendorid",
                model.VendorId.ToString(), null, null, "@billnumber", model.BillNo);
            ViewBag.CartItems = dtgetAllSpares;
            var getPo =
                "select top 1 r.PoNumber,billamount from t_receipts r join t_receiptData rd on r.BillNumber=rd.BillNumber where r.BillNumber='" +
                model.BillNo + "'";
            var dtGetPoNum = _helper.ExecuteSelectStmt(getPo);
            var getPoNumber = dtGetPoNum.AsEnumerable().Select(x => x.Field<string>("PoNumber")).FirstOrDefault();
            ViewBag.BillAmountss = model.BillAmount;
            var dtGetPoNumber = _helper.ExecuteSelectStmtusingSP("spSparePartsEditPODetails", null, null, null, null,
                "@ponumber", getPoNumber);
            ViewBag.SparesQty = dtGetPoNumber;
            Session["BillAmount"] = model.BillAmount;
            Session["Amt"] = model.Amt;
            Session["Bill"] = model.BillNo;
            Session["VendorId"] = model.VendorId;

            return View(model);
        }
        
        [HttpPost]
        public ActionResult Edit(InventoryModel postInventory)
        {
            if (postInventory == null) throw new ArgumentNullException(nameof(postInventory));
            var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetReceiptDetails");
            var row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList()
                .Single(x => x.Field<int>("Id") == postInventory.Id);
            postInventory.VendorId = Convert.ToInt32(row["vendorId"]);
            postInventory.SparePartId = Convert.ToInt32(row["SparePartId"]);
            postInventory.BillAmount = decimal.Parse(Session["BillAmount"].ToString());
            if (postInventory.BillAmount != decimal.Parse(row["BillAmount"].ToString()))
            {
                postInventory.BillAmount = postInventory.BillAmount + decimal.Parse(row["BillAmount"].ToString());
                _inventoryModel.Amt = Convert.ToDecimal(Session["Amt"].ToString());
                postInventory.BillAmount = postInventory.BillAmount - (_inventoryModel.Amt - postInventory.Amt);
                _helper.ExecuteUpdateInventoryStatement(postInventory.ManufacturerId, "spEditInventory",
                    postInventory.SparePartId, postInventory.Uprice, postInventory.Qty, postInventory.Amt,
                    postInventory.BillAmount, postInventory.BillNo, postInventory.Id, postInventory.VendorId);
                var query = "delete from t_receipts where billnumber='" + Session["Bill"] + "'";
                _helper.ExecuteDeleteInvBillNumberStatement(query);
            }
            else
            {
                _inventoryModel.Amt = Convert.ToDecimal(Session["Amt"].ToString());
                postInventory.BillAmount = postInventory.BillAmount - (_inventoryModel.Amt - postInventory.Amt);
                _helper.ExecuteUpdateInventoryStatement(postInventory.ManufacturerId, "spEditInventory",
                    postInventory.SparePartId, postInventory.Uprice, postInventory.Qty, postInventory.Amt,
                    postInventory.BillAmount, postInventory.BillNo, postInventory.Id, postInventory.VendorId);
            }
            CommonEditSparesMethod(postInventory);
            return RedirectToAction("SaveInventoryDetails");
        }

        public ActionResult DeleteStockDetails(int? id,string bill, string ponumber,int? quantity)
        {
           int result= _helper.ExecuteInsertStmtusingSp("Spdeletestocks", "@sparepartid", id.ToString(),null,null, "@billnumber", bill,"@quantity", quantity.ToString(),null,null,null,null,null,null, "@ponumber", ponumber);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditOrderItemsInventoryDetails(InventoryModel postInventory)
        {
            return View();
        }

        private void CommonEditSparesMethod(InventoryModel model)
        {
            var query = "select workshopid,receipt_id from t_receipts where billnumber='" + model.BillNo +
                        "'and vendorid=" + model.VendorId + "";
            var dtWorshopId = _helper.ExecuteSelectStmt(query);
            var workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
            var receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            _helper.ExecuteUpdateInventoryStocksStatement(workShopId, model.ManufacturerId, "spEditSParepartsInventory",
                model.SparePartId, model.Uprice, model.Qty, receiptId, model.BillNo, model.VendorId);
        }

        public ActionResult SaveLubesInventoryPODetails()
        {
            if (Session["WorkshopId"] == null)
                return RedirectToAction("Login", "Account");
            var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[4].AsDataView(), "Id",
                "ManufacturerName");

            ViewBag.Lubes = new SelectList(dsGetManufacturerVendor.Tables[3].AsDataView(), "Id", "OilName");
            var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails");
            var invModel = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel
            {
                Id = x.Field<int>("id"),
                BillNo = x.Field<string>("BillNumber"),
                ManName = x.Field<string>("ManufacturerName"),
                BillDate = x.Field<DateTime>("BillDate"),
                BillAmount = x.Field<decimal>("BillAmount"),
                LubricantName = x.Field<string>("OilName"),
                Uprice = x.Field<decimal>("UnitPrice"),
                Qty = x.Field<int>("Quantity"),
                Amt = x.Field<decimal>("Amount")
            });
            return View(invModel);
        }

        [HttpPost]
        public ActionResult SaveLubesInventoryPODetails(InventoryModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var employee = Session["Employee_Id"].ToString();
            var billDetails = new InventoryModel
            {
                PoNumber = model.PoNumber,
                PoDate = model.PoDate,
                EmployeeId = Convert.ToInt32(employee)
            };
            var result = _helper.ExecutePODetails("spInsertLubesPODetails", billDetails.PoNumber, billDetails.PoDate,
                billDetails.EmployeeId);
            foreach (var items in model.itemmodel)
                _helper.ExecuteInsertPOManufacturerDetails("spInsertLubePODetails", billDetails.PoNumber,
                    Convert.ToInt32(items.ManufacturerId), Convert.ToInt32(items.LubricantId),
                    Convert.ToDecimal(items.UnitPrice), Convert.ToInt32(items.Quantity),
                    Convert.ToDecimal(items.Amount));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveLubesInventoryDetails()
        {
            if (Session["WorkshopId"] == null)
                return RedirectToAction("Login", "Account");
            var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            ViewBag.VehicleManufacturer = new SelectList(dsGetManufacturerVendor.Tables[4].AsDataView(), "Id",
                "ManufacturerName");
            ViewBag.Vendors = new SelectList(dsGetManufacturerVendor.Tables[1].AsDataView(), "id", "vendor_name");
            ViewBag.Lubes = new SelectList(dsGetManufacturerVendor.Tables[3].AsDataView(), "Id", "OilName");
            var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails");
            var invModel = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Select(x => new InventoryModel
            {
                Id = x.Field<int>("id"),
                BillNo = x.Field<string>("BillNumber"),
                ManName = x.Field<string>("ManufacturerName"),
                BillDate = x.Field<DateTime>("BillDate"),
                BillAmount = x.Field<decimal>("BillAmount"),
                LubricantName = x.Field<string>("OilName"),
                Uprice = x.Field<decimal>("UnitPrice"),
                Qty = x.Field<int>("Quantity"),
                Amt = x.Field<decimal>("Amount")
            });
            return View(invModel);
        }

        [HttpPost]
        public ActionResult SaveLubesInventoryDetails(InventoryModel model)
        {
            var result = 0;
            var poQuantityLubes = Session["PoQuantityLubes"] as IEnumerable<GetPODetailsSpareParts>;
            if (model == null) throw new ArgumentNullException(nameof(model));
            var billDetails = new InventoryModel
            {
                BillNo = model.BillNo,
                BillDate = model.BillDate,
                BillAmount = model.BillAmount,
                VendorName = model.VendorName,
                VendorId = model.VendorId,
                PoNumber = model.PoNumber,
                PoDate = model.PoDate,
                WorkShopId = Convert.ToInt32(Session["WorkshopId"])
            };
            _helper.ExecuteBillDetails("spInsertLubeBillDetails", billDetails.BillNo, billDetails.BillDate,
                billDetails.BillAmount, billDetails.VendorId, billDetails.PoNumber, billDetails.PoDate,
                billDetails.WorkShopId);
            foreach (var items in model.itemmodel)
            {
                _helper.ExecuteInsertLubesDetails("spInsertLubricantDetails", model.BillNo, items.ManufacturerId,
                    items.LubricantId, items.UnitPrice, items.Quantity, items.Amount, billDetails.VendorId);
                foreach (var poitem in poQuantityLubes)
                    if (poitem.SparePartId == items.LubricantId)
                        result = _helper.UpdateSparePartsPoDetails("UpdateLubesPODetails",
                            poitem.ReceivedQuantity + items.Quantity, model.BillDate, items.ManufacturerId,
                            items.LubricantId, model.PoNumber);
            }
            CommonMethodLubes(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditLubeDetails(int? id = null)
        {
            if (id == null)
                return RedirectToAction("SaveLubesInventoryDetails");
            var dsGetManufacturerVendor = _helper.FillDropDownHelperMethodWithSp("spGetManufacturerVendor");
            var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails");
            var row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int?>("Id") == id);
            var model = new InventoryModel
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
                VendorId = Convert.ToInt32(row["vendorid"]),
                ManufacturerId = Convert.ToInt32(row["ManufacturerId"]),
                LubricantId = Convert.ToInt32(row["LubricantId"])
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
            if (postInventory == null) throw new ArgumentNullException(nameof(postInventory));
            var dsGetReceiptsDetails = _helper.FillDropDownHelperMethodWithSp("spGetLubesReceiptDetails");
            var row = dsGetReceiptsDetails.Tables[0].AsEnumerable().ToList()
                .Single(x => x.Field<int>("Id") == postInventory.Id);
            postInventory.VendorId = Convert.ToInt32(row["vendorid"]);
            postInventory.BillAmount = decimal.Parse(Session["BillAmount"].ToString());
            if (postInventory.BillAmount != decimal.Parse(row["BillAmount"].ToString()))
            {
                postInventory.BillAmount = postInventory.BillAmount + decimal.Parse(row["BillAmount"].ToString());
                _inventoryModel.Amt = Convert.ToDecimal(Session["Amt"].ToString());
                postInventory.BillAmount = postInventory.BillAmount - (_inventoryModel.Amt - postInventory.Amt);
                _helper.ExecuteUpdateLubesStatement(postInventory.ManufacturerId, "spEditLubes",
                    postInventory.LubricantId, postInventory.Uprice, postInventory.Qty, postInventory.Amt,
                    postInventory.BillAmount, postInventory.BillNo, postInventory.Id, postInventory.BillDate,
                    postInventory.VendorId);
                var query = "delete from t_Lubesreceipts where billnumber='" + Session["Bill"] + "'";
                _helper.ExecuteDeleteInvBillNumberStatement(query);
            }
            else
            {
                _inventoryModel.Amt = Convert.ToDecimal(Session["Amt"].ToString());
                postInventory.BillAmount = postInventory.BillAmount - (_inventoryModel.Amt - postInventory.Amt);
                _helper.ExecuteUpdateLubesStatement(postInventory.ManufacturerId, "spEditLubes",
                    postInventory.LubricantId, postInventory.Uprice, postInventory.Qty, postInventory.Amt,
                    postInventory.BillAmount, postInventory.BillNo, postInventory.Id, postInventory.BillDate,
                    postInventory.VendorId);
            }
            CommonEditLubesMethod(postInventory);
            return RedirectToAction("saveLubesInventoryDetails");
        }

        private void CommonEditLubesMethod(InventoryModel model)
        {
            var query = "select workshopid,receipt_id from t_lubesreceipts where billnumber='" + model.BillNo +
                        "'and vendorid=" + model.VendorId + "";
            var dtWorshopId = _helper.ExecuteSelectStmt(query);
            var workShopId = dtWorshopId.AsEnumerable().Select(x => x.Field<int>("workshopid")).FirstOrDefault();
            var receiptId = dtWorshopId.AsEnumerable().Select(x => x.Field<long>("receipt_id")).FirstOrDefault();
            _helper.ExecuteUpdateLubesStatement(workShopId, model.ManufacturerId, "spEditLubesInventory",
                model.LubricantId, model.Uprice, model.Qty, receiptId, model.BillNo, model.VendorId);
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
            var dtPartNumber =
                _helper.ExecuteSelectStmtusingSP("getPartNumber", null, null, null, null, "@partnumber",
                    partNumber);
            var list = dtPartNumber.AsEnumerable().Select(x => x.Field<string>("partNumber")).FirstOrDefault();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckQuantity(int qty,int sparesid)
        {
            string quantitycheck =
                "select (Quantity-sum(ReceivedQuantity)) as tquantity from t_sparepartspodetails where sparepartid="+ sparesid+" group by quantity";
            DataTable dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck);
           int quantity= dtCheckSpares.AsEnumerable().Where(x => x.Field<int>("tquantity") > qty).Select(x=>x.Field<int>("tquantity")).FirstOrDefault();

            return Json(quantity,JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckLubesAvailableQuantity(int qty, int sparesid)
        {
            string quantitycheck =
                "select (Quantity-sum(ReceivedQuantity)) as tquantity from t_LubesPodetails where lubricantid=" + sparesid + " group by quantity";
            DataTable dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck);
            int quantity = dtCheckSpares.AsEnumerable().Where(x => x.Field<int>("tquantity") >= qty).Select(x => x.Field<int>("tquantity")).FirstOrDefault();

            return Json(quantity, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckLubesQuantity(int qty, int lubesid)
        {
            string quantitycheck =
                "select (Quantity-sum(ReceivedQuantity)) as tquantity from t_LubesPodetails where LubricantId=" + lubesid + " group by quantity";
            DataTable dtCheckSpares = _helper.ExecuteSelectStmt(quantitycheck);
            int quantity = dtCheckSpares.AsEnumerable().Where(x => x.Field<int>("tquantity") > qty).Select(x => x.Field<int>("tquantity")).FirstOrDefault();

            return Json(quantity, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckLubricantNumber(string lubricantNumber)
        {
            if (!ModelState.IsValid) return null;
            var dtLubesNumber = _helper.ExecuteSelectStmtusingSP("getLubricantNumber", null, null, null, null,
                "@lubricantnumber", lubricantNumber);
            var list = dtLubesNumber.AsEnumerable().Select(x => x.Field<string>("LubricantNumber")).FirstOrDefault();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckPoNumber(string poNumber)
        {
            var poSparesQuery = "select * from SparePartsPODetails where PoNumber='" + poNumber + "'";
            var dtPoDetails = _helper.ExecuteSelectStmt(poSparesQuery);
            if (dtPoDetails == null) return null;
            var po = dtPoDetails.AsEnumerable().Select(x => x.Field<int?>("Id")).FirstOrDefault();
            return Json(po, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckLubesPoNumber(string poNumber)
        {
            var poLubesQuery = "select * from LubesPO where PONumber='" + poNumber + "'";
            var dtPoDetails = _helper.ExecuteSelectStmt(poLubesQuery);
            if (dtPoDetails == null) return null;
            var po = dtPoDetails.AsEnumerable().Select(x => x.Field<int?>("Id")).FirstOrDefault();
            return Json(po, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSparePoDetails(string ponumber)
        {
            const int response = 0;
            if (ponumber == null) return null;
            var dtSparePartsPoDatails =
                _helper.ExecuteSelectStmtusingSP("spSparePartsPODetails", null, null, null, null, "@ponumber",
                    ponumber);
            if (dtSparePartsPoDatails == null || dtSparePartsPoDatails.Rows.Count <= 0)
                return Json(response, JsonRequestBehavior.AllowGet);
            var manufactureId = dtSparePartsPoDatails.AsEnumerable().Select(x => x.Field<int>("ManufacturerId"))
                .FirstOrDefault();
            var manufacturerQuery = "select * from [dbo].[m_VehicleManufacturer] where Id=" + manufactureId + "";
            var dtManufacturerOnPo = _helper.ExecuteSelectStmt(manufacturerQuery);
            var id = dtManufacturerOnPo.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
            var manufacturerName = dtManufacturerOnPo.AsEnumerable().Select(x => x.Field<string>("ManufacturerName"))
                .FirstOrDefault();
            IEnumerable<GetPODetailsSpareParts> podatailsSpares = dtSparePartsPoDatails.AsEnumerable().Select(
                x => new GetPODetailsSpareParts
                {
                    PoDate = x.Field<DateTime>("PoDate").ToShortDateString(),
                    PartName = x.Field<string>("partname"),
                    PartNumber = x.Field<string>("partnumber"),
                    PoQuantity = x.Field<int>("poquantity"),
                    ReceivedQuantity = x.Field<int>("ReceivedQuantity"),
                    LastReceivedDate = x.Field<DateTime?>("lastreceiveddate"),
                    PendingQuantity = x.Field<int>("poquantity") - x.Field<int>("ReceivedQuantity"),
                    ManufacturerId = id,
                    ManufacturerName = manufacturerName,
                    SparePartId = x.Field<int>("sparepartId"),
                    GetLastReceivedDate = x.Field<DateTime?>("lastreceiveddate").ToString()
                });
            Session["PoQuantitySpares"] = podatailsSpares;
            return Json(podatailsSpares, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLubesPoDetails(string ponumber)
        {
            const int response = 0;
            if (ponumber == null) return null;
            var dtLubesPoDatails =
                _helper.ExecuteSelectStmtusingSP("spLubesPODetails", null, null, null, null, "@ponumber", ponumber);
            if (dtLubesPoDatails == null || dtLubesPoDatails.Rows.Count <= 0)
                return Json(response, JsonRequestBehavior.AllowGet);
            var manufactureId = dtLubesPoDatails.AsEnumerable().Select(x => x.Field<int>("ManufacturerId"))
                .FirstOrDefault();
            var manufacturerQuery = "select * from [dbo].[m_LubesManufactures] where Id=" + manufactureId + "";
            var dtManufacturerOnPo = _helper.ExecuteSelectStmt(manufacturerQuery);
            var id = dtManufacturerOnPo.AsEnumerable().Select(x => x.Field<int>("Id")).FirstOrDefault();
            var manufacturerName = dtManufacturerOnPo.AsEnumerable().Select(x => x.Field<string>("ManufacturerName"))
                .FirstOrDefault();
            IEnumerable<GetPODetailsSpareParts> podatailsLubes = dtLubesPoDatails.AsEnumerable()
                .Select(x => new GetPODetailsSpareParts
                {
                    PoDate = x.Field<DateTime>("PoDate").ToShortDateString(),
                    LubricantName = x.Field<string>("OilName"),
                    LubricantNumber = x.Field<string>("LubricantNumber"),
                    PoQuantity = x.Field<int>("poquantity"),
                    ReceivedQuantity = x.Field<int>("ReceivedQuantity"),
                    LastReceivedDate = x.Field<DateTime?>("lastreceiveddate"),
                    PendingQuantity = x.Field<int>("poquantity") - x.Field<int>("ReceivedQuantity"),
                    ManufacturerId = id,
                    ManufacturerName = manufacturerName,
                    SparePartId = x.Field<int>("LubricantId"),
                    GetLastReceivedDate = x.Field<DateTime?>("lastreceiveddate").ToString()
                });
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
            if (scrapId != null)
            {
                var dtGetScrapDetails =
                    _helper.ExecuteSelectStmtusingSP("ScrapDetails", "@scrapbinid", scrapId.ToString());

                Session["Scraps"] = dtGetScrapDetails;
                foreach (DataRow row in dtGetScrapDetails.Rows)
                {
                    var scrapBin = new ScrapBinModel
                    {
                        ScrapBinId = Convert.ToInt32(row["ScrapBinId"]),
                        PartName = row["PartName"].ToString(),
                        PartNumber = row["PartNumber"].ToString(),
                        Quantity = Convert.ToInt32(row["Quantity"])
                    };
                    scrapList.Add(scrapBin);
                }
                return Json(scrapList, JsonRequestBehavior.AllowGet);
            }

            return View();
        }

        public ActionResult InsertScrapDetails(int? netWeight, int? actualCount)
        {
            var result = 0;
            var employee = Session["Employee_Id"].ToString();
            var dtGetScrapDetails = Session["Scraps"] as DataTable;
            if (dtGetScrapDetails != null)
            {
                var count = dtGetScrapDetails.AsEnumerable().Sum(x => x.Field<long>("rownumber"));
                foreach (DataRow row in dtGetScrapDetails.Rows)
                {
                    result = _helper.ExecuteInsertStmtusingSp("SpinsertScrapDetails", "@scrapid",
                        row["ScrapBinId"].ToString(),
                        "@sparecount", count.ToString(), null, null, "@sparequantity", row["Quantity"].ToString(),
                        "@netweight", netWeight.ToString(), "@actualquantity", actualCount.ToString(), "@enterdby",
                        employee);
                    if (result > 0)
                        break;
                }
                foreach (DataRow row in dtGetScrapDetails.Rows)
                    _helper.ExecuteInsertStmtusingSp("SpUpdateScrapDetails", "@scrapid", row["ScrapBinId"].ToString(),
                        "@sparepartid", row["Id"].ToString());
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}