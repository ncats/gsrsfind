using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model.Callbacks
{
    public class LookupDataCallback : Callback
    {
        public LookupDataCallback(Range range, string queryData, string result)
        {
            DataRange = range;
            QueryData = queryData;
            Result = result;
        }

        public Range DataRange
        {
            get;
            set;
        }

        public string QueryData
        {
            get;
            set;
        }

        public string Result
        {
            get;
            set;
        }
    }
}
