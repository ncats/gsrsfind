using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model.FDAApplication
{
    public class ApplicationLookup
    {
        public ApplicationLookup()
        {
        }

        public ApplicationLookup(string provenance, string center, string appType, string number)
        {
            Provenance = provenance;
            Center = center;
            AppType = appType;
            Number = number;
        }

        public string Provenance
        { get; set; }

        public string Center
        { get; set; }

        public string AppType
        { get; set; }

        public string Number
        { get; set; }

    }
}
