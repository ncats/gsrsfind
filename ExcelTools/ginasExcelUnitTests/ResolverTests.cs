using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using ginasExcelUnitTests.Model;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Controller;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class ResolverTests
    {
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
            string newIdString = gov.ncats.ginas.excel.tools.Utils.JSTools.RandomIdentifier();
            Console.WriteLine("newId: " + newIdString);
            Assert.IsNotNull(newIdString);
            int expectedLength = 10 + 5;
            Assert.AreEqual(expectedLength, newIdString.Length);

        }

        [TestMethod]
        public void test_getHTML()
        {
            string html = FileUtils.GetHtml();
            Assert.IsTrue(html.Contains("<textarea id=\"console\">") 
                && html.Contains("_.forEach(runner.getArguments()"));
        }

        [TestMethod]
        public void getTempFile_test()
        {
            ImageOps imageOps = new ImageOps();
            string tempFileName = imageOps.getTempFile("hello", "txt");
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
            string input = "{\"aspirin\":[\"aspirin\thttps://tripod.nih.gov/ginas/app/img/a05ec20c-8fe2-4e02-ba7f-df69e5e30248.png?size=300\"]}";
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

        /* Thie input url is too temporary for this test to work
        [TestMethod]
        public void RemoteFileExistsTestTrue()
        {
            string url1 = "http://localhost:9000/ginas/app/img/3982bff1-da0a-49a5-be34-4adb8c7648af.png?size=300";
            Assert.IsTrue(ImageOps.RemoteFileExists(url1));
        }
        */

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

        private BatchCallback setupData()
        {

            BatchCallback batchCallback = new BatchCallback(new List<Callback>());
            
            Callback cb1 = new Callback();
            cb1.setKey("a");
            batchCallback.AddCallback(cb1);
            Callback cb2 = new Callback();
            cb2.setKey("B");
            batchCallback.AddCallback(cb2);
            return batchCallback;
        }
    }
}
