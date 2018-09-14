using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using gov.ncats.ginas.excel.tools.Model;
using System.Text;

namespace gov.ncats.ginas.excel.tools.Utils
{
    class RestUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static RestUtils()
        {
            RestClient = new HttpClient();
            RestClient.DefaultRequestHeaders.Accept.Clear();
            RestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public static async Task<string> SaveMolfile(string molfile)
        {
            //load configuration each call because the configuration may have changed
            GinasToolsConfiguration configuration = FileUtils.GetGinasConfiguration();
            string url = configuration.SelectedServer.ServerUrl + "structure";
            log.Debug(molfile);

           var content = new FormUrlEncodedContent(new[]
           {
                new KeyValuePair<string, string>("", molfile)
            });

            RestClient.BaseAddress = new Uri(configuration.SelectedServer.ServerUrl);
            using (HttpResponseMessage response = await RestClient.PostAsJsonAsync("structure", 
                content))
            {
                if (response.IsSuccessStatusCode)
                {
                    string result = string.Empty;

                    try
                    {
                        result = await response.Content.ReadAsStringAsync();
                        StructureReturn r = await response.Content.ReadAsAsync<StructureReturn>();
                        if (r.Structure != null)
                        {
                            return r.Structure.Id;
                        }
                        return string.Empty;
                    }
                    catch (Exception e2)
                    {
                        log.ErrorFormat("Error during structure post: " + e2.Message);
                        throw e2;
                    }

                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

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
            catch (Exception ex)
            {
                log.DebugFormat("Error contacting URL. message: {0}", ex.Message);
            }
            return data;
        }


        public static HttpClient RestClient
        {
            get;
            set;
        }
    }
}
