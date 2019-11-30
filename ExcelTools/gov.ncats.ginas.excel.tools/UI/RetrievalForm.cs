using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Controller;
using gov.ncats.ginas.excel.tools.Model;


namespace gov.ncats.ginas.excel.tools.UI
{
    [ComVisible(true)]
    public partial class RetrievalForm : Form, IStatusUpdater, IScriptExecutor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        List<string> _expectedTitles = new List<string>();
        string _baseUrl;
        const string COMPLETED_DOCUMENT_TITLE = "ginas Tools";
        const string NAVIGATION_CANCELED = "Navigation Canceled";
        GinasToolsConfiguration _configuration = null;
        string _scriptToRunUponCompletion;
        bool _savedDebugInfo;
        string _initLoadingErrorMessage = "Error loading initial ginas page";
        string _secondMessage = "Close dialog and try again or notify your administrator";

        public RetrievalForm(string noConnectionMessage, string secondMessage)
        {
            _initLoadingErrorMessage = noConnectionMessage;
            _secondMessage = secondMessage;
            PerformSetup();
        }
        public RetrievalForm()
        {
            PerformSetup();
        }

        public void SetInitialLoadingErrorMessage(string newMessage)
        {
            _initLoadingErrorMessage = newMessage;
        }

        public void SetSecondMessage(string secondMessage)
        {
            _secondMessage = secondMessage;
        }

        private void PerformSetup()
        {
            IsReady = false;
            _expectedTitles.Add("InXight API");
            _expectedTitles.Add("g-srs");
            _expectedTitles.Add("Sequence Search");
            log.Debug("Starting in RetrievalForm");
            
            Visible = false;
            try
            {
                InitializeComponent();
                LoadStartup();
            }
            catch (Exception ex)
            {
                log.Error("Error initializating RetrievalForm: " + ex.Message, ex);
            }
        }

        public void SetSize(int size)
        {
            this.Height = size;
            this.Width = size;
        }
        public IController Controller
        {
            get;
            set;
        }

        public OperationType CurrentOperationType
        {
            get;
            set;
        }

        public bool IsReady
        {
            get;
            set;
        }

        public void SetScript(string script)
        {
            _scriptToRunUponCompletion = script;
        }

        public void UpdateStatus(string message)
        {
            labelStatus.Text = message;
            this.Focus();
        }

        public void Complete()
        {
            log.Debug("Complete()");
            buttonCancel.Enabled = true;//just in case...
            buttonCancel.Text = "Close";
            if (CurrentOperationType != OperationType.Resolution)
            {
                HandleDebugInfoSave();
            }
            if( CurrentOperationType == OperationType.ProcessSdFile || CurrentOperationType == OperationType.ProcessApplication)
            {
                this.Close();
            }
        }

