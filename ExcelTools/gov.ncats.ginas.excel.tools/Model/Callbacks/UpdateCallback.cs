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
