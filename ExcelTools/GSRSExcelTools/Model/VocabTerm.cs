using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    /// <summary>
    /// One item/term within a vocabulary collection.
    /// 'display' gets shown to the user
    /// 'value' goes into the database
    /// DateTime fields cause problems with the deserializer
    /// </summary>
    public class VocabTerm
    {
        private int version;
        //private DateTime created;
        //private DateTime modified;
        private bool deprecated;
        private string value;
        private string display;
        private string[] filters;
        private bool hidden;
        private bool selected;

        public bool Deprecated { get => deprecated; }
        public int Version { get => version; set => version = value; }
        //public DateTime Created { get => created; set => created = value; }
        //public DateTime Modified { get => modified; set => modified = value; }
        public string Value { get => value; set => this.value = value; }
        public string Display { get => display; set => display = value; }
        public string[] Filters { get => filters; set => filters = value; }
        public bool Hidden { get => hidden; set => hidden = value; }
        public bool Selected { get => selected; set => selected = value; }
    }
}
