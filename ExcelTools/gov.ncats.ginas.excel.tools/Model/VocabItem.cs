using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class VocabItem
    {
        public VocabItem(string term, string display, bool deprecated)
        {
            Term = term;
            Display = display;
            Deprecated = deprecated;
        }

        public string Term
        {
            get;
            set;
        }

        public string Display

        {
            get;
            set;
        }

        public bool Deprecated
        {
            get;
            set;
        }

    }

}
