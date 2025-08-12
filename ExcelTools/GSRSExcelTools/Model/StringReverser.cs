using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Model
{
    public static class StringReverser
    {

        //from https://www.c-sharpcorner.com/uploadfile/puranindia/extension-methods-in-C-Sharp-3-0/
        public static string ReverseString(this string message)
        {
            char[] charArray = message.ToCharArray();
            Array.Reverse(charArray);
            return String.Concat(charArray);
        }

    }
}
