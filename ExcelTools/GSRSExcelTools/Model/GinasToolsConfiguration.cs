using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    public class GinasToolsConfiguration
    {
        private string _apiPath;
        private string _initPath;

        public string ApiPath
        {
            get
            {
                if( !string.IsNullOrEmpty(_apiPath)) return _apiPath;
                return "api/v1/";
            }
            set
            {
                _apiPath = value;
            }
        }

        public string InitPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_initPath)) return _initPath;
                return "api";
            }
            set
            {
                _initPath = value;
            }
        }
        public GinasToolsConfiguration()
        {
            Servers = new List<GinasServer>();
        }

        public GinasServer SelectedServer
        {
            get;
            set;
        }

        public List<GinasServer> Servers
        {
            get;
            set;
        }

        public bool DebugMode
        {
            get;
            set;
        }

        public bool SortVocabsAlphabetically
        {
            get;
            set;
        }

        public int BatchSize
        {
            get;
            set;
        }

        public float ExpirationOffset
        {
            get;
            set;
        }

        public string ChemSpiderApiKey
        {
            get;
            set;
        }

        public int PageBuildDelayMilliseconds
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("GinasToolsConfiguration ");
            stringBuilder.Append("Total servers: " + Servers.Count);
            stringBuilder.Append("; ");
            stringBuilder.Append("selected server: ");
            stringBuilder.Append(((SelectedServer == null) ? "null" : SelectedServer.ToString()));
            stringBuilder.Append("; ");
            Servers.ForEach(s => stringBuilder.Append(s.ToString()));

            return stringBuilder.ToString();
        }

        public int StructureImageSize
        {
            get;
            set;
        }

        public bool MarkupNameFields
        {
            get;
            set;

        }

    }

    public class GinasServer
    {
        public string ServerName
        {
            get;
            set;
        }

        public string ServerUrl
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string PrivateKey
        {
            get;
            set;
        }

        public string Token
        {
            get;
            set;
        }

        public string StructureUrl
        {
            get;
            set;
        }

        public bool LooksLikeSingleSignon()
        {
            if( string.IsNullOrWhiteSpace(Token) && string.IsNullOrWhiteSpace(PrivateKey)
                && string.IsNullOrWhiteSpace(Username))
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("GinasServer ");
            stringBuilder.Append("URL: " + ServerUrl);
            return stringBuilder.ToString();
        }


    }
}
