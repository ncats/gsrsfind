using GSRSExcelTools.Controller;
using GSRSExcelTools.Model;
using GSRSExcelTools.Utils;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebView2.DevTools.Dom;
using HtmlDocument = WebView2.DevTools.Dom.HtmlDocument;
using HtmlElement = WebView2.DevTools.Dom.HtmlElement;


namespace GSRSExcelTools.UI
{
    [ComVisible(true)]
    public partial class RetrievalForm : Form, IStatusUpdater, IScriptExecutor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        readonly List<string> _expectedTitles = new List<string>();
        string _baseUrl;
        const string COMPLETED_DOCUMENT_TITLE = "GSRS Landing Page";
        const string NAVIGATION_CANCELED = "Navigation Canceled";
        const string FORM_COMPLETE_TITLE = "GSRS Tools";
        GinasToolsConfiguration _configuration = null;
        string _scriptToRunUponCompletion;
        bool _savedDebugInfo;
        string _initLoadingErrorMessage = "Error loading initial GSRS page";
        string _secondMessage = "Close dialog and try again or notify your administrator";
        private int callCount = 0;
        private int maxCallCount = 10;
        bool _shuttingDown = false;
        private int SCRIPT_DELAY_MILLISECONDS = 50;


        public RetrievalForm(string noConnectionMessage, string secondMessage)
        {
            _initLoadingErrorMessage = noConnectionMessage;
            _secondMessage = secondMessage;
            PerformSetup().GetAwaiter().GetType();
        }

        public RetrievalForm()
        {
            _ = PerformSetup();
        }

        private RetrievalForm(bool skipPerformSetup)
        {
            if (!skipPerformSetup)
            {
                _=PerformSetup();
            }
        }

        public static async Task<RetrievalForm> CreateInstance()
        {
            RetrievalForm form = new RetrievalForm(true);
            await form.PerformSetup();
            return form;
        }

        public void SetInitialLoadingErrorMessage(string newMessage)
        {
            _initLoadingErrorMessage = newMessage;
        }

        public void SetSecondMessage(string secondMessage)
        {
            _secondMessage = secondMessage;
        }

