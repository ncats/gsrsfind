using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

using gov.ncats.ginas.excel.tools;
using gov.ncats.ginas.excel.tools.Controller;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;

namespace ginasExcelUnitTests
{
    public partial class MockRetrievalForm : Form, IStatusUpdater, IScriptExecutor
    {
        public MockRetrievalForm()
        {
            InitializeComponent();
            BuildGinasToolsDocument();
        }

        public OperationType CurrentOperationType
        {
            get;
            set;
        }

        public IController Controller
        {
            get;
            set;
        }
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        GinasToolsConfiguration _configuration = FileUtils.GetGinasConfiguration();
        private string _scriptToRunUponCompletion;

        private void BuildGinasToolsDocument()
        {
            string filePath = @"..\..\etc\UtilityWebPage.html";
            string loadPath = "file://" + Path.GetFullPath(filePath);
            webBrowser1.Navigate(loadPath);

            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
            //make sure the original document is completely loaded    
            int iter = 0;
            while (webBrowser1.IsBusy || webBrowser1.Document.Body ==null 
                && ++iter < 500)
            {
                log.DebugFormat("busy (2) {0}...", iter);
                System.Threading.Thread.Sleep(1000);
                if ((iter % 100) == 0)
                {
                    Application.DoEvents();
                }
            }

            log.DebugFormat("At the end of {0} iterations, state of document: {1}", iter, webBrowser1.IsBusy);
            DomUtils.BuildDocumentHead(webBrowser1.Document);
            DomUtils.BuildDocumentBody(webBrowser1.Document,
                true,
                true);
            log.DebugFormat("body: {0}", webBrowser1.Document.Body);
            //webBrowser1.Document.Title = "ginas Tools";

            ExecuteScript("GlobalSettings.setBaseURL('" + _configuration.SelectedServer.ServerUrl 
                + _configuration.ApiPath + "');");
            
            if (CurrentOperationType == OperationType.Loading)
            {
                ExecuteScript("setMode('update');");
                //Controller.ContinueSetup();
                Visible = true;
                Text = "Data Loader";
            }
            else if (CurrentOperationType == OperationType.Resolution)
            {
                ExecuteScript("setMode('resolver');");
                this.Visible = true;
                Text = "Data Retriever";
            }
            else if (CurrentOperationType == OperationType.None || CurrentOperationType == OperationType.ShowScripts)
            {
                ExecuteScript("setMode('showScripts');");
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
            else if (CurrentOperationType == OperationType.ProcessSdFile)
            {
                Visible = false;
                Controller.StartOperation();
                return;
            }

            //webBrowser1.Visible = true;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            log.Debug("Document completed");
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

        public void SetScript(string script)
        {
            throw new NotImplementedException();
        }

        void IStatusUpdater.Complete()
        {
            //Close();
        }

        bool IStatusUpdater.GetDebugSetting()
        {
            return true;
        }

        void IStatusUpdater.UpdateStatus(string message)
        {
            labelStatus.Text = message;
        }
    }
}
