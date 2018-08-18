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
             public DataTable ExecuteSelectStmtForDateTime(string insertStmt, string parameterName1 = null, string parameterValue1 = null, string parameterName2 = null, string parameterValue2 = null, string parameterName3 = null, string parameterValue3 = null)
        {
            var cs = ConfigurationManager.AppSettings["Str"];
            var dtSyncData = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(cs);
                connection.Open();
                SqlCommand cmd = new SqlCommand(insertStmt, connection);
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameterValue1 != null) cmd.Parameters.AddWithValue(parameterName1, DateTime.Parse(parameterValue1));
                if (parameterValue2 != null) cmd.Parameters.AddWithValue(parameterName2, DateTime.Parse(parameterValue2));
                if (parameterValue3 != null) cmd.Parameters.AddWithValue(parameterName3, parameterValue3);
                var dataAdapter = new SqlDataAdapter { SelectCommand = cmd };
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
   
