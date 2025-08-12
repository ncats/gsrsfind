using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ginasExcelUnitTests.Model
{
    internal class StructurallyDiverseProxy
    {
        internal StructurallyDiverseProxy()
        {

        }

        internal StructurallyDiverseProxy(string uuid, string genus, string species,
            string author, string rawPartInfo)
        {
            UUID = uuid;
            Genus = genus;
            Species = species;
            Author = author;
            RawPartInfo = rawPartInfo;
        }

        internal string UUID
        {
            get;
            set;
        }

        internal string Genus
        {
            get;
            set;
        }

        internal string Species
        {
            get;
            set;
        }

        internal string Author
        {
            get;
            set;
        }

        internal string LatinBinomial
        {
            get
            {
                return Genus + " " + Species;
            }
        }
        private string _part = null;

        internal string Part
        {
            get
            {
                if( _part==null && !string.IsNullOrEmpty(RawPartInfo))
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    PlantPart[] part = serializer.Deserialize<PlantPart[]>
                        (RawPartInfo);
                    _part = part[0].term;
                }
                return _part;
            }
        }
        internal string RawPartInfo
        {
            get;
            set;
        }

    }
}
