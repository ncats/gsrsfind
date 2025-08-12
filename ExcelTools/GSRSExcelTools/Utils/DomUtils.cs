using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using HtmlDocument = WebView2.DevTools.Dom.HtmlDocument;
using HtmlElement = WebView2.DevTools.Dom.HtmlElement;

namespace GSRSExcelTools.Utils
{
    public class DomUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task BuildDocumentBody(HtmlDocument document, bool includeScriptMaterial = false,
            bool makeDebugVisible = false)
        {
            log.Debug("Starting in BuildDocumentBod");
            HtmlElement bodyElement = await document.GetBodyAsync();
            await bodyElement.SetInnerTextAsync(string.Empty);

            HtmlElement newDiv = (HtmlElement) await document.CreateElementAsync("div");
            await newDiv.SetAttributeAsync("class", "FormDiv");
            await newDiv.SetAttributeAsync("id", "FormDiv");
            await newDiv.SetAttributeAsync("style", "margin-left: 10px;");

            HtmlElement outputForm = (HtmlElement) await document.CreateElementAsync("form");
            await outputForm.SetAttributeAsync("id", "outputSettings");
            await newDiv.AppendChildAsync(outputForm);
            await bodyElement.AppendChildAsync(newDiv);
            if (includeScriptMaterial)
            {
                HtmlElement divElement = (HtmlElement) await document.CreateElementAsync("div");
                await divElement.SetAttributeAsync("id", "showScripts");
                HtmlElement innerDiv = (HtmlElement) await document.CreateElementAsync("div");
                await innerDiv.SetAttributeAsync("className", "scriptlist");
                HtmlElement h3ElementScripts = (HtmlElement) await document.CreateElementAsync("h4");
                await h3ElementScripts.SetInnerTextAsync("Please select a script and click 'Add Sheet'");
                await h3ElementScripts.SetAttributeAsync("id", "scriptListHeader");
                await h3ElementScripts.SetAttributeAsync("className", "scriptListHeader");
                await innerDiv.AppendChildAsync(h3ElementScripts);
                HtmlElement selectElement = (HtmlElement) await document.CreateElementAsync("select");
                await selectElement.SetAttributeAsync("id", "scriptlist");
                await selectElement.SetAttributeAsync("size", "10");
                await innerDiv.AppendChildAsync(selectElement);
                await divElement.AppendChildAsync(innerDiv);

                innerDiv = (HtmlElement) await document.CreateElementAsync("div");
                await innerDiv.SetAttributeAsync("className", "scriptdetails");
                await innerDiv.SetAttributeAsync("id", "scriptdetails");
                await divElement.AppendChildAsync(innerDiv);
                HtmlElement brElement2 = (HtmlElement) await document.CreateElementAsync("br");
                await divElement.AppendChildAsync(brElement2);

                HtmlElement label = (HtmlElement) await document.CreateElementAsync("span");
                await label.SetInnerTextAsync( "Number of rows:");
                await label.SetAttributeAsync("className", "numberrows");
                await divElement.AppendChildAsync(label);
                HtmlElement textBoxRows = (HtmlElement) await document.CreateElementAsync("input");
                await textBoxRows.SetAttributeAsync("type", "text");
                await textBoxRows.SetAttributeAsync("id", "numberOfRows");
                await textBoxRows.SetAttributeAsync("name", "numberOfRows");
                await textBoxRows.SetAttributeAsync("value", "10");
                
                await divElement.AppendChildAsync(textBoxRows);
                HtmlElement label2 = (HtmlElement) await document.CreateElementAsync("span");
                await label2.SetInnerTextAsync("(Extra rows do not cause a problem)");
                await label2.SetAttributeAsync("className", "SmallerText");
                await divElement.AppendChildAsync(label2);

                await bodyElement.AppendChildAsync(divElement);
            }

            HtmlElement divElement2 = (HtmlElement) await document.CreateElementAsync("div");
            HtmlElement formElement = (HtmlElement) await document.CreateElementAsync("form");
            await formElement.SetAttributeAsync("id", "scriptArguments");
            await divElement2.AppendChildAsync(formElement);
            await bodyElement.AppendChildAsync(divElement2);

            HtmlElement divElement3 = (HtmlElement) await document.CreateElementAsync("div");
            await divElement3.SetAttributeAsync("id", "argTemplate");
            await divElement3.SetAttributeAsync("className", "template");

            HtmlElement innerDiv2= (HtmlElement) await document.CreateElementAsync("div");
            await innerDiv2.SetAttributeAsync("className", "argument");

            HtmlElement inner2Div = (HtmlElement) await document.CreateElementAsync("div");
            HtmlElement labelElement = (HtmlElement) await document.CreateElementAsync("label");
            await labelElement.SetAttributeAsync("for", "$arg$");
            await labelElement.SetInnerTextAsync("$arg$");
            HtmlElement spanElement = (HtmlElement) await document.CreateElementAsync("span");
            await spanElement.SetAttributeAsync("className", "required");
            await spanElement.SetAttributeAsync("title", "required");
            await spanElement.SetInnerTextAsync("$req$");
            await inner2Div.AppendChildAsync(labelElement);
            await inner2Div.AppendChildAsync(spanElement);
            await innerDiv2.AppendChildAsync(inner2Div);

            inner2Div = (HtmlElement) await document.CreateElementAsync("div");
            HtmlElement textAreaElement = (HtmlElement) await document.CreateElementAsync("textarea");
            await textAreaElement.SetAttributeAsync("id", "$arg$Value");
            await textAreaElement.SetAttributeAsync("name", "$arg$");
            await textAreaElement.SetAttributeAsync("className", "paramArgValue");
            await textAreaElement.SetInnerTextAsync("$value$");
            if (!includeScriptMaterial)
            {
                //await textAreaElement.EvaluateFunctionAsync("el=> el.style.visibility = 'hidden'");
                await textAreaElement.SetAttributeAsync("style", "visibility: hidden;");
            }
            await inner2Div.AppendChildAsync(textAreaElement);
            await innerDiv2.AppendChildAsync(inner2Div);
            await divElement3.AppendChildAsync(innerDiv2);
            await bodyElement.AppendChildAsync(divElement3);

            HtmlElement divElement4 = (HtmlElement) await document.CreateElementAsync("div");
            await divElement4.SetAttributeAsync("id", "fetcherTemplate");
            await divElement4.SetAttributeAsync("className", "template");
            if (!includeScriptMaterial) { 
                //await divElement3.EvaluateFunctionAsync("el=> el.style.visibility = 'hidden'");
                await divElement4.SetAttributeAsync("style", "visibility: hidden;");
            }
            innerDiv2= (HtmlElement) await document.CreateElementAsync("div");
            await innerDiv2.SetAttributeAsync("className", "checkop");
            HtmlElement inputElement = (HtmlElement) await document.CreateElementAsync("input");
            await inputElement.SetAttributeAsync("type", "checkbox");
            await inputElement.SetAttributeAsync("name", "$name$");
            await inputElement.SetAttributeAsync("id", "$name$");
            labelElement = (HtmlElement) await document.CreateElementAsync("label");
            await labelElement.SetAttributeAsync("for", "$name$");
            await labelElement.SetInnerTextAsync("$name$");

            await innerDiv2.AppendChildAsync(inputElement);
            await innerDiv2.AppendChildAsync(labelElement);

            await divElement4.AppendChildAsync(innerDiv2);
            await bodyElement.AppendChildAsync(divElement4);

            HtmlElement brElement = (HtmlElement)await document.CreateElementAsync("br");
            await bodyElement.AppendChildAsync(brElement);
            brElement = (HtmlElement) await document.CreateElementAsync("br");
            await bodyElement.AppendChildAsync(brElement);
            HtmlElement mainFormElement =(HtmlElement) await document.CreateElementAsync("form");
            await mainFormElement.SetAttributeAsync("name", "gsrs");//was 'ginas' 6 May 2021
            HtmlElement h3Element = (HtmlElement) await document.CreateElementAsync("h3");
            await h3Element.SetAttributeAsync("className", "consolehead");
            await h3Element.SetAttributeAsync("id", "consoleHeadWebOutput");
            await h3Element.SetInnerTextAsync("Web output:");
            //await h3Element.EvaluateFunctionAsync("el=>el.style.visibility='hidden'");
            await h3Element.SetAttributeAsync("style", "visibility: hidden;");
            await mainFormElement.AppendChildAsync(h3Element);

            brElement = (HtmlElement) await document.CreateElementAsync("br");
            await mainFormElement.AppendChildAsync(brElement);
            textAreaElement = (HtmlElement) await document.CreateElementAsync("textarea");
            await textAreaElement.SetAttributeAsync("id", "console");
            //await textAreaElement.EvaluateFunctionAsync("el=>el.style.visibility ='hidden'");
            await textAreaElement.SetAttributeAsync("style", "visibility: hidden;");
            await mainFormElement.AppendChildAsync(textAreaElement);

            brElement = (HtmlElement) await document.CreateElementAsync("br");
            await mainFormElement.AppendChildAsync(brElement);
            await bodyElement.AppendChildAsync(mainFormElement);
            HtmlElement lastScript = (HtmlElement) await document.CreateElementAsync("script");
            await lastScript.SetInnerTextAsync(FileUtils.GetLastJavaScript());

            HtmlElement refresherFrame = (HtmlElement) await document.CreateElementAsync("iframe");
            await refresherFrame.SetAttributeAsync("id", "refresher");
            //await refresherFrame.EvaluateFunctionAsync("el =>{el.style.height='1px'; el.style.opacity=0; el.style.display='none';}");
            await refresherFrame.SetAttributeAsync("style", "height:1px;opacity:0;display: none;");
            //refresherFrame.Style = "height:1px;opacity:0;display: none;" ;
            await bodyElement.AppendChildAsync(refresherFrame);
    
            await bodyElement.SetAttributeAsync("className", string.Empty);
            await bodyElement.SetAttributeAsync("style", "paddingTop: 10px;");
            LogDebug("Completed");
        }

