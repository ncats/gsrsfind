using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using gov.ncats.ginas.excel.tools.Model;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class SheetUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string VOCABULARY_SHEET_NAME = "_gsrs_vocabularies_";
        private static int MAX_COLUMNS = 16000;
        private static int VOCABULARY_TEST_ROW = 1;

        public GinasToolsConfiguration Configuration
        {
            get;
            set;
        }

        //from http://stackoverflow.com/questions/10373561/convert-a-number-to-a-letter-in-c-sharp-for-use-in-microsoft-excel
        public static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";
            if (index > letters.Length)
                value += letters[(index - 1) / letters.Length - 1];
            value += letters[(index - 1) % letters.Length];
            return value;
        }

        public static int FindRow(Range rangeToSearch, string textToFind,
            int columnToSearch)
        {
            for (int row = 0; row < rangeToSearch.Rows.Count; row++)
            {
                int currentRow = rangeToSearch.Row + row;
                string cellName = GetColumnName(columnToSearch) + currentRow;
                object value = rangeToSearch.Worksheet.Range[cellName].Value;
                if (value is string)
                {
                    string cellValue = (string)value;
                    if (cellValue.Equals(textToFind)) return currentRow;
                }
            }
            return 0;
        }

        public static int FindColumn(Range rangeToSearch, string textToFind,
            int rowToSearch)
        {
            for (int column = 0; column < rangeToSearch.Columns.Count; column++)
            {
                int currentColumn = rangeToSearch.Column + column;
                string cellName = GetColumnName(currentColumn) + rowToSearch;
                object value = rangeToSearch.Worksheet.Range[cellName].Value;
                if (value is string)
                {
                    string cellValue = (string)value;
                    if (cellValue.Equals(textToFind)) return currentColumn;
                }
            }
            return 0;
        }

        public bool DoesSheetExist(Workbook workbook, string sheetName)
        {
            foreach (Worksheet sheet in workbook.Sheets)
            {
                if (sheet.Name.Equals(sheetName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public void CreateSheet(Workbook workbook, string scriptName,
            IScriptExecutor scriptExecutor)
        {
            if (DoesSheetExist(workbook, scriptName))
            {
                UIUtils.ShowMessageToUser("Sheet \"" + scriptName + "\" already exists");
                (workbook.Sheets[scriptName] as _Worksheet).Activate();
                return;
            }

            scriptExecutor.ExecuteScript("tmpScript=Scripts.get('" + scriptName + "');");
            scriptExecutor.ExecuteScript("tmpRunner=tmpScript.runner();");

            Worksheet nsheet;
            int i;

            nsheet = (Worksheet)workbook.Sheets.Add();
            nsheet.Name = scriptName;

            Range topCorner = nsheet.Range["A1"];
            topCorner.FormulaR1C1 = "BATCH:" + scriptName;
            topCorner.AddComment("This column header must be here for the script to execute");
            topCorner.ColumnWidth = 15;
            topCorner.Interior.Pattern = XlPattern.xlPatternSolid;
            topCorner.Interior.PatternColorIndex = XlPattern.xlPatternAutomatic;
            topCorner.Interior.Color = 49407;
            topCorner.Interior.TintAndShade = 0;
            topCorner.Interior.PatternTintAndShade = 0;

            object lengthRaw = scriptExecutor.ExecuteScript("tmpScript.arguments.length");
            int argListLength = Convert.ToInt32(lengthRaw);
            for (i = 0; i < argListLength; i++)
            {
                Range cell = nsheet.Range["A1"].Offset[0, i + 1];
                object argNameRaw = scriptExecutor.ExecuteScript("tmpScript.arguments.getItem(" + i + ").name");
                string argName = (string)argNameRaw;
                cell.FormulaR1C1 = argName;
                string argDescription = (string)scriptExecutor.ExecuteScript("tmpScript.arguments.getItem(" + i + ").description");
                if (!string.IsNullOrWhiteSpace(argDescription))
                {
                    cell.AddComment(argDescription);
                }

                cell.ColumnWidth = 21;
                cell.Interior.Pattern = XlPattern.xlPatternSolid;
                cell.Interior.PatternColorIndex = XlPattern.xlPatternAutomatic;
                cell.Interior.ThemeColor = XlThemeColor.xlThemeColorAccent1;
                cell.Interior.TintAndShade = -0.249977111117893;
                cell.Interior.PatternTintAndShade = 0;

                cell.Font.ThemeColor = XlThemeColor.xlThemeColorDark1;
                cell.Font.TintAndShade = -4.99893185216834E-02;
                //see about a controlled vocabulary
                string vocabularyName = GetVocabName(scriptExecutor, i);
                List<VocabItem> vocabItems = GetVocab(vocabularyName);
                if (vocabItems.Count > 0)
                {
                    Range vocabCell = cell.Offset[1, 0];
                    log.DebugFormat("Will add {0} total vocabulary items to {1}", vocabItems.Count,
                        vocabCell.Address);
                    vocabCell.Validation.Delete();
                    //string vocabString = GetVocabDisplayString(vocabItems);
                    //the string contains a reference to a range of cells in a hidden sheet
                    // that contain the allowed values.
                    string vocabString = CreateVocabularyList(workbook, vocabularyName, 
                        vocabItems.Select(v=>v.Display).ToList());
                    log.Debug("using vocabString: " + vocabString);
                    try
                    {
                        vocabCell.Validation.Add(XlDVType.xlValidateList, 
                            XlDVAlertStyle.xlValidAlertStop,
                            XlFormatConditionOperator.xlEqual, vocabString);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                    vocabCell.Validation.IgnoreBlank = true;
                    vocabCell.Validation.InCellDropdown = true;
                    vocabCell.Validation.InputTitle = "";
                    vocabCell.Validation.ErrorMessage = "Please select one of the values listed and preserve text case!";
                    vocabCell.Validation.ShowError = true;
                    vocabCell.Validation.ShowInput = true;
                }
            }

            //nsheet.Range("A1").Offset(1, i + 1).FormulaR1C1 = WebBrowser1.Document.script.tmpScript.arguments.getItem(i).getValue("")
            topCorner.Offset[0, argListLength + 1].FormulaR1C1 = "IMPORT STATUS";
            topCorner.Offset[0, argListLength + 2].FormulaR1C1 = "FORCED";

            workbook.Application.ActiveWindow.SplitColumn = 0;
            workbook.Application.ActiveWindow.SplitRow = 1;
            workbook.Application.ActiveWindow.FreezePanes = true;
            nsheet.Activate();
        }

        public string GetNewSheetName(Workbook workbook, string suggest)
        {
            string nsuggest = suggest;
            for (int i = 2; i < 1000; i++)
            {
                if (DoesSheetExist(workbook, nsuggest))
                {
                    nsuggest = suggest + " " + i;
                }
                else
                {
                    return nsuggest;
                }
            }
            return string.Empty;
        }

        private string GetVocabName(IScriptExecutor scriptExecutor,
            int itemNumber)
        {
            object argTypeRaw = scriptExecutor.ExecuteScript("tmpScript.arguments.getItem("
                + itemNumber + ").type");
            log.DebugFormat("GetVocab looking at argTypeRaw {0} for arg {1}",
                argTypeRaw, itemNumber);
            if (argTypeRaw != null && argTypeRaw is string && (argTypeRaw as string).Equals("cv",
                StringComparison.CurrentCultureIgnoreCase))
            {
                object cvTypeRaw = scriptExecutor.ExecuteScript("tmpScript.arguments.getItem("
                    + itemNumber + ").cvType");
                if (cvTypeRaw != null && cvTypeRaw is string)
                {
                    string cvType = cvTypeRaw as string;

                    if (!string.IsNullOrWhiteSpace(cvType))
                    {
                        return cvType;
                    }
                }
            }
            return string.Empty;
        }
        private List<VocabItem> GetVocab(string cvType)
        {
            if (!string.IsNullOrWhiteSpace(cvType))
            {
                return VocabUtils.GetVocabularyItems(Configuration.SelectedServer.ServerUrl,
                    cvType);
            }
            return new List<VocabItem>();
        }

        public static Worksheet GetVocabularySheet(Workbook workbook)
        {
            Worksheet vocabSheet = null;
            foreach (Worksheet sheet in workbook.Sheets)
            {
                if (sheet.Name.Equals(VOCABULARY_SHEET_NAME,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    vocabSheet = sheet;
                    break;
                }
            }
            if (vocabSheet == null)
            {
                vocabSheet = (Worksheet)workbook.Sheets.Add();
                vocabSheet.Name = VOCABULARY_SHEET_NAME;
                vocabSheet.Visible = XlSheetVisibility.xlSheetHidden;
            }
            return vocabSheet;
        }

        public static string CreateVocabularyList(Workbook workbook, string vocabularyName,
            List<string> vocabularyItems)
        {
            Worksheet vocabSheet = GetVocabularySheet(workbook);
            int column = FindColumn(vocabSheet.Range["A1", "ZZ1"], vocabularyName, VOCABULARY_TEST_ROW);
            log.DebugFormat("CreateVocabularyList found column {0} for vocab {1}",
                column, vocabularyName);
            if (column == 0) column = GetFirstEmptyColumn(vocabSheet, VOCABULARY_TEST_ROW);
            //Store the name of the vocabulary on the first row
            string headerCellLabel = GetColumnName(column) + VOCABULARY_TEST_ROW;
            Range headerCell = vocabSheet.Range[headerCellLabel];//.Offset[(VOCABULARY_TEST_ROW - 1), (column - 1)];
            headerCell.FormulaR1C1 = vocabularyName;
            for (int item = 0; item < vocabularyItems.Count; item++)
            {
                vocabSheet.Range["A1"].Offset[(item + 1), (column - 1)].FormulaR1C1 = 
                    vocabularyItems[item];
            }
            StringBuilder vocabRefStringBuilder = new StringBuilder();
            vocabRefStringBuilder.Append("=");
            vocabRefStringBuilder.Append(VOCABULARY_SHEET_NAME);
            vocabRefStringBuilder.Append("!$");
            vocabRefStringBuilder.Append(GetColumnName(column));
            vocabRefStringBuilder.Append("$2:$");
            vocabRefStringBuilder.Append(GetColumnName(column));
            vocabRefStringBuilder.Append("$");
            vocabRefStringBuilder.Append((vocabularyItems.Count + 1));
            string vocabularyReference = vocabRefStringBuilder.ToString();
            //string vocabularyReference = VOCABULARY_SHEET_NAME + "!$" + GetColumnName(column) + "$2:$"
            //    + GetColumnName(column) + (vocabularyItems.Count + 1);
            log.DebugFormat(" about to return {0}", vocabularyReference);
            return vocabularyReference;
        }

        public static int GetFirstEmptyColumn(Worksheet worksheet, int row)
        {
            Range testRange = null;
            int column = 1;
            while (column < MAX_COLUMNS)
            {
                testRange = (Range)worksheet.Range["A1"].Offset[(row - 1), (column - 1)];
                if (testRange.FormulaR1C1 == null ||
                    (testRange.FormulaR1C1 is string
                    && string.IsNullOrEmpty(testRange.FormulaR1C1 as string)))
                {
                    return column;
                }
                column++;
            }

            return 0;
        }


        private string GetVocabDisplayString(List<VocabItem> vocabItems)
        {
            return string.Join(",", vocabItems.Where(v => !v.Deprecated).Select(vi => vi.Display).ToArray());
        }
    }
}

