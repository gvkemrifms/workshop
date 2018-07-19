using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Fleet_WorkShop.WorkShpModels
{
    public enum SignInStatus
    {
        Success,
        Failure,
    }
    public class Helper
    {
        public string Email{ get;set; }
        public string Password{ get;set; }
        public DataTable ExecuteSelectStmt(string query)
        {
            var cs = ConfigurationManager.AppSettings["Str"];
            var dtSyncData = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(cs);
                connection.Open();
                var dataAdapter = new SqlDataAdapter { SelectCommand = new SqlCommand(query, connection) };
                dataAdapter.Fill(dtSyncData);
                return dtSyncData;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
        }
    }
   
}