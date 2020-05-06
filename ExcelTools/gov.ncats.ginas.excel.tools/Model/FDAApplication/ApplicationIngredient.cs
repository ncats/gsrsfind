using System;
using System.Collections.Generic;
using System.Text;

namespace gov.ncats.ginas.excel.tools.Model.FDAApplication
{
    public class ApplicationIngredient
    {
        public string applicantIngredName
        { get; set; }

        public string ingredientName
        { get; set; }

        public SubRelationship[] subRelationshipList
        { get; set; }

        public string bdnum
        { get; set; }

        public string substanceId
        { get; set; }

        public string ingNameMessage
        { get; set; }

        public string basisOfStrengthName
        { get; set; }

        public string basisOfStrengthBdnum
        { get; set; }

        public string ingBasisMessage
        { get; set; }

        public string ingredientType
        { get; set; }

        public double average
        { get; set; }

        public string unit
        { get; set; }
    }
}
