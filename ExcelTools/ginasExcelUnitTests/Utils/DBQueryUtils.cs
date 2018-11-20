using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using System.Data.Odbc;
using Npgsql;

namespace ginasExcelUnitTests.Utils
{
    /// <summary>
    /// Provides the ability to query the database directly in order to check the
    /// results of script and fetcher operations.
    /// 
    /// requires npgsql (https://www.codeproject.com/Articles/30989/Using-PostgreSQL-in-your-C-NET-application-An-intr) 
    /// 
    /// </summary>
    internal class DBQueryUtils
    {
        
        private string dbHost = "localhost";
        private string dbName = "ginas_db";
        private string dbUser = "ginas";
        private string dbPw = "ginas";
        private int dbPort = 5432;
        private string conectionString = "";

        NpgsqlConnection connection;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DBQueryUtils()
        {
            log.Debug("Starting in DBQueryUtils");
            conectionString = String.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};",
                    dbHost, dbPort, dbUser,
                    dbPw, dbName);
            connection = new NpgsqlConnection(conectionString);
            connection.Open();
        }

        ~DBQueryUtils()
        {
            //if(connection != null && connection.State == System.Data.ConnectionState.Open)
            //{
            //    log.Debug("Closing connection");
            //    connection.Close();
            //}
        }

        internal List<string> GetNamesForUuid(string uuid)
        {
            string query = "Select name from ix_ginas_name where owner_uuid = '" + uuid + "' order by name";
            List<string> names = new List<string>();
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.
            

            // This loop will output the entire contents of the results, iterating
            // through each row and through each field of the row.
            while (reader.Read() == true)
            {
                names.Add(reader.GetString(0));
            }
            reader.Close();
            return names;
        }
    }
}
