using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Timers;
using System.Configuration;

using gov.ncats.ginas.excel.tools.UI;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Providers;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Model.Callbacks;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public class DataLoader : ControllerBase, IController
    {
        public const string LOAD_OPERATION = "BATCH";
        internal const string STATUS_KEY = "IMPORT STATUS";
        private DateTime _resolutionStart;
        private bool _notified = false;
        private static int _scriptNumber = 0;
        private string _scriptName;
        private readonly float _secondsPerScript = 10;
        private string _currentKey = string.Empty;
        static readonly Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); // Add an Application Setting.        
        private bool _gotVocabularies = false;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private ScriptUtils scriptUtils;

        private bool _assignedVocabs = false;

        private Dictionary<int, string> ColumnKeys = null;
        private const int MAX_RECOMMENDED_ROWS = 2000;

        /// <summary>
        /// First method to call for outside classes
        /// </summary>
        public void StartOperation()
        {
            ScriptQueue = new Queue<string>();
            CurrentOperationType = OperationType.Loading;
            ExcelSelection = (Excel.Range)ExcelWindow.Application.Selection;

            RetrievalForm form = new RetrievalForm();
            form.Controller = this;
            form.CurrentOperationType = OperationType.Loading;
            form.Visible = false;
            SetStatusUpdater(form);
            ScriptExecutor = form;
            scriptUtils = new ScriptUtils();
            scriptUtils.ScriptExecutor = ScriptExecutor;
            _gotVocabularies = false;

            form.ShowDialog();

            Excel.Range arange = GetExecutingRange();
            if (arange == null)
            {
                StatusUpdater.UpdateStatus("(0) records to execute, please enter data and select the rows to enter");
            }
            else
            {
                StatusUpdater.UpdateStatus("(" + arange.Count + ") records to execute");
            }
            _notified = false;
            _assignedVocabs = false;
        }

        public void StartSheetCreation(Excel.Window window)
        {
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
            log.Debug("Starting in ContinueSetup");
            Callbacks = new Dictionary<string, Callback>();
            string msg = "";

            //grab script name from the top row
            _scriptName = GetScriptName(ExcelSelection.Application.ActiveCell);
            log.DebugFormat("   got script name: {0}", _scriptName);
            if( string.IsNullOrWhiteSpace(_scriptName))
            {
                UIUtils.ShowMessageToUser(Properties.Resources.No_script_in_upper_corner);
                return;
            }
            scriptUtils.ScriptName = _scriptName;
            SetScriptParameters(ExcelSelection.Application.ActiveCell);
            int totalSelectedRows = (ExcelSelection.Application.Selection as Excel.Range).Rows.Count;
            if (totalSelectedRows > MAX_RECOMMENDED_ROWS)
            {
                log.DebugFormat("user has selected {0} rows to process, more than the recommended limit of {1} 130",
                    totalSelectedRows, MAX_RECOMMENDED_ROWS);
                if (!UIUtils.GetUserYesNo(
                    string.Format("You are processing more records than the recommended limit ({0}).  Excel may become unstable!\nAre you sure you want to proceed?",
                    MAX_RECOMMENDED_ROWS)))
                {
                    log.Info("user declined to proceed");
                    StatusUpdater.Complete();
                    return;
                }
            }

            Callback cb = CreateInitialUpdateCallback(ExcelSelection.Application.ActiveCell,
                ref msg, true);
            if (!string.IsNullOrEmpty(msg)) StatusUpdater.UpdateStatus(msg);
            else
            {
                msg = "Total selected rows: " + totalSelectedRows;
                log.Debug(msg);
            }
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
                if(scriptUtils == null)
                {
                    scriptUtils = new ScriptUtils();
                }
                scriptUtils.ScriptName = selectedScriptName;
                scriptUtils.ScriptExecutor = ScriptExecutor;
                log.DebugFormat("{0} setting script name to {1}", MethodBase.GetCurrentMethod().Name,
                    selectedScriptName);
                scriptUtils.StartVocabularyRetrievals();
                //when there are no vocabularies to retrieve, move to the next step immediately
                if(scriptUtils.ExpectedVocabularies.Count==0)
                {
                    SheetUtils sheetUtils = new SheetUtils();
                    sheetUtils.Configuration = GinasConfiguration;
                    sheetUtils.CreateSheet(ExcelWindow.Application.ActiveWorkbook, scriptUtils,
                        ScriptExecutor, GinasConfiguration.SortVocabsAlphabetically);
                    StatusUpdater.Complete();
                }
            }
            else
            {
                Authenticate();
                StartLoading(ExcelSelection);
            }
            return true;
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
                log.DebugFormat("Processing cells {0} with text {1}", row.Address, row.Value2);
                string cellText = row.Text as string;
                if (row.Cells.Count == 1 && string.IsNullOrWhiteSpace(cellText)) continue;
                Callback cb = CreateUpdateCallbackForExecution(row);
                if( cb==null)
                {
                    log.Debug("Perceived cancellation");
                    StatusUpdater.UpdateStatus("User has cancelled operation");
                    EndProcessNotification();
                    return;
                }
            }
            _scriptName = GetScriptName(ExcelSelection.Application.ActiveCell);
            log.DebugFormat("{0} setting script name to {1}", MethodBase.GetCurrentMethod().Name,
                _scriptName);

            scriptUtils.ScriptName = _scriptName;
            if( !_gotVocabularies)
            {
                log.Debug("calling scriptUtils.StartVocabularyRetrievals()");
            scriptUtils.StartVocabularyRetrievals();
                _gotVocabularies = true;
            }
            if(scriptUtils.ExpectedVocabularies.Count == 0)
            {
                StartFirstUpdateCallback();
                LaunchCheckJob();
            }
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
                if( msg.StartsWith("Will not execute row, because") && msg.EndsWith("cancel"))
                {
                    return null;
                }
                return cb;
            }
            string script = "tmpRunner"
                + ".execute()"
                + ".get(function(b){cresults['"
                + cb.getKey() + "']=b;sendMessageBackToCSharp('"
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
            UpdateCallback updateCallback = null;
            try
            {
               if(ColumnKeys==null) ColumnKeys= GetColumnKeys(arow);
                //If the status isn't empty, skip this one
                string statusValue = GetPropertyValue(arow, STATUS_KEY, "");
                if (!string.IsNullOrWhiteSpace(statusValue))
                {
                    message = "Will not execute row, because " + STATUS_KEY + " is not empty";
                    log.Debug(message);
                    if ( !allowFinished)
                    {
                    bool continuation = UIUtils.GetUserYesNo(message + "; Continue with next row?");
                    if (!continuation) message += "; cancel";
                    }
                    if (!allowFinished) return null;
                }
                _scriptNumber++;

                Dictionary<string, string> paramValues = new Dictionary<string, string>();
                string runnerName = "tmpRunner";
                foreach (string key in scriptUtils.ScriptParameters.Keys)
                {
                    log.Debug("Looking at ScriptParameters.Keys " + key);
                    string defaultValue = string.Empty;
                    if (scriptUtils.ScriptParameters[key].IsBoolean()) defaultValue = "FALSE";
                    string parameterValue = GetPropertyValue(arow, key, defaultValue);
                    if (!string.IsNullOrWhiteSpace(parameterValue))
                    {
                        ScriptParameter parameter = scriptUtils.ScriptParameters[key];
                        if( !parameter.name.Contains("FILE PATH"))
                        {
                            //escape characters that causes errors in JavaScript interpreter
                            string stringToReplace = ((char)92).ToString() + ((char)110).ToString();//molfiles
                            string replacement2 = "ꬷ";
                            string newLine = ((char)10).ToString();
                            parameterValue = parameterValue.Replace("'", "\\'").Replace(stringToReplace, replacement2).Replace("\n", "\\n").Replace(newLine, "\\n");
                        }
                        if (allowFinished)
                        {
                            string paramValueScript = string.Format(runnerName + ".setValue('{0}', '{1}')",
                                parameter.key, parameterValue);
                            ScriptExecutor.ExecuteScript(paramValueScript);
                        }
                        paramValues.Add(parameter.key, parameterValue);
                        log.DebugFormat("In {0}, setting parm {1} to {2}",
                            MethodBase.GetCurrentMethod().Name,
                            parameter.key, parameterValue);
                    }
                }
                string tempVal = JSTools.RandomIdentifier();
                while (Callbacks != null && Callbacks.ContainsKey(tempVal))
                {
                    tempVal = JSTools.RandomIdentifier(10, true);
                }
                int col = ColumnKeys.FirstOrDefault(k => k.Value.Equals(STATUS_KEY)).Key;
                if(col <= 0)
                {
                    string errorMessage = "Error! A worksheet  for data loading/editing must have a column called 'IMPORT STATUS'";
                    log.Error(errorMessage);
                    message = errorMessage;
                    UIUtils.ShowMessageToUser(errorMessage);
                    return null;
                }
                string rangeDesc = SheetUtils.GetColumnName(col) + arow.Row;
                updateCallback = CallbackFactory.CreateUpdateCallback(arow.Worksheet.Range[rangeDesc]);
                updateCallback.RunnerNumber = _scriptNumber;
                DateTime newExpirationDate = DateTime.Now.AddSeconds(GinasConfiguration.ExpirationOffset +
                    (Callbacks.Count * Callbacks.Count * _secondsPerScript));//trying a quadratic term
                updateCallback.SetExpiration(newExpirationDate);
                updateCallback.SetKey(tempVal);
                updateCallback.ParameterValues = paramValues;
                updateCallback.LoadScriptName = _scriptName;
                int totalSelectedRows = (application.Selection as Excel.Range).Rows.Count;
                message = "Total selected rows: " + totalSelectedRows;
                log.Debug(message);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error creating update callback: {0}", ex.Message);
                log.Debug(ex.StackTrace);
                message = ex.Message;
            }

            return updateCallback;
        }


        private Dictionary<int, string> GetColumnKeys(Excel.Range row)
        {
            Dictionary<int, string> keys = new Dictionary<int, string>();

            Excel.Application application = row.Application;
            Excel.Worksheet asheet = row.Worksheet;
            Excel.Range headerRow = application.Intersect(((Excel.Range) asheet.Rows[1]).EntireRow, asheet.UsedRange);
            foreach (Excel.Range hcell in headerRow)
            {                
                if (hcell.Value2 != null)
                {
                    string cellTextUpper = (hcell.Value2 as string).ToUpper();
                    if (!keys.ContainsValue(cellTextUpper))
                    {
                        keys.Add(hcell.Column, cellTextUpper);
                    }
                }
            }
            return keys;
        }

        private Dictionary<string, string> GetValues(Dictionary<int, string> keys, Excel.Range inputRow)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            Excel.Range rowToProcess = inputRow.Application.Intersect(inputRow.Worksheet.UsedRange, inputRow);
            foreach(Excel.Range dataCell in rowToProcess)
            {
                if( dataCell.Value2 != null && keys.ContainsKey(dataCell.Column))
                {
                    string key = keys[dataCell.Column];
                    string value = dataCell.Value2 as string;
                    values.Add(key, value);
                }
            }
            return values;
        }

        private Dictionary<string, Excel.Range> GetKeys(Excel.Range row)
        {
            Dictionary<string, Excel.Range> keys = new Dictionary<string, Excel.Range>();

            Excel.Application application = row.Application;
            Excel.Worksheet asheet = row.Worksheet;
            Excel.Range headerRow = application.Intersect(asheet.Range["1:1"], asheet.UsedRange);
            Excel.Range crow = asheet.Range[row.Row + ":" + row.Row];
            //string f = (string)application.Intersect(headerRow, asheet.Range["A:A"]).Value2;

            foreach (Excel.Range hcell in headerRow)
            {
                Excel.Range r = application.Intersect(crow, asheet.Range["A:A"].Offset[0, hcell.Column - 1]);
                if (hcell.Value2 != null)
                {
                    string cellTextUpper = (hcell.Value2 as string).ToUpper();
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
            if( headerRow == null || headerRow.Cells.Count ==0)
            {
                log.Debug("Header row empty");
                return string.Empty;
            }
            Excel.Range scriptNameRange = application.Intersect(headerRow, asheet.Range["A:A"]);
            if( scriptNameRange == null || scriptNameRange.Cells.Count==0)
            {
                log.Debug("script name cell empty");
                return string.Empty;
            }
            string upperCornerText = (string)scriptNameRange.Value2;
            if (string.IsNullOrWhiteSpace(upperCornerText))
            {
                log.Warn("No script name found on sheet!");
                return string.Empty;
            }
            string[] tokens = upperCornerText.Split(':');
            if (tokens.Length < 2)
            {
                log.Warn("No script name found on sheet (tokens)");
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(tokens[0]) && !tokens[0].Equals(LOAD_OPERATION))
            {
                UIUtils.ShowMessageToUser("Header row must start with \"BATCH\"");
                return null;
            }
            string tempScriptName = "tmpScript";
            log.DebugFormat("{0} determined script name {1} ",
                MethodBase.GetCurrentMethod().Name, tokens[1]);
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
                if (range != null && range.Value2 != null)
                {
                    if (range.Value2 is string || range.Value2 is bool ||  range.Value2 is double || range.Value2 is int)
                    {
                        return range.Value2.ToString();//.ToUpper();
                    }
                }
            }
            return def;
        }

        private string GetPropertyValue(Excel.Range row, string key, string def)
        {
            if (ColumnKeys.ContainsValue(key))
            {
                int col = ColumnKeys.FirstOrDefault(k => k.Value.Equals(key)).Key;
                Excel.Range dataRow = row.Worksheet.Range[SheetUtils.GetColumnName(col) + row.Row];
                if (dataRow.Value2 == null) return def;
                String propertyValue= dataRow.Value2.ToString();
                if (!key.ToUpper().Contains("MOLFILE") && config.AppSettings.Settings["trimTextInputForUpdate"].Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    propertyValue = propertyValue.Trim();
                }
                return propertyValue;
            }
            return def;
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


        public object HandleResults(string resultsKey, string message)
        {
            log.DebugFormat("HandleResults received message {0} for key {1}",
                 message, resultsKey);
            if (resultsKey.Equals(_currentKey))
            {
                _currentKey = string.Empty;
            }
            GinasResult result = JSTools.GetGinasResultFromString(message);
            if( !Callbacks.ContainsKey(resultsKey))
            {
                string msg = string.Format("Warning! Handling results for key '{0}' but this key was not found in the callback collection",
                    resultsKey);
                log.Warn(msg);
                return "false";
            }
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

        public new void ReceiveVocabulary(string rawVocab)
        {
            log.DebugFormat("ReceiveVocabulary will handle vocabulary ");
            int delim1 = rawVocab.IndexOf(":");
            int delim2 = rawVocab.IndexOf(":", delim1 + 1);
            if (delim1 < 0 || delim2 < 0) return;
            string vocabName = rawVocab.Substring(delim1 + 1, (delim2 - delim1 - 1));
            rawVocab = rawVocab.Substring(delim2 + 1);
            Vocab vocab = JSTools.GetVocabFromString(rawVocab);

            scriptUtils.Vocabularies.Add(vocabName, vocab);
            scriptUtils.MarkVocabArrived(vocabName);
            log.DebugFormat("adding vocabulary for {0}. Remaining: {1}",
                vocabName, scriptUtils.ExpectedVocabularies.Count);
            if (scriptUtils.ExpectedVocabularies.Count == 0)
            {
                if (CurrentOperationType == OperationType.ShowScripts)
                {
                    SheetUtils sheetUtils = new SheetUtils();
                    sheetUtils.Configuration = GinasConfiguration;
                    sheetUtils.CreateSheet(ExcelWindow.Application.ActiveWorkbook, scriptUtils,
                        ScriptExecutor, GinasConfiguration.SortVocabsAlphabetically);
                    log.Debug("sheet created");
                    StatusUpdater.Complete();
                }
                else
                {
                    log.Debug("Going to start first callback");
                    try
                    {
                        //start data loading
                        StartFirstUpdateCallback();
                        LaunchCheckJob();
                    }
                    catch(Exception ex)
                    {
                        log.ErrorFormat("Error starting loading process: {0}", ex.Message);
                        log.Error(ex.StackTrace);
                    }
                }
            }
        }

        public bool OkToWrite(int numberOfColumns)
        {
            return true;
        }

        protected override void EndProcessNotification()
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
                activeSheet.Range["2:" + r.Application.Intersect(activeSheet.UsedRange, 
                activeSheet.Range["A:A"]).Cells.Count]));
            r = (r.Application.Intersect(r, r.Cells[1] as Excel.Range).EntireColumn);
            return r;
        }

        private Excel.Range ActiveRange()
        {
            Excel.Worksheet activeSheet = (Excel.Worksheet)ExcelSelection.Application.ActiveSheet;
            Excel.Range r = ExcelSelection.Application.Intersect(activeSheet.UsedRange, 
                ExcelSelection.Cells);
            return r;
        }

        private void RunUpdateCallback(UpdateCallback updateCallback)
        {
            log.DebugFormat("RunUpdateCallback handling key {0}", updateCallback.getKey());
            scriptUtils.StartOneLoad(updateCallback.ParameterValues, updateCallback.getKey(), this.GinasConfiguration);
        }

        private void SetScriptParameters(Excel.Range range)
        {
            Excel.Application application = range.Application;

            Dictionary<string, Excel.Range> keys = GetKeys(range);
            //Dictionary<string, string> paramValues = new Dictionary<string, string>();

            string tempScriptName = "tmpScript";
            log.DebugFormat("{0} using script name {1} ",
                MethodBase.GetCurrentMethod().Name, _scriptName);
            ScriptExecutor.ExecuteScript(tempScriptName + "=Scripts.get('" + _scriptName + "');");
            string runnerName = "tmpRunner";
            ScriptExecutor.ExecuteScript(runnerName + "=" + tempScriptName + ".runner();");

            scriptUtils.BuildScriptParameters(keys.Keys);
        }

        protected override void StartFirstUpdateCallback()
        {
            if (Callbacks.Count == 0) return;
            if (Callbacks.Values.First() is UpdateCallback)
            {
                if(!_assignedVocabs)
                {
                scriptUtils.AssignVocabularies();
                    _assignedVocabs = true;
                }
                UpdateCallback updateCallback = Callbacks.Values.First() as UpdateCallback;
                if (!updateCallback.getKey().Equals(_currentKey))
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
    }
}