using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelToolsTests.utils
{
    [TestClass]
    public class TextUtilTests
    {
        [TestMethod]
        public void StripQuotesTest()
        {
            string input = "\"Hello, World!\"";
            string expected = "Hello, World!";
            string result = GSRSExcelTools.Utils.TextUtils.StripQuotes(input);
            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        public void ExtractInnerResultIdTest()
        {
            string input = " {\"DOMINE\":[\"DOMINE\\t{'innerResultId': '9919df34-51f8-68d6-17dd-af9ae519e66f'}\"],\"Item\":{},\"add\":{},\"keys\":{}} ";
            string expected = "9919df34-51f8-68d6-17dd-af9ae519e66f";
            string result = GSRSExcelTools.Utils.TextUtils.ExtractInnerResultId(input);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ReplaceInnerResultIdTest()
        {
            string input = " {\"DOMINE\":[\"DOMINE\\t{'innerResultId': '9919df34-51f8-68d6-17dd-af9ae519e66f'}\"],\"Item\":{},\"add\":{},\"keys\":{}} ";
            string replacement = "new-id-12345";
            string expected = " {\"DOMINE\":[\"DOMINE\\tnew-id-12345\"],\"Item\":{},\"add\":{},\"keys\":{}} ";
            string result = GSRSExcelTools.Utils.TextUtils.ReplaceInnerResultId(input, replacement);
            Assert.AreEqual(expected, result);
        }
    }
}
