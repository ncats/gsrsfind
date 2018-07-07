using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

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
            bodyElement.InnerHtml = string.Empty;

            HtmlElement newDiv = document.CreateElement("div");

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
                divElement.AppendChild(label);
                HtmlElement textBoxRows = document.CreateElement("input");
                textBoxRows.SetAttribute("type", "text");
                textBoxRows.SetAttribute("id", "numberOfRows");
                textBoxRows.SetAttribute("name", "numberOfRows");
                textBoxRows.SetAttribute("value", "10");
                divElement.AppendChild(textBoxRows);
                HtmlElement label2 = document.CreateElement("span");
                label2.InnerText = "(Extra rows do not cause a problem)";
                label2.Style = "SmallerText";
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
            //if (!makeDebugVisible)
            //{
                h3Element.Style = "visibility:hidden";
            //}
            mainFormElement.AppendChild(h3Element);

            brElement = document.CreateElement("br");
            mainFormElement.AppendChild(brElement);
            textAreaElement = document.CreateElement("textarea");
            textAreaElement.SetAttribute("id", "console");
            //if (!makeDebugVisible)
            //{
                textAreaElement.Style = "visibility:hidden";
            //}
            mainFormElement.AppendChild(textAreaElement);

            brElement = document.CreateElement("br");
            mainFormElement.AppendChild(brElement);

            //spanElement = document.CreateElement("span");
            //spanElement.InnerText = "Command:";
            //mainFormElement.AppendChild(spanElement);

            //HtmlElement textInputElement = document.CreateElement("input");
            //textInputElement.SetAttribute("type", "text");
            //textInputElement.SetAttribute("name", "commandInput");
            //textInputElement.SetAttribute("id", "commandInput");
            //textInputElement.SetAttribute("placeholder", "[command to run]");
            //textInputElement.SetAttribute("size", "50");
            //textInputElement.SetAttribute("value", "window");
            //mainFormElement.AppendChild(textInputElement);

            //inputElement = document.CreateElement("input");
            //inputElement.SetAttribute("type", "button");
            //inputElement.SetAttribute("value", "Run");
            //inputElement.SetAttribute("click", "alert('hello!');");
            ////inputElement.SetAttribute("onclick",
            ////    "runCommandForCSharp(document.getElementById('commandInput').value)");
            //mainFormElement.AppendChild(inputElement);

            bodyElement.AppendChild(mainFormElement);
            HtmlElement lastScript = document.CreateElement("script");
            lastScript.InnerHtml= FileUtils.GetLastJavaScript();
            bodyElement.AppendChild(lastScript);

            HtmlElement consoleScript = document.CreateElement("script");
            consoleScript.SetAttribute("type", "text/javascript");
            if (makeDebugVisible)
            {
                consoleScript.InnerHtml = "window['console'] = {log: function (r){GSRSAPI_consoleStack.push(r);}}";
            }
            else
            {
                consoleScript.InnerHtml = "window['console'] = {log: function (r){/*do nothing*/}}";
            }
            bodyElement.AppendChild(consoleScript);
        }

        public static void BuildDocumentHead(HtmlDocument document)
        {
            log.Debug("Starting in " + System.Reflection.MethodBase.GetCurrentMethod().Name);

            HtmlElement headElement = GetFirstHead(document);
            if (headElement == null)
            {
                log.Warn("No head object found; using body");
                headElement = document.Body;
            }
            string inner = headElement.InnerText;
            headElement.InnerHtml = string.Empty;

            HtmlElement metaCharset = document.CreateElement("meta");
            metaCharset.SetAttribute("http-equiv", "content-type");
            metaCharset.SetAttribute("content", "text/html; charset=UTF-8");
            metaCharset.InnerHtml = metaCharset.InnerHtml + Environment.NewLine;
            headElement.AppendChild(metaCharset);

            HtmlElement brElement = document.CreateElement("br");

            HtmlElement metaCompat = document.CreateElement("meta");
            metaCompat.SetAttribute("http-equiv", "X-UA-Compatible");
            metaCompat.SetAttribute("content", "IE=Edge");
            headElement.AppendChild(metaCompat);
            headElement.AppendChild(brElement);

            HtmlElement jQueryScript = document.CreateElement("script");
            jQueryScript.SetAttribute("type", "text/javascript");
            jQueryScript.SetAttribute("src", "https://code.jquery.com/jquery-1.12.4.js");
            //jQueryScript.SetAttribute("src", "https://code.jquery.com/jquery-1.12.4.min.js");
            jQueryScript.InnerHtml = jQueryScript.InnerHtml + Environment.NewLine;
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
            mainGinasScript.InnerHtml = FileUtils.GetJavaScript().Replace("$IMGFORMAT$", imageFormat); 
            headElement.AppendChild(mainGinasScript);

            HtmlElement shimScript = document.CreateElement("script");
            shimScript.SetAttribute("type", "text/javascript");
            shimScript.InnerHtml = "if (!Array.prototype.getItem) { Array.prototype.getItem = function (i) { return this[i]; }; };var cresults = { 'getItem': function (v) { return this[v]; }, 'popItem': function (v) { var ret = this[v]; delete this[v]; return ret; } }; "; 
               // Object.prototype.gGet = function (k) { return this[k]; }; Object.prototype.gKeys = function () { return _.keys(this); }; 
            //shimScript.InnerHtml = "if (!Array.prototype.getItem) { Array.prototype.getItem = function (i) { return this[i]; }; };Object.prototype.gGet = function (k) { return this[k]; }; Object.prototype.gKeys = function () { return _.keys(this); }; var cresults = { 'getItem': function (v) { return this[v]; }, 'popItem': function (v) { var ret = this[v]; delete this[v]; return ret; } }; window['console'] = {log: function (r){var currValue = document.getElementById('console').value; document.getElementById('console').value = currValue + '\\r\\n' +r;}}";
            headElement.AppendChild(shimScript);

            HtmlElement styleElement = document.CreateElement("style");
            styleElement.SetAttribute("type", "text/css");
            styleElement.InnerHtml = FileUtils.GetCss();
            headElement.AppendChild(styleElement);
            log.Debug("completed " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public static HtmlElement GetFirstHead(HtmlDocument document)
        {
            foreach (HtmlElement elem in document.All)
            {
                if (elem.TagName.Equals("head", StringComparison.CurrentCultureIgnoreCase))
                {
                    return elem;
                }
            }
            return null;
        }

    }
}
