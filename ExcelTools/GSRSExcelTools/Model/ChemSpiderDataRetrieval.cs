using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    public class ChemSpiderDataRetrieval
    {
        [JsonProperty("records")]
        public List<ChemSpiderDataRetrievalRecord> Records
        {
            get;
            set;
        }

    }
}
