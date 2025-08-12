using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    public class ChemSpiderDataRetrievalRequest
    {
        [JsonProperty("recordIds")]
        public List<long> recordIds
        {
            get;
            set;
        }

        [JsonProperty("fields")]
        public List<String> fields
        {
            get;
            set;
        }
    }
}
