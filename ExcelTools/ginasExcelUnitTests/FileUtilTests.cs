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
            Assert.IsTrue(folder.Contains("g-srs"));
        }

        [TestMethod]
        public void GetGinasConfigurationTest()
        {
            GinasToolsConfiguration ginasConfig = FileUtils.GetGinasConfiguration();
            Assert.IsNotNull(ginasConfig);
            Console.WriteLine(ginasConfig.ToString());
        }

        [TestMethod]
        public void GetTemporaryFilePathTest()
        {
            string extension = "txt";
            string fullPath = FileUtils.GetTemporaryFilePath(extension);
            FileUtils.WriteToFile(fullPath, "Test Data");
            Assert.IsTrue(System.IO.File.Exists(fullPath));
            Console.WriteLine("fullPath: " + fullPath);
            System.IO.File.Delete(fullPath);
        }


        [TestMethod]
        public void FolderExistsTest1()
        {
            string path1 = @"c:\temp";
            Assert.IsTrue(FileUtils.FolderExists(path1));
        }

        [TestMethod]
        public void FolderExistsTest2()
        {
            string path1 = @"c:\ImaginaryFolder";
            Assert.IsFalse(FileUtils.FolderExists(path1));
        }

    }
}
