using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    public class StructureQueryResult
    {
        [JsonProperty("id")]
        public int ID
        {
            get;
            set;
        }

        [JsonProperty("version")]
        public string Version
        {
            get;
            set;
        }

        [JsonProperty("created")]
        public long Created
        {
            get;
            set;
        }

        [JsonProperty("content")]
        public ResultItem[] Content
        {
            get;
            set;
        }

    }

    public class ResultItem
    {
        [JsonProperty("uuid")]
        public string Uuid
        {
            get;
            set;
        }

        [JsonProperty("substanceClass")]
        public string SubstanceClass
        {
            get;
            set;
        }

        [JsonProperty("structure")]
        public Structure structure
        {
            get;
            set;
        }

        [JsonProperty("_name")]
        public string PrimaryTerm
        {
            get;
            set;
        }

        [JsonProperty("_approvalIDDisplay")]
        public string ApprovalId
        {
            get;
            set;
        }
    }
}
