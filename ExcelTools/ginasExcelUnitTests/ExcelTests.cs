using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Controller;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class ExcelTests
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
        public void ImageOps_hasComment_Test()
        {
            Workbook book = getExcelSheet();
            Worksheet sheet = book.Worksheets.Item[1];
            Range cellWithComment = sheet.Range["A1"];
            Range cellWithoutComment = sheet.Range["B1"];

            ImageOps imageOps = new ImageOps();
            Assert.IsTrue( imageOps.hascomment(cellWithComment));
            Assert.IsFalse(imageOps.hascomment(cellWithoutComment));
        }

        [TestMethod]
        public void downloadTest()
        {
            String imageUrl = "https://tripod.nih.gov/dev/ginas/app/img/6721ef98-4e53-4500-82d7-31e4dbd8243c.png?size=150&context=bbhxaknghv";
            String localPath = @"c:\temp\downloadedimage.png";
            if (File.Exists(localPath)) File.Delete(localPath);

            ImageOps imageOps = new ImageOps();
            imageOps.Download_File(imageUrl, localPath);
            Assert.IsTrue(File.Exists(localPath));
        }

        [TestMethod]
         public void AddImageTest()
        {
            //String imageUrl = "https://tripod.nih.gov/dev/ginas/app/img/6721ef98-4e53-4500-82d7-31e4dbd8243c.svg?size=150&context=bbhxaknghv";
            String localImagePath = @"c:\temp\downloadedimage.png";
            int imageSize = 200;
            Workbook book = getExcelSheet();
            Worksheet sheet = book.Worksheets.Item[1];
            Range cellForImage = sheet.Range["A5"];
            ImageOps imageOps = new ImageOps();
            
            string savedFilePath = @"C:\temp\image test.xlsx";
            imageOps.AddImageCaption(cellForImage, localImagePath, imageSize);

            if (File.Exists(savedFilePath)) File.Delete(savedFilePath);
            book.SaveAs(savedFilePath);
            Assert.IsTrue(File.Exists(savedFilePath));
            //now look at file to make sure image is there!
        }

        [TestMethod]
        public void GetColumnNameTest()
        {
            int columnNumber = 1;
            string expectedColumnLetter = "A";
            string actualColumnLetter = SheetUtils.GetColumnName(columnNumber);
            Assert.AreEqual(expectedColumnLetter, actualColumnLetter);

            columnNumber = 2;
            expectedColumnLetter = "B";
            actualColumnLetter = SheetUtils.GetColumnName(columnNumber);
            Assert.AreEqual(expectedColumnLetter, actualColumnLetter);

            columnNumber = 26;
            expectedColumnLetter = "Z";
            actualColumnLetter = SheetUtils.GetColumnName(columnNumber);
            Assert.AreEqual(expectedColumnLetter, actualColumnLetter);

            columnNumber = 28;
            expectedColumnLetter = "AB";
            actualColumnLetter = SheetUtils.GetColumnName(columnNumber);
            Assert.AreEqual(expectedColumnLetter, actualColumnLetter);
        }

        [TestMethod]
        public void GetSearchValuesTest()
        {
            string sheetFilePath = @"C:\ginas_source\Excel\CSharpTest2\Test_Files\RangeParseTest.xlsx";
            string[] values = { "aspirin", "cyclosporin", "CYANOCOBALAMIN" };
            List<string> expectedValues = values.ToList();
               
            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = workbook.Worksheets[1];
            Range selection = sheet.Range["A4", "A6"];
            Retriever retriever = new Retriever();
            string methodToTest = "GetSearchValues";
            MethodInfo method = retriever.GetType().GetMethod(methodToTest, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parms = new object[1];
            parms[0] = selection;
            List<SearchValue> valuesForSearch = (List<SearchValue>) method.Invoke(retriever, parms);
            for(int v = 0; v<expectedValues.Count; v++)
            {
                Assert.AreEqual(expectedValues[v], valuesForSearch[v].Value);
            }            
        }

        [TestMethod]
        public void FindRowTest()
        {
            string sheetFilePath = @"C:\ginas_source\Excel\CSharpTest2\Test_Files\search test file";
            string searchTarget = "Value to find";

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = workbook.Worksheets[1];
            Range searchRange = sheet.Range["A1", "L33"];
            int searchColumn = 2;
            int expectedRow = 11;
            int row = SheetUtils.FindRow(searchRange, searchTarget, searchColumn);
            Assert.AreEqual(expectedRow, row);

            searchTarget = "Another value";
            searchColumn = 3;
            expectedRow = 6;
            row = SheetUtils.FindRow(searchRange, searchTarget, searchColumn);
            Assert.AreEqual(expectedRow, row);

            searchTarget = "Something else";
            searchColumn = 12;
            expectedRow = 5;
            row = SheetUtils.FindRow(searchRange, searchTarget, searchColumn);
            Assert.AreEqual(expectedRow, row);

            searchTarget = "Something that does not exist!";
            searchColumn = 6;
            expectedRow = 0;
            row = SheetUtils.FindRow(searchRange, searchTarget, searchColumn);
            Assert.AreEqual(expectedRow, row);
        }

        [TestMethod]
        public void DoesSheetExist()
        {
            string sheetThatExists = "SheetNumber2";
            string sheetThatDoesNotExist = "What the heck?";
            Workbook workbook = getExcelSheet();
            SheetUtils utils = new SheetUtils();
            Assert.IsFalse(utils.DoesSheetExist(workbook, sheetThatDoesNotExist));
            Assert.IsTrue(utils.DoesSheetExist(workbook, sheetThatExists));
        }

        private Workbook getExcelSheet()
        {
            string sheetFilePath = @"C:\ginas_source\Excel\CSharpTest2\Test_Files\comment test.xlsx";
            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            return workbook;
        }

        private byte[] getBinaryData(string file)
        {
            return File.ReadAllBytes(file);
        }
    }
}
