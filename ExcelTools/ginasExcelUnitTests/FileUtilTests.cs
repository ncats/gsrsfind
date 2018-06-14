using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Model;

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

        [TestMethod]
        public void GetGinasConfigurationTest()
        {
            GinasToolsConfiguration ginasConfig = FileUtils.GetGinasConfiguration();
            Assert.IsNotNull(ginasConfig);
            Console.WriteLine(ginasConfig.ToString());
        }
    }
}
