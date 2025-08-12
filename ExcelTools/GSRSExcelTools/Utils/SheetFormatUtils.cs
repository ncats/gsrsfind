using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Model;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class SheetFormatUtils
    {
        private static Dictionary<char, char> conversions = new Dictionary<char, char>();

        static SheetFormatUtils()
        {
            char[] inputChars =     { '', '', '', '', '', '', '', '', '', '', '', '', '', '', '®' };
            char[] outputChars =    { 'α', 'β', 'χ', 'δ', 'ε', 'φ', 'γ', 'η', 'ι', 'ϕ', 'κ', 'λ', 'μ', '→', '→' };
            string[] charReplacementSource = ";α;;β;;χ;;δ;;ε;;φ;;γ;;η;;ι;;ϕ;;κ;;λ;;μ;;→;®;→;`;';’;';ʹ;';‘;';′;';ʼ;ʽ;‛;'".Split(';');

            for( int c=0; c<charReplacementSource.Length; c++)
            {
                if( c %2 == 0)
                {
                    conversions.Add(charReplacementSource[c][0], charReplacementSource[c+1][0]);
                }
            }    
            //for (int c = 0; c < inputChars.Length; c++)
            //{
            //    conversions.Add(inputChars[c], outputChars[c]);
            //}
        }

        public static string ConvertChars(string inputString)
        {
            StringBuilder result = new StringBuilder();
            inputString.ToCharArray().ToList().ForEach(c => {
                if( conversions.ContainsKey(c))
                {
                    result.Append(conversions[c]);
                }
                else
                {
                    result.Append(c);
                }
            });
            return result.ToString();
        }

        public static string ExtractAndApplyFormatting(Range cell)
        {
            string originalString = cell.FormulaR1C1.ToString();
            List<MarkupElement> markupElements = GetItalicsRanges(cell);
            markupElements.AddRange(GetSubscriptRanges(cell));
            markupElements.AddRange(GetSuperscriptRanges(cell));
            if( markupElements.Count > 0)
            {
                markupElements = markupElements.OrderBy(e => e.startPosition).ToList();
            }
            
            string outputString = ApplyMarkup(originalString, markupElements);
            return outputString;
        }

        public static List<MarkupElement> GetItalicsRanges(Range cell)
        {
            List<MarkupElement> italicsRanges = new List<MarkupElement>();
            int pos = 1;
            while (pos <= cell.Characters.Count)
            {
                if ((!(cell.Characters[pos, 1].Font.Italic is DBNull) && (bool)cell.Characters[pos, 1].Font.Italic))
                {
                    int length = 0;
                    while (!(cell.Characters[pos + length, 1].Font.Italic is DBNull)
                        && (bool)cell.Characters[pos + length, 1].Font.Italic)
                    {
                        length++;
                    }
                    MarkupElement markupElement = new MarkupElement();
                    markupElement.startPosition = pos - 1;
                    markupElement.length = length;
                    markupElement.tag = "I";
                    italicsRanges.Add(markupElement);
                    pos += length;
                }
                else
                {
                    pos++;
                }
            }

            return italicsRanges;
        }

        public static List<MarkupElement> GetSuperscriptRanges(Range cell)
        {
            List<MarkupElement> superscriptRanges = new List<MarkupElement>();
            int pos = 1;
            while (pos <= cell.Characters.Count)
            {
                if ((!(cell.Characters[pos, 1].Font.Superscript is DBNull) && (bool)cell.Characters[pos, 1].Font.Superscript))
                {
                    int length = 0;
                    while (!(cell.Characters[pos + length, 1].Font.Superscript is DBNull)
                        && (bool)cell.Characters[pos + length, 1].Font.Superscript
                        && length < cell.Characters.Count)
                    {
                        length++;
                    }
                    MarkupElement markupElement = new MarkupElement
                    {
                        startPosition = pos - 1,
                        length = length,
                        tag = "SUP"
                    };
                    superscriptRanges.Add(markupElement);
                    pos += length;
                }
                else
                {
                    pos++;
                }
            }

            return superscriptRanges;
        }

        public static List<MarkupElement> GetSubscriptRanges(Range cell)
        {
            List<MarkupElement> subscriptRanges = new List<MarkupElement>();
            int pos = 1;
            while (pos <= cell.Characters.Count)
            {
                if ((!(cell.Characters[pos, 1].Font.Subscript is DBNull) && (bool)cell.Characters[pos, 1].Font.Subscript))
                {
                    int length = 0;
                    while (!(cell.Characters[pos + length, 1].Font.Subscript is DBNull)
                        && (bool)cell.Characters[pos + length, 1].Font.Subscript
                        && length < (cell.Characters.Count - pos + 1))
                    {
                        length++;
                    }
                    MarkupElement markupElement = new MarkupElement
                    {
                        startPosition = pos - 1,
                        length = length,
                        tag = "SUB"
                    };
                    subscriptRanges.Add(markupElement);
                    pos += length;
                }
                else
                {
                    pos++;
                }
            }

            return subscriptRanges;
        }

        public static string ApplyMarkup(String starting, List<MarkupElement> elements)
        {
            if(elements == null || elements.Count==0)
            {
                return starting;
            }
            StringBuilder results = new StringBuilder();
            int pos = 0;
            for (int elem = 0; elem < elements.Count; elem++)
            {
                MarkupElement element = elements[elem];
                int lengthOfStartingString = (elem == 0)
                    ? element.startPosition
                    : element.startPosition - elements[elem - 1].startPosition - elements[elem - 1].length;
                string startingSection = lengthOfStartingString<=0 ? "" : starting.Substring(pos, lengthOfStartingString);

                Console.WriteLine("elem: {0}; tag: {1}; startingSection: {2}", elem, element.tag, startingSection);
                results.Append(startingSection);
                results.Append("<");
                results.Append(element.tag);
                results.Append(">");
                string remaining = starting.Substring(element.startPosition, element.length);
                Console.WriteLine("remaining: {0}", remaining);
                results.Append(remaining);
                results.Append("</");
                results.Append(element.tag);
                results.Append(">");
                pos = element.startPosition + element.length;
            }
            int maxStart = elements.Max(e => (e.startPosition + e.length - 1));
            if (maxStart < starting.Length)
            {
                Console.WriteLine("last part: " + starting.Substring(maxStart + 1));
                results.Append(starting.Substring(maxStart + 1));
            }
            return results.ToString();
        }
    }
}
