using System;
using System.IO;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ginasExcelUnitTests.Utils
{

    [TestClass]
    public class RestUtilTests
    {
        GinasToolsConfiguration configuration = FileUtils.GetGinasConfiguration();

        [TestMethod]
        public void SaveMolfileTest()
        {
            string molfilePath = @"..\..\..\Test_Files\cyclohexane.mol";
            molfilePath = Path.GetFullPath(molfilePath);

            string molfileText = File.ReadAllText(molfilePath);
            molfileText = molfileText.Replace("\r", "");
            var saved = RestUtils.SaveMolfileAndDisplay(molfileText, null, configuration.SelectedServer.ServerUrl);
            string id = saved.Result;
            Console.WriteLine("id of molfile: " + id);
            Assert.IsFalse(string.IsNullOrEmpty(id));
        }

        [TestMethod]
        public void IsValidHttpUrlTest()
        {
            string url1 = "http://localhost:9000/ginas/app";
            Assert.IsTrue(RestUtils.IsValidHttpUrl(url1));

            string url1a = "localhost:9000/ginas/app";
            Assert.IsFalse(RestUtils.IsValidHttpUrl(url1a));

            string url2 = "localhost";
            Assert.IsFalse(RestUtils.IsValidHttpUrl(url2));

            string url3 = "https://tripod.nih.gov/";
            Assert.IsTrue(RestUtils.IsValidHttpUrl(url3));
        }
    }
}
