using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Model
{
    /// <summary>
    /// Represents a complete controlled vocabulary object as returned by the ginas API
    /// </summary>
    public class Vocab
    {
        
        private VocabTerm[] terms;

        private object[] facets;

        private VocabContent[] content;

        public VocabTerm[] Terms { get => terms; set => terms = value; }
        public object[] Facets { get => facets; set => facets = value; }
        public VocabContent[] Content { get => content; set => content = value; }
    }
}
