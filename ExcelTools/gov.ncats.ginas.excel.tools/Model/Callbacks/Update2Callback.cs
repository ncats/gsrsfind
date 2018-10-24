using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class Update2Callback :UpdateCallback
    {
        private Range molfileRange;
        const XlRgbColor COLOR_STARTING = XlRgbColor.rgbGreen;
        const XlRgbColor COLOR_SUCCESS = XlRgbColor.rgbAntiqueWhite;
        const XlRgbColor COLOR_ERROR = XlRgbColor.rgbAquamarine;

        public Update2Callback(Range statusRange) : base(statusRange)
        {
            ParameterValues = new Dictionary<string, string>();
        }

        public Update2Callback(Range statusRange, Range molfileRangeParm) : base(statusRange)
        {
            ParameterValues = new Dictionary<string, string>();
            molfileRange = molfileRangeParm;
        }

    }
}
