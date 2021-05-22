using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class PubChemReturn
    {
        public IdentifierListClass PropertyTable
        {
            get;
            set;
        }
    }

    public class IdentifierListClass
    {
        public List<InChIKeyCid> Properties
        {
            get;
            set;
        }
    }

    public class InChIKeyCid
    {
        public string InChIKey
        {
            get;
            set;
        }

        public string Cid
        {
            get;
            set;
        }
    }
}
