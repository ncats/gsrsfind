using Microsoft.VisualStudio.TestTools.UnitTesting;
using gov.ncats.ginas.excel.tools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gov.ncats.ginas.excel.tools.Model;

namespace gov.ncats.ginas.excel.tools.Utils.Tests
{
    [TestClass()]
    public class SDFileUtilsTests
    {
        [TestMethod()]
        public void GetFieldNameTest()
        {
            string input1 = ">  <Formula> (1)";
            string expectedName = "Formula";
            SDFileUtils sDFileUtils = new SDFileUtils();
            string actualName = sDFileUtils.GetFieldName(input1);
            Assert.AreEqual(expectedName, actualName);

            string input2 = ">  <MolfileName>";
            expectedName = "MolfileName";
            actualName = sDFileUtils.GetFieldName(input2);
            Assert.AreEqual(expectedName, actualName);

            string input3 = ">  <cas_index_name> (2)";
            expectedName = "cas_index_name";
            actualName = sDFileUtils.GetFieldName(input3);
            Assert.AreEqual(expectedName, actualName);
        }

        [TestMethod]
        public void ReadSDFileTest()
        {
            string path = @"C:\Projects\ncats\SD files from Andrzej\INN_119.sdf";
            SDFileUtils sDFileUtils = new SDFileUtils();
            List<SDFileRecord> sDFileRecords = sDFileUtils.ReadSdFile(path);
            int expectedRecords = 47;
            Assert.AreEqual(expectedRecords, sDFileRecords.Count);

            string molfilePath = @"c:\temp\test1.mol";
            System.IO.File.WriteAllText(molfilePath, sDFileRecords[0].RecordData["Molfile"]);

            
        }

        [TestMethod]
        public void ReadSDFileTest2()
        {
            string path = @"C:\Projects\ncats\SD files from Andrzej\Substances_20180816_1605.sdf";
            SDFileUtils sDFileUtils = new SDFileUtils();
            List<SDFileRecord> sDFileRecords = sDFileUtils.ReadSdFile(path);
            int expectedRecords = 47;
            Assert.AreEqual(expectedRecords, sDFileRecords.Count);

            string molfilePath = @"c:\temp\test2.mol";
            System.IO.File.WriteAllText(molfilePath, sDFileRecords[1].RecordData["Molfile"]);
        }

        [TestMethod]
        public void ReadSDFileTest3()
        {
            string path = @"C:\Projects\ncats\SDfile from Chris Jarvis\Export1d.sdf";
            SDFileUtils sDFileUtils = new SDFileUtils();
            List<SDFileRecord> sDFileRecords = sDFileUtils.ReadSdFile(path);
            int expectedRecords = 68;
            Assert.AreEqual(expectedRecords, sDFileRecords.Count);

            string molfilePath = @"c:\temp\test4.mol";
            Assert.IsTrue(sDFileRecords[1].RecordData["Molfile"].EndsWith("M  END"));
            System.IO.File.WriteAllText(molfilePath, sDFileRecords[1].RecordData["Molfile"]);
        }

        [TestMethod]
        public void GetUniqueFieldNamesTest()
        {
            List<SDFileRecord> records = new List<SDFileRecord>();
            SDFileRecord rec1 = new SDFileRecord();
            rec1.RecordData.Add("Molfile", "C1CCCC1");
            rec1.RecordData.Add("Name", "Cyclopentane");
            rec1.RecordData.Add("Boiling Point", "50");
            records.Add(rec1);

            SDFileRecord rec2 = new SDFileRecord();
            rec2.RecordData.Add("Molfile", "dummy data");
            rec2.RecordData.Add("Name", "Cyclopentane");
            rec2.RecordData.Add("Meltinging Point", "50");
            records.Add(rec2);

            SDFileRecord rec3 = new SDFileRecord();
            rec3.RecordData.Add("Molfile", "dummy data");
            rec3.RecordData.Add("Name", "Another");
            rec3.RecordData.Add("Comment", "Test data");
            rec3.RecordData.Add("Description", "More data");
            records.Add(rec3);

            string[] expectedNamesArray = { "Molfile", "Name", "Boiling Point", "Meltinging Point",
                "Comment", "Description"};
            List<string> expectedFieldNames = new List<string>();
            expectedFieldNames.AddRange(expectedNamesArray);

            SDFileUtils sDFileUtils = new SDFileUtils();
            List<string> actualFieldNames = sDFileUtils.GetUniqueFieldNames(records);
            Assert.AreEqual(expectedFieldNames.Count, actualFieldNames.Count);
            expectedFieldNames.ForEach(n => Assert.IsTrue(actualFieldNames.Contains(n)));
        }
    }
}