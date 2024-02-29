using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Model
{
    internal class StructureProxy
    {
        public StructureProxy()
        {

        }

        public StructureProxy(string id, string smiles, string molFormula, double mwt, string stereoDescription, int charge)
        {
            ID = id;
            SMILES = smiles;
            MolFormula = molFormula;
            MWt = mwt;
            StereoDescription = stereoDescription;
            Charge = charge;
        }

        internal string ID
        {
            get;
            set;
        }

        internal string SMILES
        {
            get;
            set;
        }

        internal string MolFormula
        {
            get;
            set;
        }

        internal string StereoDescription
        {
            get;
            set;
        }

        internal int Charge
        {
            get;
            set;
        }
        internal double MWt
        {
            get;
            set;
        }
    }
}
