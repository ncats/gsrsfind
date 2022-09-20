using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model.FDAApplication
{
    public class ApplicationField 
    {
        public enum Level
        {
            None = 0,
            Application = 1,
            Product = 2,
            Ingredient = 3,
            ProductName =4,
            AddIngredient = 5
        }

        public string FieldName
        {
            get;
            set;
        }

        public string VocabularyName
        {
            get;
            set;
        }

        public string JsonFieldName
        {
            get;
            set;
        }

        public string Lookup
        {
            get;
            set;
        }

        public Level FieldLevel
        {
            get;
            set;
        }

        public object FieldValue
        {
            get;
            set;
        }

        public string ResolvedValue
        {
            get;
            set;
        }

        public string ParentEntityName
        {
            get;
            set;
        }

        public bool WasResolved
        {
            get;
            set;
        } = false;

        public string HandleChange
        {
            get;
            set;
        }

        public bool IsAmount
        {
            get;
            set;
        } = false;

        public int Column
        {
            get;
            set;
        } = 0;


        public bool IncludeInSheet
        {
            get;
            set;
        }

        public string GetValue()
        {
            if (!string.IsNullOrEmpty(ResolvedValue)) return ResolvedValue;
            if (FieldValue != null)
            {
                if (FieldValue.GetType() ==  Type.GetType( "System.DateTime"))
                {
                    string formattedDate =String.Format("{0:MM/dd/yyyy}", FieldValue);
                    return formattedDate;
                }

                return FieldValue.ToString();
            }
            return string.Empty;
        }

        internal bool IsDate()
        {
            return FieldName.EndsWith("Date");
        }

        public ApplicationField Clone()
        {
            ApplicationField clone = new ApplicationField();
            clone.FieldLevel = this.FieldLevel;
            clone.FieldName = FieldName;
            clone.FieldValue = FieldValue;
            clone.JsonFieldName = JsonFieldName;
            clone.VocabularyName = VocabularyName;
            clone.ResolvedValue = ResolvedValue;
            clone.ParentEntityName = ParentEntityName;
            clone.Lookup = Lookup;
            clone.HandleChange = HandleChange;
            clone.IncludeInSheet = IncludeInSheet;
            return clone;
        }
    }
}
