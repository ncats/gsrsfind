using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.CSharpTest2.Model.Callbacks
{
    public class RangeWrapper
    {
        private Range target;

        public RangeWrapper setRange(Range r)
        {
            target = r;
            return this;
        }

        public Range getRange()
        {
            return target;
        }

        public RangeWrapper offset(int c , int r)
        {
            target = target.Offset[c, r];
            return this;
        }

    }
}
