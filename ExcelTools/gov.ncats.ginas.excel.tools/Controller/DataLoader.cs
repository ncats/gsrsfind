using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private IScriptExecutor ScriptExecutor;
        private int _scriptsProcessed = 0;
        private bool _notified = false;
        private DateTime _resolutionStart;

        public void SetScriptExecutor(IScriptExecutor scriptExecutor)
        {
            ScriptExecutor = scriptExecutor;
        }


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

        public void ContinueSetup()
        {
            string msg = "";

            Callback cb = CreateUpdateCallback(ExcelSelection.Application.ActiveCell, true, msg);
            if (!string.IsNullOrEmpty(msg)) StatusUpdater.UpdateStatus(msg);
            if (cb != null) ScriptExecutor.ExecuteScript("showPreview(tmpRunner)");
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
            _scriptsProcessed = 0;

            Callbacks = new Dictionary<string, Callback>();
            if( r == null || r.Count == 0)
            {
                UIUtils.ShowMessageToUser("No data selected to execute. Please enter data, and select the rows to enter");
                return;
            }
            _resolutionStart = DateTime.Now;
            _totalScripts = r.Rows.Count;
            Debug.Print("set totalScripts to row count " + _totalScripts);
            StatusUpdater.UpdateStatus("Processing " + _totalScripts + " updates");

            foreach(Excel.Range cell in r)
            {
                Callback cb = DoScript(cell);
                cb.Wait();
            }
            LaunchCheckJob();
        }


        public Callback DoScript(Excel.Range arow)
        {
            Callback cb = CreateUpdateCallback(arow);
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
            string script = "tmpRunner"
                + ".execute()"
                + ".get(function(b){cresults['"
                + cb.getKey() + "']=b;window.external.Notify('"
                + cb.getKey() + "');})";
            ScriptExecutor.ExecuteScript(script);
            cb.setScript(script);

            Callbacks.Add(cb.getKey(), cb);
            Debug.Print("Added callback " + cb.getKey() + "; total: " + Callbacks.Count);
            return cb;
        }


        public UpdateCallback CreateUpdateCallback(Excel.Range arow, bool allowFinished = false,
            string message = "")
        {
            Excel.Application application = arow.Application;
            Dictionary<string, Excel.Range> keys = new Dictionary<string, Excel.Range>();

            float secondsPerScript = 30.0f;
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
            //    'this is a trick to make the script accessible
            ScriptExecutor.ExecuteScript("tmpScript=Scripts.get('" + scriptName + "');");
            ScriptExecutor.ExecuteScript("tmpRunner=tmpScript.runner();");

            foreach (string key in keys.Keys)
            {
                if (!string.IsNullOrWhiteSpace(GetProperty(keys, key, "")))
                {
                    string testScript = "tmpScript.hasArgumentByName('" + key + "')";
                    object testValue = ScriptExecutor.ExecuteScript(testScript);
                    Debug.WriteLine("value: " + testValue);
                    if (testValue is string && (testValue as string).Equals("true", 
                            StringComparison.CurrentCultureIgnoreCase))
                    {
                        object param = 
                            ScriptExecutor.ExecuteScript("tmpScript.getArgumentByName('" + key + "')");
                        ScriptParameter parameter = JSTools.GetScriptParameterFromString(param as string);
                        string parameterValue = (keys[key].Text != null) ? keys[key].Text :
                            string.Empty;
                        //escape characters that causes errors in JavaScript interpreter
                        parameterValue = parameterValue.Replace("'", "\\'").Replace("\n", "\\n");
                        string paramValueScript = string.Format("tmpRunner.setValue('{0}', '{1}')",
                            parameter.key, parameterValue);
                        ScriptExecutor.ExecuteScript(paramValueScript);
                    }
                }
            }
            string tempVal = JSTools.RandomIdentifier();
            UpdateCallback updateCallback = CallbackFactory.CreateUpdateCallback(keys[STATUS_KEY]);
            DateTime newExpirationDate = DateTime.Now.AddSeconds(secondsPerScript);
            updateCallback.setExpiration(newExpirationDate);
            updateCallback.setKey(tempVal);
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
            string interval = "00:00:" + String.Format("{0:00}", _checkInterval);
            //'assume interval is less than 60 seconds!

            Debug.Print("LaunchCheckJob using interval " + interval);
            _timer = new Timer(_checkInterval * secondsToMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += CheckUpdateCallbacks;
            Debug.Print("(checkUpdateCallbacks)");
            _timer.Start();
        }

        public void CheckUpdateCallbacks(Object source, ElapsedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            String message;
            bool haveActive;

            List<string> callbackKeysToRemove = new List<string>();
            haveActive = false;
            Debug.Print("Starting in checkUpdateCallbacks");
            if (Callbacks == null || Callbacks.Count == 0)
            {
                Debug.Print("callbacks collection is empty in CheckUpdateCallbacks");
                KeepCheckingCallbacks = false;
                _timer.Stop();
                return;
            }

            Debug.Print("Total callbacks: " + Callbacks.Count);
            message = "callback total: " + Callbacks.Count;
            //'go through individual callbacks
            foreach (string cbKey in Callbacks.Keys)
            {
                Callback cb = Callbacks[cbKey];
                if (cb.hasStarted())
                {
                    if (cb is UpdateCallback)
                    {
                        UpdateCallback updateCb = cb as UpdateCallback;
                        string itemMessage = "looking at updateCallback " + updateCb.getKey();
                        if (!updateCb.isExpiredNow())
                        {
                            haveActive = true;
                            itemMessage = itemMessage + " active";
                        }
                        else
                        {
                            itemMessage = itemMessage + " expired";
                            callbackKeysToRemove.Add(cbKey);
                        }

                        Debug.Print(itemMessage);
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
            if (Callbacks.Count == 0)
            {
                haveActive = false;
            }

            Debug.Print(message);
            if (!(KeepCheckingCallbacks && haveActive))
            {
                _timer.Stop();
                Debug.Print("stopped timer");
            }


            if (_totalScripts == 0)
            {
                EndProcessNotification();
            }
            Debug.Print("end of checkUpdateCallbacks which took " + sw.ElapsedMilliseconds);
            sw.Stop();
        }

        public object HandleResults(string resultsKey, string message)
        {
            Debug.WriteLine(string.Format("HandleResults received message {0} for key {1}",
                message, resultsKey));
            GinasResult result = JSTools.GetGinasResultFromString(message);
            UpdateCallback resolverCallback = Callbacks[resultsKey] as UpdateCallback;
            resolverCallback.Execute(result.message);
            Callbacks.Remove(resultsKey);

            string statusMessage = string.Format("{0} items to go", ScriptQueue.Count);
            if (ScriptQueue.Count == 0)
            {
                statusMessage = "Processing complete!";
                StatusUpdater.Complete();
            }
            StatusUpdater.UpdateStatus(statusMessage);
            return "true";
        }

        private void EndProcessNotification()
        {
            //dialog itself will handle saving of debug info.
            StatusUpdater.UpdateStatus("Completed");
            this._notified = true;
            StatusUpdater.Complete();
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
 

    }
}