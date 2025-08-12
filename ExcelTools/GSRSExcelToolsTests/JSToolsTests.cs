using GSRSExcelTools.Utils;
using System.Text;

namespace GSRSExcelToolsTests
{
    [TestClass]
    public sealed class JSToolsTests
    {
        [TestMethod]
        public void TestCleanPotentialResults()
        {
            string input1 = "{\"Item\":{},\"METOCURINE CHLORIDE\":[\"METOCURINE CHLORIDE\\tbb26142a-6e04-48ab-bfec-8894974bf154\"],\"add\":{},\"keys\":{}}";
            string expectedOutput1 = "{\"METOCURINE CHLORIDE\":[\"METOCURINE CHLORIDE\\tbb26142a-6e04-48ab-bfec-8894974bf154\"]}";
            string actualOutput1 = JSTools.CleanPotentialResults(input1);
            Assert.AreEqual(expectedOutput1, actualOutput1);
        }

        [TestMethod]
        public void TestCleanPotentialResultsSame()
        {
            string input1 = "{\"72559-06-9\":[\"72559-06-9\\tCC(C)CN1CCC2(CC1)N=C3c4c5c(c(C)c6c4C(=O)[C@@](C)(O/C=C/[C@@H]([C@@H](C)[C@H]([C@H](C)[C@@H]([C@H](C)[C@H]([C@@H](C)/C=C/C=C(/C)\\\\C(=O)NC(=C3N2)C5=O)O)O)OC(=O)C)OC)O6)O\"],\"645406-37-7\":[\"645406-37-7\\tCC(C)C[N+]1(CCC2(CC1)N=C3c4c5c(c(C)c6c4C(=O)[C@@](C)(O/C=C/[C@@H]([C@@H](C)[C@H]([C@H](C)[C@@H]([C@H](C)[C@H]([C@@H](C)/C=C/C=C(/C)\\\\C(=O)NC(=C3N2)C5=O)O)O)OC(=O)C)OC)O6)O)[O-]\"]}";
            string expectedOutput1 = (new StringBuilder(input1)).ToString();
            string actualOutput1 = JSTools.CleanPotentialResults(input1);
            Assert.AreEqual(expectedOutput1, actualOutput1);
        }


        [TestMethod]
        public void TestCleanPotentialResultsSame2()
        {
            string input1 = "{\"DOMINE\":[\"DOMINE\\tCCCCCCCCCCCCN(C)CCN(C)C\"],\"ASPIRIN POTASSIUM\":[\"ASPIRIN POTASSIUM\\tCC(=O)Oc1ccccc1C(=O)[O-].[K+]\"],\"SELENOASPIRINE\":[\"SELENOASPIRINE\\tCC(=O)[Se]c1ccccc1C(=O)O\"]}";
            string expectedOutput1 = (new StringBuilder(input1)).ToString();
            string actualOutput1 = JSTools.CleanPotentialResults(input1);
            Assert.AreEqual(expectedOutput1, actualOutput1);
        }

    }
}