        private async Task PerformSetup()
        {
            IsReady = false;
            _expectedTitles.Add("InXight API");
            _expectedTitles.Add("g-srs");
            _expectedTitles.Add("Sequence Search");
            _expectedTitles.Add("GSRS");
            _expectedTitles.Add("GSRS landing page");
            log.Debug("Starting in RetrievalForm");

            Visible = false;
            try
            {
                InitializeComponent();
                await LoadStartup();
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
            if (labelStatus.InvokeRequired)
            {
                log.Debug("using invoke");
                labelStatus.Invoke((MethodInvoker)delegate
                {
                    labelStatus.Text = message;
                });
                return;
            }
            labelStatus.Text = message;
            this.Focus();
        }

        public async void Complete()
        {
            log.DebugFormat("Complete() CurrentOperationType: {0}", CurrentOperationType);
            Thread disableButtonThread = new Thread(ChangeCancelButtonLater);
            log.Debug("logical place to close the dialog");
            disableButtonThread.Start();
            
            if (CurrentOperationType != OperationType.Resolution && CurrentOperationType != OperationType.ShowScripts)
            {
                await HandleDebugInfoSave();
            }
            if (CurrentOperationType == OperationType.ProcessSdFile || CurrentOperationType == OperationType.ProcessApplication
                || CurrentOperationType == OperationType.AddIngredient)
            {
            }
            else if (CurrentOperationType == OperationType.ShowScripts)
            {
                UIUtils.ShowMessageToUser("Your sheet has been created!");
                Thread closeThread = new Thread(CloseLater);
                log.Debug("using a thread to close the dialog");
                closeThread.Start();
            }
        }

        private void CloseLater()
        {
            _shuttingDown = true;
            if (InvokeRequired)
            {
                log.Debug("Invoke required, calling CloseLater from thread");
                Invoke(new MethodInvoker(() => 
                {
                    log.Debug("Closing dialog from CloseLater thread");
                    CloseLater();
                }));
                
            } else
            {
                log.Debug("No invoke required");
                //Close();
            }
        }

        private void ChangeCancelButtonLater()
        {
            if( InvokeRequired)
            {
                log.Debug("Invoke required in ChangeCancelButtonLater");
                this.Invoke(new MethodInvoker(() =>
                {
                   log.Debug("Changing button via invoke");
                    buttonCancel.Enabled = true;
                    buttonCancel.Text = "Close";
                }));
            }
            else
            {
                log.Debug("No invoke required, changing button directly");
                buttonCancel.Enabled = true;
                buttonCancel.Text = "Close";
            }
        }

        internal async Task LoadStartup()
        {
            _configuration = FileUtils.GetGinasConfiguration();
            log.Debug("Loaded configuration ");
            log.Debug(" selected url: " + _configuration.SelectedServer.ServerUrl);
            labelServerURL.Text = string.Empty;
            string initURL = CreateApiUrl(_configuration.SelectedServer.ServerUrl, _configuration.InitPath);
            log.DebugFormat("initURL: {0}", initURL);
            _baseUrl = _configuration.SelectedServer.ServerUrl;
            await webViewGsrs.EnsureCoreWebView2Async(await SetupEnvironment());
            webViewGsrs.Visible = false;
            
            webViewGsrs.NavigationCompleted += WebViewGsrs_NavigationCompleted;

            log.Debug(" about to navigate to " + initURL);
            webViewGsrs.CoreWebView2.Navigate(initURL);
            _savedDebugInfo = false;
            if( _configuration.PageBuildDelayMilliseconds > 0 )
            {
                SCRIPT_DELAY_MILLISECONDS = _configuration.PageBuildDelayMilliseconds;
            }
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

        private void WebViewGsrs_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_configuration.DebugMode)
            {
                log.DebugFormat("webViewGsrs.DocumentTitle: '{0}'; suspended? {1}",
                    webViewGsrs.CoreWebView2.DocumentTitle, webViewGsrs.CoreWebView2.IsSuspended);
            }

            string title = webViewGsrs.CoreWebView2.DocumentTitle;
            if (_expectedTitles.Contains(title)
                || string.IsNullOrWhiteSpace(title)
                || /*temp*/ title.Contains("localhost"))
            {
                log.Debug("normal document completed");

                BuildGinasToolsDocument();
                return;
            }
            else if (webViewGsrs.CoreWebView2.DocumentTitle.Equals(FORM_COMPLETE_TITLE))
            {
                webViewGsrs.NavigationCompleted -= WebViewGsrs_NavigationCompleted;
                log.Debug("webViewGsrs.CoreWebView2.DocumentTitle.Equals(FORM_COMPLETE_TITLE)");
                HideAppropriateSection();
                webViewGsrs.Visible = true;
                return;
            }
        }

