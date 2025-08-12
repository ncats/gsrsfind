using gov.ncats.ginas.excel.tools.Utils;
using GSRSExcelTools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using WebView2.DevTools.Dom;

namespace gov.ncats.ginas.excel.tools.Utils.Tests
{
    [TestClass()]
    public class DomUtilsTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        WebView2.DevTools.Dom.HtmlDocument? doc = null;
        Microsoft.Web.WebView2.WinForms.WebView2? webBrowser;
        WebView2DevToolsContext devToolsContext;

        [TestMethod()]
        [Ignore]
        public async Task BuildDocumentBodyTest()
        {
            string docText = "<!DOCTYPE html><html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\"><head>    <meta charset=\"utf-8\" />    <title>Error</title></head><body style=\"text-align:center\">    <h1>Error Page</h1>    <br/>    <h2>Sorry to report that an error occurred</h2>    <br/>    <h3 id=\"ErrorMessage\">$MESSAGE1$</h3>    <br/>    <h4>$MESSAGE2$</h4></body></html>";
            webBrowser = new Microsoft.Web.WebView2.WinForms.WebView2();
            webBrowser.CreationProperties = null;
            webBrowser.Location = new System.Drawing.Point(0, 0);
            webBrowser.Name = "webViewTestForm";
            webBrowser.Size = new System.Drawing.Size(597, 375);
            webBrowser.TabIndex = 2;
            webBrowser.ZoomFactor = 1D;
            webBrowser.DefaultBackgroundColor = System.Drawing.Color.Orange;
            webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom));
            webBrowser.CoreWebView2.NavigationCompleted += WebBrowser_DocumentCompleted;
            webBrowser.CoreWebView2.NavigateToString(docText);
            devToolsContext = await webBrowser.CoreWebView2.CreateDevToolsContextAsync();
            WebView2.DevTools.Dom.HtmlElement rootElement = await devToolsContext.QuerySelectorAsync("html");
            var parent = await rootElement.GetParentAsync();
            if (parent is WebView2.DevTools.Dom.HtmlDocument)
            {
                doc = (WebView2.DevTools.Dom.HtmlDocument)parent;
                log.DebugFormat("found document object");
            }
            Assert.IsNotNull(doc);
        }

        private async void WebBrowser_DocumentCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await DomUtils.BuildDocumentBody(doc, false);
            CoreWebView2Environment environment = await SetupEnvironment();
            await webBrowser.EnsureCoreWebView2Async(environment);
            string documentText = await devToolsContext.GetContentAsync();
            Assert.IsTrue(documentText.Contains("fetcherTemplate"));
        }
        private async Task<CoreWebView2Environment> SetupEnvironment()
        {
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GsrsExcelTools/WebView2TestData"
                );
            Directory.CreateDirectory(userDataFolder); // Ensure directory exists

            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: userDataFolder
            );
            return env;

        }

    }
}