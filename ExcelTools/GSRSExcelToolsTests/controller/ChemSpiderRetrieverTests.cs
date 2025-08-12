using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ginasExcelUnitTests.Controller
{
    [TestClass]
    public class ChemSpiderRetrieverTests
    {
        [TestMethod]
        public void TestMethodFormatting()
        {
            string inchikey = "JKINPMFPGULFQY-UHFFFAOYSA-N";
            string query = string.Format(" \"inchikey\": \"{0}\"", inchikey);
            query = "{" + query + "}";
            string expected = "{ \"inchikey\": \"JKINPMFPGULFQY-UHFFFAOYSA-N\"}";
            Assert.AreEqual(expected, query);
        }
    }
}
