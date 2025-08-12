using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using System.Data.Odbc;
using System.Configuration;
using Npgsql;
using ginasExcelUnitTests.Model;

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
        System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private string dbHost;
        private string dbName;
        private string dbUser;
        private string dbPw;
        private int dbPort;
        private string conectionString = "";

        NpgsqlConnection connection;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DBQueryUtils()
        {
            log.Debug("Starting in DBQueryUtils");
            dbHost = config.AppSettings.Settings["dbHost"].Value;
            dbName = config.AppSettings.Settings["dbName"].Value;
            dbUser = config.AppSettings.Settings["dbUser"].Value;
            dbPw = config.AppSettings.Settings["dbPassword"].Value;
            dbPort = Convert.ToInt32( config.AppSettings.Settings["dbPort"].Value);

            conectionString = String.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};",
                    dbHost, dbPort, dbUser,
                    dbPw, dbName);
            connection = new NpgsqlConnection(conectionString);
            try
            {
                connection.Open();
            }
            catch(Exception ex)
            {
                log.Error("error establishing db connection", ex);
            }
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

        internal List<CodeProxy> GetCodesForName(string name)
        {
            string query =
                string.Format("select code_system, code, type, code_text, comments from ix_ginas_code where owner_uuid in (select owner_uuid from ix_ginas_name where upper(name) = upper('{0}'))",
                 name);
            

            List<CodeProxy> codes = new List<CodeProxy>();
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
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

        internal List<CodeProxy> GetCodesEtcForName(string name)
        {
            string query =
                string.Format("select code_system, code, comments, url,type from ix_ginas_code where owner_uuid in (select owner_uuid from ix_ginas_name where upper(name) = upper('{0}'))",
                 name);
            List<CodeProxy> codes = new List<CodeProxy>();
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.

            while (reader.Read() == true)
            {
                string codeSystem = reader.GetString(0);
                string code = reader.GetString(1);
                string comments = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                string url = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                string type = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                CodeProxy codeProxy = new CodeProxy();
                codeProxy.CodeSystem = codeSystem;
                codeProxy.Code = code;
                codeProxy.Url = url;
                codeProxy.Comments = comments;
                codeProxy.Type = type;
                codes.Add(codeProxy);
            }
            command.Dispose();
            reader.Close();
            return codes;
        }

        internal List<CodeProxy> GetCodesEtcForUuid(string uuid)
        {
            string query =
                string.Format("select code_system, code, comments, url,type from ix_ginas_code where owner_uuid = '{0}'",
                 uuid);
            List<CodeProxy> codes = new List<CodeProxy>();
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.

            while (reader.Read() == true)
            {
                string codeSystem = reader.GetString(0);
                string code = reader.GetString(1);
                string comments = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                string url = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                string type = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                CodeProxy codeProxy = new CodeProxy();
                codeProxy.CodeSystem = codeSystem;
                codeProxy.Code = code;
                codeProxy.Url = url;
                codeProxy.Comments = comments;
                codeProxy.Type = type;
                codes.Add(codeProxy);
            }
            command.Dispose();
            reader.Close();
            return codes;
        }


        internal List<Tuple<string, string>> GetCodesForBdNum(string bdNum)
        {
            string query =
                string.Format("select code_system, code from ix_ginas_code where owner_uuid in (select owner_uuid from ix_ginas_code where code='{0}' and code_system = 'BDNUM') and code_system != 'BDNUM'",
                bdNum);
            List<Tuple<string, string>> codes = new List<Tuple<string, string>>();
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.
            while (reader.Read() == true)
            {
                codes.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1)));
            }
            reader.Close();
            return codes;
        }

        internal List<Tuple<string, string>> GetCodesForUuid(string uuid)
        {
            string query =
                string.Format("select code_system, code from ix_ginas_code where owner_uuid = '{0}'",
                uuid);
            List<Tuple<string, string>> codes = new List<Tuple<string, string>>();
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.
            while (reader.Read() == true)
            {
                codes.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1)));
            }
            reader.Close();
            return codes;
        }

        internal string GetUuidForPt(string pt)
        {
            string query = string.Format("select owner_uuid from ix_ginas_name n where upper(n.name) = upper('{0}') and display_name = true",
                pt);
            string uuid = string.Empty;
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.
            if (reader.Read())
            {
                uuid = reader.GetString(0);
            }
            reader.Close();
            return uuid;
        }

        internal int GetVersionForUuid(string uuid)
        {
            string query = string.Format("select current_version from ix_ginas_substance where uuid ='{0}'", uuid);
            int version = -1;
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.
            if (reader.Read())
            {
                version = reader.GetInt32(0);
            }
            reader.Close();
            command.Dispose();
            return version;
        }

        internal List<StructureProxy> GetStructureForName(string name)
        {
            List<StructureProxy> structures = new List<StructureProxy>();
            string query = string.Format("select id, smiles, formula, mwt, stereo, charge from ix_core_structure where id =(SELECT structure_id FROM IX_ginas_Substance WHERE UUID =(select distinct owner_uuid from ix_ginas_name where upper(name) = upper('{0}')))", 
                name);
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.
            string idValue;
            string smiles;
            string formula;
            double mwt;
            string stereo;
            int charge;
            while(reader.Read())
            {
                idValue = reader.GetString(0);
                smiles = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                formula = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                mwt = reader.IsDBNull(3) ? double.NaN : reader.GetDouble(3);
                stereo = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                stereo = stereo.Replace("\"", "");
                charge = reader.IsDBNull(5) ? int.MinValue : reader.GetInt32(5);

                StructureProxy structureMock = new StructureProxy(idValue, smiles, formula, mwt, stereo, charge);
                structures.Add(structureMock);
            }
            reader.Close();
            return structures;
        }

        internal List<RelationshipProxy> GetRelationshipsForUuid(string uuid)
        {
            List<RelationshipProxy> relationships = new List<RelationshipProxy>();
            string query = string.Format("select r.uuid, r.owner_uuid, r.type, r.related_substance_uuid, sr.refuuid from ix_ginas_relationship r, ix_ginas_substanceref sr where r.owner_uuid = '{0}' " 
                + "and r.deprecated = 'false' and r.related_substance_uuid = sr.uuid",
                uuid);
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            // Execute the SQL command and return a reader for navigating the results.
            string uuidValue;
            string ownerUuid;
            string type;
            string relatedSubstanceUuid;
            while (reader.Read())
            {
                uuidValue = reader.GetString(0);
                ownerUuid = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                type = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                relatedSubstanceUuid = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);

                RelationshipProxy relationship = new RelationshipProxy(uuidValue, ownerUuid, relatedSubstanceUuid, type);
                relationships.Add(relationship);
            }
            reader.Close();
            return relationships;
        }

        internal List<CodeProxy> GetCodesOfSystemForName(string name, string codeSystem)
        {
            List<CodeProxy> codes = new List<CodeProxy>();
            string query = string.Format("select uuid, code, code_system, code_text, comments, type, url from ix_ginas_code where code_system = '{0}' and owner_uuid in "
                   + " (select owner_uuid from ix_ginas_name where name = '{1}') and deprecated = false",
                   codeSystem, name);
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            string uuid;
            string code;
            string retrievedCodeSystem;
            string codeText;
            string comments;
            string type;
            string url;
            while (reader.Read())
            {
                uuid = reader.GetString(0);
                code = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                retrievedCodeSystem = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                codeText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                comments = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                type = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                url = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

                CodeProxy codeProxy = new CodeProxy(uuid, code, codeSystem, codeText, comments, type, url);
                codes.Add(codeProxy);
            }
            reader.Close();
            return codes;
        }

        internal List<RelatedSubstanceProxy> GetRelatedSubstancesForName(string name, string relType)
        {
            List<RelatedSubstanceProxy> substances = new List<RelatedSubstanceProxy>();
            string query = string.Format("select uuid, ref_pname, refuuid, approval_id from ix_ginas_substanceref where uuid in "
                + "(select related_substance_uuid  from ix_ginas_relationship where owner_uuid in "
                + "(select owner_uuid from ix_ginas_name where upper(name) = ('{0}')) and deprecated = false and type = '{1}') ",
                   name, relType);
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            string uuid;
            string refPName;
            string refUuid;
            string approvalId;
            while (reader.Read())
            {
                uuid = reader.GetString(0);
                refPName= reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                refUuid = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                approvalId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);

                RelatedSubstanceProxy substProxy = new RelatedSubstanceProxy(uuid, refPName, refUuid, approvalId);
                substances.Add(substProxy);
            }
            reader.Close();
            return substances;
        }

        internal string GetProteinSequence(string name)
        {
            string query = string.Format("select uuid, sequence, subunit_index from ix_ginas_subunit where uuid in "
                + "(select ix_ginas_subunit_uuid from ix_ginas_protein_subunit where ix_ginas_protein_uuid ="
                + "(select protein_uuid from ix_ginas_substance where uuid = "
                + " (select owner_uuid from ix_ginas_name where upper(name) = ('{0}') and deprecated = false))) order by subunit_index",
                name);
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            List<string> sequences = new List<string>();
            while (reader.Read())
            {
                string uuid = reader.GetString(0);
                string sequencePart = reader.GetString(1);
                int unit_index = reader.GetInt32(2);
                sequences.Add(sequencePart);
            }
            reader.Close();

            return string.Join(",", sequences);
        }

        internal SubstanceProxy GetSubstance(string nameOrCode)
        {
            SubstanceProxy substance = null;
            string query = string.Format("select uuid, s.dtype, s.created, p1.username, s.last_edited, p2.username, s.deprecated, s.status, "
                + "   s.structure_id, s.mixture_uuid, s.nucleic_acid_uuid, s.polymer_uuid,"
                + "   s.protein_uuid, s.specified_substance_uuid, s.structurally_diverse_uuid, s.approval_id"
                + "   from ix_ginas_substance s, ix_core_principal p1, ix_core_principal p2 "
                + "   where s.created_by_id = p1.id and s.last_edited_by_id= p2.id and uuid in "
                + "   ((select owner_uuid from ix_ginas_name where upper(name) = upper('{0}')) "
                + "   union "
                + "   (select owner_uuid from ix_ginas_code where code like '{0}')) ",
                    nameOrCode);
            log.DebugFormat("{0} using SQL: {1}", MethodBase.GetCurrentMethod().Name, query);
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string uuid = reader.GetString(0);
                string dtype = reader.GetString(1);
                DateTime created = reader.GetDateTime(2);
                string createdBy =  reader.GetString(3);
                DateTime lastEdited = reader.GetDateTime(4);
                string lastEditedBy = reader.GetString(5);
                bool deprecated = reader.GetBoolean(6);
                string status = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
                string structureId = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                string mixtureId = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                string nucleicAcidId = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
                string polymerId = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
                string proteinId = reader.IsDBNull(12) ? string.Empty : reader.GetString(12);
                string specSubstanceId = reader.IsDBNull(13) ? string.Empty : reader.GetString(13);
                string structDiverseId = reader.IsDBNull(14) ? string.Empty : reader.GetString(14);
                string approvalId = reader.IsDBNull(15) ? string.Empty : reader.GetString(15);
                substance = new SubstanceProxy(uuid, dtype, deprecated, status, created, createdBy, lastEdited,
                    lastEditedBy, structureId, mixtureId, nucleicAcidId, polymerId, proteinId,
                    specSubstanceId, structDiverseId, approvalId);
            }
            reader.Close();
            return substance;
        }

        internal List<SubstanceNamesProxy> GetNamesForName(string name)
        {
            List<SubstanceNamesProxy> substanceNames = new List<SubstanceNamesProxy>();
            string query = string.Format("select name, type, preferred, display_name, languages from ix_ginas_name where owner_uuid in "
                + " (select owner_uuid from ix_ginas_name where upper(name) = upper('{0}')) and deprecated = false",
                    name);
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            List<string> sequences = new List<string>();
            while (reader.Read())
            {
                string dbName = reader.GetString(0);
                string type = reader.GetString(1);
                bool preferred = reader.GetBoolean(2);
                bool display = reader.GetBoolean(3);
                string languages = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);

                var oneName = new SubstanceNamesProxy(dbName, type, preferred, display, languages);
                substanceNames.Add(oneName);
            }
            reader.Close();
            return substanceNames;
        }

        internal List<StructurallyDiverseProxy> GetStructurallDivers(string nameOrCode)
        {
            List<StructurallyDiverseProxy> structDiverse = new List<StructurallyDiverseProxy>();

            string query = string.Format("select uuid, source_material_type, organism_family, organism_genus, organism_species, organism_author, part  from ix_ginas_strucdiv "
                + " where uuid in"
                + " (select structurally_diverse_uuid from ix_ginas_substance where uuid in"
                + " ((select owner_uuid from ix_ginas_name where upper(name) = upper('{0}'))"
                + "  union"
                + " (select owner_uuid from ix_ginas_code where code like '{0}'))) " 
                + " and deprecated = false ",
                nameOrCode);
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            NpgsqlDataReader reader = command.ExecuteReader();
            List<string> sequences = new List<string>();
            while (reader.Read())
            {
                string uuid = reader.GetString(0);
                string materialType = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                string family = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                string genus = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                string species = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                string author = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                string part = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

                var oneItem = new StructurallyDiverseProxy(uuid, genus, species, author, part);
                structDiverse.Add(oneItem);
            }
            reader.Close();
            return structDiverse;
        }

    }
}
