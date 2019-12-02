using Microsoft.VisualStudio.TestTools.UnitTesting;
using gov.ncats.ginas.excel.tools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using ginasExcelUnitTests.Model;
using gov.ncats.ginas.excel.tools.Model;

namespace gov.ncats.ginas.excel.tools.Utils.Tests
{
    [TestClass()]
    public class ScriptUtilsTests
    {
        [TestMethod()]
        public void GetVocabNameTest()
        {
            ScriptUtils scriptUtils = new ScriptUtils();
            scriptUtils.ScriptName = "Add Name Public";
            scriptUtils.ScriptExecutor = new ScriptExecutorMock();
            string vocabName = scriptUtils.GetVocabName(2);
            Assert.IsTrue(string.IsNullOrWhiteSpace(vocabName));

            string vocabName2 = scriptUtils.GetVocabName(3);
            Assert.IsFalse(string.IsNullOrWhiteSpace(vocabName2));
        }

        [TestMethod]
        public void StartVocabularyRetrievalsTest()
        {
            ScriptUtils scriptUtils = new ScriptUtils();
            scriptUtils.ScriptName = "Add Name Public";
            scriptUtils.ScriptExecutor = new ScriptExecutorMock();
            scriptUtils.StartVocabularyRetrievals();

            int expectedNumberVocabs = 3;
            Assert.AreEqual(expectedNumberVocabs, scriptUtils.ExpectedVocabularies.Count);
        }

        [TestMethod]
        public void GetVocabItemsTest()
        {
            ScriptUtils scriptUtils = new ScriptUtils();
            scriptUtils.ScriptName = "Add Name Public";
            scriptUtils.ScriptExecutor = new ScriptExecutorMock();

            List<VocabItem> items= scriptUtils.GetVocabItems("name");
            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
        public void GetVocabItemsTestFull()
        {
            ScriptUtils scriptUtils = new ScriptUtils();
            scriptUtils.ScriptName = "Add Name Public";
            scriptUtils.ScriptExecutor = new ScriptExecutorMock();
            Vocab vocab = CreateSimpleVocab();

            string paramName = "The Parm";
            scriptUtils.Vocabularies.Add(paramName, vocab);
            
            List<VocabItem> items = scriptUtils.GetVocabItems(paramName);
            Assert.AreEqual("The Other Value", items[1].Term);
        }

        [TestMethod]
        public void GetTranslationDictionaryTest()
        {
            string methodName = "GetTranslationDictionary";
            ScriptUtils scriptUtils = new ScriptUtils();
            scriptUtils.ScriptName = "Add Name Public";
            scriptUtils.ScriptExecutor = new ScriptExecutorMock();

            MethodInfo method = scriptUtils.GetType().GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance);
            Vocab vocab = CreateSimpleVocab();
            object[] parms = new object[1];
            parms[0] = vocab;
            object result = method.Invoke(scriptUtils, parms);
            Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
            Dictionary<string, string> keyValuePairs = (Dictionary<string, string>)result;
            string transformed = keyValuePairs["The Display"];
            Assert.AreEqual("The Value", transformed);
        }

        [TestMethod]
        public void GetBooleanVocabularyItemsTest()
        {
            ScriptUtils scriptUtils = new ScriptUtils();
            List<VocabItem> booleanVocabularyItems = scriptUtils.GetBooleanVocabularyItems();
            Assert.IsTrue(booleanVocabularyItems.Any(v => v.Term.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                && booleanVocabularyItems.Any(v => v.Term.Equals("false", StringComparison.InvariantCultureIgnoreCase)));


        }
        private Vocab CreateSimpleVocab()
        {
            Vocab vocab = new Vocab();
            vocab.Content = new VocabContent[1];
            vocab.Content[0] = new VocabContent();
            vocab.Content[0].Terms = new VocabTerm[2];
            vocab.Content[0].Terms[0] = new VocabTerm
            {
                Value = "The Value",
                Display = "The Display"
            };
            vocab.Content[0].Terms[1] = new VocabTerm
            {
                Value = "The Other Value",
                Display = "The Other Display"
            };
            return vocab;
        }
    }
}