using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using ginasExcelUnitTests.Model;
using Oracle.ManagedDataAccess.Client;

namespace ginasExcelUnitTests.Utils
{
    internal class OracleDbUtility
    {
        static string connectionString = "User Id=gsrs;Password=GSRS;Data Source=GsrsDataSource";
        static OracleConnection oracleConnection = new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static OracleDbUtility()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            oracleConnection.Open();
            log.Info("connection opened in OracleDbUtility");
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            oracleConnection.Close();
            log.Info("connection closed in OracleDbUtility");
        }

        internal static string GetUuidForPt(string pt)
        {
            string query = string.Format("select owner_uuid from ix_ginas_name n where upper(n.name) = upper('{0}') and preferred = 1",
                pt);
            string uuid = string.Empty;
                        
            OracleCommand command = oracleConnection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = query;
            OracleDataReader reader= command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.
            if (reader.Read())
            {
                uuid = reader.GetString(0);
            }
            reader.Close();
            return uuid;
        }

        internal static List<CodeProxy> GetCodesForName(string name)
        {
            string query =
                string.Format("select code_system, code, type, code_text, comments, url from ix_ginas_code where owner_uuid in (select owner_uuid from ix_ginas_name where upper(name) = upper('{0}'))",
                 name);

            List<CodeProxy> codes = new List<CodeProxy>();
            OracleCommand command = oracleConnection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = query;
            OracleDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.

            while (reader.Read() == true)
            {
                CodeProxy codeProxy = new CodeProxy();
                codeProxy.CodeSystem = reader.GetString(0);
                codeProxy.Code = reader.GetString(1);
                codeProxy.Type = reader.GetString(2);
                codeProxy.CodeText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                codeProxy.Comments = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                codeProxy.Url = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                codes.Add(codeProxy);
            }
            reader.Close();
            command.Dispose();
            return codes;
        }

        internal static List<CodeProxy> GetCodesForUuid(string uuid)
        {
            string query =
                string.Format("select code_system, code, type, code_text, comments from ix_ginas_code where owner_uuid = '{0}'",
                 uuid);

            List<CodeProxy> codes = new List<CodeProxy>();
            OracleCommand command = oracleConnection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = query;
            OracleDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.

            while (reader.Read() == true)
            {
                CodeProxy codeProxy = new CodeProxy();
                codeProxy.CodeSystem = reader.GetString(0);
                codeProxy.Code = reader.GetString(1);
                codeProxy.Type = reader.GetString(2);
                codeProxy.CodeText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                codeProxy.Comments = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                codes.Add(codeProxy);
            }
            reader.Close();
            command.Dispose();
            return codes;
        }

        internal static List<string> GetNamesForUuid(string uuid)
        {
            string query = "Select name from ix_ginas_name where owner_uuid = '" + uuid + "' order by name";
            List<string> names = new List<string>();
            OracleCommand command = oracleConnection.CreateCommand();
            command.CommandText = query;
            command.CommandType = System.Data.CommandType.Text;
            OracleDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.


            // This loop will output the entire contents of the results, iterating
            // through each row and through each field of the row.
            while (reader.Read() == true)
            {
                names.Add(reader.GetString(0));
            }
            reader.Close();
            command.Dispose();
            return names;
        }


    }
}
