using System;
using System.IO;
using gov.ncats.ginas.excel.tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ginasExcelUnitTests.Utils
{
    [TestClass]
    public class RestUtilTests
    {
        [TestMethod]
        public void SaveMolfileTest()
        {
            string molfilePath = @"..\..\..\Test_Files\cyclohexane.mol";
            molfilePath = Path.GetFullPath(molfilePath);

            string molfileText = File.ReadAllText(molfilePath);
            molfileText = molfileText.Replace("\r", "");
            var saved= RestUtils.SaveMolfile(molfileText);
            string id = saved.Result;
            Console.WriteLine("id of molfile: " + id);
            Assert.IsFalse(string.IsNullOrEmpty(id));
        }
    }
}
