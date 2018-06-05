using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.CSharpTest2.Model.Callbacks
{
    public class ImgCallback : Callback
    {
        private Range targetRange;

        public ImgCallback(Range target)
        {
            targetRange = target;
        }

        public override void Execute(dynamic res)
        {
            base.is_executed = true; // we cannot call base.Execute because 
            // of dispatching problems but we can accomplish the same thing 
            // via direct call to is_executed. a hack.

            string[] strs;
            strs = ((string)res).Split(Microsoft.VisualBasic.ControlChars.Tab);
            if (strs.GetUpperBound(0) - strs.GetLowerBound(0) >= 1)
            {
                ImageOps imageOps = new ImageOps();
                imageOps.AddImageCaption(targetRange, strs[0], 300);
            }

        }

    }
}
