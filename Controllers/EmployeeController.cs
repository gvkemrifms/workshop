using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Fleet_WorkShop.Models;

namespace Fleet_WorkShop.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeHelper _helper = new EmployeeHelper();
        public object UserName { get; set; }

        // GET: Employee
        public ActionResult GetEmployeeDetails() { return RedirectToAction("SaveEmployeeDetails"); }

        public ActionResult GetVendorDetails()
        {
            string districtQuery = "select * from m_districts";
            string vendorTypeQuery = "select * from Vendor_Type";
            DataTable dtVendorType;

            using (DataTable dtDistricts = _helper.ExecuteSelectStmt(districtQuery))
            {
                dtVendorType = _helper.ExecuteSelectStmt(vendorTypeQuery);
                ViewBag.Districts= new SelectList(dtDistricts.AsDataView(), "Id", "District");
            }

            ViewBag.VendorType = new SelectList(dtVendorType.AsDataView(), "Id", "VendorType");
            return View(); 

        }

        [HttpPost] public ActionResult SaveVendorDetails(VendorModel model)
        {
            int result = 0;
            if (model.ContactNumber == 0 || model.Districts == 0 || model.VendorType == 0 || model.VendorName == null || model.GstNumber == null || model.PanNumber == null || model.Location == null) return Json(result, JsonRequestBehavior.AllowGet);
            result=_helper.ExecuteInsertVendorDetails("InsertVendorDetails", model.VendorName, model.Districts, model.Location, 1, model.ContactNumber, model.EmailAddress, model.GstNumber, model.PanNumber, model.VendorType);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckVendorForPan(string pan)
        {
            var result = 0;
            string checkvendor = "select pannumber from m_vendor where pannumber='" + pan + "'";

            using (DataTable dtValVendor = _helper.ExecuteSelectStmt(checkvendor))
            {
                if (dtValVendor.Rows.Count > 0)
                {
                    result = 1;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveEmployeeDetails()
        {
            if (!ModelState.IsValid) return View((IEnumerable<EmployeeModel>) null);
            const string query = "select * from m_departments";
            const string statusQuery = "select * from employee_Status";
            const string desigQuery = "select * from emp_designation where id not in (8,9,10)";
            const string payroll = "select * from m_payroll";
            const string workShops = "select workshop_name,workshop_id  from m_workshop";
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            DataTable dtDesignation;
            DataTable dtPayroll;
            DataTable dtStatus;
            DataTable dtWorkShops;
            using (var dtDepartments = _helper.ExecuteSelectStmt(query))
            {
                dtDesignation = _helper.ExecuteSelectStmt(desigQuery);
                dtPayroll = _helper.ExecuteSelectStmt(payroll);
                dtStatus = _helper.ExecuteSelectStmt(statusQuery);
                dtWorkShops= _helper.ExecuteSelectStmt(workShops);
                Session["Payroll"] = dtPayroll;
                ViewBag.DepartmentName = new SelectList(dtDepartments.AsDataView(), "dept_id", "dept_name");
            }
        

            Session["DepartmentName"] = ViewBag.DepartmentName;
            if (dtDesignation != null) ViewBag.Designation = new SelectList(dtDesignation.AsDataView(), "id", "Designation");
            Session["Designation"] = ViewBag.Designation;
            if (dtPayroll != null) ViewBag.Payroll = new SelectList(dtPayroll.AsDataView(), "payroll_Id", "payroll_name");
            if (dtStatus != null) ViewBag.Status = new SelectList(dtStatus.AsDataView(), "Id", "Status");
            if (dtWorkShops != null) ViewBag.WorkShops = new SelectList(dtWorkShops.AsDataView(), "workshop_id", "workshop_name");
            List<DataRow> list;

            using (var dsGetEmployee = _helper.FillDropDownHelperMethodWithSp("spGetEmployeeDetails"))
            {
                Session["GetEmployeeData"] = dsGetEmployee;
                list = dsGetEmployee.Tables[0].AsEnumerable().ToList();
            }

            var empModel = list.Select(x => new EmployeeModel {Id = x.Field<int>("Id"), EmployeeId = x.Field<string>("empId"), EmployeeName = x.Field<string>("name"), AadharNumber = x.Field<long?>("aadhar_no"), ContactNumber = x.Field<long?>("mobilenumber"), Dob = x.Field<DateTime>("dob"), EmpDesignation = x.Field<string>("Designation"), DepartName = x.Field<string>("dept_name"), Doj = x.Field<DateTime>("doj"), RelievingDate = x.Field<DateTime?>("dor"), Salary = x.Field<int>("Salary"), PayrollCompany = x.Field<string>("payroll_name")});
            ViewBag.Email = UserName;
            return View(empModel);
        }

        [HttpPost] public ActionResult SaveEmployeeDetails(EmployeeModel employeeDetails)
        {
            int? retVal = 0;

            if (employeeDetails.EmployeeId != null && employeeDetails.EmployeeName != null && employeeDetails.ContactNumber != null && employeeDetails.Dob != DateTime.MinValue && employeeDetails.Doj != DateTime.MinValue && employeeDetails.DeptName != 0 && employeeDetails.Desig != 0 && employeeDetails.AadharNumber != null && employeeDetails.Qualification != null && employeeDetails.Experience != 0 && employeeDetails.Salary != 0 && employeeDetails.PayrollId != 0 && employeeDetails.StatusId != 0)
            {
                retVal = SaveEmployee(employeeDetails);
            }

            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] public int? SaveEmployee(EmployeeModel employeeDetails)
        {
            var returnVal = 0;
            if (employeeDetails == null) return null;
            var empDetails = new EmployeeModel {WorkShopId = Convert.ToInt32(Session["WorkshopId"]), EmployeeId = employeeDetails.EmployeeId, EmployeeName = employeeDetails.EmployeeName, ContactNumber = employeeDetails.ContactNumber, DeptName = employeeDetails.DeptName, Desig = employeeDetails.Desig, Dob = DateTime.Parse(employeeDetails.Dob.ToString(CultureInfo.CurrentCulture)), Doj = DateTime.Parse(employeeDetails.Doj.ToString(CultureInfo.CurrentCulture)), AadharNumber = employeeDetails.AadharNumber, EmailAddress = employeeDetails.EmailAddress, RelievingDate = employeeDetails.RelievingDate,TransferredDate =employeeDetails.TransferredDate,Qualification = employeeDetails.Qualification, Experience = employeeDetails.Experience, Salary = employeeDetails.Salary, PayrollId = employeeDetails.PayrollId,StatusId=employeeDetails.StatusId,TransferId=employeeDetails.TransferId};
            returnVal = _helper.ExecuteInsertStatement("InsetEmpDetails", empDetails.EmployeeId, empDetails.EmployeeName, Convert.ToInt32(empDetails.Desig), empDetails.ContactNumber, empDetails.EmailAddress, empDetails.Dob, Convert.ToInt32(empDetails.DeptName), empDetails.AadharNumber, empDetails.Doj, empDetails.RelievingDate,empDetails.TransferredDate, empDetails.Qualification, empDetails.Experience, empDetails.Salary, empDetails.PayrollId, empDetails.WorkShopId,employeeDetails.StatusId,empDetails.TransferId);

            if (empDetails.TransferId <= 0) return returnVal;
            var updateWorkshop= "update m_employees set workshop_id="+employeeDetails.TransferId+" where employeeId="+employeeDetails.EmployeeId+"";
            DataTable dtUpdateEmployeesWorkShop = _helper.ExecuteSelectStmt(updateWorkshop);
            return returnVal;
        }

        [HttpGet] public ActionResult Edit(int? id = null)
        {
            if (id == null) return RedirectToAction("SaveEmployeeDetails");
            if (!ModelState.IsValid) return RedirectToAction("SaveEmployeeDetails");
            var query = "select * from m_departments";
            var desigQuery = "select * from emp_designation";
            var model = new EmployeeModel();
            DataRow row;

            using (var dsEditEmployee = Session["GetEmployeeData"] as DataSet)
            {
                row = dsEditEmployee?.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
            }

            if (row != null)
            {
                model.EmployeeName = row["name"].ToString();
                model.EmpDesignation = row["Designation"].ToString();
                model.ContactNumber = Convert.ToInt64(row["mobilenumber"]);
                model.Dob = Convert.ToDateTime(row["dob"]);
                model.DepartName = row["dept_name"].ToString();
                var dtDepartments = _helper.ExecuteSelectStmt(query);
                var dtDesignation = _helper.ExecuteSelectStmt(desigQuery);
                model.DepartmentName = new SelectList(dtDepartments.AsDataView(), "dept_id", "dept_name");
                model.Designation = new SelectList(dtDesignation.AsDataView(), "id", "Designation");
                model.Doj = Convert.ToDateTime(row["doj"]);
                model.AadharNumber = Convert.ToInt64(row["aadhar_no"]);
                if (row["dor"] != DBNull.Value) model.RelievingDate = Convert.ToDateTime(row["dor"]);
                model.Salary = Convert.ToInt32(row["Salary"]);
                model.PayrollCompany = row["payroll_name"].ToString();
                model.PayrollId = Convert.ToInt32(row["payroll_id"]);
            }

            var dtPayroll = Session["Payroll"] as DataTable;
            if (dtPayroll != null) model.Payroll = new SelectList(dtPayroll.AsDataView(), "payroll_id", "payroll_name");
            return View(model);
        }

        [HttpPost] public ActionResult Edit(EmployeeModel postEmployee)
        {
            _helper.ExecuteUpdateStatement(postEmployee.Id, "spEditEmployee", postEmployee.EmployeeName, postEmployee.DesigEmp, postEmployee.ContactNumber, postEmployee.Dob, postEmployee.DeptEmp, postEmployee.AadharNumber, postEmployee.Doj, postEmployee.RelievingDate, postEmployee.Salary, postEmployee.PayrollId);
            return RedirectToAction("SaveEmployeeDetails");
        }

        [HttpDelete] public ActionResult Delete(int id)
        {
            _helper.ExecuteDeleteStatement("spDeleteEmployee", id);
            return RedirectToAction("SaveEmployeeDetails");
        }

        public ActionResult SaveInfraStructureDetails()
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View((IEnumerable<InfraStructure>) null);
            var query = "select * from m_infra_category";
            var dtinfra = _helper.ExecuteSelectStmt(query);
            ViewBag.categoryName = new SelectList(dtinfra.AsDataView(), "category_id", "category_name");
            var dsGetInfra = _helper.FillDropDownHelperMethodWithSp("spGetInfraDetails");
            Session["InfraTable"] = dsGetInfra;
            IEnumerable<InfraStructure> infastructureModel = dsGetInfra.Tables[0].AsEnumerable().ToList().Select(x => new InfraStructure {infra_id = x.Field<int>("infra_id"), InfraName = x.Field<string>("Infra_Name"), CategoryName = x.Field<string>("category_name"), Quantity = x.Field<int?>("quantity")}).ToList();
            return View(infastructureModel);
        }

        [HttpPost] public int SaveInfraStructureDetails(InfraStructure infraModel)
        {
            var returnVal = 0;
            var workshopquery = "select workshop_id from m_employees where employeeId=" + Session["Employee_Id"] + "";
            var dtWorkshop = _helper.ExecuteSelectStmt(workshopquery);
            var workshopId = dtWorkshop.AsEnumerable().Select(x => x.Field<int>("workshop_id")).FirstOrDefault();
            var infraDetails = new InfraStructure {InfraName = infraModel.InfraName, CategoryId = infraModel.CategoryId, Quantity = infraModel.Quantity, WorkShopId = workshopId,Cost=infraModel.Cost};

            if (ModelState.IsValid) returnVal = _helper.ExecuteInsertStmtusingSp("InsetInfraDetails", "@CategoryId", infraDetails.CategoryId.ToString(), "@qty", infraDetails.Quantity.ToString(), "@InfraName", infraDetails.InfraName, "@workshopid", infraDetails.WorkShopId.ToString(),null,null,null,null,null,null,null,null, "@cost", infraDetails.Cost.ToString());

            return returnVal;
        }

        [HttpGet] public ActionResult EditInfra(int? id = null)
        {
            if (id == null) return RedirectToAction("SaveInfraStructureDetails");
            const string infraQuery = "select * from m_infra_category";
            var dtinfra = _helper.ExecuteSelectStmt(infraQuery);
            var model = new InfraStructure();
            var dsEditInfra = Session["InfraTable"] as DataSet;

            if (dsEditInfra != null)
            {
                var row = dsEditInfra.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("infra_id") == id);
                model.infra_id = Convert.ToInt32(row["infra_id"]);
                Session["Infra_id"] = model.infra_id;
                model.InfraName = row["infra_name"].ToString();
                model.CategoryName = row["category_name"].ToString();
                model.Quantity = Convert.ToInt32(row["quantity"]);
            }

            model.Category = new SelectList(dtinfra.AsDataView(), "category_id", "category_name");
            return View(model);
        }

        [HttpPost] public ActionResult EditInfra(InfraStructure postInfra)
        {
            var infra = Convert.ToInt32(Session["Infra_id"]);
            _helper.ExecuteInsertStmtusingSp("spEditInfra", "@id", infra.ToString(), "@catid", postInfra.CategoryId.ToString(), "@Infraname", postInfra.InfraName, "@Qty", postInfra.Quantity.ToString());
            return RedirectToAction("SaveInfraStructureDetails");
        }

        public ActionResult InfraDelete(int id)
        {
            _helper.ExecuteDeleteStatement("spDeleteInfra", id);
            return RedirectToAction("SaveInfraStructureDetails");
        }

        public ActionResult CheckEmployeeId(int empId)
        {
            if (!ModelState.IsValid) return null;
            var dtCheckIds = _helper.ExecuteSelectStmtusingSP("CheckEmployeeId", "@empid", empId.ToString());
            var list = dtCheckIds.AsEnumerable().Select(x => x.Field<string>("empid")).FirstOrDefault();
            return list == null ? null : Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CommonExpenses()
        {
            if (Session["Employee_Id"] == null) return RedirectToAction("Login", "Account");
            var workShopId = Convert.ToInt32(Session["WorkshopId"]);
            var query = "select workshop_name from m_workshop where workshop_id =" + workShopId + "";
            var dtworkshopName = _helper.ExecuteSelectStmt(query);
            ViewBag.WorkShopName = dtworkshopName.AsEnumerable().Select(x => x.Field<string>("workshop_name")).FirstOrDefault();
            var typeExpenseQuery = "select * from m_commonExpensesTypeHeads";
            var dtTypeOfExpense = _helper.ExecuteSelectStmt(typeExpenseQuery);
            ViewBag.TypeOfExpense = new SelectList(dtTypeOfExpense.AsDataView(), "Id", "ExpenseType");
            var list = new List<SelectListItem> {new SelectListItem {Text = "Cash", Value = "1"}, new SelectListItem {Text = "Card", Value = "2"}};

            IEnumerable<SelectListItem> myCollection = list.AsEnumerable();

            ViewBag.Type = new SelectList(myCollection, "Value", "Text");
            return View();
        }

        [HttpPost] public ActionResult CommonExpenses(PettyExpenses expenses)
        {
            if (expenses.Amount != 0 && expenses.BillNumber != null && expenses.Date.Year!=1  && expenses.paymentType != 0 && expenses.TypeOfExpense != 0)
            {
                var workShopId = Convert.ToInt32(Session["WorkshopId"]);
                var result = _helper.ExecuteInsertPettyDetails("spInsertCommonExpenses", Convert.ToInt32(workShopId), Convert.ToInt32(expenses.TypeOfExpense), Convert.ToDateTime(expenses.Date), Convert.ToDecimal(expenses.Amount), expenses.BillNumber,expenses.paymentType);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return (Json(0));
        }
    }
}