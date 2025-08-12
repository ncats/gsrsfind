using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class MarkupElement
    {
        public String tag { get; set; }

        public int startPosition { get; set; }

        public int length { get; set; }
    }
}
