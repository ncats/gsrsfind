using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class ChemSpiderRecordIDRetrieval
    {
        [JsonProperty("results")]
        public List<long> results
        {
            get;
            set;
        }

    }
}
