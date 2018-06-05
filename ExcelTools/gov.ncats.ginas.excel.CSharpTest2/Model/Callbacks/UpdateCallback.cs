using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.CSharpTest2.Model.Callbacks
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
            if( res is string)
            {
                statusRange.FormulaR1C1 = res;
            }
            else if( res is GinasResult)
            {
                GinasResult result = res as GinasResult;

                statusRange.FormulaR1C1 = result.message;
            }
        }

    }
}
