using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gov.ncats.ginas.excel.tools.Controller;
using gov.ncats.ginas.excel.tools.Model;

namespace ginasExcelUnitTests.Model
{
    public class ScriptExecutorMock : IScriptExecutor
    {
        internal string TestScript;
        private List<string> scriptsExecuted = new List<string>();

        public object ExecuteScript(string script)
        {
            scriptsExecuted.Add(script);
            TestScript = script;
            Console.WriteLine("Simulating script execution. " + script);
            Dictionary<int, ScriptParameter> mockParameters = GetMockedScriptParameters();

            if (script.Contains("tmpScript.hasArgumentByName"))
            {
                return "true";
            }
            else if (script.Contains("tmpScript.getArgumentByName("))
            {
                return "{\"args\":{\"pt\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"pt\",\"name\":\"PT\",\"description\":\"Preferred Term of the new substance\",\"required\":true,\"value\":\"1-Ethenylpyrrolidin-2-one\"},\"ptLang\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"ptLang\",\"name\":\"PT LANGUAGE\",\"description\":\"2-letter language abbreviation for Preferred Term\",\"required\":true,\"defaultValue\":\"en\",\"value\":\"ENG\"},\"ptNameType\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"ptNameType\",\"name\":\"PT NAME TYPE\",\"description\":\"2/3-letter name type (e.g., cn, of) for Preferred Term\",\"required\":true,\"defaultValue\":\"cn\",\"value\":\"sys\"},\"SubstanceClass\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"SubstanceClass\",\"name\":\"SUBSTANCE CLASS\",\"description\":\"Category\",\"required\":true,\"defaultValue\":\"chemical\",\"value\":\"chemical\"},\"smiles\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"smiles\",\"name\":\"SMILES\",\"description\":\"Structure as SMILES\",\"required\":false},\"molfile\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"molfile\",\"name\":\"MOLFILE\",\"description\":\"Structure as molfile\",\"required\":false},\"referenceType\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"referenceType\",\"name\":\"REFERENCE TYPE\",\"description\":\"Type of reference\",\"defaultValue\":\"SYSTEM\",\"required\":false,\"value\":\"WEB SITE\"},\"referenceCitation\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"referenceCitation\",\"name\":\"REFERENCE CITATION\",\"description\":\"Citation text for reference\",\"required\":true,\"value\":\"https://en.wikipedia.org/wiki/N-Vinylpyrrolidone\"},\"referenceUrl\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"referenceUrl\",\"name\":\"REFERENCE URL\",\"description\":\"URL for the reference\",\"required\":false,\"value\":\"https://en.wikipedia.org/wiki/N-Vinylpyrrolidone\"},\"changeReason\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"changeReason\",\"name\":\"CHANGE REASON\",\"defaultValue\":\"Creating new substance\",\"description\":\"Text for the record change\",\"required\":false},\"public\":{\"_type\":\"argument\",\"opPromise\":{\"_promise\":true},\"key\":\"public\",\"name\":\"PD\",\"description\":\"Public Domain status of the code (sets access for reference as well)\",\"defaultValue\":false,\"required\":false}}}";
            }
            else if(script.Contains("tmpScript.arguments.getItem("))
            {
                int pos1 = script.IndexOf("(");
                int pos2 = script.IndexOf(")");
                string itemString = script.Substring(pos1 + 1, (pos2 - pos1 - 1));
                int itemNum = Convert.ToInt32(itemString);
                ScriptParameter parameter = mockParameters[itemNum];
                if( script.Contains("cvType"))
                {
                    return parameter.cvType;
                }
                else if(script.Contains("type"))
                {
                    return parameter.type;
                }
            }
            else if (script.Contains("tmpScript.arguments.length"))
            {
                return mockParameters.Count;
            }
            else if(script.Equals("_.map($('div.checkop input:checked'), 'name').join('___');"))
            {
                return "Lychi___Record Access___All Names___CAS Numbers";
            }

            return new object();
        }

        public void SetScript(string script)
        {
            TestScript = script;
        }

        internal static Dictionary<int, ScriptParameter> GetMockedScriptParameters()
        {
            Dictionary<int, ScriptParameter> scriptParms = new Dictionary<int, ScriptParameter>();
            ScriptParameter scriptParameter = new ScriptParameter
            {
                type = "string",
                key = "uuid",
                name = "UUID",
                description = "UUID of the existing substance record",
                required = true
            };
            scriptParms.Add(0, scriptParameter);

            scriptParameter = new ScriptParameter
            {
                type = "string",
                key = "pt",
                name = "PT",
                description = "Preferred Term of the existing record (used for validation)",
                required = true
            };
            scriptParms.Add(1, scriptParameter);

            scriptParameter = new ScriptParameter
            {
                type = "string",
                key = "name",
                name = "NAME",
                description = "Name text of the new name (free text)",
                required = true
            };
            scriptParms.Add(2, scriptParameter);

            scriptParameter = new ScriptParameter
            {
                type = "cv",
                key = "name type",
                name = "NAME TYPE",
                description = "Type of the new name (must match a defined list)",
                defaultValue = "cn",
                cvType = "NAME_TYPE",
                required = true
            };
            scriptParms.Add(3, scriptParameter);

            scriptParameter = new ScriptParameter
            {
                type = "cv",
                key = "language",
                name = "LANGUAGE",
                description = "Language of the new name",
                defaultValue = "En",
                cvType = "LANGUAGE",
                required = true
            };
            scriptParms.Add(4, scriptParameter);

            scriptParameter = new ScriptParameter
            {
                type = "string",
                key = "pd",
                name = "PD",
                description = "Public Domain status of the name (sets access for reference as well)",
                defaultValue = "false",
                required = false
            };
            scriptParms.Add(5, scriptParameter);


            scriptParameter = new ScriptParameter
            {
                type = "cv",
                key = "reference type",
                name = "REFERENCE TYPE",
                description = "Type of reference (must match a vocabulary)",
                defaultValue = "SYSTEM",
                cvType = "DOCUMENT_TYPE",
                required = false
            };
            scriptParms.Add(6, scriptParameter);

            scriptParameter = new ScriptParameter
            {
                type = "string",
                key = "reference citation",
                name = "REFERENCE CITATION",
                description = "Citation text for reference",
                defaultValue = "SYSTEM",
                required = true
            };
            scriptParms.Add(7, scriptParameter);

            scriptParameter = new ScriptParameter
            {
                type = "string",
                key = "reference url",
                name = "REFERENCE URL",
                description = "URL for the reference",
                required = false
            };
            scriptParms.Add(8, scriptParameter);

            scriptParameter = new ScriptParameter
            {
                type = "change reason",
                key = "CHANGE REASON",
                name = "REFERENCE URL",
                description = "Reason for the record change",
                required = false,
                defaultValue= "Added Name"
            };
            scriptParms.Add(9, scriptParameter);
            return scriptParms;
        }


        public List<string> ScriptsRun
        {
            get
            {
                return scriptsExecuted;
            }
        }

        public void SetController(IController controller)
        {

        }
    }
}
