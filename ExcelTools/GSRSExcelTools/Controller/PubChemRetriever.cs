using GSRSExcelTools.Model;
using GSRSExcelTools.Model.Callbacks;
using GSRSExcelTools.Providers;
using GSRSExcelTools.UI;
using GSRSExcelTools.Utils;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Controller
{
    public class PubChemRetriever : ControllerBase
    {

        private static object LOCK_OBJECT = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); // Add an Application Setting.

        public void HandleLookup(Application excel)
        {
            Range selectedRange = excel.Selection as Range;

        }

        public async Task<bool> StartResolution()
        {

            ItemsPerBatch = FileUtils.GetGinasConfiguration().BatchSize;
            ScriptQueue = new Queue<string>();
            Range r = null;
            try
            {
                r = ExcelWindow.RangeSelection;
            }
            catch (Exception ex)
            {
                log.Debug("Error: " + ex.Message);

            }
            if (r == null)
            {
                return false;
            }
            ExcelSelection = r;

            int currItem = 0;
            int currItemWithinBatch = 0;
            List<LookupDataCallback> dataToProcess = new List<LookupDataCallback>();
            int totalItems = 0;
            int currentBatch = 0;
            //count the items
            log.Debug("about to count data " );
            foreach (Range cell in r.Cells)
            {
                string cellData = (string)cell.Text;
                if (cell.Text != null && (!string.IsNullOrWhiteSpace(cellData) 
                    && DataUtils.IsPossibleInChiKey(cellData)))
                {
                    totalItems++;
                }
            }
            int totalBatches = Convert.ToInt32( Math.Ceiling(Convert.ToDouble(totalItems) / Convert.ToDouble(ItemsPerBatch)));
            log.DebugFormat("totalItems: {0}; totalBatches: {1}", totalItems, totalBatches);
            foreach (Range cell in r.Cells)
            {
                if (StatusUpdater!= null && StatusUpdater.HasUserCancelled())
                {
                    log.Debug("user has cancelled process");
                    return false;
                }
                string cellData = (string)cell.Text;
                if (cell.Text != null && !string.IsNullOrWhiteSpace(cellData) 
                    && DataUtils.IsPossibleInChiKey(cellData))
                {
                    currItemWithinBatch++;
                    currItem++;
                    string cellText = (string)cell.Text;
                    
                    dataToProcess.Add(new LookupDataCallback(cell, cellText, string.Empty));
                    if ((currItemWithinBatch % ItemsPerBatch) == 0)
                    {
                        string statusMessage = string.Format("Processing batch # {0} of {1}",
                            (++currentBatch), totalBatches);
                        log.Debug(statusMessage);
                        if(StatusUpdater != null ) StatusUpdater.UpdateStatus(statusMessage);
                        await ProcessOneBatch(dataToProcess);
                        
                        currItemWithinBatch = 0;
                        log.Debug("Prepared batch containing " + ItemsPerBatch + " items");
                        dataToProcess.Clear();
                    }
                }
            }
            if (StatusUpdater != null && StatusUpdater.HasUserCancelled())
            {
                log.Debug("user has cancelled process");
                return false;
            }

            if (currItemWithinBatch > 0)// process any leftovers
            {
                string statusMessage = "Processing last batch";
                if(StatusUpdater != null) StatusUpdater.UpdateStatus(statusMessage);

                await ProcessOneBatch(dataToProcess);
            }
            if (StatusUpdater != null)
            {
                StatusUpdater.UpdateStatus("Complete");
                StatusUpdater.Complete();
            }
            return true;
        }

        internal static bool IsPossibleInChiKey(string data1)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessOneBatch(List<LookupDataCallback> submittable)
        {
            log.DebugFormat("Starting in ProcessOneBatch with data  {0}", submittable.Select(s => s.QueryData));
            string pubChemBaseUrl = config.AppSettings.Settings["pubChemBaseUrl"].Value;
            BatchLookup pubChemData = await RestUtils.RunPubChemQuery(submittable, pubChemBaseUrl);

            pubChemData.LookupData.ForEach(l =>
            {
                log.DebugFormat("ProcessOneBatch processing data for InChIKey {0}", l.QueryData);
                try
                {

                    Range returnRange = l.DataRange.Offset[0, 1];
                    returnRange.FormulaR1C1 = l.Result;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error: {0}", ex);
                }

            });
        }


        public void StartOperation()
        {
            throw new NotImplementedException();
        }

        public object HandleResults(string resultsKey, string message)
        {
            throw new NotImplementedException();
        }

        public void ContinueSetup()
        {
            throw new NotImplementedException();
        }

        public bool OkToWrite(int numberOfColumns)
        {
            throw new NotImplementedException();
        }
    }
}
