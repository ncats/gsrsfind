using GSRSExcelTools;
using GSRSExcelTools.Controller;
using GSRSExcelTools.Model;
using GSRSExcelTools.UI;
using GSRSExcelTools.Utils;
using Microsoft.Web.WebView2.Core;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WebView2.DevTools.Dom;

namespace ginasExcelUnitTests
{
    [ComVisible(true)]
    public partial class TestRetrievalForm : Form, IStatusUpdater, IScriptExecutor
    {
        const string ImageFormat = "png";

        private bool _documentBuilt = false;
        public delegate object HandleResultsDelegate(string key, string result);
        public HandleResultsDelegate ResultsHandler;

        public TestRetrievalForm()
        {
            IsReady = false;
            InitializeComponent();
            LoadStartup();
        }

        private void TestRetrievalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            log.Debug("closing cancelled");
        }

        internal async void LoadStartup()
        {
            _configuration = FileUtils.GetGinasConfiguration();
           if (_configuration == null || _configuration.SelectedServer == null)
            {
                MessageBox.Show("Error determining server from configuration!");
            }
            log.Debug("Loaded configuration ");
            log.Debug(" selected url: " + _configuration.SelectedServer.ServerUrl);
            string initURL = RetrievalForm.CreateApiUrl(_configuration.SelectedServer.ServerUrl, _configuration.InitPath);
            webViewTestForm.Visible = true;
            CoreWebView2Environment setupResult = await SetupEnvironment();
            await webViewTestForm.EnsureCoreWebView2Async(setupResult);
            log.Debug("EnsureCoreWebView2Async complete");
            webViewTestForm.NavigationCompleted += WebViewGsrs_NavigationCompleted;
            log.Debug("after event wiring, about to navigate to " + initURL);
            webViewTestForm.CoreWebView2.Navigate(initURL);
        }

