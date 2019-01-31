using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Controller;
using gov.ncats.ginas.excel.tools.Providers;
using gov.ncats.ginas.excel.tools.Model.Callbacks;

using ginasExcelUnitTests.Model;
using ginasExcelUnitTests.Utils;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class ScriptTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static TestRetrievalForm retrievalForm = null;
        static DBQueryUtils dBQueryUtils = new DBQueryUtils();
        static bool scriptRunnerReady = false;
        static ScriptUtils scriptUtils = new ScriptUtils();
        static private int SCRIPT_INTERVAL = 4000;

        public static GinasToolsConfiguration CurrentConfiguration
        {
            get;
            set;
        }

        private static void StartForm()
        {
            log.Debug("Starting in StartForm");
            retrievalForm = new TestRetrievalForm();
            retrievalForm.Size = new System.Drawing.Size(5, 5);
            retrievalForm.Visible = false;

            System.Windows.Forms.Application.Run(retrievalForm);
        }

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {

            Thread formThread = new Thread(StartForm);
            formThread.SetApartmentState(ApartmentState.STA);
            formThread.Start();

            CurrentConfiguration = FileUtils.GetGinasConfiguration();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Console.WriteLine("Closed Excel");
            retrievalForm = null;
        }

        [TestMethod]
        public void AddNameTest()
        {
            CheckForm();

            ScriptUtils scriptUtils = new ScriptUtils();
            string uuidForTest = "66327985-8d29-4c36-a45d-cbe6305703de";
            List<string> namesBefore = dBQueryUtils.GetNamesForUuid(uuidForTest);

            string newName = "Name " + Guid.NewGuid();
            string newRef = "Ref " + Guid.NewGuid();
            scriptUtils.ScriptName = "Add Name";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            //"BATCH:Add Name", "UUID", "PT", "BDNUM", "NAME", "NAME TYPE", "LANGUAGE", "PD", "REFERENCE TYPE", "REFERENCE CITATION", "REFERENCE URL", "CHANGE REASON", "FORCED", "IMPORT STATUS
            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('name', '{0}')", newName));
            scripts.Enqueue("tmpRunner.setValue('name type', 'cn')");
            scripts.Enqueue("tmpRunner.setValue('language', 'en')");
            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'New name added via script')");
            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['gsrs_celfgqocjz']=b;window.external.Notify('gsrs_celfgqocjz');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            Thread.Sleep(SCRIPT_INTERVAL);
            string hostName = (string)retrievalForm.ExecuteScript("window.location.hostname");
            Console.WriteLine("hostname: " + hostName);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<string> namesAfter = dBQueryUtils.GetNamesForUuid(uuidForTest);
            Assert.AreEqual(namesBefore.Count + 1, namesAfter.Count);
            Assert.IsTrue(namesAfter.Contains(newName));
        }

        [TestMethod]
        public void AddCodeTest()
        {
            CheckForm();

            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "Cyclohexane";
            List<Tuple<string, string>> codesBefore = dBQueryUtils.GetCodesForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            scriptUtils.ScriptName = "Add Code";
            string newCode = ("Q8C" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", newCode));
            scripts.Enqueue("tmpRunner.setValue('comments', 'made-up value to test software')");
            scripts.Enqueue("tmpRunner.setValue('code type', 'PRIMARY')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')",
                newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code url', '{0}{1}')",
                "https://www.uniprot.org/uniprot/", newCode));
            scripts.Enqueue("tmpRunner.setValue('allow multiples', 'true')");

            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'New code added via script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<Tuple<string, string>> codesAfter = dBQueryUtils.GetCodesForName(ptForTest);
            Assert.AreEqual(codesBefore.Count + 1, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.Item1.Equals(newCodeSystem) && c.Item2.Equals(newCode)));
        }

        [TestMethod]
        public void AddCodeByBdNumTest()
        {
            CheckForm();

            ScriptUtils scriptUtils = new ScriptUtils();
            string bdNumForTest = "0001997AB";
            List<Tuple<string, string>> codesBefore = dBQueryUtils.GetCodesForBdNum(bdNumForTest);

            string newRef = "Ref " + Guid.NewGuid();
            scriptUtils.ScriptName = "Add Code";
            string newCode = ("Q" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('bdnum', '{0}')", bdNumForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", newCode));
            scripts.Enqueue("tmpRunner.setValue('code type', 'PRIMARY')");
            scripts.Enqueue("tmpRunner.setValue('comments', 'made-up value to test software')");
            scripts.Enqueue("tmpRunner.setValue('code text', 'made-up text to test software')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')",
                newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code url', '{0}{1}')",
                "https://www.uniprot.org/uniprot/", newCode));
            scripts.Enqueue("tmpRunner.setValue('allow multiples', 'true')");

            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'New code added via script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<Tuple<string, string>> codesAfter = dBQueryUtils.GetCodesForBdNum(bdNumForTest);
            Assert.AreEqual(codesBefore.Count + 1, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.Item1.Equals(newCodeSystem) && c.Item2.Equals(newCode)));
        }


        [TestMethod]
        public void AddCodeByUuidTest()
        {
            CheckForm();

            ScriptUtils scriptUtils = new ScriptUtils();
            string uuidForTest = "90cea970-fa49-4cee-a045-10c3b86d3147";
            List<Tuple<string, string>> codesBefore = dBQueryUtils.GetCodesForUuid(uuidForTest);

            string newRef = "Ref " + Guid.NewGuid();
            scriptUtils.ScriptName = "Add Code";
            string newCode = ("Q8C" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", newCode));
            scripts.Enqueue("tmpRunner.setValue('code type', 'PRIMARY')");
            scripts.Enqueue("tmpRunner.setValue('comments', 'made-up value to test software')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')",
                newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code url', '{0}{1}')",
                "https://www.uniprot.org/uniprot/", newCode));
            scripts.Enqueue("tmpRunner.setValue('allow multiples', 'true')");

            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'New code added via script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<Tuple<string, string>> codesAfter = dBQueryUtils.GetCodesForUuid(uuidForTest);
            Assert.AreEqual(codesBefore.Count + 1, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.Item1.Equals(newCodeSystem) && c.Item2.Equals(newCode)));
        }

        [TestMethod]
        public void RemoveNameTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string uuidForTest = "66327985-8d29-4c36-a45d-cbe6305703de";
            List<string> namesBefore = dBQueryUtils.GetNamesForUuid(uuidForTest);

            string nameToRemove = namesBefore.FirstOrDefault(n => n.IsPossibleGuidName());
            if (string.IsNullOrEmpty(nameToRemove))
            {
                Assert.Fail("No name suitable to remove found! Run the AddName test first.");
            }
            scriptUtils.ScriptName = "Remove Name";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('name', '{0}')", nameToRemove));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'removing name " + nameToRemove + "')");
            string callbackKey = JSTools.RandomIdentifier();

            string script = "tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})";
            Console.WriteLine("Key: " + callbackKey);
            scripts.Enqueue(script);

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<string> namesAfter = dBQueryUtils.GetNamesForUuid(uuidForTest);
            Assert.AreEqual(namesBefore.Count - 1, namesAfter.Count);
            Assert.IsFalse(namesAfter.Contains(nameToRemove));
        }

        [TestMethod]
        public void ReplaceCodeByNameTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "Cyclohexane";
            List<Tuple<string, string>> codesBefore = dBQueryUtils.GetCodesForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            scriptUtils.ScriptName = "Replace Code by Name";
            string newCode = ("Q8C" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            string oldCode = codesBefore.First(c => c.Item1.Equals(newCodeSystem)).Item2;
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", newCode));
            scripts.Enqueue("tmpRunner.setValue('comments', 'made-up value to test software')");
            scripts.Enqueue("tmpRunner.setValue('code type', 'PRIMARY')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')",
                newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code url', '{0}{1}')",
                "https://www.uniprot.org/uniprot/", newCode));
            scripts.Enqueue("tmpRunner.setValue('allow multiples', 'true')");

            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'New code added via script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<Tuple<string, string>> codesAfter = dBQueryUtils.GetCodesForName(ptForTest);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.Item1.Equals(newCodeSystem) && c.Item2.Equals(newCode)));
        }


        [TestMethod]
        public void ReplaceCodeTextTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "9,10-PHENANTHRENEDIONE";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            scriptUtils.ScriptName = "Replace Code Text";
            string newCodeSystem = "UNIPROT";
            CodeProxy first = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem));
            string oldCode = first.Code;
            string codeComment = first.Comments;
            string url = first.Url;
            string newComment = codeComment + " A";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", oldCode));
            scripts.Enqueue(string.Format("tmpRunner.setValue('comments', '{0}')", newComment));
            scripts.Enqueue("tmpRunner.setValue('code type', 'PRIMARY')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')",
                newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code url', '{0}{1}')",
                "https://www.uniprot.org/uniprot/", oldCode));

            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Code modification to test script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesEtcForName(ptForTest);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                && c.Comments.Equals(newComment) && c.Url.Equals(url)));
        }

        [TestMethod]
        public void FixCodeUrlsTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "UREA";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            scriptUtils.ScriptName = "Fix Code URLS";
            string newCodeSystem = "WIKIPEDIA";
            string oldCode = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Code;
            string codeComment = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Comments;
            string url = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Url;
            string urlBase = "https://fr.wikipedia.org/wiki/";
            if (url.StartsWith(urlBase))
            {
                urlBase = "https://en.wikipedia.org/wiki/";
            }
            string newComment = codeComment + " A";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", oldCode));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')", newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('url base', '{0}')", urlBase));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Code modification to test script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesEtcForName(ptForTest);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            string expectedUrl = urlBase + oldCode;
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                && c.Url.Equals(expectedUrl)));
        }


        [TestMethod]
        public void ReplaceCodeTextNoRefTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "9,10-PHENANTHRENEDIONE";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            scriptUtils.ScriptName = "Replace Code Text";
            string newCodeSystem = "UNIPROT";
            CodeProxy firstCode = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem));
            string oldCode = firstCode.Code;
            string codeComment = firstCode.Comments;
            string url = firstCode.Url;
            string newComment = codeComment + " A";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", oldCode));
            scripts.Enqueue(string.Format("tmpRunner.setValue('comments', '{0}')", newComment));
            scripts.Enqueue("tmpRunner.setValue('code type', 'PRIMARY')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')",
                newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code url', '{0}{1}')",
                "https://www.uniprot.org/uniprot/", oldCode));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Code modification to test script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesEtcForName(ptForTest);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            string msg = string.Format("looking for code with system {0}, code {1}, url {2}, comment {3}",
                newCodeSystem, oldCode, url, newComment);
            Console.WriteLine(msg);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                && c.Comments.Equals(newComment) && c.Url.Equals(url)));
        }

        [TestMethod]
        public void CreateSubstanceTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "PT " + Guid.NewGuid().ToString();

            scriptUtils.ScriptName = "Create Substance";
            string ptLanguage = "en";
            string ptNameType = "cn";
            string substanceClass = "concept";
            string newRef = "Ref " + Guid.NewGuid();

            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt language', '{0}')", ptLanguage));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt name type', '{0}')", ptNameType));
            scripts.Enqueue(string.Format("tmpRunner.setValue('substance class', '{0}')", substanceClass));
            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Substance created via unit test')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            string uuid = dBQueryUtils.GetUuidForPt(ptForTest);
            Assert.IsTrue(uuid.Length > 10);
        }

        [TestMethod]
        public void TouchRecordTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            scriptUtils.ScriptName = "Touch Record";
            string uuidForTest = "bdaf53c5-d531-413e-96d4-488817f33354";
            string versionComment = "Record change " + Guid.NewGuid().ToString();
            int versionBefore = dBQueryUtils.GetVersionForUuid(uuidForTest);
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('change reason', '{0}')", versionComment));
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;window.external.Notify('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            int versionAfter = dBQueryUtils.GetVersionForUuid(uuidForTest);
            Assert.IsTrue(versionAfter > versionBefore);
        }


        private void CheckForm()
        {
            int iter = 0;
            int maxIter = 40;

            while ((retrievalForm == null || !retrievalForm.IsReady)
                && iter < maxIter)
            {
                Thread.Sleep(1000);
                iter++;
                log.DebugFormat("init iteration {0}", iter);
            }
            log.DebugFormat("retrievalForm: {0}", retrievalForm);
            if (retrievalForm == null || !retrievalForm.IsReady)
            {
                Assert.Fail("Connection to server is not working");
            }
        }
    }
}
