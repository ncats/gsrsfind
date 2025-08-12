using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Reflection;

using GSRSExcelTools.Model;

namespace GSRSExcelTools.Utils
{
    public class VocabUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<VocabItem> GetVocabularyItems(string applicationUrl, string vocab)
        {
            if (!applicationUrl.EndsWith("/")) applicationUrl += "/";
            string vocabUrl = applicationUrl + "api/v1/vocabularies/search";
            log.DebugFormat("{0} using URL {1} and parameter {2}", MethodBase.GetCurrentMethod().Name,
                vocabUrl, vocab);
            object rawVocabulary = RestUtils.RunVocabularyQuery(vocabUrl, vocab).GetAwaiter().GetResult();
            if (rawVocabulary != null && rawVocabulary is JObject)
            {
                return ParseObject((JObject)rawVocabulary);
            }
            return new List<VocabItem>();
        }

        public static List<VocabItem> ParseObject(JObject jobj)
        {
            log.Debug("ParseObject looking at object " + ((jobj ==null) ? "null" : (jobj.ToString()).Substring(0, 120)));
            List<VocabItem> items = new List<VocabItem>();
            try
            {
                dynamic dyn = (dynamic)jobj;
                JArray inputItems = dyn.content[0].terms;
                foreach (var item in inputItems)
                {
                    string value = ((dynamic)item).value;
                    string label = ((dynamic)item).display;
                    bool deprecated = ((dynamic)item).deprecated;
                    items.Add(new VocabItem(value, label, deprecated));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return items;
        }

        public static Dictionary<string, string> BuildVocabularyDictionary(string applicationUrl, 
            string vocab)
        {
            Dictionary<string, string> vocabDictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            List<VocabItem> vocabItems = GetVocabularyItems(applicationUrl, vocab);
            foreach(VocabItem item in vocabItems)
            {
                vocabDictionary.Add(item.Display, item.Term);
            }
            
            return vocabDictionary;
        }
    }
}