        private async Task<CoreWebView2Environment> SetupEnvironment()
        {
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GsrsExcelTools/WebView2Data"
                );
            Directory.CreateDirectory(userDataFolder); // Ensure directory exists

            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: userDataFolder
            );
            return env;
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

        private async Task BuildGinasToolsDocument()
        {
            log.Debug("starting BuildGinasToolsDocument");
            WebView2.DevTools.Dom.HtmlDocument document = null;
            var devToolsContext = await webViewTestForm.CoreWebView2.CreateDevToolsContextAsync();
            WebView2.DevTools.Dom.HtmlElement rootElement = await devToolsContext.QuerySelectorAsync("html");
            var parent = await rootElement.GetParentAsync();

            if (parent is WebView2.DevTools.Dom.HtmlDocument)
            {
                document = (WebView2.DevTools.Dom.HtmlDocument)parent;
                log.DebugFormat("found document object");
            }
            else
            {
                log.Error("Unable to cast root element to HtmlDocument.  Will not build document.");
                return;
            }

            if (document != null)
            {
                HtmlHeadElement headElement = (HtmlHeadElement)await devToolsContext.QuerySelectorAsync("head");
                await DomUtils.BuildDocumentHead(document, headElement);
                log.Debug("Building document body");
                await DomUtils.BuildDocumentBody(document,
                    (CurrentOperationType == OperationType.Loading || CurrentOperationType == OperationType.ShowScripts),
                _configuration.DebugMode);
                string mainScriptText = FileUtils.GetJavaScript().Replace("$IMGFORMAT$", ImageFormat);
                string resultMain= await webViewTestForm.CoreWebView2.ExecuteScriptAsync(mainScriptText);
                log.DebugFormat("result of running main script: {0}", resultMain);
                await webViewTestForm.CoreWebView2.ExecuteScriptAsync(FileUtils.GetLastJavaScript());
                string shimScript = "var cresults = { 'getItem': function (v) { return this[v]; }, 'popItem': function (v) { var ret = this[v]; delete this[v]; return ret; } }; ";
                await webViewTestForm.CoreWebView2.ExecuteScriptAsync(shimScript);
            }

            await ExecuteScript("GlobalSettings.setBaseURL('" + _configuration.SelectedServer.ServerUrl
                + _configuration.ApiPath + "');");
            object scriptResult = await ExecuteScript("GlobalSettings.setStructureUrl('"
                + _configuration.SelectedServer.StructureUrl + "');");
            log.DebugFormat("Result of setStructureUrl: {0}", scriptResult);
            if (CurrentOperationType == OperationType.Loading)
            {
                await ExecuteScript("setMode('update');");
                //Controller.ContinueSetup();
                Visible = true;
                Text = "Data Loader";
            }
            else if (CurrentOperationType == OperationType.Resolution)
            {
                await ExecuteScript("setMode('resolver');");
                this.Visible = true;
                Text = "Data Retriever";
            }
            else if (CurrentOperationType == OperationType.None || CurrentOperationType == OperationType.ShowScripts)
            {
                await ExecuteScript("setMode('showScripts');");
                Visible = true;
                Text = "Script Selection";
            }
            else if (CurrentOperationType == OperationType.GetStructures)
            {
                await ExecuteScript("setMode('resolver');");
                //_scriptToRunUponCompletion allows the dialog to process information without becoming visible
                if (!string.IsNullOrWhiteSpace(_scriptToRunUponCompletion))
                {
                    await ExecuteScript(_scriptToRunUponCompletion);
                }
            }
            else if (CurrentOperationType == OperationType.ProcessSdFile)
            {
                Visible = false;
                Controller.StartOperation();
                return;
            }
            await Authenticate();
            IsReady = true;

            webViewTestForm.NavigationCompleted -= WebViewGsrs_NavigationCompleted;
            webViewTestForm.WebMessageReceived += WebViewTestForm_WebMessageReceived;
            webViewTestForm.CoreWebView2.Settings.IsWebMessageEnabled = true;
            log.Debug("BuildGinasToolsDocument complete; events wired");
        }

        private async void WebViewTestForm_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            log.Debug("WebViewTestForm_WebMessageReceived");
            try
            {
                string message = e.WebMessageAsJson;
                message = message.Replace("\"", string.Empty);
                log.DebugFormat("WebViewGsrs_WebMessageReceived processing message: {0}", message);
                if (message.StartsWith("gsrs_"))
                {
                    Notify(message);
                }
                else if (message.StartsWith("vocabulary:"))
                {
                    log.DebugFormat("Got back {0}", message);
                    //clean up the vocabulary received from GSRS.  For some reason, the webview2 control returns in a different format
                    // from a browser or the old browser control
                    message = message.Replace("\\", "\"").Replace("\"\"\"", "\"").Replace("\"^\"", "^").Replace("\"$\"", "$");
                    message = Regex.Replace(message, "&q.*?&", "&");

                    log.DebugFormat("message cleaned up: {0}", message);
                    await Controller.ReceiveVocabulary(message);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in WebViewTestForm_WebMessageReceived: " + ex.Message, ex);
            }
        }

        protected async Task Authenticate()
        {
            if (!string.IsNullOrWhiteSpace(_configuration.SelectedServer.Username)
                && !string.IsNullOrWhiteSpace(_configuration.SelectedServer.PrivateKey))
            {
                string script1 = string.Format("GlobalSettings.authKey = '{0}'",
                    _configuration.SelectedServer.PrivateKey);
                await ExecuteScript(script1);
                string script2 = string.Format("GlobalSettings.authUsername = '{0}'",
                    _configuration.SelectedServer.Username);
                await ExecuteScript(script2);
            }
        }


        public async void Notify(string message)
        {
            log.DebugFormat("Notify processing message: {0}", message);

            messages.Add(message);
            if (message.StartsWith("gsrs_"))
            {
                string followupCommand = "cresults.popItem('" + message + "')";
                object result =await ExecuteScript(followupCommand);
                //if (Controller != null)
                //{
                //    Controller.HandleResults(message, (string)result);
                //}
                //else
                //{
                    ResultsHandler?.Invoke(message, (string)result);
                //}

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

        private async void WebViewGsrs_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!_documentBuilt)
            {
                log.Debug("Iniital API page completed");
                await BuildGinasToolsDocument();
                _documentBuilt = true;
            }
        }

        public delegate object RunScriptDelegate(string script);
        public RunScriptDelegate myDelegate;

        /*        public Task<object> ExecuteScript(string script)
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
                    return Task.FromResult(callResult);
                }
        */
        public async Task<string> ExecuteScript(string script)
        {
            string? loggableScript = (script != null && script.Length > 120) ? script.Substring(0, 119) : script;
            log.DebugFormat("Going to run script: {0}", loggableScript);
            string result= await webViewTestForm.CoreWebView2.ExecuteScriptAsync(script);
            return result;
        }


        public Task ExecuteScriptNoReturn(string script)
        {
            //no-op
            //return Task.FromResult("null");
            return webViewTestForm.CoreWebView2.ExecuteScriptAsync(script);
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

        public bool HasUserCancelled()
        {
            return false;
        }

        async Task<object> IScriptExecutor.ExecuteScript(string script)
        {
            string return1 = await ExecuteScript(script);
            return return1;
        }

    }
}