        internal void LoadStartup()
        {
            _configuration = FileUtils.GetGinasConfiguration();
            log.Debug("Loaded configuration ");
            log.Debug(" selected url: " + _configuration.SelectedServer.ServerUrl);
            labelServerURL.Text = string.Empty;
            string initURL = _configuration.SelectedServer.ServerUrl + _configuration.InitPath;
            _baseUrl = _configuration.SelectedServer.ServerUrl;
            webBrowser1.Visible = false;
            webBrowser1.ObjectForScripting = this;

            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;

            log.Debug(" about to navigate to " + initURL);
            webBrowser1.Url = new Uri(initURL);
            _savedDebugInfo = false;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_configuration.DebugMode)
            {
                log.DebugFormat("webBrowser1.DocumentTitle: '{0}'; busy? {1}",
                    webBrowser1.DocumentTitle, webBrowser1.IsBusy);
            }
            if (_expectedTitles.Contains( webBrowser1.DocumentTitle) || string.IsNullOrWhiteSpace(webBrowser1.DocumentTitle))
            {
                log.Debug("normal document completed");
                webBrowser1.DocumentCompleted -= WebBrowser1_DocumentCompleted;
                BuildGinasToolsDocument();
            }
            else if (webBrowser1.DocumentTitle.Equals(COMPLETED_DOCUMENT_TITLE))
            {
                log.Warn("webBrowser1.DocumentTitle.Equals(COMPLETED_DOCUMENT_TITLE)");
                webBrowser1.Visible = true;
            }
            else if (webBrowser1.DocumentTitle.Equals(NAVIGATION_CANCELED))
            {
                log.Warn("detected NAVIGATION_CANCELED");
                    string html = FileUtils.GetErrorHtml();
                html = html.Replace("$MESSAGE1$", _initLoadingErrorMessage);

                html = html.Replace("$MESSAGE2$", _secondMessage);
                    buttonAddStructure.Enabled = false;
                    buttonResolve.Enabled = false;
                    webBrowser1.DocumentText = html;
                    webBrowser1.Visible = true;
                buttonCancel.Text = "Close";
                    Visible = true;
                    if (CurrentOperationType == OperationType.ProcessSdFile)
                    {
                        Controller.CancelOperation("Unable to contact server " + _configuration.SelectedServer.ServerUrl);
                    }

            }
        }

        public WebBrowser Browser
        {
            get
            {
                return this.webBrowser1;
            }
        }

        public string ScriptToExecute
        {
            set;
            private get;
        }

        public void StartSearch(string searchScript)
        {
            ExecuteScript(searchScript);
        }

        public object ExecuteScript(string script)
        {
            if (!_configuration.DebugMode) webBrowser1.ScriptErrorsSuppressed = true;
            string functionName = "runCommandForCSharp";
            log.Debug("Going to run script: " + script);
            object returnedValue = webBrowser1.Document.InvokeScript(functionName, new object[] { script });
            if (returnedValue is string && (returnedValue as string).StartsWith("'error running script: '"))
            {
                log.Warn(returnedValue);
            }
            return returnedValue;
        }

        public void Notify(string message)
        {
            try
            {
                log.DebugFormat("Notify processing message: {0}", message);
                if (message.StartsWith("gsrs_"))
                {
                    string followupCommand = "cresults.popItem('" + message + "')";
                    object result = ExecuteScript(followupCommand);
                    Controller.HandleResults(message, (string)result);
                    if (CurrentOperationType == OperationType.GetStructures)
                    {
                        log.Debug("Closing dialog after getting structures");
                        HandleDebugInfoSave();
                        Controller.Dispose();
                        Close();
                    }
                }
                else if (message.StartsWith("vocabulary:"))
                {
                    log.Debug("Got back " + message);
                    Controller.ReceiveVocabulary(message);
                }
            }
            catch(Exception ex)
            {
                log.Error("Error in Notify: " + ex.Message, ex);
            }
        }

        public void Proceed(string message)
        {
            log.Debug("message from browser: " + message);
        }

        private void buttonResolve_Click(object sender, EventArgs e)
        {
            //check for overwrite
            if(CurrentOperationType == OperationType.Resolution && ! checkBoxNewSheet.Checked)
            {
                int totalNewColumns = Convert.ToInt32(ExecuteScript("$('div.checkop input:checked').length") as string);
                log.DebugFormat("click handler detected total number of new columns: " + totalNewColumns);
                if(!Controller.OkToWrite(totalNewColumns))
                {
                    log.Debug("user elected not to overwrite data");
                    return;
                }
            }
            buttonCancel.Enabled = false;
            if (!Controller.StartResolution(checkBoxNewSheet.Checked))
            {
                MessageBox.Show("Error resolving your data.  Please try again or talk to your ginas administrator");
            }
            if (CurrentOperationType == OperationType.ShowScripts)
            {
                DialogResult = DialogResult.Yes;
                Close();
                Dispose();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.BeginInvoke((MethodInvoker)delegate { this.Close(); });
            //Close();
        }

        private void buttonAddStructure_Click(object sender, EventArgs e)
        {
            CurrentOperationType = OperationType.GetStructures;
            Controller.StartOperation();
        }

        private void RetrievalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.Debug("RetrievalForm_FormClosing");
            this.FormClosing -= RetrievalForm_FormClosing;
            if (!_savedDebugInfo) HandleDebugInfoSave();
        }

        private bool HandleDebugInfoSave()
        {
            log.Debug("at start of HandleDebugInfoSave, _savedDebugInfo: " + _savedDebugInfo);
            if ((checkBoxSaveDiagnostic.Checked ) && !_savedDebugInfo)
            {
                string script = "GSRSAPI_consoleStack.join('|')";// "$('#console').val()";
                string debugInfo = (string)ExecuteScript(script);
                if( !string.IsNullOrEmpty(debugInfo))
                {
                    debugInfo = debugInfo.Replace("|", Environment.NewLine);
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "txt files (*.txt)|*.txt|log file (*.log)|*.log|All files (*.*)|*.*";
                    saveFileDialog.Title = "Save diagnostic information?";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        FileUtils.WriteToFile(saveFileDialog.FileName, debugInfo);
                    }
                }
                else
                {
                    UIUtils.ShowMessageToUser("No JavaScript debug information found!");
                }
                _savedDebugInfo = true;
            }
            else if(CurrentOperationType == OperationType.ShowScripts && DialogResult != DialogResult.Cancel)
            {
                UIUtils.ShowMessageToUser("Your sheet has been created!");
            }

            return true;
        }

        private void BuildGinasToolsDocument()
        {
            log.Debug("webBrowser1.IsBusy: " + webBrowser1.IsBusy);
            //clear out old event handlers and scripts... optimistically
            webBrowser1.Document.InvokeScript("eval", new object[] { "$('document').off()" });
            webBrowser1.Document.InvokeScript("eval", new object[] {
                "$('script').remove(); " });
            webBrowser1.Document.InvokeScript("eval", new object[] {
                "$('link').remove(); " });
            DomUtils.BuildDocumentHead(webBrowser1.Document);
            DomUtils.BuildDocumentBody(webBrowser1.Document,
                (CurrentOperationType == OperationType.Loading || CurrentOperationType == OperationType.ShowScripts),
                _configuration.DebugMode );
            webBrowser1.Document.Title = "ginas Tools";
            
            ExecuteScript("GlobalSettings.setBaseURL('" + _baseUrl + _configuration.ApiPath + "');");
            checkBoxSaveDiagnostic.Checked = _configuration.DebugMode;
            if (CurrentOperationType == OperationType.Loading || CurrentOperationType== OperationType.ProcessApplication)
            {
                buttonResolve.Text = "Execute";
                buttonAddStructure.Enabled = false;
                buttonAddStructure.Visible = false;
                checkBoxNewSheet.Enabled = false;
                checkBoxNewSheet.Visible = false;
                ExecuteScript("setMode('update');");
                labelServerURL.Text ="Loading URL: " +  _configuration.SelectedServer.ServerUrl;
                Controller.ContinueSetup();
                Visible = true;
                Text = "Data Loader";
            }
            else if (CurrentOperationType == OperationType.Resolution)
            {
                ExecuteScript("setMode('resolver');");
                buttonAddStructure.Enabled = false;
                buttonAddStructure.Visible = false;
                this.Visible = true;
                Text = "Data Retriever";
            }
            else if (CurrentOperationType == OperationType.ShowScripts)
            {
                ExecuteScript("setMode('showScripts');");
                buttonResolve.Text = "Add Sheet";
                buttonAddStructure.Enabled = false;
                buttonAddStructure.Visible = false;
                buttonCancel.Enabled = true;
                checkBoxNewSheet.Enabled = false;
                Visible = true;
                Text = "Script Selection";
            }
            else if (CurrentOperationType == OperationType.GetStructures)
            {
                ExecuteScript("setMode('resolver');");
                //_scriptToRunUponCompletion allows the dialog to process information without becoming visible
                if (!string.IsNullOrWhiteSpace(_scriptToRunUponCompletion))
                {
                    ExecuteScript(_scriptToRunUponCompletion);
                }
            }
            else if( CurrentOperationType == OperationType.ProcessSdFile)
            {
                Visible = false;
                Controller.StartOperation();
                webBrowser1.ScriptErrorsSuppressed = !_configuration.DebugMode;
                return;
            }
            buttonDebugDOM.Enabled = false; 
            buttonDebugDOM.Visible = false;
            checkBoxSaveDiagnostic.Enabled = _configuration.DebugMode;

            if( _configuration.DebugMode && FileUtils.FolderExists(@"c:\temp"))
            {
                FileUtils.WriteToFile(@"c:\temp\debugdom.html", webBrowser1.Document.GetElementsByTagName("html")[0].OuterHtml);
            }            
            webBrowser1.Visible = true;
            IsReady = true;
            webBrowser1.ScriptErrorsSuppressed = !_configuration.DebugMode;
        }


        public void HandleClick(object obj, EventArgs args)
        {
            MessageBox.Show(args.ToString());
        }

        public bool GetDebugSetting()
        {
            return checkBoxSaveDiagnostic.Checked;
        }

        private void buttonDebugDOM_Click(object sender, EventArgs e)
        {
            string dom = (string)ExecuteScript("document.documentElement.outerHTML");
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "txt files (*.txt)|*.txt|log file (*.log)|*.log|All files (*.*)|*.*";
            saveFileDialog.Title = "Save DOM Dump?";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileUtils.WriteToFile(saveFileDialog.FileName, dom);
            }
        }

        public void SetController(Controller.IController controller)
        {
            Controller = controller;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "HTML files(*.html)|*.html|All files (*.*)|(*.*)";
            if( dialog.ShowDialog() == DialogResult.OK)
            {
                //webBrowser1.Url = new Uri(dialog.FileName);
                webBrowser1.Navigate(dialog.FileName);
            }
        }
    }
}
