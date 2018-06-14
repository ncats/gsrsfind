using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gov.ncats.ginas.excel.tools.Model;

namespace ginasExcelUnitTests.Model
{
    public class ScriptExecutorMock : IScriptExecutor
    {
        internal string TestScript;

        public object ExecuteScript(string script)
        {
            TestScript = script;
            Console.WriteLine("Simulating script execution. " + script);
            if( script.Contains("tmpScript.hasArgumentByName"))
            {
                return "true";
            }
            else if(script.Contains("tmpScript.getArgumentByName("))
            {
                return "{\"args\":{\"pt\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"pt\",\"name\":\"PT\",\"description\":\"Preferred Term of the new substance\",\"required\":true,\"value\":\"1-Ethenylpyrrolidin-2-one\"},\"ptLang\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"ptLang\",\"name\":\"PT LANGUAGE\",\"description\":\"2-letter language abbreviation for Preferred Term\",\"required\":true,\"defaultValue\":\"en\",\"value\":\"ENG\"},\"ptNameType\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"ptNameType\",\"name\":\"PT NAME TYPE\",\"description\":\"2/3-letter name type (e.g., cn, of) for Preferred Term\",\"required\":true,\"defaultValue\":\"cn\",\"value\":\"sys\"},\"SubstanceClass\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"SubstanceClass\",\"name\":\"SUBSTANCE CLASS\",\"description\":\"Category\",\"required\":true,\"defaultValue\":\"chemical\",\"value\":\"chemical\"},\"smiles\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"smiles\",\"name\":\"SMILES\",\"description\":\"Structure as SMILES\",\"required\":false},\"molfile\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"molfile\",\"name\":\"MOLFILE\",\"description\":\"Structure as molfile\",\"required\":false},\"referenceType\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"referenceType\",\"name\":\"REFERENCE TYPE\",\"description\":\"Type of reference\",\"defaultValue\":\"SYSTEM\",\"required\":false,\"value\":\"WEB SITE\"},\"referenceCitation\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"referenceCitation\",\"name\":\"REFERENCE CITATION\",\"description\":\"Citation text for reference\",\"required\":true,\"value\":\"https://en.wikipedia.org/wiki/N-Vinylpyrrolidone\"},\"referenceUrl\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"referenceUrl\",\"name\":\"REFERENCE URL\",\"description\":\"URL for the reference\",\"required\":false,\"value\":\"https://en.wikipedia.org/wiki/N-Vinylpyrrolidone\"},\"changeReason\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"changeReason\",\"name\":\"CHANGE REASON\",\"defaultValue\":\"Creating new substance\",\"description\":\"Text for the record change\",\"required\":false},\"public\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"public\",\"name\":\"PD\",\"description\":\"Public Domain status of the code (sets access for reference as well)\",\"defaultValue\":false,\"required\":false}}}";
            }
            return new object();
        }

        public void SetScript(string script)
        {
            TestScript = script;
        }
    }
}
