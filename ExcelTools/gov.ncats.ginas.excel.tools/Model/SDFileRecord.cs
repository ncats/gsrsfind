using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class SDFileRecord
    {
        public Dictionary<string, string> RecordData
        {
            get;
            set;
        } = new Dictionary<string, string>();
    }
}
