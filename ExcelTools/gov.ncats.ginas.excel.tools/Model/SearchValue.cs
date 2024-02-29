using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class SearchValue
    {
        public SearchValue( string value, int rowNumber)
        {
            Value = value;
            RowNumber = rowNumber;
        }
        public string Value
        {
            get;
            set;
        }

        public int RowNumber
        {
            get;
            set;
        }
    }
}
