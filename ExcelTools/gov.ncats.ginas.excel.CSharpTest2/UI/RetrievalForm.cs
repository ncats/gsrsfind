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

using gov.ncats.ginas.excel.CSharpTest2.Utils;
using gov.ncats.ginas.excel.CSharpTest2.Controller;
using gov.ncats.ginas.excel.CSharpTest2.Model;

namespace gov.ncats.ginas.excel.CSharpTest2.UI
{
    [ComVisible(true)]
    public partial class RetrievalForm : Form, IStatusUpdater, IScriptExecutor
    {
        string _html;
        string _javascript;
        string _expectedTitle;
        string _baseUrl;
        List<string> _completedScripts = new List<string>();
        const string COMPLETED_DOCUMENT_TITLE = "ginas Tools";

        public RetrievalForm()
        {
            InitializeComponent();
            this.LoadStartup();
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

        public void UpdateStatus(string message)
        {
            labelStatus.Text = message;
        }
        internal void LoadStartup()
        {
            GinasToolsConfiguration configuration = FileUtils.GetGinasConfiguration();

            JSTools tools = new JSTools();
            Debug.WriteLine(tools.GetType().FullName);
            string html = FileUtils.GetHtml();
            string javascript = FileUtils.GetJavaScript();
            _javascript = javascript;
            string imageFormat = Properties.Resources.ImageFormat;
            javascript = javascript.Replace("$IMGFORMAT$", imageFormat);
            string initURL = configuration.SelectedServer.ServerUrl;
            _baseUrl = initURL;
            html = html.Replace("$GSRS_LIBRARY$", javascript);
            this._html = html;
            //temp:
            //FileUtils.WriteToFile(@"c:\temp\debug.html", html);
            _expectedTitle = "g-srs";
            webBrowser1.Visible = false;
            webBrowser1.ObjectForScripting = this;

            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;

            //webBrowser1.Navigate(initURL);
            webBrowser1.Url = new Uri(initURL);
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Debug.WriteLine("title:" + webBrowser1.DocumentTitle);
            Debug.WriteLine("ReadyState:" + webBrowser1.ReadyState);
            if (webBrowser1.DocumentTitle.Equals(_expectedTitle))
            {
                BuildGinasToolsDocument();
                ExecuteScript("GlobalSettings.setBaseURL('" + _baseUrl + "api/v1/');");

                if (CurrentOperationType == OperationType.Loading)
                {
                    buttonResolve.Text = "Execute";
                    buttonAddStructure.Enabled = false;
                    buttonAddStructure.Visible = false;
                    ExecuteScript("setMode('update');");
                    Controller.ContinueSetup();
                }
                else if (CurrentOperationType == OperationType.Resolution)
                {
                    ExecuteScript("setMode('resolver');");
                    buttonAddStructure.Enabled = true;
                    buttonAddStructure.Visible = true;
                }
                else if (CurrentOperationType == OperationType.ShowScripts)
                {
                    ExecuteScript("setMode('showScripts');");
                    buttonResolve.Text = "Add Sheet";
                    buttonAddStructure.Enabled = false;
                    buttonAddStructure.Visible = false;
                }
            }
            else if (webBrowser1.DocumentTitle.Equals(COMPLETED_DOCUMENT_TITLE))
            {
                //last script
                HtmlElement lastScript = webBrowser1.Document.CreateElement("script");
                lastScript.InnerText = FileUtils.GetLastJavaScript();
                webBrowser1.Document.Body.AppendChild(lastScript);
                
                webBrowser1.Visible = true;
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
            string scriptArg = script;
            string functionName = "runCommandForCSharp";
            Debug.WriteLine("Going to run script: " + script);
            object returnedValue = webBrowser1.Document.InvokeScript(functionName, new object[] { scriptArg });
            Debug.WriteLine("Return from JS: " + returnedValue);
            return returnedValue;
        }

        public void Notify(string message)
        {
            if (message.StartsWith("gsrs_"))
            {
                string followupCommand = "cresults.popItem('" + message + "')";
                object result = ExecuteScript(followupCommand);
                Controller.HandleResults(message, (string)result);
            }
        }

        public void Proceed(string message)
        {
            Debug.WriteLine(message, "message from browser");
        }

        private void buttonResolve_Click(object sender, EventArgs e)
        {
            if (!Controller.StartResolution(checkBoxNewSheet.Checked))
            {
                MessageBox.Show("Error resolving your data.  Please try again or talk to your ginas administrator");
            }
            if (CurrentOperationType == OperationType.ShowScripts)
            {
                DialogResult = DialogResult.Yes;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonAddStructure_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ScriptToExecute))
            {
                ExecuteScript(ScriptToExecute);
                _completedScripts.Add(ScriptToExecute);
                ScriptToExecute = string.Empty;
            }
        }

        private void RetrievalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkBoxSaveDiagnostic.Checked)
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
        }

        private void BuildGinasToolsDocument()
        {
            //clear out old event handlers and scripts... optimistically
            webBrowser1.Document.InvokeScript("eval", new object[] { "$('document').off()" });
            webBrowser1.Document.InvokeScript("eval", new object[] {
                "$('script').remove(); " });

            DomUtils.BuildDocumentHead(webBrowser1.Document);
            while (webBrowser1.IsBusy)
            {
                Debug.Write("busy (2)...");
            }

            DomUtils.BuildDocumentBody(webBrowser1.Document, 
                (CurrentOperationType== OperationType.Loading || CurrentOperationType == OperationType.ShowScripts));
            webBrowser1.Document.Title = "ginas Tools";
            webBrowser1.Document.Body.SetAttribute("className", string.Empty);
            webBrowser1.Document.Body.Style = "padding-top:10px";
            FileUtils.WriteToFile(@"c:\temp\debugdom.html", webBrowser1.Document.Body.OuterHtml);
            //inputElement.AttachEventHandler("click", HandleClick);

            int after = webBrowser1.Document.All.Count;
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
            Debug.WriteLine(webBrowser1.DocumentText);
        }

        public void HandleClick(object obj, EventArgs args)
        {
            MessageBox.Show(args.ToString());
        }
    }
}
