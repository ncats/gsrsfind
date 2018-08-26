using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace gov.ncats.ginas.excel.tools.Utils
{
    class RestUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<object> RunVocabularyQuery(string baseUrl, string vocabType)
        {
            object data = null;
            string urlParms = "?cache =" + Guid.NewGuid().ToString() + "&q=root_domain:\"^"
                + vocabType + "$\"";
            log.DebugFormat("urlParms: {0}", urlParms);
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = client.GetAsync(urlParms).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadAsAsync<object>();
                    }
                }
            }
            catch(Exception ex)
            {
                log.DebugFormat("Error contacting URL. message: {0}", ex.Message);
            }
            return data;
        }


    }
}
