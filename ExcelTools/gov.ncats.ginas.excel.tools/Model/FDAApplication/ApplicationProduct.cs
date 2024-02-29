using System;
using System.Collections.Generic;
using System.Text;

namespace gov.ncats.ginas.excel.tools.Model.FDAApplication
{
    public class ApplicationProduct
    {
        public ApplicationProductName[] applicationProductNameList
        { get; set; }

        public ApplicationIngredient[] applicationIngredientList
        { get; set; }

        public string dosageForm
        { get; set; }

        public string routeAdmin
        { get; set; }

        public string unitPresentation
        { get; set; }

        public double amount
        { get; set; }

        public string unit
        { get; set; }
    }
}
