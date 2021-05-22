using Microsoft.VisualStudio.TestTools.UnitTesting;
using gov.ncats.ginas.excel.tools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Windows.Forms.VisualStyles;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Controller;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using gov.ncats.ginas.excel.tools.Model.FDAApplication;


namespace gov.ncats.ginas.excel.tools.Utils.Tests
{
    [TestClass()]
    public class PubChemRestTests
    {
        private static Microsoft.Office.Interop.Excel.Application excelApp;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            excelApp = new Microsoft.Office.Interop.Excel.Application();
            Console.WriteLine("Started Excel");
            
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (var workbook in excelApp.Workbooks)
            {
                ((Workbook)workbook).Close(false);
            }
            //excel.Workbooks.Close();
            excelApp.Quit();
            Console.WriteLine("Closed Excel");
        }


        [TestMethod()]
        public void RunPubChemQueryTest()
        {
            string pubchemBaseUrl = "https://pubchem.ncbi.nlm.nih.gov";
            Workbook workbook= excelApp.Workbooks.Add();
            Worksheet sheet = (Worksheet) workbook.Worksheets.Add();
            Range range1 = sheet.Range["C2"];
            Range range2 = sheet.Range["D2"];

            List<string> inchikeys = new List<string>(new String[] { "WRWBCPJQPDHXTJ-DTMQFJJTSA-N", "IZHVBANLECCAGF-UHFFFAOYSA-N" });

            List<LookupDataCallback> lookupData = new List<LookupDataCallback>();
            lookupData.Add(new LookupDataCallback(range1, inchikeys[0], null));
            lookupData.Add(new LookupDataCallback(range2, inchikeys[1], null));
            //inchikeys.ForEach(ik => lookupData.Add(new LookupDataCallback(null, ik, null)));

            Task<BatchLookup> ids= RestUtils.RunPubChemQuery(lookupData, pubchemBaseUrl);
            List<string> expected = new List<string>(new String[] { "229021", "101269" });
            Assert.IsTrue(ids.Result.LookupData.All(i => expected.Contains(i.Result)));
        }

        [TestMethod]
        public void IsPossibleInChiKeyTestBlank()
        {
            string data1 = "";
            Assert.IsFalse(PubChemRetriever.IsPossibleInChiKey(data1));
        }

        [TestMethod]
        public void IsPossibleInChiKeyTestSpace()
        {
            string data1 = " ";
            Assert.IsFalse(PubChemRetriever.IsPossibleInChiKey(data1));
        }

        [TestMethod]
        public void IsPossibleInChiKeyTestShort()
        {
            string data1 = "WRWBCPJQPDHXTJ";
            Assert.IsFalse(PubChemRetriever.IsPossibleInChiKey(data1));
        }

        [TestMethod]
        public void IsPossibleInChiKeyTestLong()
        {
            string data1 = "WRWBCPJQPDHXTJWRWBCPJQPDHXTJWRWBCPJQPDHXTJWRWBCPJQPDHXTJWRWBCPJQPDHXTJ";
            Assert.IsFalse(PubChemRetriever.IsPossibleInChiKey(data1));
        }

        [TestMethod]
        public void IsPossibleInChiKeyTestTrue()
        {
            string data1 = "IRPSJVWFSWAZSZ-OIUSMDOTSA-L";
            Assert.IsTrue(PubChemRetriever.IsPossibleInChiKey(data1));
        }
    }
}