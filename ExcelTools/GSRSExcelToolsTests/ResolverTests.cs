using ginasExcelUnitTests.Model;
using GSRSExcelTools;
using GSRSExcelTools.Controller;
using GSRSExcelTools.Model;
using GSRSExcelTools.Model.Callbacks;
using GSRSExcelTools.UI;
using GSRSExcelTools.Utils;
using GSRSExcelToolsTests.utils;
using Microsoft.Office.Interop.Excel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ginasExcelUnitTests
{

    [TestClass]
    public class ResolverTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static TestRetrievalForm? retrievalForm = null;
        static MySQLDBUtils dBQueryUtils = new MySQLDBUtils();
        private static Dictionary<string, string[]> resolverResults = new Dictionary<string, string[]>();
        private const int MILLISECONDS_DELAY = 6000;
        
        private static void StartForm()
        {
            log.Debug("Starting in StartForm");
            retrievalForm = new TestRetrievalForm();
            retrievalForm.ResultsHandler = HandleResults;
            retrievalForm.CurrentOperationType = OperationType.Resolution;
            retrievalForm.Size = new System.Drawing.Size(5, 5);
            retrievalForm.Visible = false;

            retrievalForm.Disposed += (s, e) =>
            {
                // Log or breakpoint here
                log.Debug("retrievalForm disposed!");
            };

            //this enables event processing within the form
            System.Windows.Forms.Application.Run(retrievalForm);
            Thread.Sleep(2000);
        }

        public static object HandleResults(string resultsKey, string message)
        {
            log.Debug(string.Format("HandleResults received message {0} for key {1}",
                message, resultsKey));

            Dictionary<string, string> results = new Dictionary<string, string>();

            if( message.Contains("\"valid\"") && message.Contains("\"message\""))
            {
                GinasResult result = JSTools.GetGinasResultFromString(message);
                string[] textResults = { result.message };
                resolverResults.Add(resultsKey, textResults);
                return resolverResults;
            }
            Dictionary<string, string[]> returnedValue = JSTools.getDictionaryFromString(message);
            
            foreach (string key in returnedValue.Keys)
            {
               log.DebugFormat("Handling result for key {0}", key);
                string keyResult = "OK";
                try
                {
                    string[] messageParts = returnedValue[key][0].Split('\t');
                    results.Add(key, keyResult);
                    if(resolverResults.ContainsKey(key))
                    {
                        resolverResults.Remove(key);
                    }
                    resolverResults.Add(key, messageParts);

                    //System.Windows.Forms.Application.DoEvents();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error handling key {0} {1} {2}", key, ex.Message, ex);
                    results.Add(key, "Exception: " + ex.Message);
                }
            }
            return results;
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
            log.Debug("in ClassCleanup; skipping null");
            //retrievalForm.Close();
            //retrievalForm = null;
        }

        public static GinasToolsConfiguration CurrentConfiguration
        {
            get;
            set;
        }

        [TestMethod]
        public void testBatchCallbackExecute()
        {
            BatchCallback testBatchCallback = setupData();
            ggetMock itemForCallback = new ggetMock();
            testBatchCallback.Execute(itemForCallback);
            Assert.IsTrue(true);//we made it here!
        }

        [TestMethod]
        public void test_RandomIdentifier()
        {
            string newIdString = JSTools.RandomIdentifier();
            Console.WriteLine("newId: " + newIdString);
            Assert.IsNotNull(newIdString);
            int expectedLength = 10 + 5;
            Assert.AreEqual(expectedLength, newIdString.Length);
        }

        [TestMethod]
        public void RandomIdentifierMutlipleTest()
        {
            string prevIdString = JSTools.RandomIdentifier();
            for (int i = 0; i < 10000; i++)
            {
                string newIdString = JSTools.RandomIdentifier();
                Assert.AreNotEqual(prevIdString, newIdString, string.Format("Expect different values on iteration {0}",i));
                prevIdString = newIdString;
            }
        }

        [TestMethod]
        public void getTempFile_test()
        {
            ImageOps imageOps = new ImageOps();
            string tempFileName = imageOps.GetTempFile("txt");
            Console.WriteLine("tempFileName: " + tempFileName);
            Assert.IsTrue(tempFileName.EndsWith("txt"));
        }

        
        [TestMethod]
        public void MakeSearch_Test()
        {
            string chemicalName1 = "BENZYL ALCOHOL";
            string chemicalName2 = "PHENYLEPHRINE";
            string chemicalName3 = "NICLOSAMIDE";
            string[] testInput = { chemicalName1, chemicalName2, chemicalName3 };
            string expectedResult = string.Format("['{0}','{1}','{2}']",
                chemicalName1, chemicalName2, chemicalName3);
            string actualResult = JSTools.MakeSearchString(testInput);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void MakeSearchWithChar_Test()
        {
            string chemicalName1 = "BUTYROPHENONE, 4-(3-AZASPIRO(5.6)DODEC-3-YL)-4'-FLUORO-";
            string[] testInput = { chemicalName1};
            string expectedResult = string.Format("['{0}']",
                chemicalName1.Replace("'", "\\'") );
            string actualResult = JSTools.MakeSearchString(testInput);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void getDictionaryFromStringTest()
        {
            string input = "{\"aspirin\":[\"aspirin\\thttps://tripod.nih.gov/ginas/app/img/a05ec20c-8fe2-4e02-ba7f-df69e5e30248.png?size=300\"]}";
            string expectedKey = "aspirin";
            string expectedValue = "aspirin\thttps://tripod.nih.gov/ginas/app/img/a05ec20c-8fe2-4e02-ba7f-df69e5e30248.png?size=300";

            Dictionary<string, string[]> actualValue = JSTools.getDictionaryFromString(input);
            string firstKey = actualValue.Keys.First();
            Assert.AreEqual(expectedKey, firstKey);
            string[] firstValues = actualValue[firstKey];
            Assert.AreEqual(expectedValue, firstValues[0]);
        }

        [TestMethod]
        public void TestVocabDeserialization()
        {
            string vocabFilePath = @"..\..\..\Test_Files\ref type vocab.json";
            vocabFilePath =  Path.GetFullPath(vocabFilePath);
            string rawVocabContent = File.ReadAllText(vocabFilePath);
            Vocab referenceTypeVocab = JSTools.GetVocabFromString(rawVocabContent);
            Assert.IsTrue(referenceTypeVocab.Content[0].Terms.Length > 10);
        }

        [TestMethod]
        public void IsImageUrlTest()
        {
            string url1 = "https://tripod.nih.gov/ginas/app/img/a10ca419-e677-4cae-bd40-5150a9eeeabe.png?size=300";
            Assert.IsTrue(ImageOps.IsImageUrl(url1));
            string url2 = "https://tripod.nih.gov/ginas/app/substances?q=%22diclofenac%22";
            Assert.IsFalse(ImageOps.IsImageUrl(url2));
            string url3 = "https://tripod.nih.gov/dev/ginas/app/img/6721ef98-4e53-4500-82d7-31e4dbd8243c.png?size=150&context=bbhxaknghv";
            Assert.IsTrue(ImageOps.IsImageUrl(url3));

        }

        [TestMethod]
        public void GetScriptParameterTest()
        {
            string serializedInput = "{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"pt\",\"name\":\"PT\",\"description\":\"Preferred Term of the new substance\",\"required\":true}";

            ScriptParameter output = JSTools.GetScriptParameterFromString(serializedInput);
            Assert.AreEqual("argument", output._type);
            Assert.AreEqual("pt", output.key);
            Assert.AreEqual("PT", output.name);
            Assert.AreEqual("Preferred Term of the new substance", output.description);
            Assert.IsNull(output.defaultValue);
        }


        [TestMethod]
        public void RemoteFileExistsTestFalse()
        {
            string url1 = "http://localhost:9000/ginas/app/img/3982bff1-da0a-49a5-be34-4adb8c7648afblah.png?size=300";
            Assert.IsFalse(ImageOps.RemoteFileExists(url1));
        }

        [TestMethod]
        public void LaunchLastScriptTest()
        {
            Retriever retriever = new Retriever();

            ScriptExecutorMock scriptExecutorMock = new ScriptExecutorMock();
            retriever.SetScriptExecutor(scriptExecutorMock);
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            retriever.SetStatusUpdater( statusUpdater);
            string dummyScript = "test 'value' for unit test";
            retriever.GetScriptQueue().Enqueue(dummyScript);
            retriever.LaunchFirstScript();
            Assert.AreEqual(dummyScript, scriptExecutorMock.TestScript);
            Assert.AreEqual(0, retriever.GetScriptQueue().Count);
        }

        [TestMethod]
        public void QueueOneBatchTest()
        {
            Retriever retriever = new Retriever();
            ScriptExecutorMock scriptExecutorMock = new ScriptExecutorMock();
            retriever.SetScriptExecutor(scriptExecutorMock);
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            retriever.SetStatusUpdater(statusUpdater);
            string methodName = "QueueOneBatch";
            int totalScriptsBefore = retriever.GetScriptQueue().Count;

            MethodInfo methodInfo = retriever.GetType().GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Callback callback = new Callback();
            callback.SetKey("unique key");
            List<string> searchValues = new List<string>(new string[] { "aspirin", "ibuprofen", "naproxen" });
            object[] parms = new object[2];
            parms[0] = callback;
            parms[1] = searchValues;

            methodInfo.Invoke(retriever, parms);
            int totalScriptsAfter = retriever.GetScriptQueue().Count;
            Assert.AreEqual(1, (totalScriptsAfter - totalScriptsBefore));
        }

        [TestMethod]
        public void MakeImageSearchTest()
        {
            Retriever retriever = new Retriever();
            ScriptExecutorMock scriptExecutorMock = new ScriptExecutorMock();
            retriever.SetScriptExecutor(scriptExecutorMock);
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            retriever.SetStatusUpdater(statusUpdater);
            string methodName = "MakeImageSearch";
            MethodInfo methodInfo = retriever.GetType().GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            string key = "Unique Search Key";
            List<string> searchNames = new List<string>(new string[] {"benzene", "water", "iodine"});
            object[] parms = new object[2];
            parms[0] = key;
            parms[1] = searchNames;

            string imageSearch = (string) methodInfo.Invoke(retriever, parms);
            Assert.IsTrue(imageSearch.Contains(".fetchers(['Image URL'])"));
        }

        [TestMethod]
        public void DecremementTotalScriptTest()
        {
            Retriever retriever = new Retriever();
            ScriptExecutorMock scriptExecutorMock = new ScriptExecutorMock();
            retriever.SetScriptExecutor(scriptExecutorMock);
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            retriever.SetStatusUpdater(statusUpdater);
            string methodName = "DecremementTotalScripts";
            MethodInfo methodInfo = retriever.GetType().GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            string fieldName = "_totalScripts";
            FieldInfo fieldInfo = retriever.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(retriever, 5);

            methodInfo.Invoke(retriever, new object[0]);
            int scriptTotalAfter = (int) fieldInfo.GetValue(retriever);
            Assert.AreEqual(4, scriptTotalAfter);
        }

        [TestMethod]
        public void DecremementTotalScriptTest2()
        {
            Retriever retriever = new Retriever();
            ScriptExecutorMock scriptExecutorMock = new ScriptExecutorMock();
            retriever.SetScriptExecutor(scriptExecutorMock);
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            retriever.SetStatusUpdater(statusUpdater);
            string methodName = "DecremementTotalScripts";
            MethodInfo methodInfo = retriever.GetType().GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            string fieldName = "_totalScripts";
            FieldInfo fieldInfo = retriever.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(retriever, 0);

            methodInfo.Invoke(retriever, new object[0]);
            int scriptTotalAfter = (int)fieldInfo.GetValue(retriever);
            Assert.AreEqual(0, scriptTotalAfter);
        }

        [TestMethod]
        public void CheckAllCallbacksTest()
        {
            Retriever retriever = new Retriever();
            ScriptExecutorMock scriptExecutorMock = new ScriptExecutorMock();
            retriever.SetScriptExecutor(scriptExecutorMock);
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            retriever.SetStatusUpdater(statusUpdater);

            //callbacks is null -- expect immediate exit
            retriever.CheckAllCallbacks(null, null);
            Assert.IsNotNull(retriever);// what else to test? we're confirming that the method runs without Exception
        }

        [TestMethod]
        public void CheckAllCallbacksTest2()
        {
            Retriever retriever = new Retriever();
            ScriptExecutorMock scriptExecutorMock = new ScriptExecutorMock();
            retriever.SetScriptExecutor(scriptExecutorMock);
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            retriever.SetStatusUpdater(statusUpdater);

            int secondsToMilliseconds = 1000;
            string fieldName = "_timer";
            FieldInfo fieldInfo = retriever.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(retriever, new System.Timers.Timer(40 * secondsToMilliseconds));
            retriever.CheckAllCallbacks(null, null);
            Assert.IsNull(fieldInfo.GetValue(retriever));
        }


        [TestMethod]
        public void LaunchCheckJobTest()
        {
            Retriever retriever = new Retriever();
            ScriptExecutorMock scriptExecutorMock = new ScriptExecutorMock();
            retriever.SetScriptExecutor(scriptExecutorMock);
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            retriever.SetStatusUpdater(statusUpdater);
            string fieldName = "_timer";
            FieldInfo fieldInfo = retriever.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            retriever.LaunchCheckJob();
            Assert.IsNotNull(fieldInfo.GetValue(retriever));
        }

        private BatchCallback setupData()
        {

            BatchCallback batchCallback = new BatchCallback(new List<Callback>());
            
            Callback cb1 = new Callback();
            cb1.SetKey("a");
            batchCallback.AddCallback(cb1);
            Callback cb2 = new Callback();
            cb2.SetKey("B");
            batchCallback.AddCallback(cb2);
            return batchCallback;
        }

        [TestMethod]
        public async Task SmilesFetcherTestAsync()
        {
            CheckForm();
            string nameForTest = "UREA STIBAMINE";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers= new List<string>();
            resolvers.Add("SMILES");

            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();

            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            log.DebugFormat("primaryScript: {0}", primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(MILLISECONDS_DELAY);
            List<StructureProxy> expected = dBQueryUtils.GetStructureForName(nameForTest);
            
            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(nameForTest));
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expected[0].SMILES));
        }

        [TestMethod]
        public async Task InChiKeyFetcherTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string nameForTest = "UREA STIBAMINE";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("InChIKey");

            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();

            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(700);
            string expectedInChIKey = "STIGEALBGPUGBV-UHFFFAOYSA-M";

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.AreEqual(expectedInChIKey, results[results.Length-1]);
        }

        [TestMethod]
        public async Task SmilesEtcFetcherTestAsync()
        {
            CheckForm();
            string nameForTest = "UREA STIBAMINE";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("SMILES");
            resolvers.Add("Molecular Weight");
            resolvers.Add("Molecular Formula");

            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            log.DebugFormat("primaryScript: {0}", primaryScript);
            scripts.Enqueue(primaryScript);

            while (scripts.Count > 0)
            {
                string result1 =await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(MILLISECONDS_DELAY);
            List<StructureProxy> expected = dBQueryUtils.GetStructureForName(nameForTest);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(nameForTest));
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expected[0].SMILES));
            Assert.IsTrue(results.Contains(expected[0].MolFormula));

            bool foundMw = false;
            double cutoff = 0.001;
            foreach(string val in results)
            {
                double mw;
                if(double.TryParse(val, out mw))
                {
                    if (Math.Abs(mw - expected[0].MWt) < cutoff) foundMw = true;
                    break;
                }
            }
            Assert.IsTrue(foundMw);
        }

        [TestMethod]
        public async Task MolfileTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string bdnumForTest = "Z35KEZ2BDU";
            List<string> chemNames = new List<string>();
            chemNames.Add(bdnumForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("Molfile");
            string expectedEnding = "M  END";

            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();

            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(3000);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(bdnumForTest));
            string[] results = resolverResults[bdnumForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Any(r=> r.EndsWith(expectedEnding)));
        }

         [TestMethod]
        [Ignore]
        public async Task ImageFromMolfileTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string idForTest = "Z35KEZ2BDU";
            List<string> chemNames = new List<string>();
            chemNames.Add(idForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("Molfile_Image");
            string expectedStart = "data:image/png;base64";

            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();

            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(3000);

            string debugInfo = (string)await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(idForTest));
            string[] results = resolverResults[idForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Any(r => r.StartsWith(expectedStart)));
        }


        [TestMethod]
        public async Task NonChemicalMolfileTest()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            string idForTest = "UFG6KMN996";
            chemNames.Add(idForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("Molfile+");

            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();

            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(4000);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(idForTest));
            string[] results = resolverResults[idForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results[1].EndsWith("M  END"));
        }

        [TestMethod]
        public async Task AltDefMolfileTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            string uniiForTest = "XE2CRB6S1Q";
            chemNames.Add(uniiForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("Molfile+");
            string expectedEnding = "M  END";

            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();

            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(5000);
            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            string[] results = resolverResults[uniiForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Any(r => r.EndsWith(expectedEnding)));
        }



        [TestMethod]
        public async Task CasFetcherTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string nameForTest = "TUBULYSIN B";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("CAS Numbers");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(1000);
            List<CodeProxy> expected = dBQueryUtils.GetCodesOfSystemForName(nameForTest, "CAS");

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(nameForTest));
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expected[0].Code));
        }

        [TestMethod]
        public async Task EVMPDFetcherTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string nameForTest = "CARICA PAPAYA LEAF";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("EVMPD Code");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(1000);
            List<CodeProxy> expected = dBQueryUtils.GetCodesOfSystemForName(nameForTest, "EVMPD");

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(nameForTest));
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expected[0].Code));
        }

        [TestMethod]
        public async Task EvpdFetcherTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string nameForTest = "FERRIC FERROCYANIDE";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("EVMPD Code");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(3000);
            List<CodeProxy> expected = dBQueryUtils.GetCodesOfSystemForName(nameForTest, "EVMPD");

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(nameForTest));
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expected[0].Code));
        }

        [TestMethod]
        public async Task AtcFetcherTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string nameForTest = "LYPRESSIN";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("ATC Code");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(4000);
            List<CodeProxy> expected = dBQueryUtils.GetCodesOfSystemForName(nameForTest, "WHO-ATC");

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(nameForTest));
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expected[0].Code));
        }

        [TestMethod]
        public async Task ActiveMoietyPTFetcherTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string nameForTest = "TERLIPRESSIN";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("Active Moiety PT");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2000);
            List<RelatedSubstanceProxy> expected = dBQueryUtils.GetRelatedSubstancesForName(nameForTest, "ACTIVE MOIETY");

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(nameForTest));
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expected[0].RefPName));
        }

        [TestMethod]
        public async Task ActiveMoietyIdFetcherTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            Console.WriteLine("Running Active Moiety ID Fetcher Test server: "+
                CurrentConfiguration.SelectedServer.ServerName);

            string nameForTest = "TERLIPRESSIN";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            List<string> resolvers = new List<string>();
            resolvers.Add("Active Moiety ID");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string debugInfo0 = await retrievalForm.ExecuteScript("cresults");
            log.DebugFormat("debugInfo0: {0}", debugInfo0);
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            log.DebugFormat("primaryScript: {0}", primaryScript);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2);
            List<RelatedSubstanceProxy> expected = dBQueryUtils.GetRelatedSubstancesForName(nameForTest, "ACTIVE MOIETY");

            string debugInfo = await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            log.Debug("JavaScript test debug: ");
            log.Debug(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(nameForTest));
            string[] results = resolverResults[nameForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expected[0].ApprovalId));
        }

        [TestMethod]
        public async Task ProteinSequenceFetcherTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string nameForTest = "CRENEZUMAB";
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            chemNames.Add("DAVUNETIDE");
            List<string> resolvers = new List<string>();
            resolvers.Add("Protein Sequence");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(5000);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Assert.IsTrue(resolverResults.ContainsKey(name));
                string sequence = dBQueryUtils.GetProteinSequence(name);
                string[] results = resolverResults[name];
                Assert.IsTrue(results.Contains(sequence));
            }
            
        }

        [TestMethod]
        public async Task SubstanceClassTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            string nameForTest = "TAMTUVETMAB";//protein
            List<string> chemNames = new List<string>();
            chemNames.Add(nameForTest);
            chemNames.Add("2,6-DICHLOROPHENOL");
            chemNames.Add("CETRIMIDE"); //mixture
            chemNames.Add("GARLIC");//structurally diverse
            chemNames.Add("DRISAPERSEN"); //nucleic acid
            chemNames.Add("BIXALOMER");//polymer
            //chemNames.Add("PT dede0e43-cc15-49fd-9148-f6df9a79f9f5"); //concept
            chemNames.Add("1208319-26-9"); //a chemical identified by CAS number
            List<string> resolvers = new List<string>();
            resolvers.Add("Substance Class");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(MILLISECONDS_DELAY);
            
            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach(string name in chemNames)
            {
                Console.WriteLine("Procesing results for substance '{0}'", name);
                string[] results = resolverResults[name];
                SubstanceProxy substance = dBQueryUtils.GetSubstance(name);
                Assert.IsTrue(results.Contains(substance.Type));
            }
        }

        [TestMethod]
        public async Task CreatedByLastEditedByUniiTestAsync()
        {
            string javaScriptDateFormat = "ddd MMM dd yyyy HH:mm:ss zzz";
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            //chemNames.Add(nameForTest);
            chemNames.Add("BUCLIZINE HYDROCHLORIDE");
            chemNames.Add("CETRIMIDE"); //mixture
            chemNames.Add("LYCOPODIUM CLAVATUM SPORE");//structurally diverse
            chemNames.Add("DRISAPERSEN"); //nucleic acid
            chemNames.Add("BIXALOMER");//polymer
            List<string> resolvers = new List<string>();
            resolvers.Add("Substance Class");
            resolvers.Add("Created By");
            resolvers.Add("Last Edited By");
            resolvers.Add("Approval ID (UNII)");
            resolvers.Add("Created Date");
            resolvers.Add("Last Edited Date");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*MILLISECONDS_DELAY);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Console.WriteLine("Procesing results for substance '{0}'", name);
                string[] results = resolverResults[name];
                SubstanceProxy substance = dBQueryUtils.GetSubstance(name);
                Assert.IsTrue(results.Contains(substance.Type));
                Assert.AreEqual(substance.CreatedBy, results[2]);
                Assert.AreEqual(substance.LastModifiedBy, results[3]);
                Assert.AreEqual(substance.ApprovalIdDisplay, results[4]);
                Console.WriteLine("Created: {0} {1}", substance.Created, results[5]);
                DateTime created;
                DateTime lastEdited;
                string dateToParse = CleanDate( results[5]);
                if ( DateTime.TryParseExact(dateToParse, javaScriptDateFormat, CultureInfo.CurrentCulture, 
                    DateTimeStyles.None,
                    out created))
                {
                    log.DebugFormat("Created for {2}, date parsed from JavaScript: {0}; from database {1}", 
                        created.Year, substance.Created.Year, name);
                    Assert.AreEqual(substance.Created.Year, created.Year);
                    Assert.AreEqual(substance.Created.DayOfYear, created.DayOfYear);
                    Assert.AreEqual(substance.Created.Hour, created.Hour);
                    Assert.AreEqual(substance.Created.Minute, created.Minute);
                    Assert.AreEqual(substance.Created.Second, created.Second);
                    //ignore sub-second units
                }
                else
                {
                    Console.WriteLine("Date did not parse!");
                }
                dateToParse = CleanDate(results[6]);
                if (DateTime.TryParseExact(dateToParse, javaScriptDateFormat, CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out lastEdited))
                {
                    Assert.AreEqual(substance.LastModified.Year, lastEdited.Year);
                    Assert.AreEqual(substance.LastModified.DayOfYear, lastEdited.DayOfYear);
                    Assert.AreEqual(substance.LastModified.Hour, lastEdited.Hour);
                    Assert.AreEqual(substance.LastModified.Minute, lastEdited.Minute);
                    Assert.AreEqual(substance.LastModified.Second, lastEdited.Second);
                    //ignore sub-second units
                }
                else
                {
                    Console.WriteLine("Date did not parse!");
                }
            }
        }

        [TestMethod]
        public async Task SubstanceNamesTest()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            chemNames.Add("TAMTUVETMAB");//protein
            chemNames.Add("ARFOLITIXORIN");
            chemNames.Add("BURIXAFOR");
            chemNames.Add("CETRIMIDE"); //mixture
            chemNames.Add("MANGO SEED OIL");//structurally diverse
            chemNames.Add("DRISAPERSEN"); //nucleic acid
            chemNames.Add("BIXALOMER");//polymer
            chemNames.Add("CEMIPLIMAB"); //concept
            List<string> resolvers = new();
            resolvers.Add("All Names");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(MILLISECONDS_DELAY);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Console.WriteLine("Processing name results for substance '{0}'", name);
                string[] results = resolverResults[name];
                List<string> allNamesFromFetcher = results[1].Split('|').ToList();
                List<SubstanceNamesProxy> substanceNamesFromDb = dBQueryUtils.GetNamesForName(name);
                foreach( SubstanceNamesProxy oneNameFromDb in substanceNamesFromDb)
                {
                    Assert.IsTrue( allNamesFromFetcher.Contains(oneNameFromDb.Name));
                }
            }
        }

        [TestMethod]
        public async Task BracketTermsTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            chemNames.Add("BALSTILIMAB");
            chemNames.Add("LEVILIMAB");
            chemNames.Add("DIROLEUTON");
            chemNames.Add("EDICOTINIB");
            List<string> resolvers = new List<string>();
            resolvers.Add("Bracket Terms");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            int it = 0;
            int maxIteration = 30;
            while(resolverResults.Count < chemNames.Count && ++it<maxIteration)
            {
                Console.WriteLine("iteration {0} resolverResults.Count: {1}", it, resolverResults.Count);
                Thread.Sleep(1000);
            }

            Thread.Sleep(1000);
            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Console.WriteLine("Procesing name results for substance '{0}'", name);
                string[] results = resolverResults[name];
                List<string> allNamesFromFetcher = results[1].Split('|').ToList();
                List<SubstanceNamesProxy> substanceNamesFromDb = dBQueryUtils.GetNamesForName(name);
                foreach (SubstanceNamesProxy oneNameFromDb in substanceNamesFromDb.Where(n=>n.IsBracketTerm()))
                {
                    Assert.IsTrue(allNamesFromFetcher.Contains(oneNameFromDb.Name));
                }
            }
        }

        [TestMethod]
        public async Task LatinBinomialTest()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            chemNames.Add("LYSIMACHIA QUADRIFOLIA WHOLE");//structurally diverse
            List<string> resolvers = new List<string>();
            resolvers.Add("Latin Binomial");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(1000);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Console.WriteLine("Procesing name results for substance '{0}'", name);
                string[] results = resolverResults[name];
                List<StructurallyDiverseProxy> structurallyDiverseProxies
                    = dBQueryUtils.GetStructurallDivers(name);
                Assert.AreEqual(structurallyDiverseProxies[0].LatinBinomial, results[1]);
                Console.WriteLine("Matched {0}", results[1]);
            }
        }
        [TestMethod]
        public async Task PlantPartTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            chemNames.Add("ORYZA SATIVA WHOLE");//structurally diverse
            List<string> resolvers = new List<string>();
            resolvers.Add("Part");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(MILLISECONDS_DELAY);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Console.WriteLine("Procesing name results for substance '{0}'", name);
                string[] results = resolverResults[name];
                List<StructurallyDiverseProxy> structurallyDiverseProxies 
                    = dBQueryUtils.GetStructurallDivers(name);
                Assert.AreEqual(structurallyDiverseProxies[0].Part, results[1]);
                Console.WriteLine("Matched {0}", results[1]);
            }
        }

        [TestMethod]
        public async Task AuthorTest()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            chemNames.Add("FASCIOLA HEPATICA");//structurally diverse
            List<string> resolvers = new List<string>();
            resolvers.Add("Author");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(1000);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Console.WriteLine("Procesing name results for substance '{0}'", name);
                string[] results = resolverResults[name];
                List<StructurallyDiverseProxy> structurallyDiverseProxies
                    = dBQueryUtils.GetStructurallDivers(name);
                Assert.AreEqual(structurallyDiverseProxies[0].Author, results[1]);
                Console.WriteLine("Matched {0}", results[1]);
            }
        }

        [TestMethod]
        public async Task StereoTypeTestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            chemNames.Add("BALOXAVIR");
            chemNames.Add("NAFCILLIN SODIUM ANHYDROUS");

            List<string> resolvers = new List<string>();
            resolvers.Add("Stereo Type");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(MILLISECONDS_DELAY);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Console.WriteLine("Procesing name results for substance '{0}'", name);
                string[] results = resolverResults[name];
                List<StructureProxy> structures = dBQueryUtils.GetStructureForName(name);
                Assert.AreEqual(structures[0].StereoDescription, results[1]);
            }
        }

        [TestMethod]
        public async Task MultipleFetchers1TestAsync()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            List<string> chemNames = new List<string>();
            chemNames.Add("TERLIPRESSIN");//protein
            chemNames.Add("ETHYL-1-(((2,4-DICHLOROPHENYL)ACETYL)OXY)CYCLOHEXANE-CARBOXYLATE");
            chemNames.Add("2,6-DICHLOROPHENOXY-.ALPHA.-PROPIONIC ACID");
            chemNames.Add("CETRIMIDE"); //mixture
            chemNames.Add("LYSIMACHIA QUADRIFOLIA WHOLE");//structurally diverse
            chemNames.Add("DEMATIRSEN"); //nucleic acid
            chemNames.Add("BIXALOMER");//polymer
            chemNames.Add("DIMETHICONOL/TRIMETHYLSILOXYSILICATE CROSSPOLYMER (35/65 W/W; 10000000 PA.S)"); //concept
            List<string> resolvers = new List<string>();
            resolvers.Add("Substance Class");
            resolvers.Add("Preferred Term");
            resolvers.Add("Molecular Formula");
            resolvers.Add("Protein Sequence");
            resolvers.Add("Latin Binomial");
            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();
            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            scripts.Enqueue(primaryScript);
            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*MILLISECONDS_DELAY);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            foreach (string name in chemNames)
            {
                Console.WriteLine("Procesing name results for substance '{0}'", name);
                string[] results = resolverResults[name];
                switch (results[1]) // substance class
                {
                    case "chemical":
                        List<StructureProxy> structures = dBQueryUtils.GetStructureForName(name);
                        Assert.AreEqual(structures[0].MolFormula, results[3]);
                        Console.WriteLine("MolFormula match: {0} and {1}",
                            structures[0].MolFormula, results[3]);
                        break;
                    case "protein":
                        string sequence = dBQueryUtils.GetProteinSequence(name);
                        Assert.AreEqual(sequence, results[4]);
                        Console.WriteLine("Sequence match: {0} and {1}",
                            sequence, results[4]);
                        break;
                    case "structurallyDiverse":
                        List<StructurallyDiverseProxy> structurallyDiverse = dBQueryUtils.GetStructurallDivers(name);
                        Assert.AreEqual(structurallyDiverse[0].LatinBinomial, results[5]);
                        Console.WriteLine("Latin Binomial match: {0} and {1}",
                            structurallyDiverse[0].LatinBinomial, results[5]);
                        break;
                    default:
                        Console.WriteLine("No fetcher/test yet for type {0}", results[1]);
                        break;
                }
            }
        }

        [TestMethod]
        public async Task ResolveMultipleMatchTest()
        {
            if (!CheckForm())
            {
                Assert.Fail("Retrieval form is not initialized.");
                return;
            }
            
            string codeForTest = "03";
            string expectedResult = "matched multiple records";
            List<string> chemNames = new List<string>
            {
                codeForTest
            };
            List<string> resolvers = new List<string>
            {
                "SMILES"
            };

            Queue<string> scripts = new Queue<string>();
            string callbackKey = JSTools.RandomIdentifier();

            string primaryScript = MakeSearch(callbackKey, chemNames, resolvers);
            Console.WriteLine("script used for search: " + primaryScript);
            scripts.Enqueue(primaryScript);

            while (scripts.Count > 0)
            {
                await retrievalForm.ExecuteScript(scripts.Dequeue());
            }
            //allow the scripts to complete execution:
            Thread.Sleep(2*MILLISECONDS_DELAY);

            string debugInfo = (string) await retrievalForm.ExecuteScript("GSRSAPI_consoleStack.join('|')");
            Console.WriteLine(debugInfo);
            Assert.IsTrue(resolverResults.ContainsKey(codeForTest));
            string[] results = resolverResults[codeForTest];
            results.ToList().ForEach(r => Console.WriteLine(r));
            Assert.IsTrue(results.Contains(expectedResult));
        }


        [TestMethod]
        public void testCreateApiUrl()
        {
            string baseUrl = "http://compound-reg.ncats.io:8080/ginas/app/";
            string apiUrl = "app/api";
            string expected = "http://compound-reg.ncats.io:8080/ginas/app/api";

            object[] callParms = new object[2];
            callParms[0] = baseUrl;
            callParms[1] = apiUrl;
            string actualUrl = RetrievalForm.CreateApiUrl(baseUrl, apiUrl);
            Assert.AreEqual(expected, actualUrl);
        }

        internal static string MakeSearch(string key, List<string> names, List<string> fetcherNames)
        {
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append("cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("']={'keys':function(){return _.keys(this);},'Item':function(k){return this[k];},");
            scriptBuilder.Append("'add':function(k,v){if(!this[k]){this[k]=[];}this[k].push(v);}};");
            scriptBuilder.Append("ResolveWorker.builder()");
            string arrayedNames = JSTools.MakeSearchString(names.ToArray());
            scriptBuilder.Append(".list(");
            scriptBuilder.Append(arrayedNames);
            scriptBuilder.Append(")");
            //_.map($('div.checkop input:checked'), 'name')
            string arrayedFetchers = JSTools.MakeSearchString(fetcherNames.ToArray());
            scriptBuilder.Append(".fetchers(");
            scriptBuilder.Append(arrayedFetchers);
            scriptBuilder.Append(")");
            scriptBuilder.Append(".consumer(function(row){cresults['");
            scriptBuilder.Append(key);
            scriptBuilder.Append("'].add(row.split('\t')[0],row);})");
            scriptBuilder.Append(".finisher(function(){sendMessageBackToCSharp('");
            scriptBuilder.Append(key);
            scriptBuilder.Append("');})");
            scriptBuilder.Append(".resolve();");
            return scriptBuilder.ToString();
        }

        private bool CheckForm()
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
                return false;
            }
            return true;
        }

        private string CleanDate(string inputDate)
        {
            string dateToClean = inputDate.Replace("GMT", "");
            int pos = dateToClean.LastIndexOf("(");
            if(pos> 0)
            {
                dateToClean = dateToClean.Substring(0, pos - 1);
            }
            return dateToClean;
        }
        public void StartOperation()
        {
            throw new NotImplementedException();
        }

        public bool StartResolution(bool newSheet)
        {
            throw new NotImplementedException();
        }

        public void SetExcelWindow(Window window)
        {
            throw new NotImplementedException();
        }

        public void SetScriptExecutor(IScriptExecutor scriptExecutor)
        {
            throw new NotImplementedException();
        }

        public void ContinueSetup()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ReceiveVocabulary(string rawVocab)
        {
            throw new NotImplementedException();
        }

        public void CancelOperation(string reason)
        {
            throw new NotImplementedException();
        }
    }
}
