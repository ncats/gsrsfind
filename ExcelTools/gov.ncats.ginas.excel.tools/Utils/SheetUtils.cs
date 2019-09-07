using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
using gov.ncats.ginas.excel.tools.Providers;
using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class SheetUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string VOCABULARY_SHEET_NAME = "_gsrs_vocabularies_";
        private static readonly int MAX_COLUMNS = 16000;
        private static readonly int VOCABULARY_TEST_ROW = 1;
        private static readonly int COLUMN_WIDTH_APP_SHEET = 20;

        public GinasToolsConfiguration Configuration
        {
            get;
            set;
        }

        public IScriptExecutor ScriptExecutor
        {
            get;
            set;
        }

        public ImageOps ImageOpsHandle
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
            foreach( Range currentSubRange in rangeToSearch.Areas)
            {
                for (int row = 0; row < currentSubRange.Rows.Count; row++)
                {
                    int currentRow = currentSubRange.Row + row;
                    string cellName = GetColumnName(columnToSearch) + currentRow;
                    object value = currentSubRange.Worksheet.Range[cellName].Value;
                    if (value is string)
                    {
                        string cellValue = (string)value;
                        if (cellValue.Equals(textToFind)) return currentRow;
                    }
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
                string vocabularyName = scriptUtils.GetVocabName(i);
                log.DebugFormat("in {0}, got vocabularyName: {1}", MethodBase.GetCurrentMethod().Name,
                        vocabularyName);

                AddVocabulary(workbook, scriptUtils, scriptExecutor, sortAlpha, vocabularyName,
                    numberOfRows, cell, 1);
            }

            topCorner.Offset[0, argListLength + 1].FormulaR1C1 = "IMPORT STATUS";
            topCorner.Offset[0, argListLength + 1].ColumnWidth = 21;

            workbook.Application.ActiveWindow.SplitColumn = 0;
            workbook.Application.ActiveWindow.SplitRow = 1;
            workbook.Application.ActiveWindow.FreezePanes = true;
            nsheet.Activate();
        }

        public Worksheet CreateSheet(Workbook workbook, string requestedSheetName,
            List<SheetSectionInfo> sheetInfoList)
        {
            Worksheet newSheet = (Worksheet)workbook.Application.Sheets.Add();

            string sheetName = GetNewSheetName(workbook, requestedSheetName);
            newSheet.Name = sheetName;
            int row = 1;
            foreach (SheetSectionInfo info in sheetInfoList)
            {
                AddLine(newSheet, row, info.FieldNames, info.Direction);
                row = row + 3;
            }
            workbook.Application.ActiveWindow.SplitColumn = 0;
            workbook.Application.ActiveWindow.SplitRow = 1;
            workbook.Application.ActiveWindow.FreezePanes = true;
            newSheet.Activate();
            return newSheet;
        }

        private void AddLine(Worksheet sheet, int row, List<string> values, string lastValue)
        {
            string startingCellAddress = "A" + row;
            int columnOffset = 0;
            foreach (string value in values)
            {
                Range cell = sheet.Range[startingCellAddress].Offset[0, columnOffset];
                cell.FormulaR1C1 = value;
                cell.ColumnWidth = COLUMN_WIDTH_APP_SHEET;
                cell.Interior.Pattern = XlPattern.xlPatternSolid;
                cell.Interior.PatternColorIndex = XlPattern.xlPatternAutomatic;
                cell.Interior.TintAndShade = -0.249977111117893;
                cell.Interior.PatternTintAndShade = 0;

                cell.Font.ThemeColor = XlThemeColor.xlThemeColorAccent1;
                cell.Font.TintAndShade = -0.499984740745262;
                columnOffset = columnOffset + 2;
            }
            Range lastCell = sheet.Range[startingCellAddress].Offset[0, columnOffset];
            lastCell.FormulaR1C1 = lastValue;
            lastCell.ColumnWidth = COLUMN_WIDTH_APP_SHEET;
            lastCell.Font.Italic = true;
            lastCell.Interior.TintAndShade = 0.599993896298105;
        }

        public void AddVocabulary(Workbook workbook, ScriptUtils scriptUtils,
                IScriptExecutor scriptExecutor, bool sortAlpha, string vocabularyName,
                int numberOfRows, Range cell, int startRow = 1)
        {
            List<VocabItem> vocabItems = scriptUtils.GetVocabItems(vocabularyName);
            if (vocabItems.Count > 0)
            {
                for (int row = startRow; row <= numberOfRows; row++)
                {
                    log.DebugFormat("Looking at offset row {0} from cell {1}", row, cell.Address);
                    Range vocabCell = cell.Offset[row, 0];
                    log.DebugFormat("Will add {0} total vocabulary items to {1} on row {2}", vocabItems.Count,
                        vocabCell.Address, row);
                    AddVocabToCell(workbook, vocabCell, vocabItems, vocabularyName, sortAlpha);
                }
            }
        }

        public void AddVocabularySingle(Workbook workbook, ScriptUtils scriptUtils,
            IScriptExecutor scriptExecutor, bool sortAlpha, string vocabularyName,
            Range cell)
        {
            List<VocabItem> vocabItems = scriptUtils.GetVocabItems(vocabularyName);
            if (vocabItems.Count > 0)
            {
                AddVocabToCell(workbook, cell, vocabItems, vocabularyName, sortAlpha);
            }
        }


        private void AddVocabToCell(Workbook workbook, Range vocabCell, List<VocabItem> vocabItems,
            string vocabularyName, bool sortAlpha)
        {
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
            if (sortAlpha)
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
                    if (Configuration.SelectedServer.LooksLikeSingleSignon()
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

        public TwoRangeWrapper TransferSDDataToRow(Dictionary<string, string> data, Dictionary<string, int> columns,
            int dataRow, Worksheet worksheet)
        {
            log.DebugFormat("starting in {0}",
                MethodBase.GetCurrentMethod().Name);
            TwoRangeWrapper rangeWrapper = null;

            log.DebugFormat("In {0}, columns.Keys.Count: {1}", MethodBase.GetCurrentMethod().Name,
                columns.Keys.Count);
            foreach (string fieldName in data.Keys)
            {
                int column = columns[fieldName];
                string cellId = GetColumnName(column) + dataRow;
                Range currentCell = worksheet.Range[cellId];

                currentCell.FormulaR1C1 = data[fieldName];
                if (fieldName.Equals(SDFileProcessor.MOLFILE_FIELD_NAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    string cellForStructureIdName = GetColumnName(columns.Keys.Count) + dataRow;
                    log.DebugFormat("Using cellForStructureIdName: {0}", cellForStructureIdName);
                    Range cellForStructureID = worksheet.Range[cellForStructureIdName];
                    rangeWrapper = RangeWrapperFactory.CreateTwoRangeWrapper(currentCell, cellForStructureID);
                }
            }
            return rangeWrapper;
        }

        public void SetColumnWidths(Worksheet sheet, List<int> columns, int width)
        {
            foreach (int column in columns)
            {
                string cellID = GetColumnName(column) + "1";
                Range cell = sheet.Range[cellID];
                cell.ColumnWidth = width;
            }
        }

        public void SetRowHeights(Worksheet sheet, int height)
        {
            foreach (Range row in sheet.UsedRange.Rows)
            {
                row.RowHeight = height;
            }
        }

        public static void SetupPTColumn(Range activeRange)
        {
            int column = activeRange.Column;
            string columnName = GetColumnName(column);
            string message = "Mark Column " + columnName + "(" + column + ") as the Preferred Term ?";

            if (UIUtils.GetUserYesNoCancel(message, "Yes=Continue; No,Cancel=forget about it")
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

        public void SetupRemainingColumns(Worksheet worksheet,
            IScriptExecutor scriptExecutor, ScriptUtils scriptUtils = null)
        {
            List<string> columnHeaders = GetColumnHeaders(worksheet);
            int numRows = worksheet.UsedRange.Rows.Count - 1;

            string[] requiredParms = {  "PT LANGUAGE", "PT NAME TYPE", "SUBSTANCE CLASS",
                "REFERENCE TYPE", "REFERENCE CITATION", "REFERENCE URL", "IMPORT STATUS"};
            if (scriptUtils == null)
            {
                scriptUtils = new ScriptUtils();
            }
            ScriptExecutor.ExecuteScript("tmpScript=Scripts.get('" + SDFileProcessor.SD_LOADING_SCRIPT_NAME
                   + "');");
            object lengthRaw = ScriptExecutor.ExecuteScript("tmpScript.arguments.length");
            int argListLength = Convert.ToInt32(lengthRaw);
            for (int i = 0; i < argListLength; i++)
            {
                object argNameRaw = scriptExecutor.ExecuteScript("tmpScript.arguments.getItem(" + i + ").name");
                string argName = (string)argNameRaw;
                if (requiredParms.Contains(argName) && !columnHeaders.Contains(argName))
                {
                    Range lastCol = SetOneHeader(worksheet, argName);
                    if (argName.Equals("SUBSTANCE CLASS"))
                    {
                        for (int row = 2; row <= numRows + 1; row++)
                        {
                            string rangeName = GetColumnName(lastCol.Column + 1) + row;
                            Range range = worksheet.Range[rangeName];
                            range.FormulaR1C1 = SDFileProcessor.DEFAULT_SUBSTANCE_TYPE;
                        }
                    }

                    string vocabularyName = scriptUtils.GetVocabName(i);
                    log.DebugFormat("in {0}, got vocabularyName: {1}", MethodBase.GetCurrentMethod().Name,
                        vocabularyName);
                    Range headerItem = worksheet.Range[GetColumnName(lastCol.Column + 1) + "1"];
                    AddVocabulary((Workbook)worksheet.Parent, scriptUtils,
                        scriptExecutor, true, vocabularyName,
                        numRows, headerItem, 0);
                }
            }
            SetOneHeader(worksheet, "IMPORT STATUS");

            List<string> messages = new List<string>();
            if (!columnHeaders.Contains("PT"))
            {
                messages.Add("Your sheet now has most of the required columns for creating a new substance,");
                messages.Add("but you must add or designate a 'PT' column!");
            }
            else
            {
                messages.Add("Your sheet now has the required columns for creating a new substance.");
            }
            messages.Add("Please fill in any data values and use 'Load data' to complete the process");
            UIUtils.ShowMessageToUser(string.Join("\n", messages));
        }

        private Range SetOneHeader(Worksheet worksheet, string headerText)
        {
            Range lastCol = (Range)worksheet.UsedRange.Columns[worksheet.UsedRange.Columns.Count];
            string newRangeAddress = GetColumnName(lastCol.Column + 1) + "1";
            Range headerItem = worksheet.Range[newRangeAddress];
            headerItem.FormulaR1C1 = headerText;
            FormatCellForParameter(headerItem);
            log.DebugFormat("Setting header {0} to {1}", newRangeAddress, headerText);
            return lastCol;
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
            if (numberOfRowsObj != null)
            {
                try
                {
                    int numberOfRows = Convert.ToInt32(numberOfRowsObj);
                    return numberOfRows;
                }
                catch (FormatException)
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

        public static async Task CheckSDSheetForDuplicates(Worksheet worksheet, List<string> messages, string serverUrl)
        {
            string molfileFieldName = "Molfile";
            string importStatusFieldName = "Import Status";
            string UniquenessFieldName = "Uniqueness";

            Range firstRow = (Range)worksheet.Rows[1];
            int molfileColumn = 0;
            int statusColumn = 0;
            int uniquenessColumn = 0;
            firstRow = worksheet.Application.Intersect(firstRow, worksheet.UsedRange);
            foreach (Range cell in firstRow.Cells)
            {
                if (cell.Value2 != null && cell.Value2.Equals(molfileFieldName))
                {
                    molfileColumn = cell.Column;
                }
                else if (cell.Value2 != null && cell.Value2.ToString().Equals(importStatusFieldName, StringComparison.CurrentCultureIgnoreCase))
                {
                    statusColumn = cell.Column;
                }
                else if (cell.Value2 != null && cell.Value2.ToString().Equals(UniquenessFieldName, StringComparison.CurrentCultureIgnoreCase))
                {
                    uniquenessColumn = cell.Column;
                }
                if (molfileColumn > 0 && statusColumn > 0) break;
            }
            if (molfileColumn == 0)
            {
                messages.Add("No molfile column located");
                return;
            }
            //temp hack:
            if (statusColumn == 0)
            {
                statusColumn = 20;
            }
            Range molfileColumnRange = (Range)worksheet.Columns[molfileColumn];
            Range fullMolfileRange = molfileColumnRange.EntireColumn;
            fullMolfileRange = worksheet.Application.Intersect(fullMolfileRange, worksheet.UsedRange);
            foreach (Range cell in fullMolfileRange)
            {
                if (cell.Value2 != null && !cell.Value2.Equals("Molfile"))
                {
                    string cellIdUniqueness = GetColumnName(uniquenessColumn) + cell.Row;
                    Range uniquenessCell = worksheet.Range[cellIdUniqueness];
                    //MAM 6 November
                    //string structureId = await RestUtils.SaveMolfileAndDisplay(cell.Value2.ToString(), cell, serverUrl, uniquenessCell);
                    //Task<StructureQueryResult> results = RestUtils.SearchMolfile(structureId, serverUrl);
                    //string message = "";
                    //if (results.Result.Content.Length == 0) message = "Unique";
                    //else message = "At least one duplicate: " + results.Result.Content[0].PrimaryTerm;

                    //worksheet.Range[cell.Row, statusColumn].FormulaR1C1 = message;
                }
            }

        }

        /// <summary>
        /// return true when sheet contains data within the columns to the right of the specified range
        /// </summary>
        /// <param name="toCheck"></param>
        /// <param name="numColumnsToBeWritten"></param>
        /// <returns></returns>
        public static bool ContainsDataInColumnsToBeWritten(Range selectedRange, int numColumnsToBeWritten)
        {
            foreach (Range startCell in selectedRange)
            {
                for (int c = 1; c <= numColumnsToBeWritten; c++)
                {
                    Range cell = startCell.Offset[0, c];
                    if (cell.Value2 != null && !string.IsNullOrWhiteSpace(cell.Value2.ToString()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Range FindFirstCellWithText(Range rangeToSearch, string textToFind)
        {
            foreach (Range cell in rangeToSearch.Cells)
            {
                if (cell.FormulaR1C1Local != null && cell.FormulaR1C1Local is string)
                {
                    string testData = (cell.FormulaR1C1Local as string).Trim();
                    if (testData.Equals(textToFind, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return cell;
                    }
                }
            }
            return null;
        }

        public static bool IsRowBlank(Range row)
        {
            Range cellsToExamine = row.Application.Intersect(row.Worksheet.UsedRange, row);
            if (cellsToExamine == null) return true;

            foreach (Range cell in cellsToExamine)
            {
                if (cell.Value2 != null && !string.IsNullOrWhiteSpace(cell.Value2.ToString()))
                {
                    return false;
                }
            }
            return true;
        }

        public static object GetSheetPropertyValue(Worksheet sheet, string propertyName)
        {
            for (int p = 1; p <= sheet.CustomProperties.Count; p++)
            {
                if (sheet.CustomProperties.Item[p].Name.Equals(propertyName))
                {
                    return sheet.CustomProperties.Item[p].Value;
                }
            }
            return null;
        }

        public static void SetSheetPropertyValue(Worksheet sheet, string propertyName, object propertyValue)
        {
            for (int p = 1; p <= sheet.CustomProperties.Count; p++)
            {
                if (sheet.CustomProperties.Item[p].Name.Equals(propertyName))
                {
                    sheet.CustomProperties.Item[p].Value = propertyValue;
                    return;
                }
            }
            sheet.CustomProperties.Add(propertyName, propertyValue);
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
