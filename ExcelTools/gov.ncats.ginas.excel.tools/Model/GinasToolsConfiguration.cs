using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class GinasToolsConfiguration
    {
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

        public int BatchSize
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
    }
}
