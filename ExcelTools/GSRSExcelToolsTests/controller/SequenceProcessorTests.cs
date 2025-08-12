using System;
using System.Linq;
using System.Collections.Generic;
using gov.ncats.ginas.excel.tools.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSRSExcelTools.Controller;
using GSRSExcelTools.Model;

namespace ginasExcelUnitTests.Controller
{
    [TestClass]
    public class SequenceProcessorTests
    {
        [TestMethod]
        public void TestCreateTriplets1()
        {
            string input1 = "abcdef";
            List<string> triplets = SequenceProcessor.CreateTriplets(input1);
            Assert.AreEqual("abc", triplets[0]);
            Assert.AreEqual("def", triplets[1]);
        }

        [TestMethod]
        public void TestCreateTriplets2()
        {
            string input1 = "abcdefg";
            List<string> triplets = SequenceProcessor.CreateTriplets(input1);
            Assert.AreEqual("g", triplets[2]);
        }

        [TestMethod]
        public void TestConvertDnaToRna()
        {
            string dna = "GATTAC";
            string expected = "GAUUAC";
            string actual = SequenceProcessor.ConvertDnaToRna(dna);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestConvertRnaToProteinSequence1()
        {
            List<string> rna1 = new List<string>(new string[]
            { "CAU", "CAC", "ACC" });
            List<string> expected = new List<string>(new string[]
                {"H", "H", "T"});
            List<string> actual = SequenceProcessor.ConvertRnaToProteinSequence(rna1);
            Assert.IsTrue(expected.TrueForAll(e => actual.Contains(e)));
        }

        [TestMethod]
        public void TestConvertDnaSequence()
        {
            string inputRna = "tctgctgagactgacatt";
            List<string> expected = new List<string>
                {"S", "A", "E", "T", "D", "I"};
            List<string> actual = SequenceProcessor.ConvertDnaSequence(inputRna);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void TestGetComplementaryDnaBase()
        {
            string b1 = "C";
            string expected = "G";
            string actual = SequenceProcessor.GetComplementaryDnaBase(b1);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetComplementaryDnaBase2()
        {
            string b1 = "A";
            string expected = "T";
            string actual = SequenceProcessor.GetComplementaryDnaBase(b1);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetComplementaryDnaSequenceTest()
        {
            List<string> dnaSequence = new List<string> { "AGG", "TTT", "CCA" };
            List<string> expected = new List<string> { "TCC", "AAA", "GGT" };
            List<string> actual = SequenceProcessor.GetComplementaryDnaSequence(dnaSequence);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void ConvertDnaSequenceForRetrovirusTest()
        {
            string input1 = "atcgctttattctgtatatgcaaattt";
            List<string> expectedRna = new List<string>
                { "AAA", "UUU", "GCA", "UAU", "ACA", "GAA", "UAA", "AGC", "GAU" };
            List<string> actual = SequenceProcessor.ConvertDnaSequenceForRetrovirus(input1);

            actual.ForEach(s => Console.WriteLine(s));
            Assert.IsTrue(expectedRna.SequenceEqual(actual));
        }

        [TestMethod]
        public void ReverseStringTest()
        {
            string input = "this is a string";
            string expected = "gnirts a si siht";
            string actual = input.ReverseString();
            Assert.AreEqual(expected, actual);
        }
    }
}
