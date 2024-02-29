using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Linq;

using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Model;
using System.Collections.Generic;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class FileUtilTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        [TestMethod]
        public void TestDefaultConfigFile()
        {
            string configFilePath = Directory.GetCurrentDirectory() + @"\etc\g-srs-config.txt";

            string configString = File.ReadAllText(configFilePath);
            log.Debug("configString: " + configString);
            GinasToolsConfiguration config = null;
            config = JSTools.GetGinasToolsConfigurationFromString(configString);
            Assert.IsNotNull(config);

        }

        [TestMethod]
        public void TestGetUniqueFileName()
        {
            List<string> fileNames = new List<string>();
            int iterations = 10;
            for(int it = 0; it < iterations; it++)
            {
                fileNames.Add(FileUtils.GetUniqueFileName("data"));
            }

            Assert.IsTrue(fileNames.TrueForAll(f => f.EndsWith("data")));
            Assert.AreEqual(fileNames.Distinct().Count(), fileNames.Count());
        }

    }
}
