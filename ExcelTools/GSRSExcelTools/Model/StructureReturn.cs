using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    /// <summary>
    /// Return from the Structure API
    /// </summary>
    public class StructureReturn
    {
        public Structure Structure
        {
            get;
            set;
        }

        public Moiety[] Moieties
        {
            get;
            set;
        }

    }

    public class Structure
    {
        public string Id
        {
            get;
            set;
        }

        public string Molfile
        {
            get;
            set;
        }

        public string smiles
        {
            get;
            set;
        }
        public string formula
        {
            get;
            set;
        }

        public string opticalActivity
        {
            get;
            set;
        }

        public string hash
        {
            get;
            set;
        }

        public bool deprecated
        {
            get;
            set;
        }

        public string stereochemistry
        {
            get;
            set;
        }


        public int stereoCenters
        {
            get;
            set;
        }

        public int definedStereo
        {
            get;
            set;
        }
}

    public class Moiety
    {

    }
}