        public static async Task BuildDocumentHead(HtmlDocument document, HtmlHeadElement headElement)
        {
            log.Debug("Starting in BuildDocumentHead");
            try
            {
                string inner = await headElement.GetInnerHtmlAsync();

                HtmlElement metaCharset = (HtmlElement) await document.CreateElementAsync("meta");
                await metaCharset.SetAttributeAsync("http-equiv", "content-type");
                await metaCharset.SetAttributeAsync("content", "text/html; charset=UTF-8");
                await headElement.AppendChildAsync(metaCharset);

                await AddScripts(document, headElement);

                HtmlElement styleElement = (HtmlElement) await document.CreateElementAsync("style");
                await styleElement.SetAttributeAsync("type", "text/css");

                await styleElement.SetInnerTextAsync(FileUtils.GetCss());
                await headElement.AppendChildAsync(styleElement);
                LogDebug("completed");
            }
            catch(Exception ex)
            {
                log.ErrorFormat("Error building header: {0}", ex.Message);
                log.Error(ex.StackTrace);
            }
        }


        private static async Task AddScripts(HtmlDocument document, HtmlElement headElement)
        {
            log.Debug("Starting in AddScripts");

            HtmlElement lodashScript = (HtmlElement) await document.CreateElementAsync("script");
            await lodashScript.SetAttributeAsync("type", "text/javascript");
            await lodashScript.SetAttributeAsync("src", "https://cdnjs.cloudflare.com/ajax/libs/lodash.js/4.17.21/lodash.js");
            await lodashScript.SetAttributeAsync("crossorigin", "anonymous");
            await headElement.AppendChildAsync(lodashScript);

            HtmlElement jQueryScript = (HtmlElement) await document.CreateElementAsync("script");
            await jQueryScript.SetAttributeAsync("type", "text/javascript");
            await jQueryScript.SetAttributeAsync("src", "https://code.jquery.com/jquery-3.7.1.min.js");
            await jQueryScript.SetAttributeAsync("integrity", "sha256-/JqT3SQfawRcv/BIHPThkBvs0OEvtFFmqPF/lYI/Cxo=");
            await jQueryScript.SetAttributeAsync("crossorigin", "anonymous");
            await headElement.AppendChildAsync(jQueryScript);

            HtmlElement json2Script = (HtmlElement) await document.CreateElementAsync("script");
            await json2Script.SetAttributeAsync("type", "text/javascript");
            await json2Script.SetAttributeAsync("src", "https://cdnjs.cloudflare.com/ajax/libs/json2/20160511/json2.js");
            await headElement.AppendChildAsync(json2Script);

            HtmlElement jsonPatchScript = (HtmlElement) await document.CreateElementAsync("script");
            await jsonPatchScript.SetAttributeAsync("type", "text/javascript");
            await jsonPatchScript.SetAttributeAsync("src",
                "https://cdnjs.cloudflare.com/ajax/libs/fast-json-patch/1.0.1/json-patch.min.js");
            await headElement.AppendChildAsync(jsonPatchScript);

            HtmlElement bootstrapScript = (HtmlElement) await document.CreateElementAsync("script");
            await bootstrapScript.SetAttributeAsync("type", "text/javascript");
            await bootstrapScript.SetAttributeAsync("src",
                "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.4/js/bootstrap.min.js");
            await headElement.AppendChildAsync(bootstrapScript);
        
            /*HtmlElement jsChemifyScript = (HtmlElement)await document.CreateElementAsync("script");
            await jsChemifyScript.SetAttributeAsync("type", "text/javascript");
            await jsChemifyScript.SetInnerTextAsync(FileUtils.GetJSChemify());
            await headElement.AppendChildAsync(jsChemifyScript);*/
            log.Debug("Completed AddScripts");
        }

        public static void BuildJavascriptDom()
        {

        }

        public static void LogDebug(string message, [CallerMemberName] string memberName = "")
        {
            log.DebugFormat("message {1} from {0}", message, memberName);
        }
    }
}
