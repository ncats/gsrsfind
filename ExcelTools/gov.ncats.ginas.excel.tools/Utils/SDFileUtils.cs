using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Model;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class SDFileUtils
    {
        internal const string MOLFILE_END = "M  END";
        internal string[] SDF_FIELD_DELIMS = { ">  <", "> <" };
        internal const string SDF_RECORD_DELIM = "$$$$";
        internal const string MOLFILE_FIELD_NAME = "Molfile";
        internal const string SD_LOADING_SCRIPT_NAME = "Create Substance from SD File";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void HandleSDFileImport(string sdFilePath, Worksheet worksheet)
        {
            log.DebugFormat("Starting in HandleSDFileImport with file {0}", sdFilePath);
            List<SDFileRecord> fileData = ReadSdFile(sdFilePath);
            log.DebugFormat("read in {0} records", fileData.Count);

            SheetUtils sheetUtils = new SheetUtils();
            List<string> fieldNames = GetUniqueFieldNames(fileData);
            log.DebugFormat("total unique fields: {0}", fieldNames.Count);
            Dictionary<string, int> fieldNamesToColumns = new Dictionary<string, int>();
            int col = 1;
            foreach (string fieldName in fieldNames)
            {
                fieldNamesToColumns.Add(fieldName, ++col);
            }
            fieldNames.Insert(0, "BATCH:" + SD_LOADING_SCRIPT_NAME);
            //todo: make sure the sheet has no data!
            ImageOps imageOps = new ImageOps();
            //create a title row
            sheetUtils.TransferDataToRow(fieldNames.ToArray(), 1, 1, imageOps, worksheet, 0);

            int row = 1;
            foreach(SDFileRecord record in fileData)
            {
                sheetUtils.TransferSDDataToRow(record.RecordData, fieldNamesToColumns, ++row, imageOps, worksheet);
            }
            sheetUtils.SetColumnWidths(worksheet, fieldNamesToColumns.Values.ToList(), 30);
        }

        public List<SDFileRecord> ReadSdFile(string sdFilePath)
        {
            List<SDFileRecord> data = new List<SDFileRecord>();
            string[] sdData = System.IO.File.ReadAllLines(sdFilePath);

            string currentFieldName = MOLFILE_FIELD_NAME;

            List<string> oneFieldData = new List<string>();
            SDFileRecord oneRecord = new SDFileRecord();
            for (int rec = 0; rec < sdData.Length; rec++)
            {
                string line = sdData[rec];
                while (!line.StartsWith(MOLFILE_END) && !SDF_FIELD_DELIMS.Any(d=> line.StartsWith(d))
                    && !line.Equals(SDF_RECORD_DELIM))
                {
                    if (currentFieldName.Equals(MOLFILE_FIELD_NAME) || !string.IsNullOrWhiteSpace(line))
                    {
                        oneFieldData.Add(line);
                    }
                    line = sdData[++rec];
                }
                if (currentFieldName.Equals(MOLFILE_FIELD_NAME))
                {
                    line = sdData[++rec];
                    if(!oneFieldData[oneFieldData.Count - 1].Equals(MOLFILE_END))
                    {
                        oneFieldData.Add(MOLFILE_END);
                    }
                }
                oneRecord.RecordData.Add(currentFieldName, string.Join("\n", oneFieldData));

                if (SDF_FIELD_DELIMS.Any(d => line.StartsWith(d)))
                {
                    currentFieldName = GetFieldName(line);
                    oneFieldData = new List<string>(); ;
                }
                else if (line.Equals(SDF_RECORD_DELIM))
                {
                    data.Add(oneRecord);
                    oneRecord = new SDFileRecord();
                    currentFieldName = MOLFILE_FIELD_NAME;
                    oneFieldData = new List<string>();
                }
            }

            return data;
        }

        public string GetFieldName(string line)
        {
            int begin = line.IndexOf("<") + 1;
            int end = line.IndexOf(">", begin + 1);
            string fieldName = line.Substring(begin, (end - begin));
            return fieldName;
        }

        public List<string> GetUniqueFieldNames(List<SDFileRecord> sDFileRecords)
        {
            List<string> uniqueFieldNames = new List<string>();
            foreach(SDFileRecord record in sDFileRecords)
            {
                foreach(string fieldName in record.RecordData.Keys)
                {
                    if (!uniqueFieldNames.Contains(fieldName)) uniqueFieldNames.Add(fieldName);
                }
            }

            return uniqueFieldNames;
        }
    }
}
