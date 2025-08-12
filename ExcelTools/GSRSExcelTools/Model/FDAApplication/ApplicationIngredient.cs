using System;
using System.Collections.Generic;
using System.Text;

namespace GSRSExcelTools.Model.FDAApplication
{
    public class ApplicationIngredient
    {
        public string applicantIngredName
        { get; set; }

        public string ingredientName
        { get; set; }

        public SubRelationship[] subRelationshipList
        { get; set; }

        public string substanceKey // bdnum
        { get; set; }

        public string substanceKeyType
        {
            get 
            {
                return "bdnum";
            }
        }
        public string substanceId
        { get; set; }

        public string ingNameMessage
        { get; set; }

        public string basisOfStrengthName
        { get; set; }

        /*public string basisOfStrengthBdnum
        { get; set; }*/

        public string basisOfStrengthSubstanceKey
        { get; set; }

        public string basisOfStrengthSubstanceKeyType
        {
            get
            {
                return "bdnum";
            }
        }

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
