using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class SheetSectionInfo
    {
        public SheetSectionInfo ()
        {
            FieldNames = new List<string>();
        }

        public List<string> FieldNames
        {
            get;
            set;
        }

        public string Direction
        {
            get;
            set;
        }
    }
}
