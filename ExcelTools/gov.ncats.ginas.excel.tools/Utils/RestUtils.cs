using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text.RegularExpressions;

using Microsoft.Office.Interop.Excel;

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
            RestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            RestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }


        public static async Task<string> SaveMolfileAndDisplay(string molfile, Range cell, string serverUrl)
        {
            molfile = Regex.Replace(molfile, "[^\x0d\x0a\x20-\x7e\t]", "");
            if (molfile.Contains("\r")) log.Debug("molfile contains CR");

            if (RestClient.BaseAddress == null) RestClient.BaseAddress = new Uri(serverUrl);
            string fullUrl = serverUrl + "structure";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, fullUrl);
            message.Content = new StringContent(molfile, Encoding.UTF8);

            using (HttpResponseMessage response = await RestClient.SendAsync(message))
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        //string result = await response.Content.ReadAsStringAsync();
                        StructureReturn r = await response.Content.ReadAsAsync<StructureReturn>();
                        if (r.Structure != null)
                        {
                            if (cell != null)
                            {
                                string structureImageUrl = serverUrl + "img/" + r.Structure.Id + ".png";
                                log.DebugFormat("using structure URL {0}", structureImageUrl);
                                ImageOps.AddImageCaption(cell, structureImageUrl, 300);
                            }
                            return r.Structure.Id;
                        }
                        log.Debug("Error saving structure: " + response.ReasonPhrase);
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

        public static bool IsValidHttpUrl(string urlText)
        {
            Uri uriResult;
            return Uri.TryCreate(urlText, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
