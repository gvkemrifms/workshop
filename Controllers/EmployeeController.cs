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

        //private readonly IEnumerable<EmployeeModel> empModel;

        public object UserName { get; set; }

        // GET: Employee
        public ActionResult GetEmployeeDetails()
        {
            return RedirectToAction("SaveEmployeeDetails");
        }

        public ActionResult SaveEmployeeDetails()
        {
            IEnumerable<EmployeeModel> empModel = null;
            if (!ModelState.IsValid) return View((IEnumerable<EmployeeModel>) null);
            const string query = "select * from m_departments";
            const string desigQuery = "select * from emp_designation";
            const string payroll = "select * from m_payroll";
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");
            var dtDepartments = _helper.ExecuteSelectStmt(query);
            var dtDesignation = _helper.ExecuteSelectStmt(desigQuery);
            var dtPayroll = _helper.ExecuteSelectStmt(payroll);
            Session["Payroll"] = dtPayroll;
            ViewBag.DepartmentName = new SelectList(dtDepartments.AsDataView(), "dept_id", "dept_name");
            Session["DepartmentName"] = ViewBag.DepartmentName;
            ViewBag.Designation = new SelectList(dtDesignation.AsDataView(), "id", "Designation");
            Session["Designation"] = ViewBag.Designation;
            ViewBag.Payroll = new SelectList(dtPayroll.AsDataView(), "payroll_Id", "payroll_name");
            var dsGetEmployee = _helper.FillDropDownHelperMethodWithSp("spGetEmployeeDetails");
            Session["GetEmployeeData"] = dsGetEmployee;
            var list = dsGetEmployee.Tables[0].AsEnumerable().ToList();
            empModel = list.Select(x => new EmployeeModel
            {
                Id = x.Field<int>("Id"),
                EmployeeId = x.Field<string>("empId"),
                EmployeeName = x.Field<string>("name"),
                AadharNumber = x.Field<long?>("aadhar_no"),
                ContactNumber = x.Field<long?>("mobilenumber"),
                Dob = x.Field<DateTime>("dob"),
                EmpDesignation = x.Field<string>("Designation"),
                DepartName = x.Field<string>("dept_name"),
                Doj = x.Field<DateTime>("doj"),
                RelievingDate = x.Field<DateTime?>("dor"),
                Salary = x.Field<int>("Salary"),
                PayrollCompany = x.Field<string>("payroll_name")
            });
            ViewBag.Email = UserName;
            return View(empModel);
        }

        [HttpPost]
        public ActionResult SaveEmployeeDetails(EmployeeModel employeeDetails)
        {
            var retVal = SaveEmployee(employeeDetails);
            return retVal == 1
                ? (ActionResult) Json(retVal, JsonRequestBehavior.AllowGet)
                : RedirectToAction("EmployeeDetails", "Employee");
        }

        [HttpPost]
        public int SaveEmployee(EmployeeModel employeeDetails)
        {
            var empDetails = new EmployeeModel
            {
                WorkShopId = Convert.ToInt32(Session["WorkshopId"]),
                EmployeeId = employeeDetails.EmployeeId,
                EmployeeName = employeeDetails.EmployeeName,
                ContactNumber = employeeDetails.ContactNumber,
                DeptName = employeeDetails.DeptName,
                Desig = employeeDetails.Desig,
                Dob = DateTime.Parse(employeeDetails.Dob.ToString(CultureInfo.InvariantCulture)),
                Doj = DateTime.Parse(employeeDetails.Doj.ToString(CultureInfo.InvariantCulture)),
                AadharNumber = employeeDetails.AadharNumber,
                EmailAddress = employeeDetails.EmailAddress,
                RelievingDate = employeeDetails.RelievingDate,
                Qualification = employeeDetails.Qualification,
                Experience = employeeDetails.Experience,
                Salary = employeeDetails.Salary,
                PayrollId = employeeDetails.PayrollId
            };
            var returnVal = _helper.ExecuteInsertStatement("InsetEmpDetails", empDetails.EmployeeId,
                empDetails.EmployeeName, Convert.ToInt32(empDetails.Desig), empDetails.ContactNumber,
                empDetails.EmailAddress, empDetails.Dob, Convert.ToInt32(empDetails.DeptName), empDetails.AadharNumber,
                empDetails.Doj, empDetails.RelievingDate, empDetails.Qualification, empDetails.Experience,
                empDetails.Salary, empDetails.PayrollId, empDetails.WorkShopId);
            return returnVal;
        }

        [HttpGet]
        public ActionResult Edit(int? id = null)
        {
            if (id == null)
                return RedirectToAction("SaveEmployeeDetails");
            if (!ModelState.IsValid) return RedirectToAction("SaveEmployeeDetails");
            var query = "select * from m_departments";
            var desigQuery = "select * from emp_designation";
            var model = new EmployeeModel();
            var dsEditEmployee = /*_helper.FillDropDownHelperMethodWithSp("spEditEmployee");*/
                Session["GetEmployeeData"] as DataSet;
            var row = dsEditEmployee?.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == id);
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
                if (row["dor"] != DBNull.Value)
                    model.RelievingDate = Convert.ToDateTime(row["dor"]);
                model.Salary = Convert.ToInt32(row["Salary"]);
                model.PayrollCompany = row["payroll_name"].ToString();
                model.PayrollId = Convert.ToInt32(row["payroll_id"]);
            }
            var dtPayroll = Session["Payroll"] as DataTable;
            if (dtPayroll != null) model.Payroll = new SelectList(dtPayroll.AsDataView(), "payroll_id", "payroll_name");
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EmployeeModel postEmployee)
        {
            _helper.ExecuteUpdateStatement(postEmployee.Id, "spEditEmployee", postEmployee.EmployeeName,
                postEmployee.DesigEmp, postEmployee.ContactNumber, postEmployee.Dob, postEmployee.DeptEmp,
                postEmployee.AadharNumber, postEmployee.Doj, postEmployee.RelievingDate, postEmployee.Salary,
                postEmployee.PayrollId);
            return RedirectToAction("SaveEmployeeDetails");
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            _helper.ExecuteDeleteStatement("spDeleteEmployee", id);
            return RedirectToAction("SaveEmployeeDetails");
        }

        public ActionResult Infrastructure()
        {
            return View();
        }

        public ActionResult SaveInfraStructureDetails()
        {
            IEnumerable<InfraStructure> infastructureModel = null;
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View((IEnumerable<InfraStructure>) null);
            var query = "select * from m_infra_category";
            var dtinfra = _helper.ExecuteSelectStmt(query);
            ViewBag.categoryName = new SelectList(dtinfra.AsDataView(), "category_id", "category_name");
            var dsGetInfra = _helper.FillDropDownHelperMethodWithSp("spGetInfraDetails");
            Session["InfraTable"] = dsGetInfra;
            infastructureModel = dsGetInfra.Tables[0].AsEnumerable().ToList().Select(x => new InfraStructure
            {
                infra_id = x.Field<int>("infra_id"),
                InfraName = x.Field<string>("Infra_Name"),
                CategoryName = x.Field<string>("category_name"),
                Quantity = x.Field<int?>("quantity")
            }).ToList();
            return View(infastructureModel);
        }

        [HttpPost]
        public int SaveInfraStructureDetails(InfraStructure infraModel)
        {
            var workshopquery = "select workshop_id from m_employees where employeeId=" + Session["Employee_Id"] + "";
            var dtWorkshop = _helper.ExecuteSelectStmt(workshopquery);
            var workshopId = dtWorkshop.AsEnumerable().Select(x => x.Field<int>("workshop_id")).FirstOrDefault();
            var infraDetails = new InfraStructure
            {
                InfraName = infraModel.InfraName,
                CategoryId = infraModel.CategoryId,
                Quantity = infraModel.Quantity,
                WorkShopId = workshopId
            };
            var returnVal = _helper.ExecuteInsertInfraStatement("InsetInfraDetails", infraDetails.CategoryId,
                infraDetails.InfraName, infraDetails.Quantity, infraDetails.WorkShopId);
            return returnVal;
        }

        [HttpGet]
        public ActionResult EditInfra(int? id = null)
        {
            if (id == null) return RedirectToAction("SaveInfraStructureDetails");
            const string infraQuery = "select * from m_infra_category";
            var dtinfra = _helper.ExecuteSelectStmt(infraQuery);
            var model = new InfraStructure();
            var dsEditInfra = /*_helper.FillDropDownHelperMethodWithSp("spEditEmployee");*/
                Session["InfraTable"] as DataSet;
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

        [HttpPost]
        public ActionResult EditInfra(InfraStructure postInfra)
        {
            var infra = Convert.ToInt32(Session["Infra_id"]);
            _helper.ExecuteUpdateInfraStatement(infra, "spEditInfra", postInfra.InfraName, postInfra.CategoryId,
                postInfra.Quantity);
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

        public ActionResult PettyExpenses()
        {
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");
            int workShopId = Convert.ToInt32(Session["WorkshopId"]);
            string query = "select workshop_name from m_workshop where workshop_id =" + workShopId + "";
           DataTable dtworkshopName=_helper.ExecuteSelectStmt(query);
            ViewBag.WorkShopName = dtworkshopName.AsEnumerable().Select(x => x.Field<string>("workshop_name"))
                .FirstOrDefault();
            string typeExpenseQuery = "select * from m_PettyExpenseTypeHeads";
            DataTable dtTypeOfExpense = _helper.ExecuteSelectStmt(typeExpenseQuery);
            ViewBag.TypeOfExpense = new SelectList(dtTypeOfExpense.AsDataView(), "Id", "ExpenseType");
            return View();
        }
        [HttpPost]
        public ActionResult PettyExpenses(PettyExpenses expenses)
        {
            int workShopId = Convert.ToInt32(Session["WorkshopId"]);
           int result= _helper.ExecuteInsertPettyDetails("spInsertPettyExpenses", Convert.ToInt32(workShopId), Convert.ToInt32(expenses.TypeOfExpense),
                Convert.ToDateTime(expenses.Date), Convert.ToDecimal(expenses.Amount));
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}