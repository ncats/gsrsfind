using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class DoubleCursorBasedResolverCallback : Callback
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private TwoRangeWrapper cursor;
        

        public string ServerUrl
        {
            get;
            set;
        }

        public DoubleCursorBasedResolverCallback(TwoRangeWrapper target)
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
            for (int i = 1; i <= (arr.GetUpperBound(0) - arr.GetLowerBound(0)); i++)
            {
                if (arr[i] != "[object Object]")
                {
                    //cursor.GetRange2().FormulaR1C1 = arr[i];
                    string structureImageUrl = ServerUrl + "img/" + arr[i]+ ".png";
                    log.DebugFormat("using structure URL {0}", structureImageUrl);
                    ImageOps.AddImageCaption(cursor.GetRange1(), structureImageUrl, 300);
                }
            }
        }

        public TwoRangeWrapper GetRangeWrapper()
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
