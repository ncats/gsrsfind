using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class TwoRangeWrapper
    {
        private Range target1;
        private Range target2;

        public TwoRangeWrapper SetRanges(Range r1, Range r2)
        {
            target1 = r1;
            target2 = r2;
            return this;
        }

        public Range GetRange1()
        {
            return target1;
        }

        public Range GetRange2()
        {
            return target2;
        }

    }
}
