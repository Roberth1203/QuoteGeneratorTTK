using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Utilities
{
    public class DBFunctions
    {
        //public DataTable ExecSP(string ConnS, string Sp, List<Params> lParams)
        //{
        //    DataTable myTable = new DataTable();

        //    DataTable table = new DataTable();
        //    using (var con = new SqlConnection(ConnS))
        //    using (var cmd = new SqlCommand(Sp, con))
        //    {
        //        cmd.CommandTimeout = 0;
        //        foreach (var item in lParams)
        //        {
        //            cmd.Parameters.Add(new SqlParameter(item.Parametro, item.Valor));
        //        }

        //        using (var da = new SqlDataAdapter(cmd))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            da.Fill(table);
        //        }
        //    }
        //    return table;
        //}
        public String collector;

        static String connectionString = String.Format(ConfigurationManager.AppSettings["AppMultiServer"], "MS_tunneltek", "", "");

        private static SqlConnection openConnection()
        {
            try
            {
                SqlConnection connector = new SqlConnection(connectionString);
                connector.Open();
                return connector;
            }
            catch (SqlException s) { Console.WriteLine(String.Format("openConnection > SQLException [{0}] \n\n ExceptionDescription -> {1}", s.Message, s.StackTrace)); return null; }
            catch (Exception e) { Console.WriteLine(String.Format("openConnection > SystemException [{0}] \n\n ExceptionDescription -> {1}", e.Message, e.StackTrace)); return null; }
        }

        private void closeConnection(SqlConnection connector)
        {
            try
            {
                connector.Close();
                SqlConnection.ClearPool(connector);
            }
            catch (SqlException s) { Console.WriteLine(String.Format("closeConnection > SQLException [{0}] \n\n ExceptionDescription -> {1}", s.Message, s.StackTrace)); }
            catch (Exception e) { Console.WriteLine(String.Format("closeConnection > SystemException [{0}] \n\n ExceptionDescription -> {1}", e.Message, e.StackTrace)); }
        }

        public DataTable getRecords(String statement, SqlConnection connector = null)
        {
            collector = String.Empty;
            DataTable dt = new DataTable();
            try
            {
                if (connector == null)
                {
                    connector = openConnection();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(statement, connector);
                    adapter.Fill(dt);
                    closeConnection(connector);
                }
            }
            catch (SqlException s) { Console.WriteLine(String.Format("getRecords > SQLException [{0}] \n\n ExceptionDescription -> {1}", s.Message, s.StackTrace)); collector = String.Format("getRecords > SQLException [{0}] \n\n ExceptionDescription -> {1}", s.Message, s.StackTrace); }
            catch (Exception e) { Console.WriteLine(String.Format("getRecords > SystemException [{0}] \n\n ExceptionDescription -> {1}", e.Message, e.StackTrace)); collector = String.Format("getRecords > SystemException [{0}] \n\n ExceptionDescription -> {1}", e.Message, e.StackTrace); }

            return dt;
        }

        public void execOperation(String sentence, SqlConnection connector = null)
        {
            collector = String.Empty;
            try
            {
                if (connector == null)
                {
                    connector = openConnection();
                    SqlCommand operation = new SqlCommand(sentence, connector);
                    operation.ExecuteNonQuery();
                    closeConnection(connector);
                }
            }
            catch (SqlException s) { Console.WriteLine(String.Format("execOperation > SystemException [{0}] \n\n ExceptionDescription -> {1}", s.Message, s.StackTrace)); collector = String.Format("execOperation > SystemException [{0}] \n\n ExceptionDescription -> {1}", s.Message, s.StackTrace); }
            catch (Exception e) { Console.WriteLine(String.Format("exeOperation > SystemException [{0}] \n\n ExceptionDescription -> {1}", e.Message, e.StackTrace)); collector = String.Format("exeOperation > SystemException [{0}] \n\n ExceptionDescription -> {1}", e.Message, e.StackTrace); }
        }
    }
}
