using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Linq;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class FormatProcessingTests
    {
        static Application excel;
        public static GinasToolsConfiguration CurrentConfiguration
        {
            get;
            set;
        }


        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {


            excel = new Application();
            Console.WriteLine("Started Excel");
            CurrentConfiguration = FileUtils.GetGinasConfiguration();
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (var workbook in excel.Workbooks)
            {
                ((Workbook)workbook).Close(false);
            }
            //excel.Workbooks.Close();
            excel.Quit();
            Console.WriteLine("Closed Excel");
        }

        [TestMethod]
        public void GetFormattedTextTest()
        {

            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A3"];
            Characters chars = selection.Characters[1, 2];
            bool itals = (Boolean)chars.Font.Italic;
            Assert.IsTrue(itals);
            Object formula = selection.Formula;
            Assert.IsNotNull(formula);
            workbook.Close(false);

        }

        [TestMethod]
        public void GetItalicsTest()
        {

            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A2"];
            List<MarkupElement> italicized = SheetFormatUtils.GetItalicsRanges(selection);
            italicized.ForEach(t => Console.WriteLine("{0}-{1}", t.startPosition, t.length));
            Assert.AreEqual(2, italicized.Count);
            Characters chars = selection.Characters;
            string charText = chars.Text;
            for (int i = 1; i <= chars.Count; i++)
            {
                Console.WriteLine("{0} chars: {1}; string: {2}", i, selection.Characters[i, 1].Text,
                    selection.Characters.Text.Substring(i - 1, 1));
            }

            selection = sheet.Range["A7"];
            List<MarkupElement> italicized2 = SheetFormatUtils.GetItalicsRanges(selection);
            workbook.Close(false);
            italicized2.ForEach(t => Console.WriteLine("{0}-{1}", t.startPosition, t.length));
            Assert.AreEqual(2, italicized2.Count);
            Assert.IsTrue(italicized2.All(e => e.tag.Equals("I")));
        }

        [TestMethod]
        public void GetSuperTest()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A6"];
            List<MarkupElement> supers = SheetFormatUtils.GetSuperscriptRanges(selection);
            workbook.Close(false);
            supers.ForEach(t => Console.WriteLine("{0}-{1}", t.startPosition, t.length));
            Assert.AreEqual(0, supers[0].startPosition);
            Assert.AreEqual(1, supers[0].length);
        }

        [TestMethod]
        public void GetSuperNoneTest()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A11"];
            List<MarkupElement> subs = SheetFormatUtils.GetSuperscriptRanges(selection);
            workbook.Close(false);
            Assert.AreEqual(0, subs.Count);
        }


        [TestMethod]
        public void GetSubTest()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A11"];
            List<MarkupElement> subs = SheetFormatUtils.GetSubscriptRanges(selection);
            workbook.Close(false);
            Assert.AreEqual(2, subs[0].length);
            Assert.AreEqual(20, subs[0].startPosition);
        }

        [TestMethod]
        public void GetSuperTest2()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A10"];
            List<MarkupElement> supers = SheetFormatUtils.GetSuperscriptRanges(selection);
            workbook.Close(false);

            supers.ForEach(t => Console.WriteLine("{0}-{1}", t.startPosition, t.length));
            Assert.AreEqual(25, supers[0].startPosition);
            Assert.AreEqual(1, supers[0].length);
        }

        [TestMethod]
        public void GetSuperTest3()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A13"];
            List<MarkupElement> supers = SheetFormatUtils.GetSuperscriptRanges(selection);
            Assert.IsTrue(supers.Count > 0);
            supers.ForEach(t => Console.WriteLine("{0}-{1}", t.startPosition, t.length));
            Assert.AreEqual(selection.Text.ToString().Length - 1, supers[0].startPosition);
            workbook.Close(false);
            Assert.AreEqual(1, supers[0].length);
        }

        [TestMethod]
        public void ProcessMarkupTest()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A15"];
            string actualMarkedUpText = SheetFormatUtils.ExtractAndApplyFormatting(selection);
            workbook.Close(false);
            string expected = "Quinine tannate φ <I>derivative</I><SUB>2</SUB> for science<SUP>1</SUP>";
            Assert.AreEqual(expected, actualMarkedUpText);
        }

        [TestMethod]
        public void ProcessMarkupTest2()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A17"];
            string actualMarkedUpText = SheetFormatUtils.ExtractAndApplyFormatting(selection);
            workbook.Close(false);
            string expected = "Quinine tannate φ <I>derivative</I> <SUB>2</SUB> for science<SUP>1</SUP>";
            Assert.AreEqual(expected, actualMarkedUpText);
        }


        [TestMethod]
        public void ExtractAndApplyFormattingTest()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A2"];
            string expected = "(<I>S</I>)-3-(3-fluoro-2-methylphenoxy)-<I>N</I>-methyl-3-phenylpropan-1-amine";
            string actual = SheetFormatUtils.ExtractAndApplyFormatting(selection);
            workbook.Close(false);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFormatParsing()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A10"];
            string expected = "Some <I>name</I> with characters<SUP>2</SUP> look at<SUB>later</SUB>";
            string actual = SheetFormatUtils.ExtractAndApplyFormatting(selection);
            workbook.Close(false);

            Assert.AreEqual(expected, actual);
        }
        /*[TestMethod]
        public void GetSmallsTest()
        {
            string sheetFilePath = @"..\..\..\Test_Files\FormattedText.xlsx";
            sheetFilePath = Path.GetFullPath(sheetFilePath);

            Workbook workbook = excel.Workbooks.Open(sheetFilePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets[1];
            Range selection = sheet.Range["A8"];
            List<MarkupElement> supers = SheetFormatUtils.GetSuperscriptRanges(selection);
            supers.ForEach(t => Console.WriteLine("{0}-{1}", t.startPosition, t.length));
            Assert.AreEqual(1, supers.Count);
        }*/

        [TestMethod]
        public void ApplyMarkupTest()
        {
            String input = "(S)-3-(3-fluoro-2-methylphenoxy)-N-methyl-3-phenylpropan-1-amine";
            List<MarkupElement> markups = new List<MarkupElement>();
            markups.Add(new MarkupElement { length = 1, startPosition = 1, tag = "i" });
            markups.Add(new MarkupElement { length = 1, startPosition = 33, tag = "i" });
            String output = SheetFormatUtils.ApplyMarkup(input, markups);
            string expected = "(<i>S</i>)-3-(3-fluoro-2-methylphenoxy)-<i>N</i>-methyl-3-phenylpropan-1-amine";
            Assert.AreEqual(expected, output);
        }


        [TestMethod]
        public void ApplyMarkupTest2()
        {
            String input = "(S)-3-(3-fluoro-2-methylphenoxy)-N-methyl-3-phenylpropan-1-D-amine";
            List<MarkupElement> markups = new List<MarkupElement>();
            markups.Add(new MarkupElement { length = 1, startPosition = 1, tag = "i" });
            markups.Add(new MarkupElement { length = 1, startPosition = 33, tag = "i" });
            markups.Add(new MarkupElement { length = 1, startPosition = 59, tag = "small" });
            String output = SheetFormatUtils.ApplyMarkup(input, markups);
            string expected = "(<i>S</i>)-3-(3-fluoro-2-methylphenoxy)-<i>N</i>-methyl-3-phenylpropan-1-<small>D</small>-amine";
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void ApplyMarkupTest3()
        {
            String input = "(S)-3-(3-fluoro-2-methylphenoxy)-N-methyl-3-phenylpropan-1-D-amine1";
            List<gov.ncats.ginas.excel.tools.Model.MarkupElement> markups = new List<MarkupElement>();
            markups.Add(new MarkupElement { length = 1, startPosition = 1, tag = "i" });
            markups.Add(new MarkupElement { length = 1, startPosition = 33, tag = "i" });
            markups.Add(new MarkupElement { length = 1, startPosition = 59, tag = "small" });
            markups.Add(new MarkupElement { length = 1, startPosition = 66, tag = "sup" });
            String output = SheetFormatUtils.ApplyMarkup(input, markups);
            string expected = "(<i>S</i>)-3-(3-fluoro-2-methylphenoxy)-<i>N</i>-methyl-3-phenylpropan-1-<small>D</small>-amine<sup>1</sup>";
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestApplyMarkupNoElement()
        {
            List<MarkupElement> elements = new List<MarkupElement>();
            string input = "hello, world";
            string expected = input;
            string actual = SheetFormatUtils.ApplyMarkup(input, elements);
            Assert.AreEqual(expected, actual);
        }

    }
}
