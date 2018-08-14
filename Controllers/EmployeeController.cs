using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Fleet_WorkShop.Models;

namespace Fleet_WorkShop.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeHelper _helper = new EmployeeHelper();
        private readonly List<EmployeeModel> _empModel = new List<EmployeeModel>();
        private readonly EmployeeModel empModelDes = new EmployeeModel();

        //private readonly IEnumerable<EmployeeModel> empModel;

        public object UserName { get; set; }
        // GET: Employee
        public ActionResult GetEmployeeDetails()
        {
            
            return RedirectToAction("SaveEmployeeDetails");
        }
        
        public ActionResult SaveEmployeeDetails()
        {
            IEnumerable<EmployeeModel> empModel=null;
            if (ModelState.IsValid)
            {
                
                string query = "select * from m_departments";
                string desigQuery = "select * from emp_designation";
                string payroll = "select * from m_payroll";
                if (Session["Employee_Id"] == null)
                    return RedirectToAction("Login", "Account");
                DataTable dtDepartments = _helper.ExecuteSelectStmt(query);
                DataTable dtDesignation = _helper.ExecuteSelectStmt(desigQuery);
                DataTable dtPayroll= _helper.ExecuteSelectStmt(payroll);
                Session["Payroll"] = dtPayroll;
                ViewBag.DepartmentName = new SelectList(dtDepartments.AsDataView(), "dept_id", "dept_name");
                Session["DepartmentName"] = ViewBag.DepartmentName;
                ViewBag.Designation = new SelectList(dtDesignation.AsDataView(), "id", "Designation");
                Session["Designation"] = ViewBag.Designation;
                ViewBag.Payroll= new SelectList(dtPayroll.AsDataView(), "payroll_Id", "payroll_name");
                DataSet dsGetEmployee = _helper.FillDropDownHelperMethodWithSp("spGetEmployeeDetails");
                Session["GetEmployeeData"] = dsGetEmployee;
                empModel = dsGetEmployee.Tables[0].AsEnumerable().ToList().Select(x => new EmployeeModel {Id= x.Field<int>("Id"), EmployeeId = x.Field<string>("empId"), EmployeeName = x.Field<string>("name"), AadharNumber = x.Field<long?>("aadhar_no"), ContactNumber = x.Field<long?>("mobilenumber"), Dob = x.Field<DateTime>("dob"), EmpDesignation = x.Field<string>("Designation"), DepartName = x.Field<string>("dept_name"), Doj = x.Field<DateTime>("doj"), RelievingDate = x.Field<DateTime?>("dor"),Salary=x.Field<int>("Salary"), PayrollCompany = x.Field<string>("payroll_name") });
                empModel.ToList();
                ViewBag.Email = UserName;               
            }
            return View(empModel);
        }
        [HttpPost]
        public ActionResult SaveEmployeeDetails(EmployeeModel employeeDetails)
        {
           
                int retVal = SaveEmployee(employeeDetails);
                if (retVal == 1)
                    return Json("Hello", JsonRequestBehavior.AllowGet);              
            return RedirectToAction("EmployeeDetails", "Employee");
        }
        [HttpPost]
 
        public int SaveEmployee(EmployeeModel employeeDetails)
        {
            EmployeeModel _empDetails = new EmployeeModel()
            {
                WorkShopId = Convert.ToInt32(Session["WorkshopId"]),
                EmployeeId = employeeDetails.EmployeeId,
                EmployeeName = employeeDetails.EmployeeName,
                ContactNumber = employeeDetails.ContactNumber,
                DeptName = employeeDetails.DeptName,
                Desig = employeeDetails.Desig,
                Dob = DateTime.Parse(employeeDetails.Dob.ToString()),
                Doj = DateTime.Parse(employeeDetails.Doj.ToString()),
                AadharNumber = employeeDetails.AadharNumber,
                EmailAddress = employeeDetails.EmailAddress,
                RelievingDate = employeeDetails.RelievingDate,
                Qualification=employeeDetails.Qualification,
                Experience=employeeDetails.Experience,
                Salary=employeeDetails.Salary,
                PayrollId=employeeDetails.PayrollId

            };
          int returnVal=  _helper.ExecuteInsertStatement("InsetEmpDetails", _empDetails.EmployeeId, _empDetails.EmployeeName, Convert.ToInt32(_empDetails.Desig), _empDetails.ContactNumber, _empDetails.EmailAddress, _empDetails.Dob, Convert.ToInt32(_empDetails.DeptName), _empDetails.AadharNumber, _empDetails.Doj,_empDetails.RelievingDate,_empDetails.Qualification,_empDetails.Experience,_empDetails.Salary,_empDetails.PayrollId,_empDetails.WorkShopId);
            return returnVal;
        }

        [HttpGet]
        public ActionResult Edit(int? Id=null)
        {
            if(Id==null)
            {
                return RedirectToAction("SaveEmployeeDetails");
            }
            if (ModelState.IsValid)
            {
                string query = "select * from m_departments";
                string desigQuery = "select * from emp_designation";
                EmployeeModel model = new EmployeeModel();
                DataSet dsEditEmployee =  /*_helper.FillDropDownHelperMethodWithSp("spEditEmployee");*/Session["GetEmployeeData"] as DataSet;
                DataRow row = dsEditEmployee.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("Id") == Id);
                model.EmployeeName = row["name"].ToString();
                model.EmpDesignation = row["Designation"].ToString();
                model.ContactNumber = Convert.ToInt64(row["mobilenumber"]);
                model.Dob = Convert.ToDateTime(row["dob"]);
                model.DepartName = row["dept_name"].ToString();
                DataTable dtDepartments = _helper.ExecuteSelectStmt(query);
                DataTable dtDesignation = _helper.ExecuteSelectStmt(desigQuery);
                model.DepartmentName = new SelectList(dtDepartments.AsDataView(), "dept_id", "dept_name");
                model.Designation = new SelectList(dtDesignation.AsDataView(), "id", "Designation");
                model.Doj = Convert.ToDateTime(row["doj"]);
                model.AadharNumber = Convert.ToInt64(row["aadhar_no"]);
                if (row["dor"] != DBNull.Value)
                    model.RelievingDate = Convert.ToDateTime(row["dor"]);
                model.Salary = Convert.ToInt32(row["Salary"]);
                model.PayrollCompany = row["payroll_name"].ToString();
                model.PayrollId = Convert.ToInt32(row["payroll_id"]);
                DataTable dtPayroll = Session["Payroll"] as DataTable;
                model.Payroll = new SelectList(dtPayroll.AsDataView(), "payroll_id", "payroll_name");

                return View(model);
            }
            return RedirectToAction("SaveEmployeeDetails");
        }
        [HttpPost]
        public ActionResult Edit(EmployeeModel postEmployee)
        {

            _helper.ExecuteUpdateStatement(postEmployee.Id,"spEditEmployee",  postEmployee.EmployeeName, postEmployee.DesigEmp, postEmployee.ContactNumber, postEmployee.Dob, postEmployee.DeptEmp, postEmployee.AadharNumber, postEmployee.Doj,postEmployee.RelievingDate,postEmployee.Salary,postEmployee.PayrollId);
            return RedirectToAction("SaveEmployeeDetails");
        }
        [HttpDelete]
        public ActionResult Delete(int Id)
        {
            _helper.ExecuteDeleteStatement("spDeleteEmployee",Id);
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
            if (ModelState.IsValid)
            {

                string query = "select * from m_infra_category";


                DataTable dtinfra = _helper.ExecuteSelectStmt(query);
                ViewBag.categoryName = new SelectList(dtinfra.AsDataView(), "category_id", "category_name");

                DataSet dsGetInfra = _helper.FillDropDownHelperMethodWithSp("spGetInfraDetails");
                Session["InfraTable"] = dsGetInfra;
                infastructureModel = dsGetInfra.Tables[0].AsEnumerable().ToList().Select(x => new InfraStructure { infra_id = x.Field<int>("infra_id"), InfraName = x.Field<string>("Infra_Name"), CategoryName = x.Field<string>("category_name"), Quantity = x.Field<int?>("quantity") }).ToList();


            }
            return View(infastructureModel);
        }
        [HttpPost]

        public int SaveInfraStructureDetails(InfraStructure InfraModel)
        {
            string workshopquery = "select workshop_id from m_employees where employeeId=" + Session["Employee_Id"] + "";
           DataTable dtWorkshop= _helper.ExecuteSelectStmt(workshopquery);
           int workshopId= dtWorkshop.AsEnumerable().Select(x => x.Field<int>("workshop_id")).FirstOrDefault();
            InfraStructure InfraDetails = new InfraStructure()
            {
                InfraName = InfraModel.InfraName,
                CategoryId = InfraModel.CategoryId,
                Quantity = InfraModel.Quantity,
                WorkShopId=workshopId

            };
            int returnVal = _helper.ExecuteInsertInfraStatement("InsetInfraDetails", InfraDetails.CategoryId, InfraDetails.InfraName, InfraDetails.Quantity,InfraDetails.WorkShopId);
            return returnVal;
        }

        [HttpGet]
        public ActionResult EditInfra(int? Id = null)
        {
            if (Id == null)
            {
                return RedirectToAction("SaveInfraStructureDetails");
            }
            
            string infraQuery = "select * from m_infra_category";
            DataTable dtinfra = _helper.ExecuteSelectStmt(infraQuery);
          
            InfraStructure model = new InfraStructure();
            DataSet dsEditInfra =  /*_helper.FillDropDownHelperMethodWithSp("spEditEmployee");*/ Session["InfraTable"] as DataSet;
            DataRow row = dsEditInfra.Tables[0].AsEnumerable().ToList().Single(x => x.Field<int>("infra_id") == Id);
            model.infra_id = Convert.ToInt32(row["infra_id"]);
            Session["Infra_id"] = model.infra_id;
            model.InfraName = row["infra_name"].ToString();
            model.CategoryName = row["category_name"].ToString();
            model.Quantity = Convert.ToInt32(row["quantity"]);
            model.Category = new SelectList(dtinfra.AsDataView(), "category_id", "category_name");

            return View(model);
        }
        [HttpPost]
        public ActionResult EditInfra(InfraStructure postInfra)
        {
            int infra = Convert.ToInt32(Session["Infra_id"]);
            _helper.ExecuteUpdateInfraStatement(infra, "spEditInfra", postInfra.InfraName, postInfra.CategoryId, postInfra.Quantity);
            return RedirectToAction("SaveInfraStructureDetails");
        }

        public ActionResult InfraDelete(int id)
        {
            _helper.ExecuteDeleteStatement("spDeleteInfra", id);
            return RedirectToAction("SaveInfraStructureDetails");
        }
        public ActionResult CheckEmployeeId(int empId)
        {
            string list = null;
            if (ModelState.IsValid)
            {
                DataTable dtCheckIds = _helper.ExecuteSelectStmtusingSP("CheckEmployeeId", "@empid", empId.ToString());
                list = dtCheckIds.AsEnumerable().Select(x => x.Field<string>("empid")).FirstOrDefault();
                if (list == null)
                    return null;
            }

            return Json(list.ToString(), JsonRequestBehavior.AllowGet);
        }
    }
}