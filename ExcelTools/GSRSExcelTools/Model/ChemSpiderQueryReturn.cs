using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    public class ChemSpiderQueryReturn
    {
        [JsonProperty("queryId")]
        public string queryId
        {
            get;
            set;
        }
    }
}
