using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Providers;
using System.Timers;
using gov.ncats.ginas.excel.tools.UI;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public class DataLoader : ControllerBase, IController
    {
        public const string LOAD_OPERATION = "BATCH";
        private const string STATUS_KEY = "IMPORT STATUS";
        private DateTime _resolutionStart;
        private bool _notified = false;
        private static int _scriptNumber = 0;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private GinasToolsConfiguration GinasConfiguration
        {
            get;
            set;
        }

        public void StartOperation()
        {
            GinasConfiguration = FileUtils.GetGinasConfiguration();

            ScriptQueue = new Queue<string>();
            CurrentOperationType = OperationType.Loading;
            ExcelSelection =ExcelWindow.Application.Selection;
        
            RetrievalForm form = new RetrievalForm();
            form.Controller = this;
            form.CurrentOperationType = OperationType.Loading;
            form.Visible = false;
            SetStatusUpdater(form);
            ScriptExecutor = form;
            form.ShowDialog();

            Excel.Range arange = GetExecutingRange();
            if( arange == null)
            {
                StatusUpdater.UpdateStatus("(0) records to execute, please enter data and select the rows to enter");
            }
            else
            {
                StatusUpdater.UpdateStatus("(" + arange.Count + ") records to execute");
                arange.EntireRow.Select();
            }
            _notified = false;
        }

        public void StartSheetCreation(Excel.Window window)
        {
            GinasConfiguration = FileUtils.GetGinasConfiguration();

            ScriptQueue = new Queue<string>();
            CurrentOperationType = OperationType.ShowScripts;
            ExcelWindow = window;

            RetrievalForm form = new RetrievalForm();
            form.Controller = this;
            form.CurrentOperationType = OperationType.ShowScripts;
            StatusUpdater = form;
            ScriptExecutor = form;
            form.Show();
        }

        /// <summary>
        /// This completes the data required for the dialog to be able to show the 
        /// user how input is parsed.  This will NOT begin the update process.
        /// </summary>
        public void ContinueSetup()
        {
            string msg = "";
            //grab values from the first selected row
            Callback cb = CreateUpdateCallbackForDisplay(ExcelSelection.Application.ActiveCell, 
                true, msg);
            if (!string.IsNullOrEmpty(msg)) StatusUpdater.UpdateStatus(msg);
            if (cb != null) ScriptExecutor.ExecuteScript("showPreview(tmpRunner" + _scriptNumber+ ")");
        }

        public bool StartResolution(bool newSheet)
        {
            if( CurrentOperationType == OperationType.ShowScripts)
            {
                string selectedScriptName =(string) ScriptExecutor.ExecuteScript("$('#scriptlist').val()");
                if( string.IsNullOrEmpty(selectedScriptName))
                {
                    UIUtils.ShowMessageToUser("Please select a script for your new sheet");
                    return true;
                }
                SheetUtils sheetUtils = new SheetUtils();
                sheetUtils.CreateSheet(ExcelWindow.Application.ActiveWorkbook, selectedScriptName, 
                    ScriptExecutor);
            }
            else
            {
                Authenticate();
                StartLoading(ExcelSelection);
            }
            return true;
        }

        private void Authenticate()
        {
            if(!string.IsNullOrWhiteSpace(GinasConfiguration.SelectedServer.Username) 
                && !string.IsNullOrWhiteSpace(GinasConfiguration.SelectedServer.PrivateKey))
            {
                string script1 = string.Format("GlobalSettings.authKey = '{0}'",
                    GinasConfiguration.SelectedServer.PrivateKey);
                ScriptExecutor.ExecuteScript(script1);
                string script2 = string.Format("GlobalSettings.authUsername = '{0}'",
                    GinasConfiguration.SelectedServer.Username);
                ScriptExecutor.ExecuteScript(script2);
            }
        }

        private void StartLoading(Excel.Range r)
        {
            _totalScripts = 0;
         
            Callbacks = new Dictionary<string, Callback>();
            if( r == null || r.Count == 0)
            {
                UIUtils.ShowMessageToUser("No data selected to execute. Please enter data, and select the rows to enter");
                return;
            }
            _resolutionStart = DateTime.Now;
            _totalScripts = r.Rows.Count;
            log.Debug("set totalScripts to row count " + _totalScripts);
            StatusUpdater.UpdateStatus("Processing " + _totalScripts + " updates");

            foreach(Excel.Range cell in r)
            {
                log.Debug("Processing cell with text" + cell.Text);
                Callback cb = CreateUpdateCallbackForExecution(cell);
                //cb.Wait();
            }

            StartFirstUpdateCallback();             
            LaunchCheckJob();
        }


        public Callback CreateUpdateCallbackForExecution(Excel.Range arow)
        {
            Callback cb = CreateUpdateCallbackForDisplay(arow);
            if (cb == null)
            {
                cb = CallbackFactory.CreateDummyCallback();
                cb.Execute("");
                DecremementTotalScripts();
                return cb;
            }
            else
            {
                ((UpdateCallback) cb).Execute("STARTED");
            }
            string script = "tmpRunner" + (cb as UpdateCallback).RunnerNumber
                + ".execute()"
                + ".get(function(b){cresults['"
                + cb.getKey() + "']=b;window.external.Notify('"
                + cb.getKey() + "');})";
            cb.SetScript(script);

            Callbacks.Add(cb.getKey(), cb);
            log.Debug("Added callback " + cb.getKey() + "; total: " + Callbacks.Count);
            return cb;
        }


        public UpdateCallback CreateUpdateCallbackForDisplay(Excel.Range arow, bool allowFinished = false,
            string message = "")
        {
            Excel.Application application = arow.Application;
            Dictionary<string, Excel.Range> keys = new Dictionary<string, Excel.Range>();

            Excel.Worksheet asheet = arow.Worksheet;
            Excel.Range headerRow = application.Intersect(asheet.Range["1:1"], asheet.UsedRange);
            Excel.Range crow = asheet.Range[arow.Row + ":" + arow.Row];
            string f = application.Intersect(headerRow, asheet.Range["A:A"]).Text;
            string[] tokens = f.Split(':');
            if (string.IsNullOrWhiteSpace(tokens[0]) && !tokens[0].Equals(LOAD_OPERATION))
            {
                UIUtils.ShowMessageToUser("Header row must start with \"BATCH\"");
                return null;
            }
            string scriptName = tokens[1];
            foreach (Excel.Range hcell in headerRow)
            {
                Excel.Range r = application.Intersect(crow, asheet.Range["A:A"].Offset[0, hcell.Column - 1]);
                if (hcell.Text != null)
                {
                    string cellTextUpper = (hcell.Text as string).ToUpper();
                    if (!keys.ContainsKey(cellTextUpper))
                    {
                        keys.Add(cellTextUpper, r);
                    }
                }

            }
            //If the status isn't empty, skip this one
            string statusValue = GetProperty(keys, STATUS_KEY, "");
            if (!string.IsNullOrWhiteSpace(statusValue))
            {
                message = "(will not execute row, because " + STATUS_KEY + " is not empty)";
                if (!allowFinished) return null;
            }
            _scriptNumber++;

            Dictionary<string, string> paramValues = new Dictionary<string, string>();

            string tempScriptName = "tmpScript" + _scriptNumber;
            ScriptExecutor.ExecuteScript(tempScriptName + "=Scripts.get('" + scriptName + "');");
            string runnerName = "tmpRunner" + _scriptNumber;
            ScriptExecutor.ExecuteScript(runnerName + "=" + tempScriptName + ".runner();");

            foreach (string key in keys.Keys)
            {
                if (!string.IsNullOrWhiteSpace(GetProperty(keys, key, "")))
                {
                    string testScript = tempScriptName + ".hasArgumentByName('" + key + "')";
                    object testValue = ScriptExecutor.ExecuteScript(testScript);
                    Debug.WriteLine("value: " + testValue);
                    if (testValue is string && (testValue as string).Equals("true",
                            StringComparison.CurrentCultureIgnoreCase))
                    {
                        object param =
                            ScriptExecutor.ExecuteScript(tempScriptName + ".getArgumentByName('" + key + "')");
                        ScriptParameter parameter = JSTools.GetScriptParameterFromString(param as string);
                        string parameterValue = (keys[key].Text != null) ? keys[key].Text :
                            string.Empty;
                        //escape characters that causes errors in JavaScript interpreter
                        parameterValue = parameterValue.Replace("'", "\\'").Replace("\n", "\\n");
                        string paramValueScript = string.Format(runnerName + ".setValue('{0}', '{1}')",
                            parameter.key, parameterValue);
                        ScriptExecutor.ExecuteScript(paramValueScript);
                        paramValues.Add(parameter.key, parameterValue);
                    }
                }
            }
            string tempVal = JSTools.RandomIdentifier();
            while (Callbacks != null && Callbacks.ContainsKey(tempVal))
            {
                tempVal = JSTools.RandomIdentifier(10, true);
            }
            UpdateCallback updateCallback = CallbackFactory.CreateUpdateCallback(keys[STATUS_KEY]);
            updateCallback.RunnerNumber = _scriptNumber;
            DateTime newExpirationDate = DateTime.Now.AddSeconds(GinasConfiguration.ExpirationOffset);
            updateCallback.SetExpiration(newExpirationDate);
            updateCallback.setKey(tempVal);
            updateCallback.ParameterValues = paramValues;
            updateCallback.LoadScriptName = scriptName;
            return updateCallback;
        }


        private string GetProperty(Dictionary<string, Excel.Range> dict, string key, string def)
        {
            if (dict.ContainsKey(key))
            {
                Excel.Range range = dict[key];
                if (range != null && range.Text != null && range.Text is string)
                {
                    return (range.Text as string).ToUpper();
                }
            }
            return def;
        }


        private void DecremementTotalScripts()
        {
            if (_totalScripts > 0) _totalScripts--;
        }

        public void LaunchCheckJob()
        {
            double secondsToMilliseconds = 1000;
        
            log.Debug("LaunchCheckJob using interval " + _checkInterval);
            _timer = new Timer(_checkInterval * secondsToMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += CheckUpdateCallbacks;
            _timer.Start();
        }

        public void CheckUpdateCallbacks(Object source, ElapsedEventArgs e)
        {
            log.Debug("Starting in checkUpdateCallbacks");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            String message;
            bool haveActive = false;

            List<string> callbackKeysToRemove = new List<string>();
            if (Callbacks == null || Callbacks.Count == 0)
            {
                log.Debug("callbacks collection is empty in CheckUpdateCallbacks");
            }

            log.Debug("Total callbacks at start: " + Callbacks.Count);
            message = "callback total: " + Callbacks.Count;
            //'go through individual callbacks
            foreach (string cbKey in Callbacks.Keys)
            {
                Callback cb = Callbacks[cbKey];
                if (cb.HasStarted())
                {
                    if (cb is UpdateCallback)
                    {
                        UpdateCallback updateCb = cb as UpdateCallback;
                        string itemMessage = "looking at updateCallback " + updateCb.getKey();
                        if (!updateCb.IsExpiredNow())
                        {
                            haveActive = true;
                            itemMessage = itemMessage + " active";
                        }
                        else
                        {
                            itemMessage = itemMessage + " expired";
                            callbackKeysToRemove.Add(cbKey);
                        }

                        log.Debug(itemMessage);
                    }
                }
            }

            KeepCheckingCallbacks = haveActive;
            if (!haveActive)
            {
                message = "No active callbacks detected";
            }
            if (callbackKeysToRemove.Count > 0)
            {
                foreach (string key in callbackKeysToRemove)
                {
                    Callback cb = Callbacks[key];
                    cb.Execute("Expired");
                    Callbacks.Remove(key);
                    DecremementTotalScripts();
                }
            }
            log.Debug("Total callbacks at end: " + Callbacks.Count);
            if (Callbacks.Count == 0)
            {
                haveActive = false;
            }

            log.Debug(message);
            if (!(KeepCheckingCallbacks && haveActive) )
            {
                _timer.Close();
                _timer.Stop();
                _timer.Enabled = false;
                log.Debug("stopped timer");
            }

            if (Callbacks.Count == 0)
            {
                KeepCheckingCallbacks = false;
                _timer.Stop();

                EndProcessNotification();
            }
            log.Debug("end of checkUpdateCallbacks which took " + sw.ElapsedMilliseconds);
            sw.Stop();
        }

        public object HandleResults(string resultsKey, string message)
        {
            Debug.WriteLine(string.Format("HandleResults received message {0} for key {1}",
                message, resultsKey));
            GinasResult result = JSTools.GetGinasResultFromString(message);
            UpdateCallback updateCallback = Callbacks[resultsKey] as UpdateCallback;
            updateCallback.Execute(result.message);
            Callbacks.Remove(resultsKey);

            string statusMessage = string.Format("{0} records to go", ScriptQueue.Count);
            if (Callbacks.Count ==0)
            {
                statusMessage = "Processing complete at " + DateTime.Now.ToShortTimeString();
                _timer.Close();
                _timer.Stop();
                _timer.Enabled = false;
                KeepCheckingCallbacks = false;
                EndProcessNotification();
                return true;
            }
            else
            {
                StartFirstUpdateCallback();
            }
            StatusUpdater.UpdateStatus(statusMessage);

            return "true";
        }

        private void EndProcessNotification()
        {
            if(!_notified )
            {
                //dialog itself will handle saving of debug info.
                log.Debug("EndProcessNotification calling Complete");
                StatusUpdater.UpdateStatus("Completed at " + DateTime.Now.ToShortTimeString());
                StatusUpdater.Complete();
            }
            _notified = true;
        }

        private Excel.Range GetExecutingRange()
        {
            Excel.Range r = ExcelSelection;
            if (r == null) return null;
            Excel.Worksheet activeSheet = r.Application.ActiveSheet;
            r = r.Application.Intersect(ActiveRange(), 
                r.Application.Intersect(activeSheet.UsedRange, 
                activeSheet.Range["2:" + r.Application.Intersect(activeSheet.UsedRange, activeSheet.Range["A:A"]).Cells.Count]));
            r = r.Application.Intersect(r, r.Cells[1].EntireColumn);
            return r;
        }

        private Excel.Range ActiveRange()
        {
            Excel.Worksheet activeSheet = ExcelSelection.Application.ActiveSheet;
            Excel.Range r = ExcelSelection.Application.Intersect(activeSheet.UsedRange, ExcelSelection.Cells);
            return r;
        }

        private void RunUpdateCallback(UpdateCallback updateCallback)
        {
            log.DebugFormat("RunUpdateCallback handling key {0}", updateCallback.getKey());
            string tempScriptName = "scriptObject";
            ScriptExecutor.ExecuteScript(tempScriptName + "=Scripts.get('" +  updateCallback.LoadScriptName+ "');");
            string runnerName = "tmpRunner";
            ScriptExecutor.ExecuteScript(runnerName + "=" + tempScriptName + ".runner();");
            foreach(string key in updateCallback.ParameterValues.Keys)
            {
                string paramValueScript = string.Format(runnerName + ".setValue('{0}', '{1}')",
                            key, updateCallback.ParameterValues[key]);
                ScriptExecutor.ExecuteScript(paramValueScript);
            }
            string executionScript = runnerName + ".execute()" +
                                                     ".get(function(b){cresults['" +
                                                    updateCallback.getKey() + "']=b;window.external.Notify('"
                                                    + updateCallback.getKey() + "');})";
            ScriptExecutor.ExecuteScript(executionScript);
        }


        private void StartFirstUpdateCallback()
        {
            if (Callbacks.Count == 0) return;
            if( Callbacks.Values.First() is UpdateCallback)
            {
                UpdateCallback updateCallback = Callbacks.Values.First() as UpdateCallback;
                RunUpdateCallback(updateCallback);
                updateCallback.Start();
            }
        }
    }
}