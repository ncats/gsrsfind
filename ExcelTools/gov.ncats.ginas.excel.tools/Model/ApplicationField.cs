using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class ApplicationField 
    {
        public enum Level
        {
            None = 0,
            Application = 1,
            Product = 2,
            Ingredient = 3
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

        public string GetValue()
        {
            if (!string.IsNullOrEmpty(ResolvedValue)) return ResolvedValue;
            if (FieldValue != null) return FieldValue.ToString();
            return string.Empty;
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
            return clone;
        }
    }
}
