using Microsoft.VisualStudio.TestTools.UnitTesting;
using gov.ncats.ginas.excel.tools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gov.ncats.ginas.excel.tools.Utils.Tests
{
    [TestClass()]
    public class VocabUtilsTests
    {
        [TestMethod()]
        public void GetVocabularyItemsTest()
        {
            string url = "http://localhost:9000/ginas/app";
            string requiredVocab = "NAME_TYPE";
            List<Model.VocabItem> items = VocabUtils.GetVocabularyItems(url, requiredVocab);
            Assert.IsTrue(items.Any(i => i.Term.Equals("cn") && i.Display.Equals("Common Name")));
        }

        [TestMethod]
        public void GetVocabularyItemsTest2()
        {
            string url = "http://localhost:9000/ginas/app/";
            string requiredVocab = "RELATIONSHIP_TYPE";
            List<Model.VocabItem> items = VocabUtils.GetVocabularyItems(url, requiredVocab);
            Assert.IsTrue(items.Any(i => i.Term.Equals("ACTIVATOR->TARGET") 
                && i.Display.Equals("ACTIVATOR -> TARGET")));
       }

        [TestMethod]
        public void GetVocabularyDictionaryTest2()
        {
            string url = "http://localhost:9000/ginas/app/";
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