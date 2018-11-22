using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ginasExcelUnitTests.Utils
{
    internal static class StringExtensions
    {
        public static bool IsGuid(this string testGuid)
        {
            Guid guid;
            if( Guid.TryParse(testGuid, out guid))
            {
                return true;
            }
            return false;
        }

        public static bool IsPossibleGuidName(this string testName)
        {
            if (string.IsNullOrWhiteSpace(testName) || !testName.Contains(" "))
            {
                return false;
            }

            string[] parts = testName.Split(' ');
            if( parts.Length == 2 && parts[0].Equals("Name") && parts[1].IsGuid())
            {
                return true;
            }
            return false;

        }
    }
}
