using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

namespace Fleet_WorkShop.Models
{
    public class EmployeeHelper
    {
        public DataTable ExecuteSelectStmt(string query)
        {
            var cs = ConfigurationManager.AppSettings["Str"];
            var dtSyncData = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(cs);
                connection.Open();
                var dataAdapter = new SqlDataAdapter {SelectCommand = new SqlCommand(query, connection)};
                dataAdapter.Fill(dtSyncData);
                TraceService(query);
                return dtSyncData;
            }
            catch (Exception ex)
            {
                TraceService("executeSelectStmt() " + ex + query);
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public int ExecuteInsertInfraStatement(string insertStmt, int CategoryId, string InfraName, int? Quantity,
            int workshopid)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@CategoryId", CategoryId);
                    comm.Parameters.AddWithValue("@InfraName", InfraName);
                    comm.Parameters.AddWithValue("@qty", Quantity);
                    comm.Parameters.AddWithValue("@workshopid", workshopid);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }


        public int ExecuteUpdateInfraStatement(int Infraid, string insertStmt, string InfraName, int catid, int? qty)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@id", Infraid);
                    comm.Parameters.AddWithValue("@Infraname", InfraName);
                    comm.Parameters.AddWithValue("@catid", catid);
                    comm.Parameters.AddWithValue("@Qty", qty);

                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteUpdateSparesMaster(int sparesId, string insertStmt, int manufacturerId, string partName,
            decimal cost)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@id", sparesId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@partname", partName);
                    comm.Parameters.AddWithValue("@cost", cost);

                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public DataTable ExecuteSelectStmtusingSP(string insertStmt, string parameterName1 = null,
            string parameterValue1 = null, string parameterName2 = null, string parameterValue2 = null,
            string parameterName3 = null, string parameterValue3 = null,string parameterName4 = null, string parameterValue4 = null,
            string parameterName5 = null, string parameterValue5 = null, string parameterName6 = null, string parameterValue6 = null)
        {
            var cs = ConfigurationManager.AppSettings["Str"];
            var dtSyncData = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(cs);
                connection.Open();
                var cmd = new SqlCommand(insertStmt, connection) {CommandType = CommandType.StoredProcedure};
                if (parameterValue1 != null) cmd.Parameters.AddWithValue(parameterName1, int.Parse(parameterValue1));
                if (parameterValue2 != null) cmd.Parameters.AddWithValue(parameterName2, int.Parse(parameterValue2));
                if (parameterValue3 != null) cmd.Parameters.AddWithValue(parameterName3, parameterValue3);
                if (parameterValue4 != null) cmd.Parameters.AddWithValue(parameterName4, int.Parse(parameterValue4));
                if (parameterValue5 != null) cmd.Parameters.AddWithValue(parameterName5, int.Parse(parameterValue5));
                if (parameterValue6 != null) cmd.Parameters.AddWithValue(parameterName6, int.Parse(parameterValue6));
                var dataAdapter = new SqlDataAdapter {SelectCommand = cmd};
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


        public int ExecuteInsertStmtusingSp(string insertStmt, string parameterName1 = null,
            string parameterValue1 = null, string parameterName2 = null, string parameterValue2 = null,
            string parameterName3 = null, string parameterValue3 = null, string parameterName4 = null, string parameterValue4 = null,
            string parameterName5 = null, string parameterValue5 = null, string parameterName6 = null, string parameterValue6 = null, string parameterName7 = null, string parameterValue7 = null)
        {
            var i = 0;
            var cs = ConfigurationManager.AppSettings["Str"];
            SqlConnection connection = null;
            try
            {
                
                connection = new SqlConnection(cs);
                connection.Open();
                var cmd = new SqlCommand(insertStmt, connection) { CommandType = CommandType.StoredProcedure };
                if (parameterValue1 != null) cmd.Parameters.AddWithValue(parameterName1, int.Parse(parameterValue1));
                if (parameterValue2 != null) cmd.Parameters.AddWithValue(parameterName2, int.Parse(parameterValue2));
                if (parameterValue3 != null) cmd.Parameters.AddWithValue(parameterName3, parameterValue3);
                if (parameterValue4 != null) cmd.Parameters.AddWithValue(parameterName4, int.Parse(parameterValue4));
                if (parameterValue5 != null) cmd.Parameters.AddWithValue(parameterName5, int.Parse(parameterValue5));
                if (parameterValue6 != null) cmd.Parameters.AddWithValue(parameterName6, int.Parse(parameterValue6));
                if (parameterValue7 != null) cmd.Parameters.AddWithValue(parameterName7, int.Parse(parameterValue7));
                i = cmd.ExecuteNonQuery();
                TraceService(insertStmt);
                return i;
            }
            catch (Exception ex)
            {
                TraceService(" executeInsertStatement " + ex + insertStmt);
                return i;
            }
            finally
            {
                connection.Close();
            }
        }
        internal int ExecuteInsertSparePartsMasterDetails(string insertStmt, int manufacturerId, string partName,
            string partNumber, decimal cost, int ScrapBinId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@partname", partName);
                    comm.Parameters.AddWithValue("@partnumber", partNumber);
                    comm.Parameters.AddWithValue("@cost", cost);
                    comm.Parameters.AddWithValue("@scrapbinid", ScrapBinId);

                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteUpdateLubesMaster(int lubesId, string insertStmt, int manufacturerId, string oilName,
            decimal costPerLitre)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@oilname", oilName);
                    comm.Parameters.AddWithValue("@costPerlitre", costPerLitre);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@id", lubesId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal DataSet getCost(string commandText, int estimatedCost)
        {
            var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]);
            var ds = new DataSet();
            conn.Open();
            var cmd = new SqlCommand
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = commandText
            };
            if (estimatedCost != 0)
                cmd.Parameters.AddWithValue("@serviceid", estimatedCost);
            var da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        internal int ExecuteInsertJobCardDetails(string insertStmt, int districtId, int vehId, DateTime dateOfRepair,
            int modelNumber, int odometer, string receivedLocation, string pilotId, string pilotName,
            DateTime dateOfDelivery, int natureOfComplaint, int approximateCost, int allotedmechanic, int workshopid,
            int serviceEngineer, int laborCharges, int categoryid, int subCategory, int manufacturerId,int rmid,int pmid,int emeid)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@districtid", districtId);
                    comm.Parameters.AddWithValue("@vehicleId", vehId);
                    comm.Parameters.AddWithValue("@dor", dateOfRepair);
                    comm.Parameters.AddWithValue("@model", modelNumber);
                    comm.Parameters.AddWithValue("@odometer", odometer);
                    comm.Parameters.AddWithValue("@receivedlocation", receivedLocation);
                    comm.Parameters.AddWithValue("@pilotid", pilotId);
                    comm.Parameters.AddWithValue("@pilotname", pilotName);
                    comm.Parameters.AddWithValue("@dateofdelivary", dateOfDelivery);
                    comm.Parameters.AddWithValue("@natureofcomplaint", natureOfComplaint);
                    comm.Parameters.AddWithValue("@approxcost", approximateCost);
                    comm.Parameters.AddWithValue("@allotedmechanic", allotedmechanic);
                    comm.Parameters.AddWithValue("@workshopid", workshopid);
                    comm.Parameters.AddWithValue("@serviceengineer", serviceEngineer);
                    comm.Parameters.AddWithValue("@laborcharges", laborCharges);
                    comm.Parameters.AddWithValue("@categories", categoryid);
                    comm.Parameters.AddWithValue("@subcategory", subCategory);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@rmid", rmid);
                    comm.Parameters.AddWithValue("@pmid", pmid);
                    comm.Parameters.AddWithValue("@emeid", emeid);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertPOManufacturerDetails(string insertStmt, string poNumber, int manufacturerId,
            int sparePartId, decimal unitPrice, int quantity, decimal amount)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@ponumber", poNumber);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@sparepartid", sparePartId);
                    comm.Parameters.AddWithValue("@unitprice", unitPrice);
                    comm.Parameters.AddWithValue("@quantity", quantity);
                    comm.Parameters.AddWithValue("@amount", amount);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecutePODetails(string insertStmt, string poNumber, DateTime poDate, int employeeId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@ponumber", poNumber);
                    comm.Parameters.AddWithValue("@podate", poDate);
                    comm.Parameters.AddWithValue("@employeeid", employeeId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int UpdateSparePartsPoDetails(string insertStmt, int quantity, DateTime billDate, int manufacturerId, int sparePartId, string poNumber)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@quantity", quantity);
                    comm.Parameters.AddWithValue("@billdate", billDate);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@sparepartid", sparePartId);
                    comm.Parameters.AddWithValue("@ponumber", poNumber);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertStockDetails(string insertStmt, int workShopId, int manufacturerId, int sparePartId,
            int unitPrice, int quantity, long receiptId, string billNumber, int vendorId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@workshopid", workShopId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@sparepartid", sparePartId);
                    comm.Parameters.AddWithValue("@unitprice", unitPrice);
                    comm.Parameters.AddWithValue("@quantity", quantity);
                    comm.Parameters.AddWithValue("@receiptid", receiptId);
                    comm.Parameters.AddWithValue("@billnumber", billNumber);
                    comm.Parameters.AddWithValue("@vendorid", vendorId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int InsertJobAggregateDetails(string insertStmt, int manufacturerId, string aggregateName, int v2)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@createdby", v2);
                    comm.Parameters.AddWithValue("@manufacturerId", manufacturerId);
                    comm.Parameters.AddWithValue("@aggregateName", aggregateName);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal void ExecuteDeleteAggregates(string v, int? id)
        {
            throw new NotImplementedException();
        }

        internal int ExecuteJobAggregateUpdateStatement(string insertStmt, int manufacturerId, string aggregateName,
            int id)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@manufacturerId", manufacturerId);
                    comm.Parameters.AddWithValue("@aggregateName", aggregateName);
                    comm.Parameters.AddWithValue("@id", id);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int InsertJobCategoryDetails(string insertStmt, int v2, string categoryName)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@aggregateId", v2);
                    comm.Parameters.AddWithValue("@categoryname", categoryName);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService("executeInsertStatement" + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteUpdateOutSourcingJobDetails(int vehicleId, string insertStmt, string vendor,
            string workOrder, string jobWork, DateTime completedDate, int outSourcingStatus, decimal amount)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@vehicleid", vehicleId);
                    comm.Parameters.AddWithValue("@vendorname", vendor);
                    comm.Parameters.AddWithValue("@jobwork", jobWork);
                    comm.Parameters.AddWithValue("@workorder", workOrder);
                    comm.Parameters.AddWithValue("@completeddate", completedDate);
                    comm.Parameters.AddWithValue("@outsourcingstatus", outSourcingStatus);
                    comm.Parameters.AddWithValue("@amount", amount);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertLubeStockDetails(string insertStmt, int workShopId, int manufacturerId,
            int lubricantId, int unitPrice, int quantity, long receiptId, string billNo, int vendorId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@workshopid", workShopId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@lubricantid", lubricantId);
                    comm.Parameters.AddWithValue("@unitprice", unitPrice);
                    comm.Parameters.AddWithValue("@quantity", quantity);
                    comm.Parameters.AddWithValue("@receiptid", receiptId);
                    comm.Parameters.AddWithValue("@billnumber", billNo);
                    comm.Parameters.AddWithValue("@vendorid", vendorId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int InsertJobSubCategoriesDetails(string insertStmt, int manufacturerId, int serviceGroupId,
            string serviceName, int timeTaken, int v2)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@servicegroupid", serviceGroupId);
                    comm.Parameters.AddWithValue("@servicename", serviceName);
                    comm.Parameters.AddWithValue("@timetaken", timeTaken);
                    comm.Parameters.AddWithValue("@createdby", v2);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteJobSubCategoryUpdateStatement(string insertStmt, int manufacturerId, int serviceGroupId,
            string serviceName, int timeTaken, int v2)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@manufacturerId", manufacturerId);
                    comm.Parameters.AddWithValue("@servicegroupid", serviceGroupId);
                    comm.Parameters.AddWithValue("@servicename", serviceName);
                    comm.Parameters.AddWithValue("@timetaken", timeTaken);
                    comm.Parameters.AddWithValue("@ServiceId", v2);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertSparesIssueStatement(string insertStmt, string vehicleNumber, int workShopId,int sparePartId, int quantity, decimal total, int handOverToId, int jobcardId, string status,string billNumber=null)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@WorkshopId", workShopId);
                    comm.Parameters.AddWithValue("@VehicleNumber", vehicleNumber);
                    comm.Parameters.AddWithValue("@SparePartId", sparePartId);
                    comm.Parameters.AddWithValue("@Quantity", quantity);
                    comm.Parameters.AddWithValue("@TotalAmount", total);
                    comm.Parameters.AddWithValue("@HandOverTo", handOverToId);
                    comm.Parameters.AddWithValue("@jobcardid", jobcardId);
                    comm.Parameters.AddWithValue("@status", status);
                    if (billNumber != null)
                        comm.Parameters.AddWithValue("@billnumber", billNumber);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal DataSet FillDropDownHelperMethodWithSp1(string commandText, int aggregateId)
        {
            var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]);
            var ds = new DataSet();
            conn.Open();
            var cmd = new SqlCommand
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = commandText
            };
            if (aggregateId != 0)
                cmd.Parameters.AddWithValue("@servicegroupid", aggregateId);
            var da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        internal DataSet FillDropDownHelperMethodWithSp2(string commandText, int manufacturerId)
        {
            var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]);
            var ds = new DataSet();
            conn.Open();
            var cmd = new SqlCommand
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = commandText
            };
            if (manufacturerId != 0)
                cmd.Parameters.AddWithValue("@manufacturerid", manufacturerId);
            var da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        internal DataSet FillDropDownHelperMethodWithSp3(string commandText, int aggregateId)
        {
            var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]);
            var ds = new DataSet();
            conn.Open();
            var cmd = new SqlCommand
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = commandText
            };
            if (aggregateId != 0)
                cmd.Parameters.AddWithValue("@aggregateid", aggregateId);
            var da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        internal int ExecuteInsertLubesIssueStatement(string insertStmt, string vehicleNumber, int workShopId,
            int LubricantId, int quantity, decimal total, int handOverToId, int JobcardId, string status)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@WorkshopId", workShopId);
                    comm.Parameters.AddWithValue("@VehicleNumber", vehicleNumber);
                    comm.Parameters.AddWithValue("@LubricantId", LubricantId);
                    comm.Parameters.AddWithValue("@Quantity", quantity);
                    comm.Parameters.AddWithValue("@TotalAmount", total);
                    comm.Parameters.AddWithValue("@HandOverTo", handOverToId);
                    comm.Parameters.AddWithValue("@jobcardid", JobcardId);
                    comm.Parameters.AddWithValue("@status", status);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteUpdateSparesIssueStatement(string insertStmt, long receiptId, int updatedQuantity)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@quantity", updatedQuantity);
                    comm.Parameters.AddWithValue("@receipt_id", receiptId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertLubesMasterDetails(string insertStmt, int manufacturerId, string oilName,
            decimal costPerLitre, string LubricantNumber, int IsActive)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@oilname", oilName);
                    comm.Parameters.AddWithValue("@costperlitre", costPerLitre);
                    comm.Parameters.AddWithValue("@lubricantnumber", LubricantNumber);
                    comm.Parameters.AddWithValue("@isactive", IsActive);

                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal void ExecuteJobSubCategoryUpdateCost(string insertStmt, int? approxCost, int subcategoryId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@cost", Convert.ToInt32(approxCost));
                    comm.Parameters.AddWithValue("@subcategoryid", Convert.ToInt32(subcategoryId));
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public void TraceService(string content)
        {
            var str = @"C:\smslog_1\Log.txt";
            var path1 = str.Substring(0, str.LastIndexOf("\\", StringComparison.Ordinal));
            var path2 = str.Substring(0, str.LastIndexOf(".txt", StringComparison.Ordinal)) + "-" +
                        DateTime.Today.ToString("yyyy-MM-dd") + ".txt";
            try
            {
                if (!Directory.Exists(path1)) Directory.CreateDirectory(path1);
                if (path2.Length >= Convert.ToInt32(4000000))
                    path2 = str.Substring(0, str.LastIndexOf(".txt", StringComparison.Ordinal)) + "-" + "2" + ".txt";
                var streamWriter = File.AppendText(path2);
                streamWriter.WriteLine("====================" + DateTime.Now.ToLongDateString() + "  " +
                                       DateTime.Now.ToLongTimeString());
                streamWriter.WriteLine(content);
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch
            {
                // traceService(ex.ToString());
            }
        }


        internal int ExecuteJobUpdateStatement(int districtId, string insertStmt, int vehId, int modelYear,
            int odometer, int approximateCost, DateTime dateOfRepair, int NatureOfComplaint, int Id)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@districtid", districtId);
                    comm.Parameters.AddWithValue("@vehicleid", vehId);
                    comm.Parameters.AddWithValue("@model", modelYear);
                    comm.Parameters.AddWithValue("@odometer", odometer);
                    comm.Parameters.AddWithValue("@approxcost", approximateCost);
                    comm.Parameters.AddWithValue("@dor", dateOfRepair);
                    comm.Parameters.AddWithValue("@natureofcomplaint", NatureOfComplaint);
                    comm.Parameters.AddWithValue("@jobcardnumber", Id);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertInventoryDetails(string insertStmt, string billnumber, int manufacturerid,
            int sparepartid, decimal unitprice, int quantity, decimal amount, int vendorId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@billnumber", billnumber);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerid);
                    comm.Parameters.AddWithValue("@sparepartid", sparepartid);
                    comm.Parameters.AddWithValue("@unitprice", unitprice);
                    comm.Parameters.AddWithValue("@quantity", quantity);
                    comm.Parameters.AddWithValue("@amount", amount);
                    comm.Parameters.AddWithValue("@vendorid", vendorId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }


        internal void UpdateTotalBillDetails(DataTable dtGetTotalAmount)
        {
            var stocks = new SparePartStocks();
            var rows = dtGetTotalAmount.AsEnumerable().ToList();
            foreach (var row in rows)
            {
                stocks.TotalAmount = Convert.ToDecimal(row["TotalBill"]);
                var billnumber = row["BillNumber"].ToString();
                var updatebillQuery = "update t_receipts set BillAmount = " + stocks.TotalAmount +
                                      " where BillNumber = '" + billnumber + "'";
                ExecuteTotalBillUpdateStatement(updatebillQuery);
            }
        }

        internal int ExecuteUpdateInventoryStocksStatement(int workshopId, int manufacturerId, string insertStmt,
            int sparePartId, decimal uprice, int qty, long receiptId, string billNumber, int vendorId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@workshopid", workshopId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@sparepartsid", sparePartId);
                    comm.Parameters.AddWithValue("@unitprice", uprice);
                    comm.Parameters.AddWithValue("@quantity", qty);
                    comm.Parameters.AddWithValue("@receiptid", receiptId);
                    comm.Parameters.AddWithValue("@billnumber", billNumber);
                    comm.Parameters.AddWithValue("@vendorid", vendorId);

                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal void UpdateTotalBillDetailsLubes(DataTable dtGetTotalAmount)
        {
            var stocks = new SparePartStocks();
            var rows = dtGetTotalAmount.AsEnumerable().ToList();
            foreach (var row in rows)
            {
                stocks.TotalAmount = Convert.ToDecimal(row["TotalBill"]);
                var billnumber = row["BillNumber"].ToString();
                var updatebillQuery = "update t_lubesreceipts set BillAmount = " + stocks.TotalAmount +
                                      " where BillNumber = '" + billnumber + "'";
                ExecuteTotalBillUpdateStatement(updatebillQuery);
            }
        }


        public void InsertStockDetals(DataTable dtQuantity, string filter = null)
        {
            var stocks = new SparePartStocks();
            var rows = dtQuantity.AsEnumerable().ToList();
            foreach (var row in rows)
            {
                stocks.ManufacturerId = Convert.ToInt32(row["manufacturerid"]);
                stocks.WorkShopId = Convert.ToInt32(row["workshopid"]);
                stocks.SparePartId = Convert.ToInt32(row["sparepartid"]);
                stocks.TotalAmount = Convert.ToDecimal(row["totalamount"]);
                stocks.Quantity = Convert.ToInt32(row["Totalquantity"]);
                if (filter == null)
                    ExecuteInsertStockDetails("spSparePartStockDetails", stocks.WorkShopId, stocks.ManufacturerId,
                        stocks.SparePartId, stocks.Quantity, stocks.TotalAmount);
                else
                    ExecuteInsertStockDetails("spSparePartsUpdateDetails", stocks.WorkShopId, stocks.ManufacturerId,
                        stocks.SparePartId, stocks.Quantity, stocks.TotalAmount);
            }
        }

        public void InsertLubricantStockDetails(DataTable dtQuantity, string filter = null)
        {
            var stocks = new SparePartStocks();
            var rows = dtQuantity.AsEnumerable().ToList();
            foreach (var row in rows)
            {
                stocks.ManufacturerId = Convert.ToInt32(row["manufacturerid"]);
                stocks.WorkShopId = Convert.ToInt32(row["workshopid"]);
                stocks.LubricantId = Convert.ToInt32(row["lubricantid"]);
                stocks.TotalAmount = Convert.ToDecimal(row["totalamount"]);
                stocks.Quantity = Convert.ToInt32(row["Totalquantity"]);
                if (filter == null)
                    ExecuteInsertLubesSummaryDetails("spLubesStockDetails", stocks.WorkShopId, stocks.ManufacturerId,
                        stocks.LubricantId, stocks.Quantity, stocks.TotalAmount);
                else
                    ExecuteInsertLubesSummaryDetails("spLubesUpdateDetails", stocks.WorkShopId, stocks.ManufacturerId,
                        stocks.LubricantId, stocks.Quantity, stocks.TotalAmount);
            }
        }

        internal int ExecuteUpdateLubesStatement(int workShopId, int manufacturerId, string insertStmt, int lubricantId,
            decimal uprice, int qty, long receiptId, string billNo, int vendorId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@workshopid", workShopId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@lubricantid", lubricantId);
                    comm.Parameters.AddWithValue("@unitprice", uprice);
                    comm.Parameters.AddWithValue("@quantity", qty);
                    comm.Parameters.AddWithValue("@billnumber", billNo);
                    comm.Parameters.AddWithValue("@vendorid", vendorId);
                    comm.Parameters.AddWithValue("@receiptid", receiptId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertLubesSummaryDetails(string insertStmt, int workShopId, int manufacturerId,
            int lubricantid, int quantity, decimal totalAmount)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@workshopid", workShopId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@lubricantid", lubricantid);
                    comm.Parameters.AddWithValue("@quantity", quantity);
                    comm.Parameters.AddWithValue("@totalamount", totalAmount);

                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertStockDetails(string insertStmt, int workShopId, int manufacturerId, int sparePartId,
            int quantity, decimal totalAmount)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@workshopid", workShopId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@sparepartid", sparePartId);
                    comm.Parameters.AddWithValue("@quantity", quantity);
                    comm.Parameters.AddWithValue("@totalamount", totalAmount);

                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertLubesDetails(string insertStmt, string billnumber, int manufacturerid, int Lubesid,
            decimal unitprice, int quantity, decimal amount, int vendorId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@billnumber", billnumber);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerid);
                    comm.Parameters.AddWithValue("@lubricantid", Lubesid);
                    comm.Parameters.AddWithValue("@unitprice", unitprice);
                    comm.Parameters.AddWithValue("@quantity", quantity);
                    comm.Parameters.AddWithValue("@amount", amount);
                    comm.Parameters.AddWithValue("@vendorid", vendorId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteTotalBillUpdateStatement(string query)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand(query, conn))
                {
                    var i = 0;
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }


        internal int ExecuteUpdateInventoryStatement(int manufacturerId, string insertStmt, int sparePartId,
            decimal uprice, int qty, decimal amt, decimal billAmount, string billNo, int id, int vendorId)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@sparepartsid", sparePartId);
                    comm.Parameters.AddWithValue("@unitprice", uprice);
                    comm.Parameters.AddWithValue("@quantity", qty);
                    comm.Parameters.AddWithValue("@amount", amt);
                    comm.Parameters.AddWithValue("@id", id);
                    comm.Parameters.AddWithValue("@billamount", billAmount);
                    comm.Parameters.AddWithValue("@billnumber", billNo);
                    comm.Parameters.AddWithValue("@vendorid", vendorId);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteUpdateLubesStatement(int manufacturerId, string insertStmt, int LubricantId, decimal uprice,
            int qty, decimal amt, decimal billAmount, string billNo, int id, DateTime billdate, int vendorid)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@lubricantid", LubricantId);
                    comm.Parameters.AddWithValue("@unitprice", uprice);
                    comm.Parameters.AddWithValue("@quantity", qty);
                    comm.Parameters.AddWithValue("@amount", amt);
                    comm.Parameters.AddWithValue("@id", id);
                    comm.Parameters.AddWithValue("@billamount", billAmount);
                    comm.Parameters.AddWithValue("@billnumber", billNo);
                    comm.Parameters.AddWithValue("@billdate", billdate);
                    comm.Parameters.AddWithValue("@vendorid", vendorid);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }


        internal int ExecuteBillDetails(string insertStmt, string billnumber, DateTime billdate, decimal billamount,
            int vendorid, string poNumber, DateTime poDate, int workshopid)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@billnumber", billnumber);
                    comm.Parameters.AddWithValue("@billdate", billdate);
                    comm.Parameters.AddWithValue("@billamount", billamount);
                    comm.Parameters.AddWithValue("@vendorid", vendorid);
                    comm.Parameters.AddWithValue("@ponumber", poNumber);
                    comm.Parameters.AddWithValue("@podate", poDate);
                    comm.Parameters.AddWithValue("@workshopid", workshopid);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public int ExecuteUpdateStatement(int id, string insertStmt, string empName, int DesgID, long? mobileNumber,
            DateTime DOB, int deptID, long? Aadhar, DateTime DOJ, DateTime? DOR, int salary, int payroll)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@id", id);
                    comm.Parameters.AddWithValue("@name", empName);
                    comm.Parameters.AddWithValue("@designation", DesgID);
                    comm.Parameters.AddWithValue("@mobileNumber", mobileNumber);
                    comm.Parameters.AddWithValue("@dob", DOB);
                    comm.Parameters.AddWithValue("@department", deptID);
                    comm.Parameters.AddWithValue("@aadhar", Aadhar);
                    comm.Parameters.AddWithValue("@doj", DOJ);
                    comm.Parameters.AddWithValue("@dor", DOR);
                    comm.Parameters.AddWithValue("@salary", salary);
                    comm.Parameters.AddWithValue("@payroll", payroll);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteInsertVehicleDetails(string insertStmt, string vehicleId, int manufacturerId,
            int districtId, string model, string chasisNumber, string engineNumber, string locationOfCommission,
            DateTime? DOC)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@vehicleid", vehicleId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@districtid", districtId);
                    comm.Parameters.AddWithValue("@model", model);
                    comm.Parameters.AddWithValue("@chassisnumber", chasisNumber);
                    comm.Parameters.AddWithValue("@enginemumber", engineNumber);
                    comm.Parameters.AddWithValue("@loc", locationOfCommission);
                    comm.Parameters.AddWithValue("@doc", DOC);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }


        public DateTime CheckDBNull(object dateTime)
        {
            if (dateTime == DBNull.Value)
                return DateTime.MinValue;
            return (DateTime) dateTime;
        }

        public int ExecuteInsertStatement(string insertStmt, string empId, string empName, int DesgID,
            long? mobileNumber, string email, DateTime DOB, int deptID, long? Aadhar, DateTime DOJ,
            DateTime? ReleivingDate, string Qualification, int Experience, int Salary, int Payroll, int workshopid)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@empId", empId);
                    comm.Parameters.AddWithValue("@Name", empName);
                    comm.Parameters.AddWithValue("@DesgID", DesgID);
                    comm.Parameters.AddWithValue("@mobileNumber", mobileNumber);
                    comm.Parameters.AddWithValue("@email", email);
                    comm.Parameters.AddWithValue("@DOB", DOB);
                    comm.Parameters.AddWithValue("@department", deptID);
                    comm.Parameters.AddWithValue("@aadhar_no", Aadhar);
                    comm.Parameters.AddWithValue("@DOJ", DOJ);
                    comm.Parameters.AddWithValue("@DOR", ReleivingDate);
                    comm.Parameters.AddWithValue("@qualification", Qualification);
                    comm.Parameters.AddWithValue("@experience", Experience);
                    comm.Parameters.AddWithValue("@salary", Salary);
                    comm.Parameters.AddWithValue("@payroll", Payroll);
                    comm.Parameters.AddWithValue("@workshopid", workshopid);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal int ExecuteVehicleUpdateStatement(int id, string insertStmt, int districtId, int manufacturerId,
            string vehId, int modelYear, string chasisNumber, string engineNumber, string locationOfCommission,
            DateTime?DOC)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@id", id);
                    comm.Parameters.AddWithValue("@districtid", districtId);
                    comm.Parameters.AddWithValue("@manufacturerid", manufacturerId);
                    comm.Parameters.AddWithValue("@vehicleid", vehId);
                    comm.Parameters.AddWithValue("@model", modelYear);
                    comm.Parameters.AddWithValue("@chasisnumber", chasisNumber);
                    comm.Parameters.AddWithValue("@enginenumber", engineNumber);
                    comm.Parameters.AddWithValue("@loc", locationOfCommission);
                    comm.Parameters.AddWithValue("@doc", DOC);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public void ErrorsEntry(Exception ex)
        {
            var appSetting = ConfigurationManager.AppSettings["LogLocation"];
            if (appSetting == null) throw new ArgumentNullException(nameof(appSetting));
            var path = appSetting.Substring(0, appSetting.LastIndexOf("\\", StringComparison.Ordinal));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using (var streamWriter = File.AppendText(ConfigurationManager.AppSettings["LogLocation"]))
            {
                var trace = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = trace.GetFrame(0);
                if (frame == null) throw new ArgumentNullException(nameof(frame));
                // Get the line number from the stack frame
                var errorNo = frame.GetFileLineNumber();
                //Get  Error Source
                var errorSource = ex.Source;
                if (errorSource == null) throw new ArgumentNullException(nameof(errorSource));
                //Get Error Description
                var errorDescription = ex.Message;
                streamWriter.WriteLine("====================" + DateTime.Now.ToLongDateString() + "  " +
                                       DateTime.Now.ToLongTimeString());
                streamWriter.WriteLine(errorDescription);
                streamWriter.WriteLine(errorSource);
                streamWriter.WriteLine(errorNo.ToString());
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        internal void ExecuteDeleteStatement(string spDelete, int? id)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = spDelete;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@id", id);
                    try
                    {
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        internal void ExecuteDeleteInvBillNumberStatement(string query)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = query;
                    try
                    {
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public void FillDropDownHelperMethodWithDataTable(DataTable table, string textFieldValue, string valueField,
            DropDownList dropdownId = null, DropDownList dropdownId1 = null)
        {
            dropdownId.Items.Clear();
            dropdownId.DataSource = table;
            dropdownId.DataTextField = textFieldValue;
            dropdownId.DataValueField = valueField;
            dropdownId.DataBind();
            dropdownId.Items.Insert(0, new ListItem("--Select--", "0"));
            dropdownId.Items[0].Value = "0";
            dropdownId.SelectedIndex = 0;
        }

        public DataSet FillDropDownHelperMethodWithSp(string commandText, int districtId = 0)
        {
            var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]);
            var ds = new DataSet();
            conn.Open();
            var cmd = new SqlCommand
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = commandText
            };
            if (districtId != 0)
                cmd.Parameters.AddWithValue("@districtId", districtId);
            var da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        public DataSet FillDropDownHelperMethodWithSpCategory(string commandText, int districtId = 0)
        {
            var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]);
            var ds = new DataSet();
            conn.Open();
            var cmd = new SqlCommand
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = commandText
            };
            if (districtId != 0)
                cmd.Parameters.AddWithValue("@categoryid", districtId);
            var da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        public DataSet FillModelNumbers(string commandText, int vehicleId = 0)
        {
            if (vehicleId == 0) return null;
            var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]);
            var ds = new DataSet();
            conn.Open();
            var cmd = new SqlCommand
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = commandText
            };
            if (vehicleId != 0)
                cmd.Parameters.AddWithValue("@id", vehicleId);
            var da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        public int ExecuteInsertPettyDetails(string insertStmt, int workShopId, int expensesTypeOfExpense, DateTime expensesDate, decimal expensesAmount)
        {
            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["Str"]))
            {
                using (var comm = new SqlCommand())
                {
                    var i = 0;
                    comm.Connection = conn;
                    comm.CommandText = insertStmt;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@workshopId", workShopId);
                    comm.Parameters.AddWithValue("@type0fexpense", expensesTypeOfExpense);
                    comm.Parameters.AddWithValue("@date", expensesDate);
                    comm.Parameters.AddWithValue("@amount", expensesAmount);
                    try
                    {
                        conn.Open();
                        i = comm.ExecuteNonQuery();
                        TraceService(insertStmt);
                        return i;
                    }
                    catch (SqlException ex)
                    {
                        TraceService(" executeInsertStatement " + ex + insertStmt);
                        return i;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
    }
}