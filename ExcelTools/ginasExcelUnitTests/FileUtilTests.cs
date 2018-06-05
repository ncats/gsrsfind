using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using gov.ncats.ginas.excel.tools.Utils;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class FileUtilTests
    {
        [TestMethod]
        public void GetUserFolderTest()
        {
            string folder = FileUtils.GetUserFolder();
            Console.WriteLine("user folder: " + folder);
            Assert.IsTrue(folder.Contains("ginas"));

        }
    }
}
