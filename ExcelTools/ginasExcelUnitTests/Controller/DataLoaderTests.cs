using Microsoft.VisualStudio.TestTools.UnitTesting;
using gov.ncats.ginas.excel.tools.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Model.Callbacks;
using ginasExcelUnitTests.Model;

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

        [TestMethod()]
        public void CreateUpdateCallbackTest()
        {
            Workbook testWorkbook = getExcelSheet();
            Worksheet testWorksheet = testWorkbook.Sheets[2];
            Range testRow = testWorksheet.Range["A4"].EntireRow;
            DataLoader loader = new DataLoader();
            ScriptExecutorMock scriptExecutor = new ScriptExecutorMock();
            loader.SetScriptExecutor(scriptExecutor);
            UpdateCallback callback = loader.CreateUpdateCallback(testRow);
            Assert.IsNotNull(callback.getKey());
            Console.WriteLine("callback.getkey(): " + callback.getKey());
        }


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
            string sheetFilePath = @"C:\ginas_source\Excel\CSharpTest2\Test_Files\manual data load test.xlsx";
            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            return workbook;
        }

    }
}