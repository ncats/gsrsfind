using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Model
{
    internal class SubstanceNamesProxy
    {

        internal SubstanceNamesProxy()
        {

        }

        internal SubstanceNamesProxy(string name, string type, bool preferred, bool display, string languages)
        {
            Name = name;
            Type = type;
            Preferred = preferred;
            Display = display;
            Languages = languages;
        }

        internal bool IsBracketTerm()
        {
            if (Name.EndsWith("]") && Name.Contains("[")) return true;
            return false;
        }
        internal string Name
        {
            get;
            set;
        }

        internal string Type
        {
            get;
            set;
        }

        internal bool Preferred
        {
            get;
            set;
        }

        internal bool Display
        {
            get;
            set;
        }
        //unparsed JSON for now
        internal string Languages
        {
            get;
            set;
        }
    }
}
