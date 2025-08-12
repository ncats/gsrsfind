using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Utils
{
    public class DataUtils
    {
        private const int INCHIKEY_LENGTH = 27;

        public static bool IsPossibleInChiKey(string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }
            if (candidate.Trim().Length != INCHIKEY_LENGTH)
            {
                return false;
            }
            if (candidate[14] != '-' || candidate[25] != '-')
            {
                return false;
            }
            candidate = candidate.Remove(25, 1).Remove(14, 1);
            if (candidate.ToArray().Any(c => !char.IsLetter(c)))
            {
                return false;
            }
            return true;
        }

    }
}
