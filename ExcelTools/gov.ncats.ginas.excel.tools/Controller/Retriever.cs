using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;

using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Model.Callbacks;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Providers;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public class Retriever : ControllerBase, IController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Retriever()
        {
            ItemsPerBatch = GetBatchSize();
            ScriptQueue = new Queue<string>();
            Callbacks = new Dictionary<string, Callback>();
        }

        //private bool _notified = false;

        private bool _resolveToNewSheet = false;
        private static object LOCK_OBJECT = new object();

        //for unit tests
        public void SetSelection(Excel.Range selection)
        {
            ExcelSelection = selection;
        }


        /// <summary>
        /// Starts off structure fetching
        /// </summary>
        public void StartOperation()
        {
            ScriptQueue = new Queue<string>();

            CallbackFactory factory = new CallbackFactory();
            Excel.Worksheet activeWorksheet = ((Excel.Worksheet)ExcelWindow.Application.ActiveSheet);
            ExcelSelection = (Excel.Range)ExcelWindow.Application.Selection;
            if (ExcelSelection == null)
            {
                UIUtils.ShowMessageToUser("Error obtaining access to Excel!");
                return;
            }
            List<SearchValue> searchValues = GetSearchValues(ExcelSelection);
            string callbackKey = JSTools.RandomIdentifier();

            if (searchValues.Any(v => !string.IsNullOrWhiteSpace(v.Value)))
            {
                string searchScript = MakeImageSearch(callbackKey, searchValues.Select(sv => sv.Value).ToList());
                //ImgCallback imgCallback = new ImgCallback(ExcelSelection);
                ScriptExecutor.SetScript(searchScript);
            }
            else
            {
                UIUtils.ShowMessageToUser("Please select a chemical name or ID");
                return;
            }
        }

        /// <summary>
        /// Process a set of results. Fetch details from JavaScript and transfer
        /// individual values to the spreadsheet
        /// </summary>
        /// <param name="resultsKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public object HandleResults(string resultsKey, string message)
        {
            log.Debug(string.Format("HandleResults received message {0} for key {1}",
                message, resultsKey));

            Dictionary<string, string> results = new Dictionary<string, string>();

            Dictionary<string, string[]> returnedValue = JSTools.getDictionaryFromString(message);
            ImageOps imageOps = new ImageOps();

            SheetUtils sheetUtils = new SheetUtils();
            sheetUtils.Configuration = GinasConfiguration;
            foreach (string key in returnedValue.Keys)
            {
                if (GinasConfiguration.DebugMode)
                {
                    log.DebugFormat("Handling result for key {0}", key);
                }

                string keyResult = "OK";
                try
                {
                    string[] messageParts = returnedValue[key][0].Split('\t');
                    int currentRow = ExcelSelection.Row;

                    int currentColumn = ExcelSelection.Column;
                    int dataRow =
                        SheetUtils.FindRow(ExcelSelection, key, currentColumn);
                    if( dataRow == 0)
                    {
                        dataRow = SheetUtils.FindRow(ExcelSelection.Worksheet.UsedRange, key, currentColumn);
                        log.DebugFormat("First attempt to locate key {0} failed. Second search yielded: {1}",
                            key, dataRow);
                    }
                    if (_resolveToNewSheet)
                    {
                        if (!ResolveRowToNewSheet(currentColumn, key, ref dataRow, ref keyResult))
                        {
                            continue;
                        }
                    }
                    sheetUtils.TransferDataToRow(messageParts, currentColumn, dataRow, imageOps,
                        ExcelSelection.Worksheet);
                    results.Add(key, keyResult);
                    System.Windows.Forms.Application.DoEvents();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error handling key {0} {1} {2}", key, ex.Message, ex);
                    results.Add(key, "Exception: " + ex.Message);
                }
            }

            if (ScriptQueue != null && ScriptQueue.Count > 0)
            {
                log.Debug("calling LaunchLastScript");
                LaunchFirstScript();
            }
            else log.Debug("Skipping call to LaunchLastScript");
            MarkCallbackExecuted(resultsKey);
            string statusMessage = string.Format("{0} batches to go", ScriptQueue.Count);
            if (ScriptQueue.Count == 0) statusMessage = "Processing complete!";
            if (StatusUpdater != null)
            {
                StatusUpdater.UpdateStatus(statusMessage);
                if (ScriptQueue.Count == 0) StatusUpdater.Complete();//reenable the close button
            }
            return results;
        }

        public bool ResolveRowToNewSheet(int currentColumn, string key, ref int dataRow,
            ref string keyResult)
        {
            //string keyResult = string.Empty;
            //increase row by one to account for header
            int originalRowNum = FindRowForKey(key);
            if (originalRowNum < 0)
            {
                keyResult = "Unable to locate for for key " + key;
                log.Warn(keyResult);
                return false;
            }
            int rowForData = originalRowNum + 1;

            string cellId = SheetUtils.GetColumnName(currentColumn) + rowForData;
            log.DebugFormat("Located row {0} for key {1}. CellId: {2}",
                originalRowNum, key, cellId);
            dataRow = rowForData;
            Excel.Range currentCell = ExcelSelection.Worksheet.Range[cellId];
            try
            {
                currentCell.NumberFormat = "@";//prevent cas numbers from being interpreted as dates
            }
            catch (Exception ex)
            {
                log.WarnFormat("Error setting format for call {0}. Error: {1} ",
                    cellId, ex.Message);
            }

            currentCell.Value = key;
            return true;
        }

        public bool StartResolution(bool newSheet)
        {
            _resolveToNewSheet = newSheet;
            lock (LOCK_OBJECT)
            {
                Callbacks = new Dictionary<string, Callback>();
            }
            ScriptQueue = new Queue<string>();
            Excel.Range r = null;
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
            BatchCallback cb = CallbackFactory.CreateBatchCallback();
            RangeWrapper wrapped = null;

            int currItem = 0;
            int currItemWithinBatch = 0;
            List<string> preSubmit = new List<string>();
            if (newSheet)
            {
                log.Debug("Resolving to new sheet");
                wrapped = GetNewSheetResolverCursor();
            }

            foreach (Excel.Range cell in r.Cells)
            {

                if (cell.Text != null && (!string.IsNullOrWhiteSpace((string)cell.Text)))
                {
                    currItemWithinBatch++;
                    currItem++;
                    string cellText = (string)cell.Text;
                    log.DebugFormat("   processing input cell text {0}", cellText);
                    preSubmit.Add(cellText.Replace("'", "\'"));
                    Callback rcb;
                    if (newSheet)
                    {
                        rcb = CallbackFactory.CreateCursorBasedResolverCallback(wrapped);
                        (rcb as CursorBasedResolverCallback).OriginalRow = currItem;
                    }
                    else
                    {
                        rcb = CallbackFactory.CreateResolverCallback(cell);
                    }
                    rcb.SetKey(cellText);
                    cb.AddCallback(rcb);

                    if ((currItemWithinBatch % ItemsPerBatch) == 0)
                    {
                        QueueOneBatch(cb, preSubmit);
                        cb = CallbackFactory.CreateBatchCallback();
                        currItemWithinBatch = 0;
                        log.Debug("Prepared batch containing " + ItemsPerBatch + " items");
                        preSubmit.Clear();
                    }
                }
            }
            if (currItemWithinBatch > 0)// process any leftovers
            {
                QueueOneBatch(cb, preSubmit);
            }

            if (ScriptQueue.Count > 0)
            {
                KeepCheckingCallbacks = true;
                LaunchFirstScript();
                LaunchCheckJob();
                _totalBatches = ScriptQueue.Count;
                StatusUpdater.UpdateStatus("Starting...");
            }
            return true;
        }

        private void QueueOneBatch(Callback cb, List<string> submittable)
        {
            cb.SetKey(JSTools.RandomIdentifier());
            while (Callbacks.ContainsKey(cb.getKey()))
            {
                log.Error("Callback contains duplicate key: " + cb.getKey());
                System.Threading.Thread.Sleep(1);
                cb.SetKey(JSTools.RandomIdentifier());
            }
            lock (LOCK_OBJECT)
            {
                Callbacks.Add(cb.getKey(), cb);
            }
            log.Debug("preparing callback with key " + cb.getKey() + " at " + DateTime.Now);
            string script = MakeSearch(cb.getKey(), submittable);
            log.Debug("script: " + script);

            ScriptQueue.Enqueue(script);
        }


        private List<SearchValue> GetSearchValues(Excel.Range selection)
        {
            List<SearchValue> searchValues = new List<SearchValue>();
            foreach (Excel.Range row in selection.Rows)
            {
                string cellName = SheetUtils.GetColumnName(row.Column) + row.Row;
                string cellValue = (string)selection.Worksheet.get_Range(cellName).Value;
                log.Debug(string.Format("cell {0} = value: {1}",
                    cellName, cellValue));
                searchValues.Add(new SearchValue(cellValue, row.Row));
            }
            return searchValues;
        }

        private string MakeImageSearch(string key, List<string> names)
        {
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append("cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("']={'keys':function(){return _.keys(this);},'Item':function(k){return this[k];}, 'add':function(k,v){if(!this[k]){this[k]=[];}this[k].push(v);}};ResolveWorker.builder()");
            scriptBuilder.Append(".list(");
            string arrayedNames = JSTools.MakeSearchString(names.ToArray());
            scriptBuilder.Append(arrayedNames);
            //scriptBuilder.Append("'.split('\n'))");
            scriptBuilder.Append(")");
            scriptBuilder.Append(".fetchers(['Image URL'])");
            scriptBuilder.Append(".consumer(function(row){console.log('row: '+JSON.stringify(row));cresults['");
            //scriptBuilder.Append(".consumer(function(row){cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("'].add(row.split('\t')[0],row);})");
            scriptBuilder.Append(".finisher(function(){sendMessageBackToCSharp('");
            scriptBuilder.Append(key);
            scriptBuilder.Append("');})");
            scriptBuilder.Append(".resolve();");
            return scriptBuilder.ToString();
        }

        private string MakeSearch(string key, List<string> names)
        {
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append("cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("']={'keys':function(){return _.keys(this);},'Item':function(k){return this[k];},");
            scriptBuilder.Append("'add':function(k,v){if(!this[k]){this[k]=[];}this[k].push(v);}};");
            scriptBuilder.Append("ResolveWorker.builder()");
            string arrayedNames = JSTools.MakeSearchString(names.ToArray());
            scriptBuilder.Append(".list(");
            scriptBuilder.Append(arrayedNames);
            scriptBuilder.Append(")");
            scriptBuilder.Append(".fetchers(_.map($('div.checkop input:checked'), 'name'))");
            scriptBuilder.Append(".consumer(function(row){cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("'].add(row.split('\t')[0],row);})");
            scriptBuilder.Append(".finisher(function(){sendMessageBackToCSharp('");
            scriptBuilder.Append(key);
            scriptBuilder.Append("');})");
            scriptBuilder.Append(".resolve();");
            return scriptBuilder.ToString();
        }

        new private void DecremementTotalScripts()
        {
            if (_totalScripts > 0)
            {
                _totalScripts--;
            }
        }

        protected override void EndProcessNotification()
        {
            //dialog itself will handle saving of debug info.
            StatusUpdater.UpdateStatus("Completed");
            //_notified = true;
        }

        public void CheckAllCallbacks(Object source, ElapsedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string message;
            bool haveActive = false;
            log.Debug("Starting in checkAllCallbacks");
            if (Callbacks == null || Callbacks.Count == 0)
            {
                message = "callbacks null or empty";
                log.Debug(message);
                if (_timer == null) return;
                _timer.Close();
                _timer.Stop();
                _timer.Enabled = false;
                _timer = null;
                return;
            }
            message = "callback total: " + Callbacks.Count;
            List<string> callbackKeysToRemove = new List<string>();
            //'go through individual callbacks
            lock (LOCK_OBJECT)
            {
                foreach (string cbKey in Callbacks.Keys)
                {
                    Callback cb = Callbacks[cbKey];
                    if (cb.HasStarted())
                    {
                        if (cb is BatchCallback)
                        {
                            BatchCallback batchCb = cb as BatchCallback;
                            //'look for started items 
                            if (batchCb.ContainsActiveCallback())
                            {
                                haveActive = true;
                                break;
                            }
                            else
                            {
                                callbackKeysToRemove.Add(cbKey);
                            }
                        }
                    }
                }
                callbackKeysToRemove.ForEach(k =>
                {
                    Callbacks.Remove(k);
                    log.Debug("just removed batchcallback with key " + k);
                });
            }
            KeepCheckingCallbacks = haveActive;
            if (!haveActive)
            {
                message = string.Format("No active callbacks detected. Total callbacks: {0}, ScriptQueue count: {1}",
                    Callbacks.Count, ScriptQueue.Count);
            }

            log.Debug(message);

            if (!KeepCheckingCallbacks && ((ScriptQueue == null) || ScriptQueue.Count == 0))
            {
                _timer.Elapsed -= CheckAllCallbacks;
                _timer.AutoReset = false;
                _timer.Stop();
                _timer.Close();
                _timer = null;
                log.Debug("_timer closed");
                if (!haveActive)
                {
                    log.Debug("about to clear callbacks");
                    lock (LOCK_OBJECT) Callbacks.Clear();
                }
            }

            log.Debug("end of checkAllCallbacks which took " + sw.Elapsed);
            sw.Stop();
        }

        public void LaunchCheckJob()
        {
            double secondsToMilliseconds = 1000;
            string interval = "00:00:" + String.Format("{0:00}", _checkInterval);
            //'assume interval is less than 60 seconds!

            log.Debug("LaunchCheckJob using interval " + interval);
            _timer = new Timer(_checkInterval * secondsToMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += CheckAllCallbacks;
            log.Debug("(checkAllCallbacks)");
            _timer.Start();
        }

        public void ContinueSetup()
        {

        }

        public new void Dispose()
        {
            base.Dispose();
            ExcelWindow = null;
            ExcelSelection = null;
        }

        //for unit tests
        public Queue<String> GetScriptQueue()
        {
            return ScriptQueue;
        }

        public bool OkToWrite(int numberOfColumns)
        {
            if (!SheetUtils.ContainsDataInColumnsToBeWritten(ExcelWindow.RangeSelection, numberOfColumns) ||
                    UIUtils.GetUserYesNo("There is data in columns that will be overwritten! Continue with operation?"))
            {
                return true;
            }
            return false;
        }

        private RangeWrapper GetNewSheetResolverCursor()
        {
            string[] headers;
            object checkedInput = ScriptExecutor.ExecuteScript("_.map($('div.checkop input:checked'), 'name').join('___');");
            string[] splitters = { "___" };
            headers = (checkedInput as string).Split(splitters, StringSplitOptions.None);

            Excel.Worksheet newSheet = (Excel.Worksheet)ExcelWindow.Application.Sheets.Add();
            SheetUtils sheetUtils = new SheetUtils();
            newSheet.Name = sheetUtils.GetNewSheetName(ExcelSelection.Application.ActiveWorkbook,
                "Resolved");
            newSheet.Range["A1"].FormulaR1C1 = "Query";
            int currCol = -1;
            foreach (string header in headers)
            {
                if (string.IsNullOrWhiteSpace(header))
                {
                    log.Debug("Header contains blank");
                    continue;
                }
                newSheet.Range["B1"].Offset[0, ++currCol].FormulaR1C1 = header;
                log.DebugFormat("put header into column {0}", currCol);
            }
            RangeWrapper wrapped = RangeWrapperFactory.CreateRangeWrapper(newSheet.Range["A2"]);
            ExcelSelection = newSheet.Range["A2"];
            return wrapped;
        }

        internal Dictionary<string, Callback> GetCallbacks()
        {
            return Callbacks;
        }

        private int FindRowForKey(string key)
        {
            foreach (Callback callback in Callbacks.Values)
            {
                if (callback is BatchCallback)
                {
                    Callback innerCallback = (callback as BatchCallback).FindInnerCallback(key);
                    if (innerCallback is CursorBasedResolverCallback)
                    {
                        return (innerCallback as CursorBasedResolverCallback).OriginalRow;
                    }
                }
            }
            return -1;
        }

        protected void MarkCallbackExecuted(string callbackKey)
        {
            log.DebugFormat("{0} called with key {1}",
                System.Reflection.MethodBase.GetCurrentMethod().Name, callbackKey);
            if (Callbacks.ContainsKey(callbackKey))
            {
                lock (LOCK_OBJECT)
                {
                    Callback cb = Callbacks[callbackKey];
                    if (cb is BatchCallback)
                    {
                        log.Debug("Marking inner callbacks of BatchCallback");
                        (cb as BatchCallback).SetInnerExecuted();
                    }
                    else
                    {
                        cb.SetExecuted();
                    }
                }
            }
        }

    }
}