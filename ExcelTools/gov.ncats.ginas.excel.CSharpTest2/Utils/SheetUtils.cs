using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.CSharpTest2.Utils
{
    public class SheetUtils
    {
        //from http://stackoverflow.com/questions/10373561/convert-a-number-to-a-letter-in-c-sharp-for-use-in-microsoft-excel
        public static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";
            if (index > letters.Length)
                value += letters[(index-1) / letters.Length - 1];
            value += letters[(index-1) % letters.Length];
            return value;
        }

        public static int FindRow(Range rangeToSearch, string textToFind, int columnToSearch)
        {

            for (int row = 0; row < rangeToSearch.Rows.Count; row++)
            {
                int currentRow = rangeToSearch.Row + row;
                string cellName = GetColumnName(columnToSearch) + currentRow;
                object value = rangeToSearch.Worksheet.Range[cellName].Value;
                if( value is string)
                {
                    string cellValue = (string) value;
                    if (cellValue.Equals(textToFind)) return currentRow;

                }
            }
            return 0;
        }

        public bool DoesSheetExist(Workbook workbook, string sheetName )
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
            Model.IScriptExecutor scriptExecutor)
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

            nsheet= workbook.Sheets.Add();
            nsheet.Name = scriptName;

            Range topCorner = nsheet.Range["A1"];
            topCorner.FormulaR1C1 = "BATCH:" + scriptName;
            topCorner.AddComment( "This column header must be here for the script to execute");
            topCorner.ColumnWidth = 15;
            topCorner.Interior.Pattern = XlPattern.xlPatternSolid;
            topCorner.Interior.PatternColorIndex = XlPattern.xlPatternAutomatic;
            topCorner.Interior.Color = 49407;
            topCorner.Interior.TintAndShade = 0;
            topCorner.Interior.PatternTintAndShade = 0;
            
            object lengthRaw = scriptExecutor.ExecuteScript("tmpScript.arguments.length");
            int argListLength = Convert.ToInt32(lengthRaw);
            for ( i = 0; i < argListLength; i++)
            {
                Range cell = nsheet.Range["A1"].Offset[0, i + 1];
                object argNameRaw = scriptExecutor.ExecuteScript("tmpScript.arguments.getItem(" + i + ").name");
                string argName = (string)argNameRaw;
                cell.FormulaR1C1 = argName;
                string argDescription = (string) scriptExecutor.ExecuteScript("tmpScript.arguments.getItem(" + i + ").description");
                if(!string.IsNullOrWhiteSpace(argDescription))
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
            }

            //nsheet.Range("A1").Offset(1, i + 1).FormulaR1C1 = WebBrowser1.Document.script.tmpScript.arguments.getItem(i).getValue("")
            topCorner.Offset[0, argListLength + 1].FormulaR1C1 = "IMPORT STATUS";
            topCorner.Offset[0, argListLength + 2].FormulaR1C1 = "FORCED";

            workbook.Application.ActiveWindow.SplitColumn = 0;
            workbook.Application.ActiveWindow.SplitRow = 1;
            workbook.Application.ActiveWindow.FreezePanes = true;
            nsheet.Activate();
        }
    }
}
