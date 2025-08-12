using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using gov.ncats.ginas.excel.tools.Utils;
using System.Threading.Tasks;
using GSRSExcelTools.Utils;

namespace ginasExcelUnitTests.Utils
{
    [TestClass]
    public class FileUtilTests
    {
        [TestMethod]
        public void TestIsBinary1()
        {
            string filePath = @"..\..\..\Test_Files\Substances_20180816_1605.sdf";
            filePath = Path.GetFullPath(filePath);
            bool result = FileUtils.IsBinary(filePath);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestIsBinary2()
        {
            string filePath = @"..\..\..\Test_Files\downloadedimage0.png";
            filePath = Path.GetFullPath(filePath);
            bool result = FileUtils.IsBinary(filePath);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestGetNucleotideMasterData()
        {
            Dictionary<string, string> nucleotideData
                = FileUtils.GetNucleotideMasterData();
            foreach (string key in nucleotideData.Keys)
            {
                Console.WriteLine("key: {0}; value: {1}", key, nucleotideData[key]);
            }
            
            Assert.AreEqual(64, nucleotideData.Keys.Count);
        }

        [TestMethod]
        public void TestGetAminoAcidMasterData()
        {
            Dictionary<string, string> aminoAcids = FileUtils.GetAminoAcidMasterData();
            aminoAcids.Keys.ToList().ForEach(k => Console.WriteLine("long form: {0} - short form {1}",
                k, aminoAcids[k]));
            Assert.AreEqual("C", aminoAcids["Cys"]);
        }
    }
}
