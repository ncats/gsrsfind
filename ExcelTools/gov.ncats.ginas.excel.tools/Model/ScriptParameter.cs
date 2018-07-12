using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class ScriptParameter
    {
        public string _type;
        public Promise opPromise;
        public string key;
        public string name;
        public string description;
        public bool required;
        public string defaultValue;
        public string cvType;
        public string type;

        private Dictionary<string, string> vocabulary;

        public Dictionary<string, string> Vocabulary
        {
            get { return this.vocabulary; }
            set { this.vocabulary = value; }
        }

        public bool IsBoolean()
        {
            if( !string.IsNullOrWhiteSpace(type) && type.Equals("boolean", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }

    public class Promise
    {
        public bool _promise;
    }
    
}
