using ginasExcelUnitTests.Model;
using ginasExcelUnitTests.Utils;
using GSRSExcelTools;
using GSRSExcelTools.Model;
using GSRSExcelTools.Utils;
using GSRSExcelToolsTests.utils;
using System.Reflection;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class ScriptTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static TestRetrievalForm? retrievalForm = null;
        static readonly MySQLDBUtils dBQueryUtils = new MySQLDBUtils();
        static private int SCRIPT_INTERVAL = 12000;

        private static Dictionary<string, string[]> scriptResults = new Dictionary<string, string[]>();

        public static GinasToolsConfiguration? CurrentConfiguration
        {
            get;
            set;
        }

        public static object HandleResults(string resultsKey, string message)
        {
            log.Debug(string.Format("HandleResults received message {0} for key {1}",
                message, resultsKey));

            Dictionary<string, string> results = new Dictionary<string, string>();

            if (message.Contains("\"valid\"") && message.Contains("\"message\""))
            {
                GinasResult result = JSTools.GetGinasResultFromString(message);
                string[] textResults = { result.message };
                scriptResults.Add(resultsKey, textResults);
                return scriptResults;
            }
            Dictionary<string, string[]> returnedValue = JSTools.getDictionaryFromString(message);

            SheetUtils sheetUtils = new SheetUtils();
            sheetUtils.Configuration = CurrentConfiguration;
            foreach (string key in returnedValue.Keys)
            {
                log.DebugFormat("Handling result for key {0}", key);
                string keyResult = "OK";
                try
                {
                    string[] messageParts = returnedValue[key][0].Split('\t');
                    results.Add(key, keyResult);
                    if (scriptResults.ContainsKey(key))
                    {
                        scriptResults.Remove(key);
                    }
                    scriptResults.Add(key, messageParts);

                    System.Windows.Forms.Application.DoEvents();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error handling key {0} {1} {2}", key, ex.Message, ex);
                    results.Add(key, "Exception: " + ex.Message);
                }
            }
            return results;
        }

        private static void StartForm()
        {
            log.Debug("Starting in StartForm");
            retrievalForm = new TestRetrievalForm();
            retrievalForm.Size = new System.Drawing.Size(5, 5);
            retrievalForm.Visible = false;
            retrievalForm.ResultsHandler = HandleResults;
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
        public async Task AddNameTest()
        {
            CheckForm();

            string uuidForTest = "70df30e7-00a3-4e38-842e-7574d04674e4";// "5a85db6c-2736-42cc-8c25-5efcae0a7e62";
            List<string> namesBefore = dBQueryUtils.GetNamesForUuid(uuidForTest);

            string newName = "Name " + Guid.NewGuid();
            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Add Name";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            //"BATCH:Add Name", "UUID", "PT", "BDNUM", "NAME", "NAME TYPE", "LANGUAGE", "PD", "REFERENCE TYPE", "REFERENCE CITATION", "REFERENCE URL", "CHANGE REASON", "FORCED", "IMPORT STATUS
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('name', '{0}')", newName));
            scripts.Enqueue("tmpRunner.setValue('name type', 'cn')");
            scripts.Enqueue("tmpRunner.setValue('language', 'en')");
            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'New name added via script')");
            string identifier = JSTools.RandomIdentifier();
            string script = "tmpRunner.execute().get(function(b){cresults['" + identifier
                + "']=b;sendMessageBackToCSharp('" + identifier + "');})";
            Console.WriteLine("identifier: {0}; script: {1}", identifier, script);
            scripts.Enqueue(script);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            Thread.Sleep(13*SCRIPT_INTERVAL);
            string hostName =  await retrievalForm.ExecuteScript("window.location.hostname");
            Console.WriteLine("hostname: " + hostName);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            log.DebugFormat("debugInfo: {0}", debugInfo);
            List<string> namesAfter = dBQueryUtils.GetNamesForUuid(uuidForTest);
            Assert.AreEqual(namesBefore.Count + 1, namesAfter.Count);
            Assert.IsTrue(namesAfter.Contains(newName));
        }

        [TestMethod]
        public async Task AddNameStandardizeTest()
        {
            CheckForm();

            string uuidForTest = "70df30e7-00a3-4e38-842e-7574d04674e4";// "5a85db6c-2736-42cc-8c25-5efcae0a7e62";
            List<string> namesBefore = dBQueryUtils.GetNamesForUuid(uuidForTest);

            string epsilon = "ε";
            string guidValue = Guid.NewGuid().ToString();
            string newName = epsilon + "Name " + guidValue;
            string expectedName = ".EPSILON." + ("Name " + guidValue).ToUpper();
            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Add Name";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('name', '{0}')", newName));
            scripts.Enqueue("tmpRunner.setValue('name type', 'cn')");
            scripts.Enqueue("tmpRunner.setValue('language', 'en')");
            scripts.Enqueue("tmpRunner.setValue('standardize', 'true')");
            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'New name added via script')");
            string identifier = JSTools.RandomIdentifier();
            string script = "tmpRunner.execute().get(function(b){ cresults['" + identifier+ "'] = b; sendMessageBackToCSharp('"
                + identifier +"'); })";
            Console.WriteLine("identifier: {0}; script: {1}", identifier, script);
            scripts.Enqueue(script);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            Thread.Sleep(SCRIPT_INTERVAL*12);
            string hostName = await retrievalForm.ExecuteScript("window.location.hostname");
            Console.WriteLine("hostname: " + hostName);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<string> namesAfter = dBQueryUtils.GetNamesForUuid(uuidForTest);
            Assert.AreEqual(namesBefore.Count + 1, namesAfter.Count);
            Assert.IsTrue(namesAfter.Contains(expectedName));
        }

        [TestMethod]
        public async Task AddNamesAndRemoveNamesTest()
        {
            CheckForm();

            string uuidForTest = "8b867ef5-c19c-4be2-8373-754b419eff1f";
            string ptForTest = "PT B07C8B28-1184-49F5-BCEB-6FA5CDD73EBC";
            List<string> namesBefore = dBQueryUtils.GetNamesForUuid(uuidForTest);

            List<string> namesAdded = new List<string>();
            int maxName = 5;
            for (int iName = 0; iName < maxName; iName++)
            {
                string newName = "Name " + Guid.NewGuid();
                namesAdded.Add(newName);
                string newRef = "Ref " + Guid.NewGuid();
                string scriptName = "Add Name";
                retrievalForm.CurrentOperationType = OperationType.Loading;
                string callbackId = JSTools.RandomIdentifier();

                Queue<string> scripts = new Queue<string>();
                scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
                scripts.Enqueue("tmpRunner=tmpScript.runner();");
                scripts.Enqueue("tmpRunner.clearValues();");
                scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
                scripts.Enqueue(string.Format("tmpRunner.setValue('name', '{0}')", newName));
                scripts.Enqueue("tmpRunner.setValue('name type', 'cn')");
                scripts.Enqueue("tmpRunner.setValue('language', 'en')");
                scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
                scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
                scripts.Enqueue(string.Format("tmpRunner.setValue('change reason', 'Adding name '{0} via script')",
                    newName));
                //concatenate because the additional curly braces in the base string make it impossible to use string format
                scripts.Enqueue( "tmpRunner.execute().get(function(b){cresults['" + callbackId 
                    +"']=b;sendMessageBackToCSharp('" + callbackId + "');})");

                while (scripts.Count > 0)
                {
                    await retrievalForm.ExecuteScript(scripts.Dequeue());
                }
                Thread.Sleep(SCRIPT_INTERVAL*3);
            }
            Thread.Sleep(SCRIPT_INTERVAL*3);
            string debugInfo0 = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('~')");
            log.DebugFormat("debugInfo after adds: {0}", debugInfo0);
            List<string> namesAfter = dBQueryUtils.GetNamesForUuid(uuidForTest);
            Assert.IsTrue(namesAdded.All(n=> namesAfter.Contains(n)),"missing at least one name");
            for(int iName = namesAdded.Count-1; iName>=0; iName--)
            {
                string nameToRemove = namesAdded[iName];
                string scriptName = "Remove Name";
                retrievalForm.CurrentOperationType = OperationType.Loading;

                Queue<string> scripts = new Queue<string>();
                scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
                scripts.Enqueue("tmpRunner=tmpScript.runner();");
                scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
                scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
                scripts.Enqueue(string.Format("tmpRunner.setValue('name', '{0}')", nameToRemove));
                scripts.Enqueue("tmpRunner.setValue('change reason', 'removing name " + nameToRemove + "')");
                string callbackKey = JSTools.RandomIdentifier();

                string script = "tmpRunner.execute().get(function(b){cresults['" + callbackKey
                    + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})";
                Console.WriteLine("Key: " + callbackKey);
                scripts.Enqueue(script);

                while (scripts.Count > 0)
                {
                    await retrievalForm.ExecuteScript(scripts.Dequeue());
                }
                Thread.Sleep(SCRIPT_INTERVAL*2);

                string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
                log.DebugFormat("debugInfo in round {0}: {1}", iName, debugInfo);

                List<string> namesCurrent = dBQueryUtils.GetNamesForUuid(uuidForTest);
                for (int i =0; i< iName; i++)
                {
                    Assert.IsTrue(namesCurrent.Contains(namesAdded[i]));
                    Console.WriteLine("Verified presence of name " + namesAdded[i]);
                }
            }
        }

        [TestMethod]
        public async Task AddCodeTest()
        {
            CheckForm();

            string ptForTest = "PT CEF0D453-D1CE-49F7-A19D-35E6993F6298";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Add Code";
            string newCode = ("Q8C" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
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
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(4*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesForName(ptForTest);
            Assert.AreEqual(codesBefore.Count + 1, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem)
            && c.Code.Equals(newCode)));
        }

        [TestMethod]
        public async Task AddCodeByPT()
        {
            CheckForm();

            string lookupCodeForTest = "CHEMBL265174";
            string ptForLookup = "(4-allyl-2-methoxy-phenyl) adamantane-1-carboxylate";

            List<Tuple<string, string>> codesBefore = new List<Tuple<string, string>>();
            try
            {
                codesBefore = dBQueryUtils.GetCodesForName(ptForLookup)
                    .Select(c=>new Tuple<string, string>(c.Code, c.CodeSystem)).ToList();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to retrieve codes for code {0} in CHEMBL: {1}", lookupCodeForTest, ex.Message);
                Assert.Fail("Failed to retrieve codes for code {0} in CHEMBL: {1}", lookupCodeForTest, ex.Message);
            }
            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Add Code";
            string newCode = ("Q" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForLookup));
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
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL*2);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<Tuple<string, string>> codesAfter = dBQueryUtils.GetCodesForName(ptForLookup)
                .Select(c => new Tuple<string, string>(c.CodeSystem, c.Code)).ToList();
            Console.WriteLine("Codes after");
            foreach (Tuple<string, string> code in codesAfter)
            {
                Console.WriteLine("Code system: {0}; code: {1}", code.Item2, code.Item1);
            }
            Assert.AreEqual(codesBefore.Count + 1, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.Item1.Equals(newCodeSystem) && c.Item2.Equals(newCode)));
        }


        [TestMethod]
        public async Task AddCodeByUuidTest()
        {
            CheckForm();

            string uuidForTest = "0e30cc4b-c151-481b-976a-5f1283883fd6"; //GUIDs are site-specific. Select one that contains a garbage substance, created for test only
            List<Tuple<string, string>> codesBefore = dBQueryUtils.GetCodesForUuid(uuidForTest);

            string newRef = "Ref " + Guid.NewGuid();
            string newCode = ("Q8C" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", "Add Code"));
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
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            int sleepInterval = 1000;
            Thread.Sleep(sleepInterval);
            int maxTests = 10;
            int currTest = 1;
            bool foundCode = false;
            while (!foundCode && (currTest < maxTests))
            {
                List<Tuple<string, string>> codesAfter = dBQueryUtils.GetCodesForUuid(uuidForTest);
                foundCode = (codesBefore.Count + 1 == codesAfter.Count &&
                    codesAfter.Any(c => c.Item1.Equals(newCodeSystem) && c.Item2.Equals(newCode)));
                if (!foundCode) Thread.Sleep(sleepInterval);
                Console.WriteLine("After {0} intervals, foundCode: {1}", currTest, foundCode);
                currTest++;
            }

            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(foundCode);
        }

        [TestMethod]
        public async Task RemoveNameTest()
        {
            CheckForm();
            string uuidForTest = "70df30e7-00a3-4e38-842e-7574d04674e4";
            string pt = "IMIFOPLATIN";
            List<string> namesBefore = dBQueryUtils.GetNamesForUuid(uuidForTest);

            string nameToRemove = namesBefore.FirstOrDefault(n => n.IsPossibleGuidName());
            if (string.IsNullOrEmpty(nameToRemove))
            {
                Assert.Fail("No name suitable to remove found! Run the AddName test first.");
            }
            log.DebugFormat("going to remove name {0}", nameToRemove);
            string scriptName = "Remove Name";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", pt));
            scripts.Enqueue(string.Format("tmpRunner.setValue('name', '{0}')", nameToRemove));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'removing name " + nameToRemove + "')");
            string callbackKey = JSTools.RandomIdentifier();

            string script = "tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})";
            Console.WriteLine("Key: " + callbackKey);
            scripts.Enqueue(script);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            Thread.Sleep(6*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<string> namesAfter = dBQueryUtils.GetNamesForUuid(uuidForTest);
            Assert.AreEqual(namesBefore.Count - 1, namesAfter.Count);
            Assert.IsFalse(namesAfter.Contains(nameToRemove));

            Assert.IsTrue(namesBefore.Where(n => !n.Equals(nameToRemove)).All(n => namesAfter.Any(n2 => n.Equals(n2))));
        }

        [TestMethod]
        public async Task ReplaceCodeTest()
        {
            CheckForm();
            string ptForTest = "ANDROSTERONE SULFATE";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesForName(ptForTest);
            Console.WriteLine("Codes before modification: ");
            foreach (CodeProxy code in codesBefore)
            {
                Console.WriteLine("Looking at code {0} {1}", code.CodeSystem, code.Code);
            }
            string uuidCrossRef = "88813ffe-ff1f-4a47-870b-26635fa101ef";

            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Replace Code";
            string newCode = ("Q8C" + Guid.NewGuid()).Substring(0, 6).ToUpper();
            string newCodeSystem = "UNIPROT";
            string oldCode = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Code;
            retrievalForm.CurrentOperationType = OperationType.Loading;
            string commentGuid = Guid.NewGuid().ToString();
            string commentValue = "made-up value to test software " + commentGuid.Split('-')[0];
            string textValue = "made-up value to test software " + commentGuid.Split('-')[1];

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
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
            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'New code added via script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesForName(ptForTest);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem)
                && c.Code.Equals(newCode)));
            CodeProxy matchingCode = codesAfter.First(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(newCode));
            Assert.AreEqual(textValue, matchingCode.CodeText);
            Assert.AreEqual(commentValue, matchingCode.Comments);

            bool noMatch = false;
            /*foreach(CodeProxy code in  codesBefore.Where(c => !c.CodeSystem.Equals(newCodeSystem) ))
            {
                Console.Write("looking at code {0} {1}", code.CodeSystem, code.Code);
                if( codesAfter.Any(c=> c.CodeSystem.Equals(code.CodeSystem) && c.Code.Equals(code.Code)))
                {
                    Console.WriteLine(" has a match");
                }
                else
                {
                    Console.WriteLine(" has NO match");
                    noMatch = true;
                }
            }*/
            Assert.IsTrue(codesBefore.Where(c => !(c.CodeSystem.Equals(newCodeSystem))).All(c => codesAfter.Any(c2 => c.CodeSystem.Equals(c2.CodeSystem) && c.Code.Equals(c2.Code))));
            Assert.IsFalse(noMatch);
        }

        [TestMethod]
        public async Task ReplaceCodeMultiplexTest()
        {
            int maxRep = 10;
            CheckForm();
            string ptForTest = "PT E1B7B110-1721-4238-B003-694D95FE6241";
            for (int i = 0; i < maxRep; i++)
            {
                List<CodeProxy> codesBefore = dBQueryUtils.GetCodesForName(ptForTest);
                Console.WriteLine("Codes before modification (iteration {0}): ", i);
                foreach (CodeProxy code in codesBefore)
                {
                    Console.WriteLine("Looking at code {0} {1}", code.CodeSystem, code.Code);
                }
                string uuidCrossRef = "0e30cc4b-c151-481b-976a-5f1283883fd6";

                string newRef = "Ref " + Guid.NewGuid();
                string scriptName = "Replace Code";
                string newCode = ("Q8C" + Guid.NewGuid()).Substring(0, 6).ToUpper();
                string newCodeSystem = "UNIPROT";
                string oldCode = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Code;
                retrievalForm.CurrentOperationType = OperationType.Loading;
                string commentGuid = Guid.NewGuid().ToString();
                string commentValue = "made-up value to test software " + commentGuid.Split('-')[0];
                string textValue = "made-up value to test software " + commentGuid.Split('-')[1];

                Queue<string> scripts = new Queue<string>();
                scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
                scripts.Enqueue("tmpRunner=tmpScript.runner();");
                scripts.Enqueue("tmpRunner.clearValues();");
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
                scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
                scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
                scripts.Enqueue("tmpRunner.setValue('change reason', 'New code added via script')");
                string callbackKey = JSTools.RandomIdentifier();

                scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                    + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");
                
                while (scripts.Count > 0)
                {
                    string script = scripts.Dequeue();
                    string result = await retrievalForm.ExecuteScript(script);
                    Console.WriteLine("ran script '{0}' and got result '{1}'", script, result);
                }
                //allow the scripts to complete execution:
                Thread.Sleep(6*SCRIPT_INTERVAL);
                string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
                log.DebugFormat("debugInfo: {0}", debugInfo);
                Console.WriteLine(debugInfo);
                List<CodeProxy> codesAfter = dBQueryUtils.GetCodesForName(ptForTest);
                Assert.AreEqual(codesBefore.Count, codesAfter.Count);
                Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem)
                    && c.Code.Equals(newCode)));
                CodeProxy matchingCode = codesAfter.First(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(newCode));
                Assert.AreEqual(textValue, matchingCode.CodeText);
                Assert.AreEqual(commentValue, matchingCode.Comments);
                Assert.IsTrue(codesBefore.Where(c => !(c.CodeSystem.Equals(newCodeSystem))).All(c => codesAfter.Any(c2 => c.CodeSystem.Equals(c2.CodeSystem) && c.Code.Equals(c2.Code))));
            }
        }


        [TestMethod]
        public async Task ReplaceCodeTextBlockedTest()
        {
            CheckForm();
            string ptForTest = "ANDROSTERONE SULFATE";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Replace Code Text";
            string newCodeSystem = "UNIPROT";
            CodeProxy first = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem));
            string oldCode = first.Code;
            string codeComment = first.Comments;
            string url = first.Url;
            string newComment = codeComment + " A";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
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
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesEtcForName(ptForTest);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            Assert.IsFalse(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                && c.Comments.Equals(newComment) && c.Url.Equals(url)));
        }

        [TestMethod]
        public async Task ReplaceCodeTextWorksTest()
        {
            CheckForm();
            string ptForTest = "ANDROSTERONE SULFATE";
            string uuid = "88813ffe-ff1f-4a47-870b-26635fa101ef";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Replace Code Text";
            string newCodeSystem = "UNIPROT";
            CodeProxy first = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem));
            string oldCode = first.Code;
            string codeComment = first.Comments;
            string url = first.Url;
            string newComment = codeComment + " A";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
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
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesEtcForName(ptForTest);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(newCodeSystem) && c.Code.Equals(oldCode)
                && c.Comments.Equals(newComment) && c.Url.Equals(url)));
        }

        [TestMethod]
        public async Task FixCodeUrlsWorksTest()
        {
            CheckForm();
            string ptForTest = "PT B07C8B28-1184-49F5-BCEB-6FA5CDD73EBC";
            string uuid = "8b867ef5-c19c-4be2-8373-754b419eff1f";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Fix Code URLs";
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
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuid));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", oldCode));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')", newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('url base', '{0}')", urlBase));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Code modification to test script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
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
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(foundCode);
        }

        [TestMethod]
        public async Task FixCodeUrlsBlockedTest()
        {
            //expect the change to be blocked by the script because there's only one look-up factor
            CheckForm();
            string ptForTest = "PT E1B7B110-1721-4238-B003-694D95FE6241";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            string newRef = "Ref " + Guid.NewGuid();
            string scriptName = "Fix Code URLs";
            string newCodeSystem = "UNIPROT";
            string oldCode = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Code;
            string codeComment = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Comments;
            string url = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem)).Url;
            string urlBase = "https://fr.wikipedia.org/wiki/";
            if (url.StartsWith(urlBase))
            {
                urlBase = "https://en.wikipedia.org/wiki/";
            }
            string newComment = codeComment + " A";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", oldCode));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')", newCodeSystem));
            scripts.Enqueue(string.Format("tmpRunner.setValue('url base', '{0}')", urlBase));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Code modification to test script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
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
            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsFalse(foundCode);
        }


        [TestMethod]
        public async Task ReplaceCodeTextNoRefBlockedTest()
        {
            CheckForm();
            string ptForTest = "ANDROSTERONE SULFATE";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            string scriptName = "Replace Code Text";
            string newCodeSystem = "UNIPROT";
            CodeProxy firstCode = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem));
            string oldCode = firstCode.Code;
            string codeComment = firstCode.Comments;
            string url = firstCode.Url;
            string newComment = codeComment + " A";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
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
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
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
        public async Task ReplaceCodeTextNoRefWorksTest()
        {
            CheckForm();
            string ptForTest = "ANDROSTERONE SULFATE";
            string uuid = "88813ffe-ff1f-4a47-870b-26635fa101ef";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForName(ptForTest);

            string scriptName = "Replace Code Text";
            string newCodeSystem = "UNIPROT";
            CodeProxy firstCode = codesBefore.First(c => c.CodeSystem.Equals(newCodeSystem));
            string oldCode = firstCode.Code;
            string codeComment = firstCode.Comments;
            string url = firstCode.Url;
            string newComment = codeComment + " A";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
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
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Code modification to test script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
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
        public async Task ReplaceCodeTypeWorksTest()
        {
            CheckForm();
            string ptForTest = "ANDROSTERONE SULFATE";
            string uuid = "88813ffe-ff1f-4a47-870b-26635fa101ef";
            List<CodeProxy> codesBefore = dBQueryUtils.GetCodesEtcForUuid(uuid);
                //dBQueryUtils.GetCodesEtcForName(ptForTest);

            string scriptName = "Replace Code Type";
            string codeSystem = "UNIPROT";
            CodeProxy firstCode = codesBefore.First(c => c.CodeSystem.Equals(codeSystem));
            string oldCode = firstCode.Code;
            string codeComment = firstCode.Comments;
            string url = firstCode.Url;
            string newComment = codeComment + " A";
            string newType = firstCode.Type.Equals("PRIMARY", StringComparison.InvariantCultureIgnoreCase)
                ? "ALTERNATIVE" : "PRIMARY";
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            //scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuid));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code', '{0}')", oldCode));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code type', '{0}')", newType));
            scripts.Enqueue(string.Format("tmpRunner.setValue('code system', '{0}')",
                codeSystem));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Code type modification to test script')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<CodeProxy> codesAfter = dBQueryUtils.GetCodesEtcForUuid(uuid);
            Assert.AreEqual(codesBefore.Count, codesAfter.Count);
            string msg = string.Format("looking for code with system {0}, code {1}, url {2}, comment {3}",
                codeSystem, oldCode, url, newComment);
            Console.WriteLine(msg);
            Assert.IsTrue(codesAfter.Any(c => c.CodeSystem.Equals(codeSystem) && c.Code.Equals(oldCode)
                && c.Type.Equals(newType)));
        }

        [TestMethod]
        public async Task CreateSubstanceTest()
        {
            CheckForm();
            string ptForTest = "PT " + Guid.NewGuid().ToString();

            string scriptName = "Create Substance";
            string ptLanguage = "en";
            string ptNameType = "cn";
            string substanceClass = "concept";
            string newRef = "Ref " + Guid.NewGuid();

            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt language', '{0}')", ptLanguage));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt name type', '{0}')", ptNameType));
            scripts.Enqueue(string.Format("tmpRunner.setValue('substance class', '{0}')", substanceClass));
            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Substance created via unit test')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            string uuid = dBQueryUtils.GetUuidForPt(ptForTest);
            Assert.IsTrue(uuid.Length > 10);
            Console.WriteLine("Created substance with uuid: " + uuid + " and name: " + ptForTest);
        }

        [TestMethod]
        public async Task CreateChemicalSubstanceDuplicateTest()
        {
            CheckForm();
            string ptForTest = "Chem " + Guid.NewGuid().ToString();

            string scriptName = "Create Substance";
            string ptLanguage = "en";
            string ptNameType = "cn";
            string substanceClass = "chemical";
            string smiles = "CC1=CC(=O)C2=C(C=C[SiH]=C2)C1=O";
            string newRef = "Ref " + Guid.NewGuid();

            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", ptForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt language', '{0}')", ptLanguage));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt name type', '{0}')", ptNameType));
            scripts.Enqueue(string.Format("tmpRunner.setValue('substance class', '{0}')", substanceClass));
            scripts.Enqueue(string.Format("tmpRunner.setValue('smiles', '{0}')", smiles));
            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'Substance created via unit test')");
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            string uuid = dBQueryUtils.GetUuidForPt(ptForTest);
            Assert.IsTrue(string.IsNullOrEmpty(uuid));
            Assert.IsTrue(debugInfo.Contains("Structure has 1 or more duplicates"));
        }

        [TestMethod]
        public async Task TouchRecordTestAsync()
        {
            CheckForm();
            string scriptName = "Touch Record";
            string uuidForTest = "e81e84bf-5b3f-46d8-a505-3254aa5d67e9";
            string versionComment = "Record change " + Guid.NewGuid().ToString();
            int versionBefore = dBQueryUtils.GetVersionForUuid(uuidForTest);
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('change reason', '{0}')", versionComment));
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(3*SCRIPT_INTERVAL);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            int versionAfter = dBQueryUtils.GetVersionForUuid(uuidForTest);
            Assert.IsTrue(versionAfter > versionBefore);
        }


        [TestMethod]
        public async Task AddRelationshipTest()
        {
            CheckForm();
            string scriptName = "Add Relationship";
            string uuidForTest = "0e30cc4b-c151-481b-976a-5f1283883fd6";
            string pt1 = "PT E1B7B110-1721-4238-B003-694D95FE6241";

            string pt2 = "PT D41AB14E-D646-487F-9314-2311D280CCBB";
            string uuid2 = "2702b5d4-0aca-4497-a032-fc01d82fa6d2";
            string relationshipType = "METABOLITE->PARENT";
            string newRef = "Ref " + Guid.NewGuid();

            string versionComment = "Adding a relationship to " + pt1;
            int versionBefore = dBQueryUtils.GetVersionForUuid(uuidForTest);
            retrievalForm.CurrentOperationType = OperationType.Loading;

            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", pt1));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt2', '{0}')", pt2));
            scripts.Enqueue(string.Format("tmpRunner.setValue('relationship type', '{0}')", relationshipType));
            scripts.Enqueue("tmpRunner.setValue('reference type', 'OTHER')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('reference citation', '{0}')", newRef));
            scripts.Enqueue("tmpRunner.setValue('pd', 'true')");
            scripts.Enqueue(string.Format("tmpRunner.setValue('change reason', '{0}')", versionComment));
            string callbackKey = JSTools.RandomIdentifier();

            scripts.Enqueue("tmpRunner.execute().get(function(b){cresults['" + callbackKey
                + "']=b;sendMessageBackToCSharp('" + callbackKey + "');})");

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(SCRIPT_INTERVAL*4);
            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            List<RelationshipProxy> relationshipProxies = dBQueryUtils.GetRelationshipsForUuid(uuidForTest);
            Assert.IsTrue(relationshipProxies.Any(r => r.RelatedSubstanceUUID.Equals(uuid2) && r.RelationshipType.Equals(relationshipType)));
        }

        [TestMethod]
        public async Task SetObjectJsonTest()
        {
            CheckForm();
            string uuidForTest = "70df30e7-00a3-4e38-842e-7574d04674e4";
            List<string> namesBefore = dBQueryUtils.GetNamesForUuid(uuidForTest);

            string pt = "PT-112";
            string scriptName = "Set Object JSON";
            retrievalForm.CurrentOperationType = OperationType.Loading;
            string vocabFilePath = @"..\..\..\Test_Files\F5I3T42BXCTrunc.json";
            vocabFilePath = Path.GetFullPath(vocabFilePath);
            string truncatedJson = File.ReadAllText(vocabFilePath);
            truncatedJson = truncatedJson.Replace("'", "\\'");

            SheetUtils sheetUtils = new SheetUtils();
            Queue<string> scripts = new Queue<string>();
            scripts.Enqueue(string.Format("tmpScript=Scripts.get('{0}');", scriptName));
            scripts.Enqueue("tmpRunner=tmpScript.runner();");
            scripts.Enqueue("tmpRunner.clearValues();");
            scripts.Enqueue(string.Format("tmpRunner.setValue('uuid', '{0}')", uuidForTest));
            scripts.Enqueue(string.Format("tmpRunner.setValue('pt', '{0}')", pt));
            scripts.Enqueue(string.Format("tmpRunner.setValue('json', '{0}')", truncatedJson));
            scripts.Enqueue("tmpRunner.setValue('change reason', 'truncated JSON')");
            string identifier = JSTools.RandomIdentifier();
            string script = "tmpRunner.execute().get(function(b){cresults['" + identifier
                + "']=b;window.external.Notify('" + identifier + "');})";
            Console.WriteLine("identifier: {0}; script: {1}", identifier, script);
            scripts.Enqueue(script);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            Thread.Sleep(2000);

            string debugInfo = (string)await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            string[] results = scriptResults[identifier];
            results.ToList().ForEach(r => Console.WriteLine(r));
            string expectedResult = "Warning! The value of the JSON parameter is probably truncated.";
            Assert.IsTrue(results.Contains(expectedResult));
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
