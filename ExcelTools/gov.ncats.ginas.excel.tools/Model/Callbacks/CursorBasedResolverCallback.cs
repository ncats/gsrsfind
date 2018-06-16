using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class CursorBasedResolverCallback : Callback
    {
        private RangeWrapper cursor;

        public CursorBasedResolverCallback(RangeWrapper target)
        {
            cursor = target;
        }
        public void Callback_Execute(dynamic res)
        {
            //base.Execute(jsResp);
            base.is_executed = true; // we cannot call base.Execute because 
            // of dispatching problems but we can accomplish the same thing 
            // via direct call to is_executed. a hack.

            string[] arr;
            arr = ((string)res).Split(Microsoft.VisualBasic.ControlChars.Tab);
            //For i = 1 To(UBound(arr) - LBound(arr))
            for (int i = 1; i <= (arr.GetUpperBound(0) - arr.GetLowerBound(0)); i++)
            {
                if (arr[i] != "[object Object]")
                {
                    cursor.getRange().Offset[0, i].FormulaR1C1 = arr[i];
                }
            }
        }

        public RangeWrapper GetRangeWrapper()
        {
            return this.cursor;
        }

        public int OriginalRow
        {
            get;
            set;
        }

    }
}
