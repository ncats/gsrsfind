using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model.FDAApplication
{
    public class ApplicationProcessingResult
    {
        public bool valid
        {
            get;
            set;
        }

        public bool modification
        {
            get;
            set;
        }

        public string message
        {
            get;
            set;
        }
    }
}
