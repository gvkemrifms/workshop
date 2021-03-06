﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Fleet_WorkShop.Models
{
    public class EmpModels
    {
        [Required(ErrorMessage = "Payroll is Required")]
        public int PayrollId { get; set; }

    }
    public class EmployeeModel
    {
        [Required(ErrorMessage = "Payroll is Required")]
        public int PayrollId { get; set; }
        public SelectList Payroll { get; set; }
        public int WorkShopId { get; set; }
        [DisplayName("ID")]
        public int Id { get; set; }
        public int StatusId { get; set; }
        public int DesigEmp { get; set; }
        public int TransferId { get; set; }
        public int DeptEmp { get; set; }
        [DisplayName("Employee ID")]
        //public int EmpId { get; set; }
        [Required(ErrorMessage = "EmployeeId is Required")]
        public string EmployeeId { get; set; }
        [Required(ErrorMessage = "Name is Required")]
        [DisplayName("Employee Name")]
        public string  EmployeeName{ get;set; }
        [Required(ErrorMessage ="Mobile Number is Required")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid.")]
        public long? ContactNumber{ get;set; }
        [DisplayName("Email")]
        public string EmailAddress{ get;set; }
        [Required(ErrorMessage ="Birth Date  is Required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DisplayName("Birth Date")]
        public DateTime Dob { get;set; }

        [DisplayName("Department")]
        public SelectList DepartmentName { get;set; }

        public SelectList Designation { get;set; }
        [Required(ErrorMessage ="Aadhar Number is Required")]
        [Display(Name="Aadhar")]
        [Range(100000000000, 999999999999,ErrorMessage = "Please enter Valid Aadhar Number")]
        public long? AadharNumber{ get;set; }

        [Required(ErrorMessage = "Joining Date is Required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        [DisplayName("Joining Date")]
        public DateTime Doj{ get;set; }
        [Required(ErrorMessage = "Department is Required")]
        public int DeptName { get; set; }
        [Display(Name ="Department")]
     
        public string DepartName { get; set; }
        [Display(Name = "Designation")]
       
        public string EmpDesignation { get; set; }
        [Required(ErrorMessage = "Designation is Required")]
        public int Desig { get; set; }
        [DisplayName("Relieving Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? RelievingDate { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? TransferredDate { get; set; }
        [Required(ErrorMessage = "Qualification is Required")]
        public string Qualification { get; set; }
        [Required(ErrorMessage = "Experience is Required")]
        public int Experience { get; set; }
        [Required(ErrorMessage = "Salary is Required")]
        public int Salary { get; set; }

        [Display(Name = "Payroll")]
        public string PayrollCompany { get; set; }



    }

    public class EmpDepatmentDetails
    {
        public IEnumerable<SelectListItem> DepartmentName { get; set; }
        public IEnumerable<SelectListItem> Designation { get; set; }
    }

    public class PettyExpenses
    {
        //public int WorkShopId { get; set; }
        public string WorkShopName { get; set; }
        public int TypeOfExpense { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string BillNumber { get; set; }
        public int paymentType { get; set; }
    }

    public class VendorModel
    {
        public string VendorName { get; set; }
        public long ContactNumber { get; set; }
        public string EmailAddress { get; set; }
        public int Districts { get; set; }
        public int VendorType { get; set; }
        public string GstNumber { get; set; }
        public string PanNumber { get; set; }
        public string Location { get; set; }
    }
}