using System;
using System.Collections.Generic;
using System.Text;

namespace gov.ncats.ginas.excel.tools.Model.FDAApplication
{
    public class SubRelationship
    {
        public string id
        { get; set; }

        public string substanceId
        { get; set; }

        public string ownerBdnum
        { get; set; }

        public string relationshipType
        { get; set; }

        public string relationshipName
        { get; set; }

        public string relationshipUnii
        { get; set; }
    }
}
