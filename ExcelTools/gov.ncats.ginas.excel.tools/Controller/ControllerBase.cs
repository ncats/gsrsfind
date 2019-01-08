using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using gov.ncats.ginas.excel.tools.Model;

using Excel = Microsoft.Office.Interop.Excel;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using gov.ncats.ginas.excel.tools.Utils;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public class ControllerBase : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected int _totalBatches;
        protected ScriptUtils scriptUtils = new ScriptUtils();

        public ControllerBase()
        {
            GinasConfiguration = FileUtils.GetGinasConfiguration();
        }

        public ControllerBase()
        {
            GinasConfiguration = FileUtils.GetGinasConfiguration();
        }

        public void SetScriptExecutor(IScriptExecutor scriptExecutor)
        {
            ScriptExecutor = scriptExecutor;
        }

        protected IScriptExecutor ScriptExecutor;

        protected static int _checkInterval = 30;
        protected Timer _timer;
        protected int _totalScripts = 0;
        protected Excel.Range ExcelSelection;
        protected Excel.Window ExcelWindow;
        protected Queue<string> ScriptQueue;
        protected int _NumTimesFoundNoActives = 0;
        protected const int MAX_TIMES_NO_ACTIVE = 4;
        protected const int CONSOLE_CLEARANCE_INTERVAL = 50;

        protected GinasToolsConfiguration GinasConfiguration
        {
            get;
            set;
        }

        protected Dictionary<string, Callback> Callbacks;

        public void SetExcelWindow(Excel.Window window)
        {
            ExcelWindow = window;
        }

        public OperationType CurrentOperationType
        {
            get;
            set;
        }

        protected int ItemsPerBatch
        {
            get;
            set;
        }

        public bool KeepCheckingCallbacks
        {
            get;
            set;
        }

        public virtual void CancelOperation(string reason)
        {

        }

        protected IStatusUpdater StatusUpdater;

        public void SetStatusUpdater(IStatusUpdater statusUpdater)
        {
            StatusUpdater = statusUpdater;
        }

        public int GetBatchSize()
        {
            if (GinasConfiguration != null && GinasConfiguration.BatchSize > 0)
            {
                return GinasConfiguration.BatchSize;
            }

            int batchSize;
            string batchSizeRaw = Properties.Resources.DefaultBatchSize;
            if (!int.TryParse(batchSizeRaw, out batchSize))
            {
                batchSize = 30;
            }
            return batchSize;
        }

        public void Dispose()
        {
            try
            {
                _timer.Dispose();
            }
            catch (Exception ignore)
            {
                Debug.WriteLine("Error disposing timer: " + ignore.Message);
            }
        }

        //public GinasToolsConfiguration ToolsConfiguration = Utils.FileUtils.GetGinasConfiguration();

        public void LaunchFirstScript()
        {
            DateTime startLaunch = DateTime.Now;
            if (ScriptQueue.Count > 0)
            {
                log.Debug("About to run script from queue. Script queue count: "
                    + ScriptQueue.Count + " at " + DateTime.Now);
                StartCorrespondingCallback(ScriptQueue.Peek());
                TimeSpan afterStartCallback = DateTime.Now.Subtract(startLaunch);
                log.Debug(" through StartCorrespondingCallback: " + afterStartCallback.Milliseconds
                    + " milliseconds");
                ScriptExecutor.ExecuteScript(ScriptQueue.Dequeue());
                TimeSpan afterExecuteScript = DateTime.Now.Subtract(startLaunch);
                log.Debug(" ExecuteScript: " + afterExecuteScript.Milliseconds + " milliseconds");

                StatusUpdater.UpdateStatus("Processing batch " + (_totalBatches - ScriptQueue.Count)
                    + " of " + _totalBatches);
            }
            else
            {
                StatusUpdater.UpdateStatus("All records have been processed");
                log.Debug("No scripts in queue. ");
            }
        }

        protected void StartCorrespondingCallback(string script)
        {
            // locate the key
            int pos1;
            int pos2;
            string key;
            Callback cb;
            pos1 = script.IndexOf("'");
            pos2 = script.IndexOf("'", pos1 + 1);
            key = script.Substring(pos1 + 1, (pos2 - pos1 - 1));
            if (Callbacks.ContainsKey(key))
            {
                cb = Callbacks[key];
                cb.Start();
                DateTime newExpirationDate = DateTime.Now.AddSeconds(GetExpirationOffset());
                log.DebugFormat("StartCorrespondingCallback about to set expiration to {0}",
                    newExpirationDate.ToLongTimeString());
                if (cb is BatchCallback)
                {
                    (cb as BatchCallback).SetExpiration(newExpirationDate);
                }
                else
                {
                    cb.SetExpiration(newExpirationDate);
                }
                log.DebugFormat(" ... found callback for key {0}, marked it as started and set expiration date to {1}",
                     key, newExpirationDate.ToLongTimeString());
            }
        }

        protected float GetExpirationOffset()
        {
            if (GinasConfiguration.ExpirationOffset > 0)
            {
                return GinasConfiguration.ExpirationOffset;
            }
            return 120;
        }

        public void ReceiveVocabulary(string rawVocab)
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
                CompleteSheet();
                if( CurrentOperationType != OperationType.ProcessApplication)
                {
                    log.Debug("about to call StatusUpdater.Complete");
                    StatusUpdater.Complete();
                }
            }
        }

        protected virtual void StartFirstUpdateCallback()
        {

        }

        protected virtual void EndProcessNotification()
        {

        }

        public void CheckUpdateCallbacks(Object source, ElapsedEventArgs e)
        {
            log.Debug("Starting in checkUpdateCallbacks");

            bool haveActive = false;

            List<string> callbackKeysToRemove = new List<string>();
            log.Debug("Total callbacks at start: " + Callbacks.Count);
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
                log.Debug("No active callbacks detected");
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
                    log.Debug("; will now call StartFirstUpdateCallback");
                    StartFirstUpdateCallback();
                    _NumTimesFoundNoActives = 0;
                }
                else
                {
                    _NumTimesFoundNoActives++;
                }
            }

            log.Debug("end of checkUpdateCallbacks ");
        }

        protected void DecremementTotalScripts()
        {
            if (_totalScripts > 0) _totalScripts--;
        }

        protected void SaveAndClearDebugInfo()
        {
            log.Debug("Starting in SaveAndClearDebugInfo");
            string fileName = FileUtils.GetTemporaryFilePath("gsrs.excel.log");
            string content = (string)ScriptExecutor.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            FileUtils.WriteToFile(fileName, content);
            ScriptExecutor.ExecuteScript("GSRSAPI_consoleStack=[]");//clear the old stuff
        }

        protected void Authenticate()
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

        public virtual void CompleteSheet()
        {
            log.Debug("Base class CompleteSheet");
        }

        protected virtual void StartFirstUpdateCallback()
        {

        }

        protected virtual void EndProcessNotification()
        {

        }

        public void CheckUpdateCallbacks(Object source, ElapsedEventArgs e)
        {
            log.Debug("Starting in checkUpdateCallbacks");

            bool haveActive = false;

            List<string> callbackKeysToRemove = new List<string>();
            log.Debug("Total callbacks at start: " + Callbacks.Count);
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
                log.Debug("No active callbacks detected");
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
                    log.Debug("; will now call StartFirstUpdateCallback");
                    StartFirstUpdateCallback();
                    _NumTimesFoundNoActives = 0;
                }
                else
                {
                    _NumTimesFoundNoActives++;
                }
            }

            log.Debug("end of checkUpdateCallbacks ");
        }

        protected void DecremementTotalScripts()
        {
            if (_totalScripts > 0) _totalScripts--;
        }

        protected void SaveAndClearDebugInfo()
        {
            log.Debug("Starting in SaveAndClearDebugInfo");
            string fileName = FileUtils.GetTemporaryFilePath("gsrs.excel.log");
            string content = (string)ScriptExecutor.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            FileUtils.WriteToFile(fileName, content);
            ScriptExecutor.ExecuteScript("GSRSAPI_consoleStack=[]");//clear the old stuff
        }

        protected void Authenticate()
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

    }
}
