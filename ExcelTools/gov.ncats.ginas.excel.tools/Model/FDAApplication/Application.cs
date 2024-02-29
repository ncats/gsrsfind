using System;
using System.Collections.Generic;
using System.Text;

namespace gov.ncats.ginas.excel.tools.Model.FDAApplication
{
    public class Application
    {
        public ApplicationIndication[] applicationIndicationList
        { get; set; }

        public ApplicationProduct[] applicationProductList
        { get; set; }

        public string center
        { get; set; }

        public string appType
        { get; set; }

        public string appNumber
        { get; set; }

        public string status
        { get; set; }

        public string publicDomain
        { get; set; }

        public string nonProprietaryName
        { get; set; }

        public string sponsorName
        { get; set; }

        public string appSubType
        { get; set; }

        public string title
        { get; set; }

        public string externalTitle
        { get; set; }

        public int id
        { get; set; }
    }
}
