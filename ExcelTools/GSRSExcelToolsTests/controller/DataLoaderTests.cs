using Microsoft.VisualStudio.TestTools.UnitTesting;
using gov.ncats.ginas.excel.tools.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using ginasExcelUnitTests.Model;

using System.IO;
using GSRSExcelTools.Controller;
using GSRSExcelTools.Model.Callbacks;
using GSRSExcelTools.Utils;
using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Controller.Tests
{
    [TestClass()]
    public class DataLoaderTests
    {
        Application excel;
        [TestInitialize]
        public void SetUp()
        {
            excel = new Application();
            Console.WriteLine("Started Excel");
        }
        [TestCleanup]
        public void Cleanup()
        {
            excel.Workbooks.Close();
            excel.Quit();
            Console.WriteLine("Closed Excel");
        }

        [TestMethod]
        public void HandleResultsTest()
        {
            //HandleResults on DataLoader
            DataLoader dataLoader = new DataLoader();
            string filePath = @"..\..\..\Test_Files\Registration test.xlsx";
            filePath = Path.GetFullPath(filePath);

            Workbook workbook = ReadExcelWorkbook(filePath);
            Worksheet sheet = (Worksheet)workbook.Sheets[1];
            StatusUpdaterMock statusUpdater = new StatusUpdaterMock();
            dataLoader.SetStatusUpdater(statusUpdater);
            Range statusRange = sheet.Range["M2"];
            string testKey = "unique key";
            UpdateCallback updateCallback = new UpdateCallback(statusRange);
            FieldInfo callbackInfo = dataLoader.GetType().GetField("Callbacks", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<string, Callback> callbacks = new Dictionary<string, Callback>();
            callbacks.Add(testKey, updateCallback);
            callbackInfo.SetValue(dataLoader, callbacks);
            string resultsMessage = "{valid: true, message: \"done!\"}";
            dataLoader.HandleResults(testKey, resultsMessage);
            string contents = (string)statusRange.FormulaR1C1;
            Assert.AreEqual("done!", contents);
            workbook.Close(false);
        }

        //[TestMethod()]
        //public void CreateUpdateCallbackTest()
        //{
        //    Workbook testWorkbook = getExcelSheet();
        //    Worksheet testWorksheet = testWorkbook.Sheets[2];
        //    Range testRow = testWorksheet.Range["A4"].EntireRow;
        //    DataLoader loader = new DataLoader();
        //    ScriptExecutorMock scriptExecutor = new ScriptExecutorMock();
        //    loader.SetScriptExecutor(scriptExecutor);
        //    UpdateCallback callback = loader.CreateUpdateCallbackForDisplay(testRow);
        //    Assert.IsNotNull(callback.getKey());
        //    Console.WriteLine("callback.getkey(): " + callback.getKey());
        //}


        // The method below gives System.ArgumentException: Object of type 'System.Collections.Generic.Dictionary`2[System.String,Microsoft.Office.Interop.Excel.Range]' cannot be converted to type 'System.Collections.Generic.Dictionary`2[System.String,Microsoft.Office.Interop.Excel.Range]'.
        //[TestMethod]
        //public void GetPropertyTest()
        //{
        //    string sheetFilePath = @"C:\ginas_source\Excel\CSharpTest2\Test_Files\RangeParseTest.xlsx";
        //    string[] values = { "aspirin", "cyclosporin", "CYANOCOBALAMIN" };
        //    List<string> expectedValues = values.ToList();

        //    Workbook workbook = excel.Workbooks.Open(sheetFilePath);
        //    Worksheet sheet = workbook.Worksheets[2];
        //    Range e2Range = sheet.Range["E2"];
        //    Range e3Range = sheet.Range["E3"];
        //    Range e4Range = sheet.Range["E4"];
        //    Dictionary<string, Excel.Range> nameToRange = new Dictionary<string, Excel.Range>
        //    {
        //        { "E2", e2Range },
        //        { "E3", e3Range },
        //        { "E4", e4Range }
        //    };

        //    DataLoader loader = new DataLoader();
        //    string methodToTest = "GetProperty";
        //    MethodInfo method = loader.GetType().GetMethod(methodToTest, BindingFlags.Instance | BindingFlags.NonPublic);
        //    object[] parameters = new object[3];
        //    parameters[0] = nameToRange;
        //    parameters[1] = "E3";
        //    parameters[2] = "hidden";
        //    string expected = "Range E3";
        //    string actual = (string)method.Invoke(loader, parameters);
        //    Assert.AreEqual(expected, actual);
        //}

        private Workbook getExcelSheet()
        {
            string sheetFilePath = @"..\..\..\Test_Files\manual data load test.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);
            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            return workbook;
        }

        internal Workbook ReadExcelWorkbook(string filePath)
        {
            return excel.Workbooks.Open(filePath);
        }

        [TestMethod()]
        public void ReceiveVocabularyTest()
        {
            string vocabFilePath = @"..\..\..\Test_Files\ref type vocab.json";
            vocabFilePath = Path.GetFullPath(vocabFilePath);
            string rawVocabContent = File.ReadAllText(vocabFilePath);
            rawVocabContent = "vocabulary:DOCUMENT_TYPE:" + rawVocabContent;
            DataLoader dataLoader = new DataLoader();
            FieldInfo scriptUtilsInfo = dataLoader.GetType().GetField("scriptUtils", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            ScriptUtils scriptUtils = new ScriptUtils();
            List<string> expectedVocabs = new List<string>();
            expectedVocabs.Add("DOCUMENT_TYPE");
            scriptUtils.ExpectedVocabularies = expectedVocabs;
            //PropertyInfo expectedVocabInfo = scriptUtils.GetType().GetProperty("ExpectedVocabularies", 
            //    BindingFlags.NonPublic | BindingFlags.Instance);
            //expectedVocabInfo.SetValue(scriptUtils, expectedVocabs);
            scriptUtilsInfo.SetValue(dataLoader, scriptUtils);
            dataLoader.ReceiveVocabulary(rawVocabContent);
            Assert.AreEqual(0, expectedVocabs.Count);
        }
    }
}