using gov.ncats.ginas.excel.tools.Model.Callbacks;
using gov.ncats.ginas.excel.tools.Utils;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Controller
{
    class ChemSpiderRetriever : ControllerBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public async Task<bool> StartResolution()
        {
            if(string.IsNullOrWhiteSpace(FileUtils.GetGinasConfiguration().ChemSpiderApiKey))
            {
                UIUtils.ShowMessageToUser("To retrieve data from ChemSpider, please obtain an API key and add it to your configuration");
                return false;
            }
            ItemsPerBatch = FileUtils.GetGinasConfiguration().BatchSize;
            int currentBatch = 0;
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
            // int currentBatch = 0;
            //count the items
            log.Debug("about to count data ");
            foreach (Range cell in r.Cells)
            {
                string cellData = (string)cell.Text;
                if (cell.Text != null && (!string.IsNullOrWhiteSpace(cellData) 
                    && DataUtils.IsPossibleInChiKey(cellData)))
                {
                    totalItems++;
                }
            }
            int totalBatches = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(totalItems) / Convert.ToDouble(ItemsPerBatch)));
            log.DebugFormat("totalItems: {0}; totalBatches: {1}", totalItems, totalBatches);
            foreach (Range cell in r.Cells)
            {
                if (StatusUpdater != null && StatusUpdater.HasUserCancelled())
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
                        if (StatusUpdater != null) StatusUpdater.UpdateStatus(statusMessage);
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
                if (StatusUpdater != null) StatusUpdater.UpdateStatus(statusMessage);

                await ProcessOneBatch(dataToProcess);
            }
            if (StatusUpdater != null)
            {
                StatusUpdater.UpdateStatus("Complete");
                StatusUpdater.Complete();
            }
            return true;
        }

        private async Task ProcessOneBatch(List<LookupDataCallback> submittable)
        {
            log.DebugFormat("Starting in ProcessOneBatch with data {0}", submittable.Select(s => s.QueryData));
            string baseUrl = config.AppSettings.Settings["chemSpiderBaseUrl"].Value;
            BatchLookup chemSpiderData = await RestUtils.RunChemSpiderQuery(submittable, baseUrl);

            chemSpiderData.LookupData.ForEach(l =>
            {
                log.DebugFormat("ProcessOneBatch processing data for InChIKey {0}", l.QueryData);
                try
                {
                    Range returnRange;
                    if ( l.DataRange== null)
                    {
                        returnRange= SheetUtils.FindFirstCellWithText(ExcelWindow.RangeSelection, l.QueryData);
                    }
                    else
                    {
                        returnRange = l.DataRange.Offset[0, 1];
                    }
                    if(returnRange==null)
                    {
                        log.WarnFormat("Unable to locate data {0}", l.QueryData);
                    }
                    else
                    {
                        returnRange.FormulaR1C1 = l.Result;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error: {0}", ex);
                }

            });
        }

        /*
         * Open a ChemSpider search URL like this: http://www.chemspider.com/Search.aspx?q=PAYRUJLWNCNPSJ-UHFFFAOYSA-N
         */
        public async Task<bool> StartGeneralResolution()
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

            log.Debug("about to retrieve data ");
            if( r.Cells.Count != 1)
            {
                UIUtils.ShowMessageToUser("please select a single cell with an InChIKey");
                return false;
            }

            
            string cellData = (string)r.Text;
            if (r.Text != null && (!string.IsNullOrWhiteSpace(cellData)
                && DataUtils.IsPossibleInChiKey(cellData)))
            {
                string baseUrl = config.AppSettings.Settings["chemSpiderBaseUiUrl"].Value;
                string url = baseUrl + "?q=" + cellData;
                log.DebugFormat("using URL for ChemSpider: {0}", url);
                System.Diagnostics.Process.Start(url);
                return true;
            }
            else 
            {
                UIUtils.ShowMessageToUser("please select a single cell with an InChIKey");
                return false;
            }
        }

    }
}
