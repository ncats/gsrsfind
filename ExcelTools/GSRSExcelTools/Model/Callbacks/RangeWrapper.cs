using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace GSRSExcelTools.Model.Callbacks
{
    public class RangeWrapper
    {
        private Range target;

        public RangeWrapper SetRange(Range r)
        {
            target = r;
            return this;
        }

        public Range GetRange()
        {
            return target;
        }

        public RangeWrapper Offset(int c , int r)
        {
            target = target.Offset[c, r];
            return this;
        }
    }
}
