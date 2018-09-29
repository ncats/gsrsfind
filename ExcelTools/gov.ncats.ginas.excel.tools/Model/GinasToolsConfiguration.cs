using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class GinasToolsConfiguration
    {
        private string _apiPath;

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
