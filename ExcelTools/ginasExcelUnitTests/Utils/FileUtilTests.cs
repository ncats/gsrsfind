using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gov.ncats.ginas.excel.tools.Utils;
using System.Threading.Tasks;

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

    }
}
