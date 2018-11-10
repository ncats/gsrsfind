using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Controller;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using gov.ncats.ginas.excel.tools.Providers;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class SDFileProcessor : ControllerBase, IController
    {

        internal const string MOLFILE_END = "M  END";
        internal string[] SDF_FIELD_DELIMS = { ">  <", "> <" };
        internal const string SDF_RECORD_DELIM = "$$$$";
        internal const string MOLFILE_FIELD_NAME = "Molfile";
        internal const string SD_LOADING_SCRIPT_NAME = "Create Substance from SD File";
        internal const string DEFAULT_SUBSTANCE_TYPE = "chemical";
        internal const string FIELD_NAME_UNIQUENESS = "Uniqueness";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ScriptUtils scriptUtils = new ScriptUtils();
        private string _currentKey = string.Empty;
        private readonly float _secondsPerScript = 6;
        private List<SDFileRecord> _fileData;
        private SheetUtils _sheetUtils;
        private Dictionary<string, int> _fieldNamesToColumns;
        private Worksheet _worksheet;
        private int _scriptNumber = 0;
        internal const string STRUCTURE_SAVING_SCRIPT_NAME = "Save Temporary Structure";
        internal const int MOLFILE_ROW_HEIGHT = 15;

        public void HandleSDFileImport(string sdFilePath, Worksheet worksheet)
        {
            scriptUtils.ScriptExecutor = ScriptExecutor;
            Callbacks = new Dictionary<string, Callback>();
            log.DebugFormat("Starting in HandleSDFileImport with file {0}", sdFilePath);
            SDFileUtils sDFileUtils = new SDFileUtils();
            _fileData = sDFileUtils.ReadSdFile(sdFilePath);
            log.DebugFormat("read in {0} records", _fileData.Count);
            this._worksheet = worksheet;
            _sheetUtils = new SheetUtils();
            _sheetUtils.ImageOpsHandle = new ImageOps();

            List<string> fieldNames = SDFileUtils.GetUniqueFieldNames(_fileData);
            fieldNames.Insert(0, "BATCH:" + SD_LOADING_SCRIPT_NAME);
            fieldNames.Add(FIELD_NAME_UNIQUENESS);

            _fieldNamesToColumns = new Dictionary<string, int>();
            int col = 1;
            foreach (string fieldName in fieldNames)
            {
                _fieldNamesToColumns.Add(fieldName, col++);
            }
            log.DebugFormat("total columns: {0}", fieldNames.Count);

            ImageOps imageOps = new ImageOps();
            //create a title row
            _sheetUtils.TransferDataToRow(fieldNames.ToArray(), 1, 1, imageOps, worksheet, 0);
        }


        //transfers data to the sheet and initiates image generation and duplicate check for structures
        public void StartOperation()
        {
            log.DebugFormat("starting in {0}", MethodBase.GetCurrentMethod().Name);
            Authenticate();
            int row = 1;
            string tempScriptHandle = "tmpScript";
            ScriptExecutor.ExecuteScript(tempScriptHandle + "=Scripts.get('Save Temporary Structure');");
            string runnerName = "tmpRunner";

            ScriptExecutor.ExecuteScript(runnerName + "=" + tempScriptHandle + ".runner();");

            int currRecnum = 0;
            foreach (SDFileRecord record in _fileData)
            {
                string message = string.Empty;

                TwoRangeWrapper wrapper = _sheetUtils.TransferSDDataToRow(record.RecordData, 
                    _fieldNamesToColumns, ++row, _worksheet);
                Update2Callback callback = CreateUpdateCallback(wrapper.GetRange2(), wrapper.GetRange1(), ref message);
                log.DebugFormat("Created callback for SD record {0}", ++currRecnum);
            }
            _sheetUtils.SetColumnWidths(_worksheet, _fieldNamesToColumns.Values.ToList(), 25);
            _sheetUtils.SetRowHeights(_worksheet, MOLFILE_ROW_HEIGHT);
            StartFirstUpdateCallback();
        }

        public bool StartResolution(bool newSheet)
        {
            return true;
        }

        public void ContinueSetup()
        {
            log.DebugFormat("starting in {0}", MethodBase.GetCurrentMethod().Name);

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
            Update2Callback callback = Callbacks[resultsKey] as Update2Callback;
            callback.Execute(result);
            Callbacks.Remove(resultsKey);

            string statusMessage = string.Format("{0} records remain to process", Callbacks.Count);
            log.Debug(statusMessage);
            if (Callbacks.Count == 0)
            {
                log.Debug("Processing complete at " + DateTime.Now.ToShortTimeString());
                statusMessage = "Processing complete at " + DateTime.Now.ToShortTimeString();
                if (_timer != null)
                {
                    _timer.Close();
                    _timer.Stop();
                    _timer.Enabled = false;
                }
                KeepCheckingCallbacks = false;
                StatusUpdater.UpdateStatus(statusMessage);
                if (UIUtils.GetUserYesNo("Set up the necessary fields for substance creation?"))
                {
                    ManageSetupRemainingColumns();
                }
                else
                {
                    EndProcessNotification();
                }

                return true;
            }
            else
            {
                log.Debug("HandleResults will now call StartFirstUpdateCallback");
                StartFirstUpdateCallback();
            }
            return "true";
        }

        protected override void StartFirstUpdateCallback()
        {
            if (Callbacks.Count == 0) return;
            if (Callbacks.Values.First() is Update2Callback)
            {
                scriptUtils.AssignVocabularies();
                Update2Callback updateCallback = Callbacks.Values.First() as Update2Callback;
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
                    RunCallback(updateCallback);
                    updateCallback.Start();
                }
                else
                {
                    log.Debug("Skipped first update callback because it appears to be running already");
                }
            }
        }

        private void RunCallback(UpdateCallback updateCallback)
        {
            log.DebugFormat("RunCallback handling key {0}", updateCallback.getKey());
            scriptUtils.StartOneLoad(updateCallback.ParameterValues, updateCallback.getKey());
        }

        public Update2Callback CreateUpdateCallback(Range messageCell, Range molfileCell,
            ref string message)
        {
            Application application = molfileCell.Application;
            Update2Callback updateCallback = null;
            try
            {
                updateCallback = CallbackFactory.CreateUpdate2Callback(messageCell, molfileCell);
                updateCallback.RunnerNumber = ++_scriptNumber;
                updateCallback.GinasConfiguration = GinasConfiguration;

                string defaultValue = string.Empty;
                string parameterValue = (string)molfileCell.Value2;
                if (!string.IsNullOrWhiteSpace(parameterValue))
                {
                    string stringToReplace = ((char)92).ToString() + ((char)110).ToString();//molfiles
                    string replacement2 = "ꬷ";
                    string newLine = ((char)10).ToString();
                    parameterValue = parameterValue.Replace("'", "\\'").Replace(stringToReplace, replacement2).Replace("\n", "\\n").Replace(newLine, "\\n");

                    updateCallback.ParameterValues.Add("molfile", parameterValue);
                    log.DebugFormat("In {0}, setting parm {1} to {2}",
                        MethodBase.GetCurrentMethod().Name,
                        "molfile", parameterValue);
                }

                string callbackKey = JSTools.RandomIdentifier();
                DateTime newExpirationDate = DateTime.Now.AddSeconds(GinasConfiguration.ExpirationOffset +
                    (Callbacks.Count * Callbacks.Count * _secondsPerScript));//trying a quadratic term
                updateCallback.SetExpiration(newExpirationDate);
                updateCallback.SetKey(callbackKey);
                updateCallback.LoadScriptName = STRUCTURE_SAVING_SCRIPT_NAME;
                string script = "tmpRunner.execute().get(function(b){cresults['"
                    + callbackKey + "']=b;window.external.Notify('" + callbackKey + "');})";
                updateCallback.SetScript(script);
                Callbacks.Add(callbackKey, updateCallback);
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

        public void ManageSetupRemainingColumns()
        {
            if (_sheetUtils == null)
            {
                log.DebugFormat("{0} instantiated sheetUtilities", MethodBase.GetCurrentMethod().Name);
                _sheetUtils = new SheetUtils();
            }
            if (scriptUtils == null)
            {
                log.DebugFormat("{0} instantiated scriptUtilities", MethodBase.GetCurrentMethod().Name);
                scriptUtils = new ScriptUtils();
            }
            scriptUtils.ScriptName = SD_LOADING_SCRIPT_NAME;
            scriptUtils.ScriptExecutor = ScriptExecutor;
            _sheetUtils.ScriptExecutor = ScriptExecutor;
            scriptUtils.StartVocabularyRetrievals();
            
        }

        public override void CancelOperation(string reason)
        {
            UIUtils.ShowMessageToUser(reason + Environment.NewLine + "Please check on the server and try again.");
        }

        protected override void EndProcessNotification()
        {
            StatusUpdater.Complete();
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
                _sheetUtils.SetupRemainingColumns(_worksheet, ScriptExecutor, scriptUtils);
                EndProcessNotification();
            }
        }
    }
}