        private async void WebViewGsrs_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            log.Debug("WebViewGsrs_WebMessageReceived");
            try
            {
                string message = e.WebMessageAsJson;
                message = message.Replace("\"", string.Empty);
                log.DebugFormat("WebViewGsrs_WebMessageReceived processing message: {0}", message);
                if (message.StartsWith("gsrs_"))
                {
                    string followupCommand = "cresults.popItem('" + message + "')";

                    object result = await ExecuteScriptInner(followupCommand, true);
                    log.DebugFormat("result of follow-up command: {0}", result);
                    if( result != null && result.ToString().Contains("'innerResultId':")) {
                        log.Debug("result starts with inner: ");
                        string cleanedResult = result.ToString();

                        string innerResultId = TextUtils.ExtractInnerResultId(cleanedResult);
                        string resultKey = "window.innerResults['" + innerResultId + "']";
                        result = await ExecuteScriptInner(resultKey, true);
                        result = TextUtils.ReplaceInnerResultId(cleanedResult, TextUtils.StripQuotes(result.ToString()));
                        log.DebugFormat("result after innerResults lookup: {0}", result);
                    }
                    await Controller.HandleResults(message, (string)result);
                    if (CurrentOperationType == OperationType.GetStructures)
                    {
                        log.Debug("Closing dialog after getting structures");
                        await HandleDebugInfoSave();
                        Controller.Dispose();
                        Close();
                    }
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
                else if(message.StartsWith("ImageFromMol:"))
                {
                    string cleanMessage = message.Replace("ImageFromMol:", string.Empty);
                    log.DebugFormat("ImageFromMol message: {0}", cleanMessage);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in WebViewGsrs_WebMessageReceived: " + ex.Message, ex);
            }

        }

        
        public string ScriptToExecute
        {
            set;
            private get;
        }

        public void StartSearch(string searchScript)
        {
            _=ExecuteScript(searchScript);
        }

        public async Task<object> ExecuteScript(string script)
        {
            if (script.StartsWith("cresults[") && script.EndsWith(".resolve();"))
            {
                log.Debug("ExecuteScript: " + script);
                _= ExecuteScriptInner(script, false);
                return null;
            }
            return await ExecuteScriptInner(script, true);
        }

        public async Task ExecuteScriptNoReturn(string script)
        {
            await ExecuteScriptInner(script, false);
        }

        private async Task<string> ExecuteScriptInner(string script, Boolean needResponse)
        {
            string loggableScript = (script != null && script.Length > 120) ? script.Substring(0, 119) : script;
            log.DebugFormat("Going to run script: {0}, needResponse: {1}", loggableScript, needResponse);
            if(_shuttingDown)
            {
                log.DebugFormat("This form is shutting down, not executing script: {0}", script);
                return null;
            }
            if (needResponse)
            {
                CoreWebView2ExecuteScriptResult result = await webViewGsrs.CoreWebView2.ExecuteScriptWithResultAsync(script);
                if (!result.Succeeded)
                {
                    log.ErrorFormat("Error running script: {0}", script);
                    log.Error($"Error executing script: {result.Exception.LineNumber}");
                    return null;
                }
                log.Debug("completed ExecuteScriptAsync call with results");
                string returnedValue = result.ResultAsJson;
                log.Debug($"returnedValue: {returnedValue} ");
                if (returnedValue is string && (returnedValue as string).StartsWith("'error running script: '"))
                {
                    log.Warn(returnedValue);
                }
                return returnedValue;
            }
            else
            {
                await webViewGsrs.CoreWebView2.ExecuteScriptAsync(script);
                log.Debug("completed ExecuteScriptAsync call without results ");
                return null;
            }
        }

        
        public void Proceed(string message)
        {
            log.Debug("message from browser: " + message);
        }

        private async void buttonResolve_Click(object sender, EventArgs e)
        {
            log.DebugFormat("buttonResolve_Click.  controller: {0}", Controller.GetType().Name);
            //check for overwrite
            if (CurrentOperationType == OperationType.Resolution && !checkBoxNewSheet.Checked)
            {
                string script = @"document.querySelectorAll('div.checkop input:checked').length;";
                //"$('div.checkop input:checked').length"
                string lengthString = await ExecuteScriptInner(script, true);
                log.DebugFormat($"lengthString: {lengthString}");
                if (string.IsNullOrEmpty(lengthString))
                {
                    log.Error("lengthString is null or empty");
                    return;
                }
                int totalNewColumns = Convert.ToInt32(lengthString);
                log.DebugFormat("click handler detected total number of new columns: " + totalNewColumns);
                if (!Controller.OkToWrite(totalNewColumns))
                {
                    log.Debug("user elected not to overwrite data");
                    return;
                }
            }
            buttonCancel.Enabled = true;
            if (!await Controller.StartResolution(checkBoxNewSheet.Checked))
            {
                MessageBox.Show("Error resolving your data.  Please try again or talk to your GSRS administrator");
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            //this.BeginInvoke((MethodInvoker)delegate { this.Close(); });
            //await HandleDebugInfoSave();
            Close();
        }

        private void buttonAddStructure_Click(object sender, EventArgs e)
        {
            CurrentOperationType = OperationType.GetStructures;
            Controller.StartOperation();
        }

        private async void RetrievalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.Debug("RetrievalForm_FormClosing");
            this.FormClosing -= RetrievalForm_FormClosing;
            if (!_savedDebugInfo) await HandleDebugInfoSave();
        }

        private async Task<bool> HandleDebugInfoSave()
        {
            log.Debug("at start of HandleDebugInfoSave, _savedDebugInfo: " + _savedDebugInfo);
            if ((checkBoxSaveDiagnostic.Checked) && !_savedDebugInfo)
            {
                string script = "GSRSAPI_consoleStack.join('|')";
                string debugInfo = await ExecuteScriptInner(script, true);
                if (!string.IsNullOrEmpty(debugInfo))
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
            return true;
        }

        private async void BuildGinasToolsDocument()
        {
            //clear out old event handlers and scripts... optimistically
            await webViewGsrs.CoreWebView2.ExecuteScriptAsync("$('document').off()");
            var before = await webViewGsrs.CoreWebView2.ExecuteScriptAsync("$('script').length");
            await webViewGsrs.CoreWebView2.ExecuteScriptAsync("$('script').remove();");
            var after = await webViewGsrs.CoreWebView2.ExecuteScriptAsync("$('script').length");
            log.DebugFormat("script count before: {0}; after: {1}", before, after);

            await webViewGsrs.CoreWebView2.ExecuteScriptAsync("$('link').remove(); ");
            await webViewGsrs.EnsureCoreWebView2Async();
            var devToolsContext = await webViewGsrs.CoreWebView2.CreateDevToolsContextAsync();
            
            HtmlElement rootElement = await devToolsContext.QuerySelectorAsync("html");
            var parent = await rootElement.GetParentAsync();

            log.DebugFormat("root: {0}; type: {1}; parent: {2} type: {3}", rootElement?.ToString(), rootElement.GetType().Name,
                parent?.ToString(), parent.GetType().Name);

            HtmlDocument document = null;
            if (parent is HtmlDocument documentTyped)
            {
                document = documentTyped;
                log.DebugFormat("found document object");
            }
            else
            {
                log.Error("Unable to cast root element to HtmlDocument.  Will not build document.");
                return;
            }

            if (document != null)
            {
                HtmlHeadElement headElement = (HtmlHeadElement) await devToolsContext.QuerySelectorAsync("head");
                await DomUtils.BuildDocumentHead(document, headElement);
                log.Debug("Building document body");
                await DomUtils.BuildDocumentBody(document,
                    (CurrentOperationType == OperationType.Loading || CurrentOperationType == OperationType.ShowScripts),
                _configuration.DebugMode);
                string imageFormat = Properties.Resources.ImageFormat;
                string mainScriptText = FileUtils.GetJavaScript().Replace("$IMGFORMAT$", imageFormat);
                
                await webViewGsrs.CoreWebView2.ExecuteScriptAsync(mainScriptText);
                await webViewGsrs.CoreWebView2.ExecuteScriptAsync(FileUtils.GetLastJavaScript());
                string shimScript = "if (!Array.prototype.getItem) { Array.prototype.getItem = function (i) { return this[i]; }; };var cresults = { 'getItem': function (v) { return this[v]; }, 'popItem': function (v) { var ret = this[v]; delete this[v]; return ret; } }; ";
                await webViewGsrs.CoreWebView2.ExecuteScriptAsync(shimScript);
                await webViewGsrs.CoreWebView2.ExecuteScriptAsync(FileUtils.GetJSChemify());
            }

            string baseUrl = CreateApiUrl(_baseUrl, _configuration.ApiPath);
            log.DebugFormat("baseUrl: {0}", baseUrl);
            string scriptResult = await ExecuteScriptInner("GlobalSettings.setBaseURL('" + baseUrl + "');", true);
            log.DebugFormat("Result of setBaseURL: {0}", scriptResult);
            if (scriptResult == null || scriptResult.ToString().Length == 0 ||
                scriptResult.ToString().StartsWith("error running script"))
            {
                callCount++;
                if (callCount < maxCallCount)
                {
                    log.WarnFormat("After attempt {0}, page is not valid", callCount);
                }
            }
            await ExecuteScriptInner("GlobalSettings.setStructureUrl('"
                + _configuration.SelectedServer.StructureUrl + "');", false);
            log.Debug("completed setStructureUrl");
            checkBoxSaveDiagnostic.Checked = _configuration.DebugMode;
            webViewGsrs.WebMessageReceived += WebViewGsrs_WebMessageReceived;
            log.Debug("assigned WebMessageReceived event");

            if (CurrentOperationType == OperationType.Loading || CurrentOperationType == OperationType.ProcessApplication)
            {
                buttonResolve.Text = "Execute";
                buttonAddStructure.Enabled = false;
                buttonAddStructure.Visible = false;
                checkBoxNewSheet.Enabled = false;
                checkBoxNewSheet.Visible = false;
                await ExecuteScriptInner("setMode('update');", false);
                await ExecuteScriptInner("$('#showScripts').hide();", false);
                await ExecuteScriptInner("$('#FormDiv').hide();", false);
                await ExecuteScriptInner("$('#argTemplate').hide();", false);
                await ExecuteScriptInner("$('div:contains(\"$name$\")').hide()", false);

                labelServerURL.Text = "Loading URL: " + _configuration.SelectedServer.ServerUrl;
                Controller.ContinueSetup();
                Visible = true;
                Text = "Data Loader";
            }
            else if (CurrentOperationType == OperationType.Resolution)
            {
                await ExecuteScriptInner("setMode('resolver');", false);
                await ExecuteScriptInner("$('#showScripts').hide();", false);
                await ExecuteScriptInner("$('#argTemplate').hide();", false);
                buttonAddStructure.Enabled = false;
                buttonAddStructure.Visible = false;
                webViewGsrs.Visible = true;
                this.Visible = true;
                Text = "Data Retriever";

            }
            else if (CurrentOperationType == OperationType.ShowScripts)
            {
                log.Debug("OperationType.ShowScripts");
                await ExecuteScriptInner("setMode('showScripts');", false);
                await ExecuteScriptInner("$('#FormDiv').hide();", false);
                await ExecuteScriptInner("$('#fetcherTemplate').hide();", false);
                await ExecuteScriptInner("$('#showScripts').show();", false);
                await ExecuteScriptInner("$('#argTemplate').hide();", false);
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
                await ExecuteScriptInner("setMode('resolver');", false);
                //_scriptToRunUponCompletion allows the dialog to process information without becoming visible
                if (!string.IsNullOrWhiteSpace(_scriptToRunUponCompletion))
                {
                    await ExecuteScriptInner(_scriptToRunUponCompletion, false);
                }
            }
            else if (CurrentOperationType == OperationType.ProcessSdFile)
            {
                Visible = false;
                Controller.StartOperation();
                return;
            }
            else if (CurrentOperationType == OperationType.AddIngredient)
            {
                Controller.ContinueSetup();
            }
            buttonDebugDOM.Enabled = false;
            buttonDebugDOM.Visible = false;
            checkBoxSaveDiagnostic.Enabled = _configuration.DebugMode;

            string command = "GSRSAPI.setImageWidth(" + _configuration.StructureImageSize + ")";
            await ExecuteScriptNoReturn(command);
            command = "GSRSAPI.setImageHeight(" + _configuration.StructureImageSize + ")";
            await ExecuteScriptNoReturn(command);

            webViewGsrs.Visible = true;
            webViewGsrs.BringToFront();
            await devToolsContext.DisposeAsync();
            IsReady = true;
        }


        public void HandleClick(object obj, EventArgs args)
        {
            MessageBox.Show(args.ToString());
        }

        public bool GetDebugSetting()
        {
            return checkBoxSaveDiagnostic.Checked;
        }

        private async void buttonDebugDOM_Click(object sender, EventArgs e)
        {
            string dom = await ExecuteScriptInner("document.documentElement.outerHTML", true);
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
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "HTML files(*.html)|*.html|All files (*.*)|(*.*)"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                webViewGsrs.CoreWebView2.Navigate(dialog.FileName);
            }
        }

        public bool HasUserCancelled()
        {
            return false;
        }

        public static string CreateApiUrl(string baseUrl, string apiPortion)
        {
            string workingUrl = baseUrl;
            if (workingUrl.EndsWith("/"))
            {
                workingUrl = workingUrl.Substring(0, workingUrl.Length - 1);
            }
            if (workingUrl.EndsWith("app"))
            {
                workingUrl = workingUrl.Substring(0, workingUrl.Length - 4);
            }
            if (!apiPortion.StartsWith("app", StringComparison.CurrentCultureIgnoreCase) && workingUrl.Contains("ginas"))
            {
                log.Debug("appending 'app'");
                workingUrl += "/app/";
            }
            else if (!apiPortion.StartsWith("/"))
            {
                workingUrl += "/";
            }
            workingUrl += apiPortion;
            log.DebugFormat("CreateApiUrl about to return {0}", workingUrl);
            return workingUrl;
        }

        private async void HideAppropriateSection()
        {
            if (CurrentOperationType == OperationType.Loading || CurrentOperationType == OperationType.ProcessApplication)
            {
                await ExecuteScriptInner("$('#FormDiv').hide();", false);
            }
            else if (CurrentOperationType == OperationType.Resolution)
            {
                await ExecuteScriptInner("$('#FormDiv').show();", false);
                await ExecuteScriptInner("$('#showScripts').hide();", false);
            }
            else if (CurrentOperationType == OperationType.ShowScripts)
            {
                await ExecuteScriptInner("$('#FormDiv').hide();", false);
                await ExecuteScriptInner("$('#showScripts').show();", false);
                await ExecuteScriptInner("showScripts();", false);
            }
        }
    }
}
