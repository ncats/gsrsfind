using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    public class VocabContent
    {
        private int id;
        private int version;
        //private DateTime created;
        //private DateTime modified;
        private bool deprecated;
        private string domain;
        private string vocabularyTermType;
        private object[] fields;
        private bool editable;
        private bool filterable;
        private VocabTerm[] terms;

        public int Id { get => id; set => id = value; }
        public int Version { get => version; set => version = value; }
        //public DateTime Created { get => created; set => created = value; }
        //public DateTime Modified { get => modified; set => modified = value; }
        public bool Deprecated { get => deprecated; set => deprecated = value; }
        public string Domain { get => domain; set => domain = value; }
        public string VocabularyTermType { get => vocabularyTermType; set => vocabularyTermType = value; }
        public object[] Fields { get => fields; set => fields = value; }
        public bool Editable { get => editable; set => editable = value; }
        public bool Filterable { get => filterable; set => filterable = value; }
        public VocabTerm[] Terms { get => terms; set => terms = value; }
    }
}
