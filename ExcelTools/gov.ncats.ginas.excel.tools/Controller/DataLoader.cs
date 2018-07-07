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
        private string _scriptName;
        private float _secondsPerScript = 10;
        private Dictionary<string, ScriptParameter> _scriptParameters;
        private bool _foundNoActivesLastTime = false;
        private int _NumTimesFoundNoActives = 0;
        private const int MAX_TIMES_NO_ACTIVE = 4;
        private string _currentKey = string.Empty;
        private const int CONSOLE_CLEARANCE_INTERVAL = 50;

        internal static string STATUS_STARTED = "STARTED";

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
            ExcelSelection = (Excel.Range)ExcelWindow.Application.Selection;

            RetrievalForm form = new RetrievalForm();
            form.Controller = this;
            form.CurrentOperationType = OperationType.Loading;
            form.Visible = false;
            SetStatusUpdater(form);
            ScriptExecutor = form;
            form.ShowDialog();

            Excel.Range arange = GetExecutingRange();
            if (arange == null)
            {
                StatusUpdater.UpdateStatus("(0) records to execute, please enter data and select the rows to enter");
            }
            else
            {
                StatusUpdater.UpdateStatus("(" + arange.Count + ") records to execute");
                //arange.EntireRow.Select();
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
            Callbacks = new Dictionary<string, Callback>();
            string msg = "";
            //grab values from the first selected row

            _scriptName = GetScriptName(ExcelSelection.Application.ActiveCell);
            SetScriptParameters(ExcelSelection.Application.ActiveCell);

            Callback cb = CreateInitialUpdateCallback(ExcelSelection.Application.ActiveCell,
                ref msg, true);
            if (!string.IsNullOrEmpty(msg)) StatusUpdater.UpdateStatus(msg);
            if (cb != null) ScriptExecutor.ExecuteScript("showPreview(tmpRunner)");
        }

        public bool StartResolution(bool newSheet)
        {
            if (CurrentOperationType == OperationType.ShowScripts)
            {
                string selectedScriptName = (string)ScriptExecutor.ExecuteScript("$('#scriptlist').val()");
                if (string.IsNullOrEmpty(selectedScriptName))
                {
                    UIUtils.ShowMessageToUser("Please select a script for your new sheet");
                    return true;
                }
                SheetUtils sheetUtils = new SheetUtils();
                sheetUtils.Configuration = GinasConfiguration;
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
            if (!string.IsNullOrWhiteSpace(GinasConfiguration.SelectedServer.Username)
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
            if (r == null || r.Count == 0)
            {
                UIUtils.ShowMessageToUser("No data selected to execute. Please enter data, and select the rows to enter");
                return;
            }
            _resolutionStart = DateTime.Now;
            _totalScripts = r.Rows.Count;
            log.Debug("set totalScripts to row count " + _totalScripts);
            StatusUpdater.UpdateStatus("Processing " + _totalScripts + " updates");

            log.Debug("total cells: " + r.Count);
            foreach (Excel.Range row in r.Rows)
            {
                log.DebugFormat("Processing cells {0} with text {1}", row.Address, row.Text);
                string cellText = row.Text as string;
                if (row.Cells.Count == 1 && string.IsNullOrWhiteSpace(cellText)) continue;
                Callback cb = CreateUpdateCallbackForExecution(row);
                //cb.Wait();
            }

            StartFirstUpdateCallback();
            LaunchCheckJob();
        }


        public Callback CreateUpdateCallbackForExecution(Excel.Range arow)
        {
            string msg = string.Empty;
            Callback cb = CreateInitialUpdateCallback(arow, ref msg);
            if (cb == null)
            {
                cb = CallbackFactory.CreateDummyCallback();
                cb.Execute("");
                DecremementTotalScripts();
                return cb;
            }
            else
            {
                ((UpdateCallback)cb).SetRangeText(STATUS_STARTED);
            }
            string script = "tmpRunner"
                + ".execute()"
                + ".get(function(b){cresults['"
                + cb.getKey() + "']=b;window.external.Notify('"
                + cb.getKey() + "');})";
            cb.SetScript(script);

            Callbacks.Add(cb.getKey(), cb);
            log.Debug("Added callback " + cb.getKey() + "; total: " + Callbacks.Count);
            return cb;
        }


        public UpdateCallback CreateInitialUpdateCallback(Excel.Range arow,
            ref string message, bool allowFinished = false)
        {
            Excel.Application application = arow.Application;

            Dictionary<string, Excel.Range> keys = GetKeys(arow);
            //If the status isn't empty, skip this one
            string statusValue = GetProperty(keys, STATUS_KEY, "");
            if (!string.IsNullOrWhiteSpace(statusValue))
            {
                message = "(will not execute row, because " + STATUS_KEY + " is not empty)";
                if (!allowFinished) return null;
            }
            _scriptNumber++;

            Dictionary<string, string> paramValues = new Dictionary<string, string>();
            string runnerName = "tmpRunner";
            foreach (string key in keys.Keys)
            {
                if (!string.IsNullOrWhiteSpace(GetProperty(keys, key, ""))
                    && _scriptParameters.ContainsKey(key))
                {
                    ScriptParameter parameter = _scriptParameters[key];
                    string parameterValue = (string)((keys[key].Text != null) ? keys[key].Text :
                        string.Empty);
                    //escape characters that causes errors in JavaScript interpreter
                    parameterValue = parameterValue.Replace("'", "\\'").Replace("\n", "\\n");
                    if (allowFinished)
                    {
                        string paramValueScript = string.Format(runnerName + ".setValue('{0}', '{1}')",
                            parameter.key, parameterValue);
                        ScriptExecutor.ExecuteScript(paramValueScript);
                    }
                    paramValues.Add(parameter.key, parameterValue);
                }
            }
            string tempVal = JSTools.RandomIdentifier();
            while (Callbacks != null && Callbacks.ContainsKey(tempVal))
            {
                tempVal = JSTools.RandomIdentifier(10, true);
            }
            UpdateCallback updateCallback = CallbackFactory.CreateUpdateCallback(keys[STATUS_KEY]);
            updateCallback.RunnerNumber = _scriptNumber;
            DateTime newExpirationDate = DateTime.Now.AddSeconds(GinasConfiguration.ExpirationOffset+ 
                (Callbacks.Count* Callbacks.Count * _secondsPerScript));//trying a quadratic term
            updateCallback.SetExpiration(newExpirationDate);
            updateCallback.setKey(tempVal);
            updateCallback.ParameterValues = paramValues;
            updateCallback.LoadScriptName = _scriptName;
            message = "Total selected rows: " + (application.Selection as Excel.Range).Rows.Count;
            return updateCallback;
        }


        private Dictionary<string, Excel.Range> GetKeys(Excel.Range row)
        {
            Dictionary<string, Excel.Range> keys = new Dictionary<string, Excel.Range>();

            Excel.Application application = row.Application;
            Excel.Worksheet asheet = row.Worksheet;
            Excel.Range headerRow = application.Intersect(asheet.Range["1:1"], asheet.UsedRange);
            Excel.Range crow = asheet.Range[row.Row + ":" + row.Row];
            string f = (string)application.Intersect(headerRow, asheet.Range["A:A"]).Text;

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
            return keys;
        }

        private string GetScriptName(Excel.Range row)
        {
            Dictionary<string, Excel.Range> keys = new Dictionary<string, Excel.Range>();

            Excel.Application application = row.Application;
            Excel.Worksheet asheet = row.Worksheet;
            Excel.Range headerRow = application.Intersect(asheet.Range["1:1"], asheet.UsedRange);
            Excel.Range crow = asheet.Range[row.Row + ":" + row.Row];
            string f = (string)application.Intersect(headerRow, asheet.Range["A:A"]).Text;
            string[] tokens = f.Split(':');
            if (string.IsNullOrWhiteSpace(tokens[0]) && !tokens[0].Equals(LOAD_OPERATION))
            {
                UIUtils.ShowMessageToUser("Header row must start with \"BATCH\"");
                return null;
            }
            string tempScriptName = "tmpScript";
            ScriptExecutor.ExecuteScript(tempScriptName + "=Scripts.get('" + tokens[1] + "');");
            string runnerName = "tmpRunner";
            ScriptExecutor.ExecuteScript(runnerName + "=" + tempScriptName + ".runner();");

            return tokens[1];
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
                        if (updateCb.IsExpiredNow())
                        {
                            itemMessage = itemMessage + " expired; script: " + updateCb.getScript(); ;
                            callbackKeysToRemove.Add(cbKey);
                        }
                        else
                        {
                            haveActive = true;
                            itemMessage = itemMessage + " active";
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
            log.DebugFormat("Total callbacks at end: {0}", Callbacks.Count);
            if (Callbacks.Count == 0)
            {
                haveActive = false;
                KeepCheckingCallbacks = false;
                _timer.Close();
                _timer.Stop();
                _timer.Enabled = false;
                log.Debug("stopped timer");
                EndProcessNotification();
            }
            else if (!haveActive)
            {
                // <N> runs of this check with no active callbacks mean it's ok to start a new callback
                if (_NumTimesFoundNoActives < MAX_TIMES_NO_ACTIVE)
                {
                    message += "; will now call StartFirstUpdateCallback";
                    StartFirstUpdateCallback();
                    _NumTimesFoundNoActives = 0;
                }
                else
                {
                    _NumTimesFoundNoActives++;
                }
            }

            log.Debug(message);

            log.Debug("end of checkUpdateCallbacks which took " + sw.ElapsedMilliseconds);
            sw.Stop();
        }

        public object HandleResults(string resultsKey, string message)
        {
           log.DebugFormat("HandleResults received message {0} for key {1}",
                message, resultsKey);
            if( resultsKey.Equals(_currentKey ))
            {
                _currentKey = string.Empty;
            }
            GinasResult result = JSTools.GetGinasResultFromString(message);
            UpdateCallback updateCallback = Callbacks[resultsKey] as UpdateCallback;
            updateCallback.Execute(result.message);
            Callbacks.Remove(resultsKey);

            string statusMessage = string.Format("{0} of {1} records remain to process", Callbacks.Count,
                _totalScripts);
            StatusUpdater.UpdateStatus(statusMessage);

            if (Callbacks.Count == 0)
            {
                statusMessage = "Processing complete at " + DateTime.Now.ToShortTimeString();
                if (_timer != null)
                {
                    _timer.Close();
                    _timer.Stop();
                    _timer.Enabled = false;
                }
                KeepCheckingCallbacks = false;
                StatusUpdater.UpdateStatus(statusMessage);
                EndProcessNotification();
                return true;
            }
            else
            {
                log.Debug("HandleResults will now call StartFirstUpdateCallback");
                StartFirstUpdateCallback();
            }
            return "true";
        }

        public Dictionary<string, Callback> GetCallbacksForUnitTests()
        {
            if (Callbacks == null) Callbacks = new Dictionary<string, Callback>();
            return Callbacks;
        }

        private void EndProcessNotification()
        {
            if (!_notified)
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
            Excel.Worksheet activeSheet = (Excel.Worksheet)r.Application.ActiveSheet;
            r = r.Application.Intersect(ActiveRange(),
                r.Application.Intersect(activeSheet.UsedRange,
                activeSheet.Range["2:" + r.Application.Intersect(activeSheet.UsedRange, activeSheet.Range["A:A"]).Cells.Count]));
            r = (r.Application.Intersect(r, r.Cells[1] as Excel.Range).EntireColumn);
            return r;
        }

        private Excel.Range ActiveRange()
        {
            Excel.Worksheet activeSheet = (Excel.Worksheet)ExcelSelection.Application.ActiveSheet;
            Excel.Range r = ExcelSelection.Application.Intersect(activeSheet.UsedRange, ExcelSelection.Cells);
            return r;
        }

        private void RunUpdateCallback(UpdateCallback updateCallback)
        {
            log.DebugFormat("RunUpdateCallback handling key {0}", updateCallback.getKey());
            //string tempScriptName = "scriptObject";
            //ScriptExecutor.ExecuteScript(tempScriptName + "=Scripts.get('" + updateCallback.LoadScriptName + "');");
            string runnerName = "tmpRunner";
            //ScriptExecutor.ExecuteScript(runnerName + "=" + tempScriptName + ".runner();");
            ScriptExecutor.ExecuteScript(runnerName + ".clearValues();");
            try
            {


                foreach (string key in updateCallback.ParameterValues.Keys)
                {
                    //see if there's a vocabulary translation
                    string parameterValue = updateCallback.ParameterValues[key];
                    if (_scriptParameters.ContainsKey(key.ToUpper()) 
                        && _scriptParameters[key.ToUpper()].Vocabulary != null
                        && _scriptParameters[key.ToUpper()].Vocabulary.Count > 0)
                    {
                        if (!_scriptParameters[key.ToUpper()].Vocabulary.ContainsValue(parameterValue)
                            && _scriptParameters[key.ToUpper()].Vocabulary.ContainsKey(parameterValue))
                        {
                            string newParameterValue = _scriptParameters[key.ToUpper()].Vocabulary[parameterValue];
                            log.DebugFormat("Used vocabulary to translate {0} to {1}",
                                parameterValue, newParameterValue);
                            parameterValue = newParameterValue;
                        }
                    }
                    string paramValueScript = string.Format(runnerName + ".setValue('{0}', '{1}')",
                                key, parameterValue);
                    ScriptExecutor.ExecuteScript(paramValueScript);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
            string executionScript = runnerName + ".execute()" +
                                                     ".get(function(b){cresults['" +
                                                    updateCallback.getKey() + "']=b;window.external.Notify('"
                                                    + updateCallback.getKey() + "');})";
            ScriptExecutor.ExecuteScript(executionScript);
        }

        private void SetScriptParameters(Excel.Range range)
        {
            _scriptParameters = new Dictionary<string, ScriptParameter>();
            Excel.Application application = range.Application;

            Dictionary<string, Excel.Range> keys = GetKeys(range);
            //If the status isn't empty, skip this one
            string statusValue = GetProperty(keys, STATUS_KEY, "");

            Dictionary<string, string> paramValues = new Dictionary<string, string>();

            string tempScriptName = "tmpScript";
            ScriptExecutor.ExecuteScript(tempScriptName + "=Scripts.get('" + _scriptName + "');");
            string runnerName = "tmpRunner";
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
                            ScriptExecutor.ExecuteScript(tempScriptName
                            + ".getArgumentByName('" + key + "')");
                        ScriptParameter parameter = JSTools.GetScriptParameterFromString(param as string);
                        if(!string.IsNullOrWhiteSpace(parameter.cvType))
                        {
                            Dictionary<string, string> vocab = VocabUtils.BuildVocabularyDictionary(
                                GinasConfiguration.SelectedServer.ServerUrl, parameter.cvType);
                            parameter.Vocabulary = vocab;
                            log.Debug("Attached vocabulary for parameter " + key);
                        }

                        _scriptParameters.Add(key, parameter);
                    }
                }
            }
        }

        private void StartFirstUpdateCallback()
        {
            if (Callbacks.Count == 0) return;
            if (Callbacks.Values.First() is UpdateCallback)
            {
                UpdateCallback updateCallback = Callbacks.Values.First() as UpdateCallback;
                if(! updateCallback.getKey().Equals(_currentKey))
                {
                    if ((GinasConfiguration.DebugMode || StatusUpdater.GetDebugSetting())
                        && updateCallback.RunnerNumber % CONSOLE_CLEARANCE_INTERVAL == 0)
                    {
                        SaveAndClearDebugInfo();
                    }
                    _currentKey = updateCallback.getKey();
                    DateTime newExpirationDate = DateTime.Now.AddSeconds(GinasConfiguration.ExpirationOffset +
                        updateCallback.RunnerNumber * _secondsPerScript);
                    updateCallback.SetExpiration(newExpirationDate);
                    RunUpdateCallback(updateCallback);
                    updateCallback.Start();                    
                }
                else
                {
                    log.Debug("Skipped first update callback because it appears to be running already");
                }
            }
        }

        private void SaveAndClearDebugInfo()
        {
            log.Debug("Starting in SaveAndClearDebugInfo");
            string fileName = FileUtils.GetTemporaryFilePath("gsrs.excel.log");
            string content = (string) ScriptExecutor.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            FileUtils.WriteToFile(fileName, content);
            ScriptExecutor.ExecuteScript("GSRSAPI_consoleStack=[]");//clear the old stuff
        }
    }
}