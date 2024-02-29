using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class GinasResult
    {
        public bool valid
        {
            get;
            set;
        }

        public string message
        {
            get;
            set;
        }

        public object returned
        {
            get;
            set;
        }

        public object[] matches
        {
            get;
            set;
        }
    }
}
