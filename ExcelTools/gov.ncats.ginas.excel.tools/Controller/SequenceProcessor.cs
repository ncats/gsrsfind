using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Excel = Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public class SequenceProcessor
    {
        private static Dictionary<string, string> tripletToAmino;
        private static Dictionary<string, string> aminoLongToShort;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static SequenceProcessor()
        {
            tripletToAmino = FileUtils.GetNucleotideMasterData();
            aminoLongToShort = FileUtils.GetAminoAcidMasterData();
        }

        public static void StartDnaToProtein( Excel.Window excelWindow)
        {
            Excel.Worksheet activeWorksheet = ((Excel.Worksheet)excelWindow.Application.ActiveSheet);
            Excel.Range selection = (Excel.Range)excelWindow.Application.Selection;
            if (selection == null)
            {
                UIUtils.ShowMessageToUser("Error obtaining access to Excel!");
                return;
            }
            List<SearchValue> searchValues = Retriever.GetSearchValues(selection);
            
            if (searchValues.Any(v => string.IsNullOrWhiteSpace(v.Value)))
            {
                UIUtils.ShowMessageToUser("Please select a chemical name or ID");
                return;
            }
            searchValues.ForEach(sv =>
            {
                if( !string.IsNullOrWhiteSpace(sv.Value))
                {
                    List<string> proteinEquivalent = ConvertDnaSequence(sv.Value);
                    int col = SheetUtils.FindColumn(selection, sv.Value, sv.RowNumber);
                    col++;
                    SheetUtils.SetCellValue(activeWorksheet, sv.RowNumber, col, string.Join(" ", proteinEquivalent));
                }
            });
        }

        public static void StartDnaToRetrovirusRna(Excel.Window excelWindow)
        {
            Excel.Worksheet activeWorksheet = ((Excel.Worksheet)excelWindow.Application.ActiveSheet);
            Excel.Range selection = (Excel.Range)excelWindow.Application.Selection;
            if (selection == null)
            {
                UIUtils.ShowMessageToUser("Error obtaining access to Excel!");
                return;
            }
            List<SearchValue> searchValues = Retriever.GetSearchValues(selection);

            if (searchValues.Any(v => string.IsNullOrWhiteSpace(v.Value)))
            {
                UIUtils.ShowMessageToUser("Please select a chemical name or ID");
                return;
            }
            searchValues.ForEach(sv =>
            {
                if (!string.IsNullOrWhiteSpace(sv.Value))
                {
                    List<string> retrovirusRna = ConvertDnaSequenceForRetrovirus(sv.Value);
                    int col = SheetUtils.FindColumn(selection, sv.Value, sv.RowNumber);
                    col++;
                    SheetUtils.SetCellValue(activeWorksheet, sv.RowNumber, col, string.Join(" ", retrovirusRna));
                }
            });
        }

        public static List<string> ConvertDnaSequence(string dnaSequence)
        {
            //step 1: uppercase
            string processingSequence = dnaSequence.Trim().ToUpper();
            //step 2: break up into triplets
            List<string> dnaTriplets = CreateTriplets(processingSequence);
            //step 3: convert DNA to RNA 
            List<string> rnaTriplets = new List<string>();
            dnaTriplets.ForEach(s => rnaTriplets.Add( ConvertDnaToRna(s)));
            //step 4: convert RNA to protein
            List<string> protein = ConvertRnaToProteinSequence(rnaTriplets);
            return protein;
        }

        public static List<string> ConvertDnaSequenceForRetrovirus(string dnaSequence)
        {
            //step 1: uppercase
            string processingSequence = dnaSequence.Trim().ToUpper();
            //step 2: break up into triplets
            List<string> dnaTriplets = CreateTriplets(processingSequence);

            //step 3: reverse the sequence -- both the triplets themselves and 
            // the characters within the triplets
            List<string> backwardDnaTriplets = new List<string>();
            dnaTriplets.ForEach(t=> backwardDnaTriplets.Add(t.ReverseString()));
            backwardDnaTriplets.Reverse();

            //step 4: create complementary sequence
            List<string> backwardComplementaryDnaTriplets = GetComplementaryDnaSequence(backwardDnaTriplets);

            //step 5: convert to RNA
            List<string> rnaTriplets = new List<string>();
            backwardComplementaryDnaTriplets.ForEach(s => rnaTriplets.Add(ConvertDnaToRna(s)));

            return rnaTriplets;
        }

        public static List<string> CreateTriplets(string inputString)
        {
            int i = 1;
            List<string> triplets = new List<string>();
            StringBuilder tripletBuilder = new StringBuilder();
            while (i <= inputString.Length )
            {
                tripletBuilder.Append(inputString.Substring(i-1, 1));
                if((i % 3) == 0)
                {
                    triplets.Add(tripletBuilder.ToString());
                    tripletBuilder.Clear();
                }
                i++;
            }
            if (tripletBuilder.Length>0) triplets.Add(tripletBuilder.ToString());
            return triplets;
        }

        public static string ConvertDnaToRna(string inputSequence)
        {
            string outputSequence = inputSequence.Replace("T", "U");
            return outputSequence;
        }

        public static List<string> ConvertRnaToProteinSequence(List<string> rnaSequence)
        {
            List<String> proteinSequence = new List<string>();
            foreach(string triplet in rnaSequence)
            {
                if( !tripletToAmino.ContainsKey(triplet))
                {
                    proteinSequence.Add("No match for: " + triplet);
                    continue;
                }
                string longProtein = tripletToAmino[triplet];
                if( aminoLongToShort.ContainsKey(longProtein))
                {
                    proteinSequence.Add(aminoLongToShort[longProtein]);
                }
                else
                {
                    proteinSequence.Add(longProtein);
                }
            }
            return proteinSequence;
        }

        public static List<string> GetComplementaryDnaSequence(List<string> bases)
        {
            List<string> complements = new List<string>();
            bases.ForEach(b =>
            {
                StringBuilder complementBuilder = new StringBuilder();
                b.ToCharArray().ToList().ForEach(c =>
                {
                    complementBuilder.Append(GetComplementaryDnaBase(c.ToString()));
                });
                complements.Add(complementBuilder.ToString());
            });

            return complements;
        }
        public static string GetComplementaryDnaBase(string baseCode)
        {
            switch(baseCode)
            {
                case "T":
                    return "A";
                case "A":
                    return "T";
                case "G":
                    return "C";
                case "C":
                    return "G";

            }
            return "";
        }

    }
}
