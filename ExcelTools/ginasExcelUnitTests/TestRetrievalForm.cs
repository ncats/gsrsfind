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
using System.Runtime.InteropServices;

using gov.ncats.ginas.excel.tools;
using gov.ncats.ginas.excel.tools.Controller;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;

namespace ginasExcelUnitTests
{
    [ComVisible(true)]
    public partial class TestRetrievalForm : Form, IStatusUpdater, IScriptExecutor
    {
        private bool _documentBuilt = false;
        public delegate object HandleResultsDelegate(string key, string result);
        public HandleResultsDelegate ResultsHandler;

        public TestRetrievalForm()
        {
            IsReady = false;
            InitializeComponent();
            LoadStartup();
        }

        private void BeginLoading()
        {
            //string filePath = @"..\..\etc\UtilityWebPage.html";
            //string loadPath = "file://" + Path.GetFullPath(filePath);
            //webBrowser1.Navigate(loadPath);

            string initURL = _configuration.SelectedServer.ServerUrl + _configuration.InitPath;
            webBrowser1.Visible = false;
            
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
            log.Debug(" about to navigate to " + initURL);
            webBrowser1.Url = new Uri(initURL);
        }


        internal void LoadStartup()
        {
            _configuration = FileUtils.GetGinasConfiguration();
            if( _configuration == null || _configuration.SelectedServer == null )
            {
                MessageBox.Show("Error determining server from configuration!");

            }
            log.Debug("Loaded configuration ");
            log.Debug(" selected url: " + _configuration.SelectedServer.ServerUrl);
            string initURL = _configuration.SelectedServer.ServerUrl;
            webBrowser1.Visible = false;
            webBrowser1.ObjectForScripting = this;

            webBrowser1.ScriptErrorsSuppressed = !_configuration.DebugMode;
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;

            log.Debug(" about to navigate to " + initURL);
            webBrowser1.Url = new Uri(initURL);
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

        public Type ControllerType
        {
            get;
            set;
        }


        public bool IsReady
        {
            get;
            private set;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        GinasToolsConfiguration _configuration = FileUtils.GetGinasConfiguration();
        private string _scriptToRunUponCompletion;
        private List<string> messages = new List<string>();

        private void BuildGinasToolsDocument()
        {
            webBrowser1.ObjectForScripting = this;

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
            Authenticate();
            IsReady = true;
            
            //webBrowser1.Visible = true;
        }

        protected void Authenticate()
        {
            if (!string.IsNullOrWhiteSpace(_configuration.SelectedServer.Username)
                && !string.IsNullOrWhiteSpace(_configuration.SelectedServer.PrivateKey))
            {
                string script1 = string.Format("GlobalSettings.authKey = '{0}'",
                    _configuration.SelectedServer.PrivateKey);
                ExecuteScript(script1);
                string script2 = string.Format("GlobalSettings.authUsername = '{0}'",
                    _configuration.SelectedServer.Username);
                ExecuteScript(script2);
            }
        }


        public void Notify(string message)
        {
            log.DebugFormat("Notify processing message: {0}", message);

            messages.Add(message);
            if (message.StartsWith("gsrs_"))
            {
                string followupCommand = "cresults.popItem('" + message + "')";
                object result = ExecuteScript(followupCommand);
                if (Controller != null)
                {
                    Controller.HandleResults(message, (string)result);
                }
                else
                {
                    ResultsHandler?.Invoke(message, (string)result);
                }

            }
            else if (message.StartsWith("vocabulary:"))
            {
                log.Debug("Got back " + message);
                ExcelTests.ReceiveVocabulary(message);
                //Controller.ReceiveVocabulary(message);
            }
        }

        public List<string> GetMessages()
        {
            return messages;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!_documentBuilt )
            {
                log.Debug("Document completed");
                BuildGinasToolsDocument();
                _documentBuilt = true;
            }
        }

        public delegate object RunScriptDelegate(string script);
        public RunScriptDelegate myDelegate;

        public object ExecuteScript(string script)
        {
            RunScriptDelegate runScriptDelegate = new RunScriptDelegate(ExecuteScriptHere );
            object callResult= Invoke(runScriptDelegate, script);
            log.DebugFormat("invoke returned {0}", callResult);
            if( callResult is string && (callResult as string).Contains("error running script"))
            {
                string followUpScript = "GSRSAPI_consoleStack.join('|')";
                object callResult2 = Invoke(runScriptDelegate, followUpScript);
                log.DebugFormat("JS log: {0}", callResult2);
            }
            return callResult;
        }
        public object ExecuteScriptHere(string script)
        {
            //if (!_configuration.DebugMode) webBrowser1.ScriptErrorsSuppressed = true;
            string functionName = "runCommandForCSharp";
            log.Debug("Going to run script: " + script);
            object returnedValue = null;
            try
            {
                object[] parms = new object[1];
                parms[0] = script;
                returnedValue = webBrowser1.Document.InvokeScript(functionName,
                    parms);
            }
            catch(Exception ex)
            {
                log.ErrorFormat(ex.Message);
                log.DebugFormat(ex.StackTrace);
            }
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

        public void SetController(IController controller)
        {
            Controller = controller;
        }

    }
}
