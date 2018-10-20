using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using gov.ncats.ginas.excel.tools.Model;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class SheetUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string VOCABULARY_SHEET_NAME = "_gsrs_vocabularies_";
        private static readonly int MAX_COLUMNS = 16000;
        private static readonly int VOCABULARY_TEST_ROW = 1;

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

        public void CreateSheet(Workbook workbook, ScriptUtils scriptUtils,
            IScriptExecutor scriptExecutor, bool sortAlpha)
        {
            string scriptName = scriptUtils.ScriptName;
            if (DoesSheetExist(workbook, scriptName))
            {
                UIUtils.ShowMessageToUser("Sheet \"" + scriptName + "\" already exists");
                (workbook.Sheets[scriptName] as _Worksheet).Activate();
                return;
            }

            scriptExecutor.ExecuteScript("tmpScript=Scripts.get('" + scriptName + "');");
            scriptExecutor.ExecuteScript("tmpRunner=tmpScript.runner();");
            int numberOfRows = GetNumberOfRows(scriptExecutor);
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
                string vocabularyName = scriptUtils.GetVocabName( i);
                List<VocabItem> vocabItems = scriptUtils.GetVocabItems(vocabularyName);
                if (vocabItems.Count > 0)
                {
                    for (int row = 1; row <= numberOfRows; row++)
                    {
                        Range vocabCell = cell.Offset[row, 0];
                        log.DebugFormat("Will add {0} total vocabulary items to {1} on row {2}", vocabItems.Count,
                            vocabCell.Address, row);
                        vocabCell.Validation.Delete();
                        //the string contains a reference to a range of cells in a hidden sheet
                        // that contain the allowed values.
                        string vocabString = CreateVocabularyList(workbook, vocabularyName,
                            vocabItems.Select(v => v.Display).ToList(), sortAlpha);
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
            }

            topCorner.Offset[0, argListLength + 1].FormulaR1C1 = "IMPORT STATUS";
            topCorner.Offset[0, argListLength + 1].ColumnWidth = 21;

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

        private List<VocabItem> GetVocab(string cvType)
        {
            log.DebugFormat("In GetVocab with cvType: {0}", cvType);
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
            List<string> vocabularyItems, bool sortAlpha)
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
            if( sortAlpha)
            {
                vocabularyItems.Sort(StringComparer.CurrentCultureIgnoreCase);
            }
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


        public string TransferDataToRow(string[] data, int currentColumn, int dataRow,
            ImageOps imageOps, Worksheet worksheet, int firstPart = 1)
        {
            string imageFormat = Properties.Resources.ImageFormat;

            for (int part = firstPart; part < data.Length; part++)
            {
                int column = currentColumn + part;
                string cellId = GetColumnName(column) + dataRow;
                string result = data[part];
                if (string.IsNullOrWhiteSpace(result) || result.Equals("[object Object]")) continue;
                
                if (ImageOps.IsImageUrl(result))
                {
                    if( Configuration.SelectedServer.LooksLikeSingleSignon()
                        || ImageOps.RemoteFileExists(result))
                    {
                        log.Debug("(image)");
                        cellId = GetColumnName(column - 1) + dataRow;
                        Range currentCell = worksheet.Range[cellId];
                        ImageOps.AddImageCaption(currentCell, result, 240);
                    }
                    else
                    {
                        return "Invalid Image URL";
                    }
                }
                else
                {
                    Range currentCell = worksheet.Range[cellId];
                    currentCell.Value = result;
                }
            }
            return string.Empty;
        }

        public string TransferSDDataToRow(Dictionary<string,string> data, Dictionary<string, int> columns, 
            int dataRow,
            ImageOps imageOps, Worksheet worksheet)
        {
            log.DebugFormat("In {0}, columns.Keys.Count: {1}", MethodBase.GetCurrentMethod().Name,
                columns.Keys.Count);
            foreach(string fieldName in data.Keys)
            {
                int column = columns[fieldName];
                string cellId = GetColumnName(column) + dataRow;
                Range currentCell = worksheet.Range[cellId];
                
                currentCell.FormulaR1C1 = data[fieldName];
                if( fieldName.Equals(SDFileUtils.MOLFILE_FIELD_NAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    string cellForStructureIdName = GetColumnName(columns.Keys.Count) + dataRow;
                    log.DebugFormat("Using cellForStructureIdName: {0}", cellForStructureIdName);
                    Range cellForStructureID = worksheet.Range[cellForStructureIdName];
                    imageOps.CreateMolfileImage(currentCell, data[fieldName], cellForStructureID);
                }
            }
            return string.Empty;
        }

        public void SetColumnWidths(Worksheet sheet, List<int> columns, int width)
        {
            foreach(int column in columns)
            {
                string cellID = GetColumnName(column) + "1";
                Range cell = sheet.Range[cellID];
                cell.ColumnWidth = width;
            }
        }

        public void SetRowHeights(Worksheet sheet, int height)
        {
            foreach(Range row in sheet.UsedRange.Rows)
            {
                row.RowHeight = height;
            }
        }

        public static void SetupPTColumn(Range activeRange)
        {
            int column = activeRange.Column;
            string columnName = GetColumnName(column);
            string message = "Mark Column " + columnName + "(" + column + ") as the Preferred Term ?";

            if ( UIUtils.GetUserYesNoCancel( message, "Yes=Continue; No,Cancel=forget about it")
                == DialogYesNoCancel.Yes)
            {
                string selectionRangeAddress = GetColumnName(column + 2) + "1";
                Range newSelectionRange = activeRange.Worksheet.Range[selectionRangeAddress];
                newSelectionRange.Select();
                string newRangeAddress = GetColumnName(column) + "1";
                Range ptLangHeader = activeRange.Worksheet.Range[newRangeAddress];
                ptLangHeader.FormulaR1C1 = "PT";
                FormatCellForParameter(ptLangHeader);

           

            }
        }

        public static void SetupRemainingColumns(Worksheet worksheet)
        {
            List<string> columnHeaders = GetColumnHeaders(worksheet);
            string[] requiredParms = {  "PT LANGUAGE", "PT NAME TYPE", "SUBSTANCE CLASS",
                "REFERENCE TYPE", "REFERENCE CITATION", "REFERENCE URL", "FORCED", "IMPORT STATUS"};
            foreach (string parmName in requiredParms)
            {
                if (!columnHeaders.Contains(parmName))
                {
                    Range lastCol = (Range) worksheet.UsedRange.Columns[worksheet.UsedRange.Columns.Count];
                    string newRangeAddress = GetColumnName(lastCol.Column+1 ) + "1";
                    Range headerItem = worksheet.Range[newRangeAddress];
                    headerItem.FormulaR1C1 = parmName;
                    FormatCellForParameter(headerItem);
                    log.DebugFormat("Setting header {0} to {1}", newRangeAddress, parmName);
                }
            }
            List<string> messages = new List<string>();
            messages.Add("Your sheet now has the required columns for creating a new substance.");
            messages.Add("Please fill in any values and use 'Load data' to complete the process");

            if( !columnHeaders.Contains("PT"))
            {
                messages.Add("Note: you must also add or designate a 'PT' column!");
            }
            UIUtils.ShowMessageToUser(string.Join("\n", messages));
        }

        public static bool IsSheetBlank(Worksheet sheet)
        {
            return sheet.Application.WorksheetFunction.CountA(sheet.UsedRange) == 0;
       }

        private static List<string> GetColumnHeaders(Worksheet worksheet)
        {
            List<string> colHeaders = new List<string>();
            foreach (Range col in worksheet.UsedRange.Columns)
            {
                string colheaderAddress = GetColumnName(col.Column) + "1";

                Range colHeader = worksheet.Range[colheaderAddress];
                log.DebugFormat("Looking at cell {0} with value {1}", colheaderAddress,
                     colHeader.FormulaR1C1);
                colHeaders.Add(colHeader.FormulaR1C1.ToString());
            }
            return colHeaders;
        }

        private string GetVocabDisplayString(List<VocabItem> vocabItems)
        {
            return string.Join(",", vocabItems.Where(v => !v.Deprecated).Select(vi => vi.Display).ToArray());
        }

        private int GetNumberOfRows(IScriptExecutor scriptExecutor)
        {
            object numberOfRowsObj = scriptExecutor.ExecuteScript("$('#numberOfRows').val()");
            log.DebugFormat("numberOfRowsObj : {0}", numberOfRowsObj);
            if ( numberOfRowsObj != null )
            {
                try
                {
                    int numberOfRows = Convert.ToInt32(numberOfRowsObj);
                    return numberOfRows;
                }
                catch(FormatException)
                {
                    log.WarnFormat("Error parsing number from {0}", numberOfRowsObj);
                }
                catch (OverflowException)
                {
                    log.WarnFormat("Overflow error parsing number from {0}", numberOfRowsObj);
                }
            }
            return 1;
        }

        public static void CheckSDSheetForDuplicates(Worksheet worksheet, List<string> messages, string serverUrl)
        {
            string molfileFieldName = "Molfile";
            string importStatusFieldName = "Import Status";
            Range firstRow = (Range)worksheet.Rows[1];
            int molfileColumn = 0;
            int statusColumn = 0;
            foreach (Range cell in firstRow.Cells)
            {
                if( cell.Value2 != null && cell.Value2.Equals(molfileFieldName))
                {
                    molfileColumn = cell.Column;
                }
                else if(cell.Value2 != null && cell.Value2.Equals(importStatusFieldName))
                {
                    statusColumn = cell.Column;
                }
                if (molfileColumn > 0 && statusColumn > 0) break;
            }
            if( molfileColumn == 0)
            {
                messages.Add("No molfile column located");
                return;
            }
            //temp hack:
            if( statusColumn ==0)
            {
                statusColumn = 20;
            }
            Range molfileColumnRange = (Range) worksheet.Columns[0, molfileColumn];
            Range fullMolfileRange = molfileColumnRange.EntireColumn;
            fullMolfileRange = worksheet.Application.Intersect(fullMolfileRange, worksheet.UsedRange);
            foreach(Range cell in fullMolfileRange)
            {
                if( cell.Value2 != null )
                {
                    Task<StructureQueryResult> results = RestUtils.SearchMolfile((string) cell.Value2, serverUrl);
                    string message = "";
                    if (results.Result.Content.Length == 0) message = "Unique";
                    else message = "At least one duplicate: " + results.Result.Content[0].PrimaryTerm;

                    worksheet.Range[cell.Row, statusColumn].FormulaR1C1 = message;
                }
            }
            
        }

        private static void FormatCellForParameter(Range cell)
        {
            cell.ColumnWidth = 21;
            cell.Interior.Pattern = XlPattern.xlPatternSolid;
            cell.Interior.PatternColorIndex = XlPattern.xlPatternAutomatic;
            cell.Interior.ThemeColor = XlThemeColor.xlThemeColorAccent1;
            cell.Interior.TintAndShade = -0.249977111117893;
            cell.Interior.PatternTintAndShade = 0;
            cell.Font.ThemeColor = XlThemeColor.xlThemeColorDark1;
            cell.Font.TintAndShade = -4.99893185216834E-02;

        }

    }
}
