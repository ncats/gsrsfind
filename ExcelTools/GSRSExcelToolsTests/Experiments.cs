using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelToolsTests
{
    [TestClass]
    public class Experiments
    {

        [TestMethod]
        public void Test1()
        {
            string input1 = "root_domain:\"\"\"^\"\"\"LANGUAGE\"\"\"$\"\"\"";
            string output1 = input1.Replace("\"\"\"", string.Empty);
            Console.WriteLine("output: " + output1);
            Assert.IsFalse(output1.Contains("\""));
        }
    }
}
