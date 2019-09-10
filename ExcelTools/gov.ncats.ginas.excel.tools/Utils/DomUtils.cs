using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using mshtml;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class DomUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void BuildDocumentBody(HtmlDocument document, bool includeScriptMaterial = false,
            bool makeDebugVisible = false)
        {
            log.Debug("Starting in " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            HtmlElement bodyElement = document.Body;
            bodyElement.InnerText = string.Empty;

            HtmlElement newDiv = document.CreateElement("div");
            newDiv.SetAttribute("class", "FormDiv");
            newDiv.SetAttribute("id", "FormDiv");
            newDiv.SetAttribute("style", "margin-left: 10px;");

            HtmlElement outputForm = document.CreateElement("form");
            outputForm.SetAttribute("id", "outputSettings");
            newDiv.AppendChild(outputForm);
            bodyElement.AppendChild(newDiv);
            if (includeScriptMaterial)
            {
                HtmlElement divElement = document.CreateElement("div");
                divElement.SetAttribute("id", "showScripts");
                HtmlElement innerDiv = document.CreateElement("div");
                innerDiv.SetAttribute("className", "scriptlist");
                HtmlElement h3ElementScripts = document.CreateElement("h4");
                h3ElementScripts.InnerHtml = "Please select a script and click 'Add Sheet'";
                h3ElementScripts.SetAttribute("id", "scriptListHeader");
                h3ElementScripts.SetAttribute("className", "scriptListHeader");
                innerDiv.AppendChild(h3ElementScripts);
                HtmlElement selectElement = document.CreateElement("select");
                selectElement.SetAttribute("id", "scriptlist");
                selectElement.SetAttribute("size", "10");
                innerDiv.AppendChild(selectElement);
                divElement.AppendChild(innerDiv);

                innerDiv = document.CreateElement("div");
                innerDiv.SetAttribute("className", "scriptdetails");
                innerDiv.SetAttribute("id", "scriptdetails");
                divElement.AppendChild(innerDiv);
                HtmlElement brElement2 = document.CreateElement("br");
                divElement.AppendChild(brElement2);

                HtmlElement label = document.CreateElement("span");
                label.InnerText = "Number of rows:";
                label.SetAttribute("className", "numberrows");
                divElement.AppendChild(label);
                HtmlElement textBoxRows = document.CreateElement("input");
                textBoxRows.SetAttribute("type", "text");
                textBoxRows.SetAttribute("id", "numberOfRows");
                textBoxRows.SetAttribute("name", "numberOfRows");
                textBoxRows.SetAttribute("value", "10");
                
                divElement.AppendChild(textBoxRows);
                HtmlElement label2 = document.CreateElement("span");
                label2.InnerText = "(Extra rows do not cause a problem)";
                label2.SetAttribute("className", "SmallerText");
                divElement.AppendChild(label2);

                bodyElement.AppendChild(divElement);
            }

            HtmlElement divElement2 = document.CreateElement("div");
            HtmlElement formElement = document.CreateElement("form");
            formElement.SetAttribute("id", "scriptArguments");
            divElement2.AppendChild(formElement);
            bodyElement.AppendChild(divElement2);

            divElement2 = document.CreateElement("div");
            divElement2.SetAttribute("id", "argTemplate");
            divElement2.SetAttribute("className", "template");

            HtmlElement innerDiv2= document.CreateElement("div");
            innerDiv2.SetAttribute("className", "argument");

            HtmlElement inner2Div = document.CreateElement("div");
            HtmlElement labelElement = document.CreateElement("label");
            labelElement.SetAttribute("for", "$arg$");
            labelElement.InnerText = "$arg$";
            HtmlElement spanElement = document.CreateElement("span");
            spanElement.SetAttribute("className", "required");
            spanElement.SetAttribute("title", "required");
            spanElement.InnerText = "$req$";
            inner2Div.AppendChild(labelElement);
            inner2Div.AppendChild(spanElement);
            innerDiv2.AppendChild(inner2Div);

            inner2Div = document.CreateElement("div");
            HtmlElement textAreaElement = document.CreateElement("textarea");
            textAreaElement.SetAttribute("id", "$arg$Value");
            textAreaElement.SetAttribute("name", "$arg$");
            textAreaElement.SetAttribute("className", "paramArgValue");
            textAreaElement.InnerText = "$value$";
            if(!includeScriptMaterial) textAreaElement.Style = "visibility:hidden";
            inner2Div.AppendChild(textAreaElement);
            innerDiv2.AppendChild(inner2Div);
            divElement2.AppendChild(innerDiv2);
            bodyElement.AppendChild(divElement2);

            HtmlElement divElement3 = document.CreateElement("div");
            divElement3.SetAttribute("id", "fetcherTemplate");
            divElement3.SetAttribute("className", "template");
            if (!includeScriptMaterial) divElement3.Style = "visibility:hidden";
            innerDiv2= document.CreateElement("div");
            innerDiv2.SetAttribute("className", "checkop");
            HtmlElement inputElement = document.CreateElement("input");
            inputElement.SetAttribute("type", "checkbox");
            inputElement.SetAttribute("name", "$name$");
            inputElement.SetAttribute("id", "$name$");
            labelElement = document.CreateElement("label");
            labelElement.SetAttribute("for", "$name$");
            labelElement.InnerText = "$name$";

            innerDiv2.AppendChild(inputElement);
            innerDiv2.AppendChild(labelElement);

            divElement3.AppendChild(innerDiv2);
            bodyElement.AppendChild(divElement3);

            HtmlElement brElement = document.CreateElement("br");
            bodyElement.AppendChild(brElement);
            brElement = document.CreateElement("br");
            bodyElement.AppendChild(brElement);
            HtmlElement mainFormElement = document.CreateElement("form");
            mainFormElement.SetAttribute("name", "ginas");
            HtmlElement h3Element = document.CreateElement("h3");
            h3Element.SetAttribute("className", "consolehead");
            h3Element.SetAttribute("id", "consoleHeadWebOutput");
            h3Element.InnerText = "Web output:";
            h3Element.Style = "visibility:hidden";
            mainFormElement.AppendChild(h3Element);

            brElement = document.CreateElement("br");
            mainFormElement.AppendChild(brElement);
            textAreaElement = document.CreateElement("textarea");
            textAreaElement.SetAttribute("id", "console");
            textAreaElement.Style = "visibility:hidden";
            mainFormElement.AppendChild(textAreaElement);

            brElement = document.CreateElement("br");
            mainFormElement.AppendChild(brElement);
            bodyElement.AppendChild(mainFormElement);
            HtmlElement lastScript = document.CreateElement("script");
            IHTMLScriptElement scriptElement = (IHTMLScriptElement)lastScript.DomElement;
            scriptElement.text = FileUtils.GetLastJavaScript();
            bodyElement.AppendChild(lastScript);
            
            HtmlElement refresherFrame = document.CreateElement("iframe");
            refresherFrame.SetAttribute("id", "refresher");
            refresherFrame.Style = "height:1px;opacity:0;display: none;" ;
            bodyElement.AppendChild(refresherFrame);
    
            /*if (makeDebugVisible)
            {
                HtmlElement consoleScript = document.CreateElement("script");
                consoleScript.SetAttribute("type", "text/javascript");

                //allow 'normal' logging as well as custom
                consoleScript.SetAttribute("text", "window['oldconsole'] = window['console'];window['console'] = {log: function (r){ GSRSAPI_consoleStack.push(r);}}");//;oldconsole.log(r)
                bodyElement.AppendChild(consoleScript);
            }*/
            bodyElement.SetAttribute("className", string.Empty);
            bodyElement.Style = "padding-top:10px";
            log.Debug("Completed " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public static void BuildDocumentHead(HtmlDocument document)
        {
            
            log.Debug("Starting in " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            try
            {
                HtmlElement headElement = GetFirstHead(document);
                if (headElement == null)
                {
                    log.Warn("No head element found; using body");
                    headElement = document.Body;
                }
                string inner = headElement.InnerText;

                HTMLHeadElement head = (HTMLHeadElement) headElement.DomElement;

                HtmlElement metaCompat = document.CreateElement("meta");
                metaCompat.SetAttribute("http-equiv", "X-UA-Compatible");
                metaCompat.SetAttribute("content", "IE=Edge ");
                headElement.AppendChild(metaCompat);

                HtmlElement metaCharset = document.CreateElement("meta");
                metaCharset.SetAttribute("content", "text/html; charset=UTF-8");
                metaCharset.SetAttribute("http-equiv", "content-type");
                headElement.AppendChild(metaCharset);

                AddScripts(headElement);
                
                HtmlElement styleElement = document.CreateElement("style");
                styleElement.SetAttribute("type", "text/css");
                object domStyleElement = styleElement.DomElement;
                MSHTML.HTMLStyleElement htmlStyleElement = (MSHTML.HTMLStyleElement)domStyleElement;
                htmlStyleElement.styleSheet.cssText = Environment.NewLine + Environment.NewLine 
                    + FileUtils.GetCss();
                headElement.AppendChild(styleElement);
                log.Debug("completed " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            catch(Exception ex)
            {
                log.ErrorFormat("Error building header: {0}", ex.Message);
                log.Error(ex.StackTrace);
            }
        }

        public static HtmlElement GetFirstHead(HtmlDocument document)
        {
            HtmlElement headElement = null;
            foreach (HtmlElement elem in document.All)
            {
                if (elem.TagName.Equals("head", StringComparison.CurrentCultureIgnoreCase))
                {
                    return elem;
                }
            }
            return headElement;
        }

        private static void AddScripts(HtmlElement headElement)
        {
            log.Debug("Starting in AddScripts");
            HtmlDocument document = headElement.Document;
            HtmlElement brElement = document.CreateElement("BR");
            HtmlElement json2Script = document.CreateElement("script");
            json2Script.SetAttribute("type", "text/javascript");
            json2Script.SetAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/json2/20160511/json2.min.js");
            headElement.AppendChild(json2Script);

            HtmlElement jQueryScript = document.CreateElement("script");
            jQueryScript.SetAttribute("type", "text/javascript");
            jQueryScript.SetAttribute("src", "https://code.jquery.com/jquery-1.12.4.js");
            headElement.AppendChild(jQueryScript);
            headElement.AppendChild(brElement);

            HtmlElement lodashScript = document.CreateElement("script");
            lodashScript.SetAttribute("type", "text/javascript");
            lodashScript.SetAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/lodash.js/3.0.0/lodash.min.js");
            headElement.AppendChild(lodashScript);
            headElement.AppendChild(brElement);

            HtmlElement jsonPatchScript = document.CreateElement("script");
            jsonPatchScript.SetAttribute("type", "text/javascript");
            jsonPatchScript.SetAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/fast-json-patch/1.0.1/json-patch.min.js");
            headElement.AppendChild(jsonPatchScript);
            headElement.AppendChild(brElement);

            HtmlElement bootstrapScript = document.CreateElement("script");
            bootstrapScript.SetAttribute("type", "text/javascript");
            bootstrapScript.SetAttribute("src", "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.4/js/bootstrap.min.js");
            headElement.AppendChild(bootstrapScript);
            headElement.AppendChild(brElement);

            HtmlElement mainGinasScript = document.CreateElement("script");
            mainGinasScript.SetAttribute("type", "text/javascript");
            string imageFormat = Properties.Resources.ImageFormat;
            //mainGinasScript.InnerHtml = FileUtils.GetJavaScript().Replace("$IMGFORMAT$", imageFormat);
            IHTMLScriptElement element = (IHTMLScriptElement)mainGinasScript.DomElement;
            element.text = FileUtils.GetJavaScript().Replace("$IMGFORMAT$", imageFormat);
            headElement.AppendChild(mainGinasScript);

            HtmlElement shimScript = document.CreateElement("script");
            shimScript.SetAttribute("type", "text/javascript");
            IHTMLScriptElement scriptElement = (IHTMLScriptElement)shimScript.DomElement;
            scriptElement.text = "if (!Array.prototype.getItem) { Array.prototype.getItem = function (i) { return this[i]; }; };var cresults = { 'getItem': function (v) { return this[v]; }, 'popItem': function (v) { var ret = this[v]; delete this[v]; return ret; } }; ";
            headElement.AppendChild(shimScript);
        }

        public static void BuildJavascriptDom()
        {

        }
    }
}
