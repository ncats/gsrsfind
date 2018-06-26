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
        string _html;
        string _javascript;
        string _expectedTitle;
        string _baseUrl;
        const string COMPLETED_DOCUMENT_TITLE = "ginas Tools";
        const string NAVIGATION_CANCELED = "Navigation Canceled";
        GinasToolsConfiguration _configuration = null;
        string _scriptToRunUponCompletion;
        bool _savedDebugInfo;

        public RetrievalForm()
        {
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
            buttonCancel.Enabled = true;//just in case...
            buttonCancel.Text = "Close";
            if( CurrentOperationType != OperationType.Resolution)
            {
                HandleDebugInfoSave();
                Close();
            }
        }

        internal void LoadStartup()
        {
            _configuration = FileUtils.GetGinasConfiguration();
            log.Debug("Loaded configuration ");
            log.Debug(" selected url:" + _configuration.SelectedServer.ServerUrl);
            JSTools tools = new JSTools();
            string html = FileUtils.GetHtml();
            string javascript = FileUtils.GetJavaScript();
            _javascript = javascript;
            string imageFormat = Properties.Resources.ImageFormat;
            javascript = javascript.Replace("$IMGFORMAT$", imageFormat);
            string initURL = _configuration.SelectedServer.ServerUrl;
            _baseUrl = initURL;
            html = html.Replace("$GSRS_LIBRARY$", javascript);
            this._html = html;
            //temp:
            FileUtils.WriteToFile(@"c:\temp\debug.html", html);
            _expectedTitle = "g-srs";
            webBrowser1.Visible = false;
            webBrowser1.ObjectForScripting = this;

            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;

            //webBrowser1.Navigate(initURL);
            log.Debug(" about to navigate to " + initURL);
            webBrowser1.Url = new Uri(initURL);
            _savedDebugInfo = false;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if(_configuration.DebugMode) log.Debug("webBrowser1.DocumentTitle: " + webBrowser1.DocumentTitle);
            if (webBrowser1.DocumentTitle.Equals(_expectedTitle))
            {
                BuildGinasToolsDocument();
                ExecuteScript("GlobalSettings.setBaseURL('" + _baseUrl + "api/v1/');");

                if (CurrentOperationType == OperationType.Loading)
                {
                    buttonResolve.Text = "Execute";
                    buttonAddStructure.Enabled = false;
                    buttonAddStructure.Visible = false;
                    checkBoxNewSheet.Enabled = false;
                    checkBoxNewSheet.Visible = false;
                    ExecuteScript("setMode('update');");
                    Controller.ContinueSetup();
                    this.Visible = true;
                    Text = "Data Loader";
                }
                else if (CurrentOperationType == OperationType.Resolution)
                {
                    ExecuteScript("setMode('resolver');");
                    buttonAddStructure.Enabled = true;
                    buttonAddStructure.Visible = true;
                    this.Visible = true;
                    Text = "Data Retriever";
                }
                else if (CurrentOperationType == OperationType.ShowScripts)
                {
                    ExecuteScript("setMode('showScripts');");
                    buttonResolve.Text = "Add Sheet";
                    buttonAddStructure.Enabled = false;
                    buttonAddStructure.Visible = false;
                    this.Visible = true;
                    Text = "Script Selection";
                }
                else if (CurrentOperationType == OperationType.GetStructures)
                {
                    ExecuteScript("setMode('resolver');");
                    if(!string.IsNullOrWhiteSpace(_scriptToRunUponCompletion))
                    {
                        ExecuteScript(_scriptToRunUponCompletion);
                    }
                }
                buttonDebugDOM.Enabled = false;// _configuration.DebugMode;
                buttonDebugDOM.Visible = false;// _configuration.DebugMode;
            }
            else if (webBrowser1.DocumentTitle.Equals(COMPLETED_DOCUMENT_TITLE))
            {
                //last script
                log.Warn("webBrowser1.DocumentTitle.Equals(COMPLETED_DOCUMENT_TITLE)");
                webBrowser1.Visible = true;
            }
            else if (webBrowser1.DocumentTitle.Equals(NAVIGATION_CANCELED))
            {
                string html = FileUtils.GetErrorHtml();
                html = html.Replace("$MESSAGE1$", "Error loading initial ginas page");

                html = html.Replace("$MESSAGE2$", "Close dialog and try again or notify your administrator");
                webBrowser1.DocumentText = html;
                webBrowser1.Visible = true;
                Visible = true;
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

        public bool IsDebugOn()
        {
            return checkBoxSaveDiagnostic.Checked;
        }

        public void StartSearch(string searchScript)
        {
            ExecuteScript(searchScript);
        }

        public object ExecuteScript(string script)
        {
            webBrowser1.ScriptErrorsSuppressed = true;
            string functionName = "runCommandForCSharp";
            if( _configuration.DebugMode)
            {
                log.Debug("Going to run script: " + script);
            }
            object returnedValue = webBrowser1.Document.InvokeScript(functionName, new object[] { script });
            return returnedValue;
        }

        public void Notify(string message)
        {
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
        }

        public void Proceed(string message)
        {
            log.Debug("message from browser: " + message);
        }

        private void buttonResolve_Click(object sender, EventArgs e)
        {
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
            Close();
        }

        private void buttonAddStructure_Click(object sender, EventArgs e)
        {
            CurrentOperationType = OperationType.GetStructures;
            Controller.StartOperation();
        }

        private void RetrievalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.Debug("RetrievalForm_FormClosing");
            if(!_savedDebugInfo) HandleDebugInfoSave();
        }

        private void HandleDebugInfoSave()
        {
            log.Debug("at start of HandleDebugInfoSave, _savedDebugInfo: " + _savedDebugInfo);
            if ((checkBoxSaveDiagnostic.Checked || CurrentOperationType == OperationType.GetStructures )
                && !_savedDebugInfo)
            {
                string script = "$('#console').val()";
                string debugInfo = (string)ExecuteScript(script);
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|log file (*.log)|*.log|All files (*.*)|*.*";
                saveFileDialog.Title = "Save diagnostic information?";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FileUtils.WriteToFile(saveFileDialog.FileName, debugInfo);
                }
            }
            _savedDebugInfo = true;
            
        }
        private void BuildGinasToolsDocument()
        {
            //clear out old event handlers and scripts... optimistically
            webBrowser1.Document.InvokeScript("eval", new object[] { "$('document').off()" });
            webBrowser1.Document.InvokeScript("eval", new object[] {
                "$('script').remove(); " });

            DomUtils.BuildDocumentHead(webBrowser1.Document);
            int iter = 0;
            while (webBrowser1.IsBusy && ++iter < 10000)
            {
                log.Debug("busy (2)...");
                System.Threading.Thread.Sleep(10);
                if ((iter % 1000) == 0)
                {
                    if( !UIUtils.GetUserYesNo("Loading web page is slow. Continue waiting?"))
                    {
                        return;
                    }
                }
            }
            

            DomUtils.BuildDocumentBody(webBrowser1.Document,
                (CurrentOperationType == OperationType.Loading || CurrentOperationType == OperationType.ShowScripts),
                _configuration.DebugMode);
            webBrowser1.Document.Title = "ginas Tools";
            webBrowser1.Document.Body.SetAttribute("className", string.Empty);
            webBrowser1.Document.Body.Style = "padding-top:10px";
            this.checkBoxSaveDiagnostic.Checked = _configuration.DebugMode;
            //FileUtils.WriteToFile(@"c:\temp\debugdom.html", webBrowser1.Document.Body.OuterHtml);
            webBrowser1.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //collect info on scripts
            List<string> functionNames = new List<string>();
            foreach (HtmlElement elem in webBrowser1.Document.All)
            {
                if (elem.TagName.Equals("script", StringComparison.CurrentCultureIgnoreCase))
                {
                    string script = elem.InnerText;

                }
            }
            webBrowser1.Document.InvokeScript("handleReady");
            log.Debug(webBrowser1.DocumentText);
        }

        public void HandleClick(object obj, EventArgs args)
        {
            MessageBox.Show(args.ToString());
        }

        private void buttonDebugDOM_Click(object sender, EventArgs e)
        {
            string dom = (string)ExecuteScript("document.body.outerHTML");
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "txt files (*.txt)|*.txt|log file (*.log)|*.log|All files (*.*)|*.*";
            saveFileDialog.Title = "Save DOM Dump?";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileUtils.WriteToFile(saveFileDialog.FileName, dom);
            }
        }
    }
}
