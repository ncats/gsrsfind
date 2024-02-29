using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    /// <summary>
    /// Return from POSTing a file to the gsrs API
    /// </summary>
    public class FilePostReturn
    {
        public string id
        {
            get;
            set;
        }

        public string url
        {
            get;
            set;
        }

        public string name
        {
            get;
            set;
        }

        public string mimeType
        {
            get;
            set;
        }

        override public string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("id: ");
            stringBuilder.AppendLine(id);
            stringBuilder.Append("name: ");
            stringBuilder.AppendLine(name);
            stringBuilder.Append("url: ");
            stringBuilder.AppendLine(url);
            return stringBuilder.ToString();
        }
    }
}
