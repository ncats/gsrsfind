using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model.Callbacks
{
    public class BatchLookup
    {
        public BatchLookup(List<LookupDataCallback> callbacks)
        {
            this.LookupData = callbacks;
        }

        public List<LookupDataCallback> LookupData
        {
            get;
            set;
        }
    }
}
