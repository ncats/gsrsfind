using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;

using Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Model.FDAApplication;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Model.Callbacks;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public enum SheetType
    {
        Unknown = 0,
        Application =1,
        AddIngredients=2
    }

    public class ApplicationSheetCreator : ControllerBase, IController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private SheetUtils _sheetUtils = new SheetUtils();
        private Worksheet _worksheet;
        internal static string SHEET_PROPERTY = "Application Sheet Designation";
        internal static string SHEET_PROPERTY_VALUE = "Application Sheet";
        private Dictionary<string, string> _lookups = new Dictionary<string, string>();
        static List<ApplicationField> _ingredientFieldsToMonitor = null;
        private SheetType _sheetType = SheetType.Unknown;

        static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); // Add an Application Setting.

        private const string MESSAGE_MULTIPLE_RESULTS = "matched multiple records";

        static ApplicationSheetCreator()
        {
            if (config.AppSettings.Settings["handleProductApplications"].Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                _ingredientFieldsToMonitor = ApplicationMetadata.Metadata.Where(f => f.FieldLevel == ApplicationField.Level.Ingredient
                    && !string.IsNullOrEmpty(f.HandleChange)).ToList();
            }

        }

        public void CreateApplicationSheet()
        {
            _sheetType = SheetType.Application;
            List<SheetSectionInfo> sheetSections = new List<SheetSectionInfo>();
            SheetSectionInfo firstSheetInfo = new SheetSectionInfo();

            firstSheetInfo.FieldNames.AddRange(ApplicationMetadata.GetFieldNames(ApplicationField.Level.Application));
            firstSheetInfo.Direction = "[One per Application]";
            sheetSections.Add(firstSheetInfo);
            SheetSectionInfo secondSheetInfo = new SheetSectionInfo();
            secondSheetInfo.FieldNames.AddRange(ApplicationMetadata.GetFieldNames(ApplicationField.Level.Product));
            IEnumerable<string> prodNameFieldNames = ApplicationMetadata.GetFieldNames(ApplicationField.Level.ProductName);

            secondSheetInfo.FieldNames.AddRange(prodNameFieldNames.Where(fn => !secondSheetInfo.FieldNames.Contains(fn)));
            secondSheetInfo.Direction = "[One per Application for now]";
            sheetSections.Add(secondSheetInfo);
            SheetSectionInfo thirdSheetInfo = new SheetSectionInfo();
            thirdSheetInfo.FieldNames.AddRange(ApplicationMetadata.GetFieldNames(ApplicationField.Level.Ingredient));
            thirdSheetInfo.Direction = "[Multiple per Application]";
            sheetSections.Add(thirdSheetInfo);
            SheetUtils sheetUtils = new SheetUtils();
            _worksheet = sheetUtils.CreateSheet(ExcelWindow.Application.ActiveWorkbook,
                "Create Application", sheetSections);
            MarkSheet(_worksheet);
        }

        public void CreateIngredientSheet()
        {
            _sheetType = SheetType.AddIngredients;
            List<SheetSectionInfo> sheetSections = new List<SheetSectionInfo>();
            SheetSectionInfo firstSheetInfo = new SheetSectionInfo();
            firstSheetInfo.FieldNames.AddRange(ApplicationMetadata.GetFieldNames(ApplicationField.Level.AddIngredient));
            firstSheetInfo.Direction = "[Test]";
            sheetSections.Add(firstSheetInfo);
            SheetUtils sheetUtils = new SheetUtils();
            _worksheet = sheetUtils.CreateSheet(ExcelWindow.Application.ActiveWorkbook,
                "Add Ingredient", sheetSections);
            MarkSheet(_worksheet);
        }


        private void _worksheet_Change(Range Target)
        {
            throw new NotImplementedException();
        }

        public void StartOperation()
        {
            log.Debug("Starting in StartOperation");
        }

        public void ContinueSetup()
        {
            IEnumerable<ApplicationField> fieldsWithVocabs = ApplicationMetadata.GetVocabularyFields();
            scriptUtils.ScriptExecutor = ScriptExecutor;
            scriptUtils.StartVocabularyRetrievals(fieldsWithVocabs.Select(f => f.VocabularyName).ToList());
        }

        public override void CompleteSheet()
        {
            log.Debug("Derived class CompleteSheet");
            foreach (string vocabName in scriptUtils.Vocabularies.Keys)
            {
                //find the corresponding field
                ApplicationField field = (_sheetType == SheetType.AddIngredients)
                    ? ApplicationMetadata.GetFieldForVocab(vocabName, ApplicationField.Level.AddIngredient) 
                    : ApplicationMetadata.GetFieldForVocab(vocabName);
                if( field == null)
                {
                    log.DebugFormat("No field found for vocab {0}", vocabName);
                    continue;
                }    
                log.DebugFormat("Located field {0} for vocab {1} and will now assign vocabulary.", field.FieldName, vocabName);
                Range rangeToSearch = _worksheet.Range["A1"].EntireRow;
                if (field.FieldLevel == ApplicationField.Level.Product || field.FieldLevel == ApplicationField.Level.ProductName)
                {
                    rangeToSearch = _worksheet.Range["A4", "Z6"].EntireRow;
                }
                else if (field.FieldLevel == ApplicationField.Level.Ingredient)
                {
                    rangeToSearch = _worksheet.Range["A7", "Z9"].EntireRow;
                }
                else if (field.FieldLevel == ApplicationField.Level.AddIngredient)
                {
                    rangeToSearch = _worksheet.Range["A1", "Z1"].EntireRow;
                }
                Range header = SheetUtils.FindFirstCellWithText(rangeToSearch, field.FieldName);
                if (header != null)
                {
                    Range fieldCell = header.Offset[1, 0];
                    _sheetUtils.AddVocabularySingle(_worksheet.Application.ActiveWorkbook, scriptUtils, ScriptExecutor, true,
                        vocabName, fieldCell);
                }
                else
                {
                    log.DebugFormat("Field {0} not found", field.FieldName);
                }                    
            }
            log.Debug("completed");

        }

        public bool OkToWrite(int numberOfColumns)
        {
            return true;
        }

        public object HandleResults(string resultsKey, string message)
        {
            log.DebugFormat("{0}.  Key: {1} message: {2}", MethodBase.GetCurrentMethod().Name,
                resultsKey, message);
            Dictionary<string, string[]> returnedValue = JSTools.getDictionaryFromString(message);
            string cellAddress = _lookups[resultsKey];
            cellAddress = cellAddress.Replace("$", "");
            log.DebugFormat("looking for cell {0}", cellAddress);
            Range cell = _worksheet.Range[cellAddress];
            string[] results = returnedValue.Values.First()[0].Split('\t');
            if( results.Length >1 && results.Any(r=>r.Equals(MESSAGE_MULTIPLE_RESULTS, StringComparison.CurrentCultureIgnoreCase)))
            {
                log.Debug("detected multiple match");
                cell.Interior.Color = UpdateCallback.COLOR_ERROR;
                cell.AddComment(MESSAGE_MULTIPLE_RESULTS);
            }    
            else if (results.Length > 1 && results.Any(r => r.Equals(cell.Value2.ToString(), StringComparison.CurrentCultureIgnoreCase)))
            {
                //UIUtils.ShowMessageToUser("The value you entered is confirmed as a valid database name");
                cell.Interior.Color = UpdateCallback.COLOR_STARTING;
                cell.ClearComments();
            }
            else
            {
                //UIUtils.ShowMessageToUser("The value you entered does NOT match a substance in the database");
                cell.Interior.Color = UpdateCallback.COLOR_ERROR;
                cell.AddComment("The value in this cell does NOT match a substance in the database");
            }

            return null;
        }

        public bool StartResolution(bool newSheet)
        {
            log.DebugFormat("{0}", MethodBase.GetCurrentMethod().Name);
            return true;
        }

        public static bool IsApplicationWorksheet(Worksheet worksheet)
        {
            object value = SheetUtils.GetSheetPropertyValue(worksheet, SHEET_PROPERTY);
            if (value != null && value.ToString().Equals(SHEET_PROPERTY_VALUE)) return true;
            return false;
        }

        private static bool IsMonitoredCell(Range cell)
        {
            for (int row = cell.Row - 1; row > 0; row--)
            {
                if (row == 0) return false;
                Range testCell = cell.Offset[-1 * row, 0];
                if (testCell.Value2 == null) continue;
                string testValue = testCell.Value2.ToString();
                if (_ingredientFieldsToMonitor.Any(f => f.FieldName.Equals(testValue, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        public void HandleChange(Range range)
        {
            if (!Globals.ThisAddIn.Listening) return;
            _worksheet = range.Worksheet;
            if (range.Cells.Count == 1 && IsMonitoredCell(range))
            {
                log.DebugFormat("Detected a cell for lookup. Value: {0}", range.Value2);
                if (range.Value2 != null)
                {
                    List<Range> cellsToResolve = new List<Range>();
                    cellsToResolve.Add(range);
                    ResolveIngredients(cellsToResolve);
                }
            }
        }

        public static void MarkSheet(Worksheet sheet)
        {
            SheetUtils.SetSheetPropertyValue(sheet, SHEET_PROPERTY, SHEET_PROPERTY_VALUE);
            sheet.Activate();
        }

        public void ResolveIngredients(List<Range> ingredients)
        {
            log.DebugFormat("Starting in {0}", MethodBase.GetCurrentMethod().Name);
            string[] resolvers = { "Preferred Term" };
            foreach (Range cell in ingredients)
            {
                if (cell.Value2 == null)
                {
                    log.DebugFormat(
                        "Skipping cell {0} because the value is null", cell.Address);
                    continue;
                }
                string address = cell.Address;
                string value = cell.Value2.ToString();

                string identifier = JSTools.RandomIdentifier();
                StringBuilder scriptBuilder = new StringBuilder();
                scriptBuilder.Append("cresults['");
                scriptBuilder.Append(identifier);
                scriptBuilder.Append("'] ={ 'keys':function(){ return _.keys(this); },'Item':function(k){ return this[k]; },");
                scriptBuilder.Append("'add':function(k, v){ if (!this[k]) { this[k] =[]; } this[k].push(v); } }; ResolveWorker.builder().list(");
                scriptBuilder.Append(JSTools.MakeSearchString(new string[] { value }));
                scriptBuilder.Append(").fetchers(");
                scriptBuilder.Append(JSTools.MakeSearchString(resolvers));
                scriptBuilder.Append(").consumer(function(row){ cresults['");
                scriptBuilder.Append(identifier);
                scriptBuilder.Append("'].add(row.split('	')[0], row); }).finisher(function(){ sendMessageBackToCSharp('");
                scriptBuilder.Append(identifier);
                scriptBuilder.Append("'); }).resolve();");
                string script = scriptBuilder.ToString();

                if(ScriptExecutor == null)
                {
                    log.ErrorFormat("Error in {0} ScriptExecutor", MethodBase.GetCurrentMethod().Name);
                }
                ScriptExecutor.SetController(this);
                ScriptExecutor.ExecuteScript(script);

                _lookups.Add(identifier, address);
            }
        }

        private static bool ClearAmountCells(Range cell)
        {
            List<ApplicationField> ingredientAmountFields = ApplicationMetadata.Metadata.Where(
                f => f.FieldLevel == ApplicationField.Level.Ingredient && f.IsAmount).ToList();

            List<Range> cells = new List<Range>();
            for (int row = 0; row < cell.Row; row++)
            {
                Range testCell = cell.Offset[-1 * row, 0];
                if (testCell.Value2 == null) continue;
                log.DebugFormat("cell {0} value {1}", testCell.Address, testCell.Value2);
                string testValue = testCell.Value2.ToString();
                if (ingredientAmountFields.Any(
                    f => f.FieldName.Equals(testValue, StringComparison.CurrentCultureIgnoreCase)))
                {
                    for (int rowToClear = 1; rowToClear <= row; rowToClear++)
                    {
                        testCell.Offset[rowToClear, 0].Value2 = string.Empty;
                    }
                    return true;
                }
            }
            return false;
        }

        public void CopySheet(Worksheet sheet, bool skipAmounts)
        {
            Workbook workbook = sheet.Application.ActiveWorkbook;
            SheetUtils sheetUtils = new SheetUtils();
            int oldIndex = sheet.Index;
            sheet.Select();
            sheet.Copy(sheet);
            //find our new sheet:
            Worksheet newSheet = (Worksheet)workbook.Sheets[oldIndex];
            newSheet.Name = sheetUtils.GetNewSheetName(sheet.Application.ActiveWorkbook,
                sheet.Name + " Product");
            MarkSheet(newSheet);
            int lastRowNum = newSheet.UsedRange.Rows.Count + newSheet.UsedRange.Row - 1;
            log.DebugFormat("lastRow: {0}", lastRowNum);
            Range lastRow = sheet.Application.Intersect(newSheet.Range["A" + lastRowNum].EntireRow,
                newSheet.UsedRange);
            foreach (Range cell in lastRow.Cells)
            {
                ClearAmountCells(cell);
            }
            StatusUpdater.Complete();
        }

        public bool OperationCompleted()
        {
            return false;
        }

    }
}
