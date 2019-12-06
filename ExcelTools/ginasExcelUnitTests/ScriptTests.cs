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
        static private int SCRIPT_INTERVAL = 7000;

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
            string uuidForTest = "70df30e7-00a3-4e38-842e-7574d04674e4";// "5a85db6c-2736-42cc-8c25-5efcae0a7e62";
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
            string ptForTest = "PT c77f1ff0-eee1-4726-88b8-864111547c6a";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesForName(ptForTest);

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
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesForName(ptForTest);
            Assert.AreEqual(codesBefore.Count + 1, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) 
            && c.Code.Equals(newCode)));
        }

        [TestMethod]
        public void AddCodeByBdNumTest()
        {
            CheckForm();

            ScriptUtils scriptUtils = new ScriptUtils();
            string bdNumForTest = "0002186AB";
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
            string uuidForTest = "5a85db6c-2736-42cc-8c25-5efcae0a7e62"; //GUIDs are site-specific. Select one that contains a garbage substance, created for test only
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
            int sleepInterval = 1000;
            Thread.Sleep(sleepInterval);
            int maxTests = 10;
            int currTest = 1;
            bool foundCode = false;
            while( !foundCode && (currTest<maxTests))
            {
                List<Tuple<string, string>> codesAfter = dBQueryUtils.GetCodesForUuid(uuidForTest);
                foundCode =( codesBefore.Count + 1 == codesAfter.Count && 
                    codesAfter.Any(c => c.Item1.Equals(newCodeSystem) && c.Item2.Equals(newCode)));
                if( !foundCode) Thread.Sleep(sleepInterval);
                Console.WriteLine("After {0} intervals, foundCode: {1}", currTest, foundCode);
                currTest++;
            }

            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(foundCode);

        }

        [TestMethod]
        public void RemoveNameTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string uuidForTest = "70df30e7-00a3-4e38-842e-7574d04674e4";
            string bdnum = "0001765AB";
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
            scripts.Enqueue(string.Format("tmpRunner.setValue('bdnum', '{0}')", bdnum));
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
        public void ReplaceCodeTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "ANDROSTERONE SULFATE";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesForName(ptForTest);
            string uuidCrossRef = "88813ffe-ff1f-4a47-870b-26635fa101ef";

            string newRef = "Ref " + Guid.NewGuid();
            scriptUtils.ScriptName = "Replace Code";
            string newCode = ("Q8C" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            string oldCode = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Code;
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;
            string commentGuid = Guid.NewGuid().ToString();
            string commentValue = "made-up value to test software " + commentGuid.Split('-')[0];
            string textValue = "made-up value to test software " + commentGuid.Split('-')[1];

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidCrossRef));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", newCode));
            scripts.Enqueue(string.Format("tmpRunner.setValue('comments', '{0}')", textValue));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code text', '{0}')", commentValue));
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
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesForName(ptForTest);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) 
                && c.Code.Equals(newCode)));
            CodeProxy matchingCode = codesAfter.First(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(newCode));
            Assert.AreEqual(textValue, matchingCode.CodeText);
            Assert.AreEqual(commentValue, matchingCode.Comments);
        }


        [TestMethod]
        public void ReplaceCodeTextBlockedTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "ANDROSTERONE SULFATE";
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
            Assert.IsFalse(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                && c.Comments.Equals(newComment) && c.Url.Equals(url)));
        }

        [TestMethod]
        public void ReplaceCodeTextWorksTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "ANDROSTERONE SULFATE";
            string uuid = "88813ffe-ff1f-4a47-870b-26635fa101ef";
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
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuid));
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
        public void FixCodeUrlsWorksTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "PT 88deffa2-593a-48fd-9d8f-9d999ce6ee49";
            string uuid = "c889b7f1-284e-4cd4-b241-fa7353bdbdea";
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
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuid));
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
            int sleepInterval = 1000;
            Thread.Sleep(sleepInterval);
            int maxTests = 10;
            int currTest = 1;
            bool foundCode = false;
            while (!foundCode && (currTest < maxTests))
            {
                List<CodeProxy> codesAfter = dBQueryUtils.GetCodesEtcForName(ptForTest);
                string expectedUrl = urlBase + oldCode;
                foundCode =( codesBefore.Count == codesAfter.Count) && codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                    && c.Url.Equals(expectedUrl));
                
                if (!foundCode) Thread.Sleep(sleepInterval);
                Console.WriteLine("After {0} intervals, foundCode: {1}", currTest, foundCode);
                currTest++;
            }
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(foundCode);
        }

        [TestMethod]
        public void FixCodeUrlsBlockedTest()
        {
            //expect the change to be blocked by the script because there's only one look-up factor
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "PT 88deffa2-593a-48fd-9d8f-9d999ce6ee49";
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
            int sleepInterval = 1000;
            Thread.Sleep(sleepInterval);
            int maxTests = 10;
            int currTest = 1;
            bool foundCode = false;
            while (!foundCode && (currTest < maxTests))
            {
                List<CodeProxy> codesAfter = dBQueryUtils.GetCodesEtcForName(ptForTest);
                string expectedUrl = urlBase + oldCode;
                foundCode = (codesBefore.Count == codesAfter.Count) && codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                    && c.Url.Equals(expectedUrl));

                if (!foundCode) Thread.Sleep(sleepInterval);
                Console.WriteLine("After {0} intervals, foundCode: {1}", currTest, foundCode);
                currTest++;
            }
            string debugInfo = (string)retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsFalse(foundCode);
        }


        [TestMethod]
        public void ReplaceCodeTextNoRefBlockedTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "ANDROSTERONE SULFATE";
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
            Assert.IsFalse(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                && c.Comments.Equals(newComment) && c.Url.Equals(url)));
        }

        [TestMethod]
        public void ReplaceCodeTextNoRefWorksTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "ANDROSTERONE SULFATE";
            string uuid = "88813ffe-ff1f-4a47-870b-26635fa101ef";
            string bdnum = "0005779AB";
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
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuid));
            scripts.Enqueue(string.Format("tmpRunner.setValue('bdnum', '{0}')", bdnum));
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
        public void ReplaceCodeTypeWorksTest()
        {
            CheckForm();
            ScriptUtils scriptUtils = new ScriptUtils();
            string ptForTest = "ANDROSTERONE SULFATE";
            string uuid = "88813ffe-ff1f-4a47-870b-26635fa101ef";
            string bdnum = "0005779AB";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            scriptUtils.ScriptName = "Replace Code Type";
            string codeSystem = "UNIPROT";
            CodeProxy firstCode = codesBefore.First(c => c.CodeSystem.Equals(codeSystem));
            string oldCode = firstCode.Code;
            string codeComment = firstCode.Comments;
            string url = firstCode.Url;
            string newComment = codeComment + " A";
            string newType = firstCode.Type.Equals("PRIMARY", StringComparison.InvariantCultureIgnoreCase)
                ? "ALTERNATIVE" : "PRIMARY";
            retrievalForm.CurrentOperationType = gov.ncats.ginas.excel.tools.OperationType.Loading;

            scriptUtils.ScriptExecutor = retrievalForm;
            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptUtils.ScriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuid));
            scripts.Enqueue(string.Format("tmpRunner.setValue('bdnum', '{0}')", bdnum));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", oldCode));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code type', '{0}')", newType));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')",
                codeSystem));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Code type modification to test script')");
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
                codeSystem, oldCode, url, newComment);
            Console.WriteLine(msg);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(codeSystem) && c.Code.Equals(oldCode)
                && c.Type.Equals(newType) ));
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
            string uuidForTest = "e81e84bf-5b3f-46d8-a505-3254aa5d67e9";
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
