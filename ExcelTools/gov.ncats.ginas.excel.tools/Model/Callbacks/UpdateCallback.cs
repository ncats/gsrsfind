using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class UpdateCallback : Callback
    {
        private Range statusRange;
        const XlRgbColor COLOR_STARTING = XlRgbColor.rgbGreen;
        const XlRgbColor COLOR_SUCCESS = XlRgbColor.rgbYellow;
        const XlRgbColor COLOR_ERROR = XlRgbColor.rgbRed;

        public UpdateCallback(Range status)
        {
            statusRange = status;
        }

        public new void Execute(dynamic res)
        {
            base.is_executed = true;
            if (res is string)
            {
                SetRangeText(res as string);
            }
            else if (res is GinasResult)
            {
                GinasResult result = res as GinasResult;
                SetRangeText(result.message);
            }
        }

        public void SetRangeText(string rangeText)
        {
            statusRange.FormulaR1C1 = rangeText;
            if (rangeText.Equals("started", StringComparison.CurrentCultureIgnoreCase))
            {
                statusRange.Interior.Color = COLOR_STARTING;
            }
            else if ( rangeText.Equals("success", StringComparison.CurrentCultureIgnoreCase))
            {
                statusRange.EntireRow.Interior.Color = COLOR_SUCCESS;
            }
            else
            {
                statusRange.EntireRow.Interior.Color = COLOR_ERROR;
            }
            
        }

        public int RunnerNumber
        {
            get;
            set;
        }

        public Dictionary<string, string> ParameterValues
        {
            get;
            set;
        }

        public string LoadScriptName
        {
            get;
            set;
        }
    }
}
