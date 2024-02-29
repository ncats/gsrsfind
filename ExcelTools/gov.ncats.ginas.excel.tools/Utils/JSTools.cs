using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Reflection;

using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Model.FDAApplication;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class JSTools
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        const int id_length = 10;
        const String id_prefix = "gsrs_";
        static Random rnd = new Random();

        public static string RandomIdentifier(int length = id_length, bool foundDupe = false)
        {
            String ident;
            String alpha;
            ident = "";
            alpha = "abcdefghijklmnopqrstuvwxyz";
            //(foundDupe ) ? DateTime.Now.Millisecond+1 : DateTime.Now.Millisecond

            int i;

            for (int j = 0; j < length; j++)
            {
                i = rnd.Next(alpha.Length);
                ident += alpha.Substring(i, 1);
            }
            return id_prefix + ident;
        }


        public static string MakeSearchString(string[] inputValues)
        {
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append("[");
            List<string> cleanedValues = new List<string>();
            foreach (string value in inputValues.Where(s=>!string.IsNullOrEmpty(s)))
            {
                cleanedValues.Add("'" + value.Replace("'", "\\'") + "'");
            }
            outputBuilder.Append(string.Join(",", cleanedValues));
            outputBuilder.Append("]");

            return outputBuilder.ToString();
        }

        public static Dictionary<string, string[]> getDictionaryFromString(string dictionaryFromJS)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, string[]> returnedValue = serializer.Deserialize<Dictionary<string, string[]>>
                (dictionaryFromJS);
            return returnedValue;
        }

        public static Vocab GetVocabFromString(string rawVocab)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Vocab returnedValue = serializer.Deserialize<Vocab>(rawVocab);
            try
            {
                returnedValue.Content[0].Terms = returnedValue.Content[0].Terms.OrderBy(t => t.Display).ToArray();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error sorting 2: {0}", ex.Message, ex);
            }

            return returnedValue;
        }

        public static ScriptParameter GetScriptParameterFromString(string scriptParamFromJS)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ScriptParameter scriptParameter = serializer.Deserialize<ScriptParameter>(scriptParamFromJS);
            return scriptParameter;
        }

        public static string getTagName(string htmlFragment)
        {
            if (string.IsNullOrWhiteSpace(htmlFragment)) return "";
            int pos = htmlFragment.IndexOf(">");
            string tagName = htmlFragment.Substring(1, (pos - 1));
            return tagName;
        }

        public static GinasToolsConfiguration GetGinasToolsConfigurationFromString(string configString)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            GinasToolsConfiguration config = serializer.Deserialize<GinasToolsConfiguration>(configString);
            return config;
        }

        public static string GetStringFromGinasToolsConfiguration(GinasToolsConfiguration config)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string configString = serializer.Serialize(config);
            return configString;
        }

        public static GinasResult GetGinasResultFromString(string ginasResultRaw)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            GinasResult result = serializer.Deserialize<GinasResult>(ginasResultRaw);
            return result;
        }
        public static StructureReturn GetStructureFromString(string structureJson)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<StructureReturn>(structureJson);
        }

        public static List<ApplicationField> GetApplicationMetadataFromString(string metadata)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<ApplicationField>>(metadata);
        }

        public static FilePostReturn GetFilePostReturnFromString(string rawData)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<FilePostReturn>(rawData);
        }

        public static Application GetApplicationFromString(string appInput)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Application application = serializer.Deserialize<Application>(appInput);
            return application;
        }

        public static string GetStringFromApplication(Application application)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(application);
        }

        public static ApplicationProcessingResult GetApplicationResultFromString( string resultInput)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ApplicationProcessingResult result = serializer.Deserialize<ApplicationProcessingResult>(resultInput);
            return result;
        }
    }
}
