using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Utils
{
    public class TextUtils
    {
        public static string StripQuotes(String initalText)
        {
            if(string.IsNullOrEmpty(initalText))
            {
                return initalText;
            }
            if(initalText.StartsWith("\"") && initalText.EndsWith("\""))
            {
                return initalText.Substring(1, initalText.Length - 2);
            }
            else
            {
                return initalText;
            }
        }

        public static string ExtractInnerResultId(string input)
        {
            string prefix = "{'innerResultId': '";
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            int pos = input.IndexOf(prefix);
            if(pos < 0)
            {
                return input;
            }
            string cleaned = input.Substring(pos + prefix.Length);
            int endPos = cleaned.IndexOf("'}");
            if (endPos < 0)
            {
                return cleaned;
            }
            else
            {
                return cleaned.Substring(0, endPos);
            }

        }

        public static string ReplaceInnerResultId(string input, string replacement)
        {
            string prefix = "{'innerResultId': '";
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            int pos = input.IndexOf(prefix);
            if (pos < 0)
            {
                return input;
            }
            int endPos = input.IndexOf("'}");
            if (endPos < 0)
            {
                return input;
            }
            else
            {
                return input.Substring(0, pos) + replacement + input.Substring(endPos + 2);
            }

        }
    }
}
