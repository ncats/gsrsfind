using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

using GSRSExcelTools.Model.Callbacks;

namespace GSRSExcelTools.Providers
{
    public class RangeWrapperFactory
    {
        public static RangeWrapper CreateRangeWrapper(Range target)
        {
            RangeWrapper wrapped = new RangeWrapper();
            wrapped.SetRange(target);
            return wrapped;
        }

        public static TwoRangeWrapper CreateTwoRangeWrapper(Range target1, Range target2)
        {
            TwoRangeWrapper wrapped = new TwoRangeWrapper();
            wrapped.SetRanges(target1, target2);
            return wrapped;
        }

    }
}
