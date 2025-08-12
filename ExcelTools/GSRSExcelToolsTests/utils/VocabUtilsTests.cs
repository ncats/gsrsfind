using Microsoft.VisualStudio.TestTools.UnitTesting;
using gov.ncats.ginas.excel.tools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GSRSExcelTools.Model;
using GSRSExcelTools.Utils;

namespace gov.ncats.ginas.excel.tools.Utils.Tests
{
    [TestClass()]
    public class VocabUtilsTests
    {
        private static string gsrsUrl = "http://localhost:8081/ginas/app/";

        [TestMethod()]
        public void GetVocabularyItemsTest()
        {
            string url = gsrsUrl;
            string requiredVocab = "NAME_TYPE";
            List<VocabItem> items = VocabUtils.GetVocabularyItems(url, requiredVocab);
            Assert.IsTrue(items.Any(i => i.Term.Equals("cn") && i.Display.Equals("Common Name")));
        }

        [TestMethod]
        public void GetVocabularyItemsTest2()
        {
            string url = gsrsUrl;
            string requiredVocab = "RELATIONSHIP_TYPE";
            List<VocabItem> items = VocabUtils.GetVocabularyItems(url, requiredVocab);
            Assert.IsTrue(items.Any(i => i.Term.Equals("ACTIVATOR->TARGET") 
                && i.Display.Equals("ACTIVATOR -> TARGET")));
       }

        [TestMethod]
        public void GetVocabularyDictionaryTest2()
        {
            string url = gsrsUrl;
            string requiredVocab = "DOCUMENT_TYPE";
            Dictionary<string, string> dictionary = VocabUtils.BuildVocabularyDictionary(url, requiredVocab);
            Assert.IsTrue(dictionary.Any(i => i.Key.Equals("CHEMID")
                && i.Value.Equals("CHEMID")));
        }
        [TestMethod]
        public void testJsonFile()
        {
            string jsonFilePath = @"..\..\..\Test_Files\results has rel.json";
            jsonFilePath = Path.GetFullPath(jsonFilePath);
            string json = System.IO.File.ReadAllText(jsonFilePath);
            string jsonClean = json.Replace(Environment.NewLine, "");
            Assert.AreNotEqual(json, jsonClean);
        }
    }
}