using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class ChemSpiderDataRetrievalRecord
    {
        [JsonProperty("id")]
        public long Id
        {
            get;
            set;
        }

        [JsonProperty("mol2D")]
        public string Mol2D
        {
            get;
            set;
        }

        [JsonProperty("inchiKey")]
        public string InChIKey
        {
            get;
            set;
        }

    }
}
