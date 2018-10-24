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


        public static async Task<string> SaveMolfileAndDisplay(string molfile, Range cell, string serverUrl,
            Range idCell)
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
                        StructureReturn r = await response.Content.ReadAsAsync<StructureReturn>();
                        if (r.Structure != null)
                        {
                            if (cell != null)
                            {
                                string structureImageUrl = serverUrl + "img/" + r.Structure.Id + ".png";
                                log.DebugFormat("using structure URL {0}", structureImageUrl);
                                ImageOps.AddImageCaption(cell, structureImageUrl, 300);
                            }
                            if (idCell != null)
                            {
                                log.DebugFormat("structure id {0} for cell {1}", r.Structure.Id, idCell.Address);
                                SearchMolfile(r.Structure.Id, serverUrl, idCell);
                            }
                            return r.Structure.Id;
                        }
                        log.Debug("Error saving structure: " + response.ReasonPhrase);
                        if (idCell != null)
                        {
                            if (molfile.Contains("V3000")) idCell.FormulaR1C1 = "V 3000 Molfiles fail duplicate checks but may register OK";
                            else  idCell.FormulaR1C1 = "Error processing this structure (it may still register OK)";
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

        public static bool IsValidHttpUrl(string urlText)
        {
            Uri uriResult;
            return Uri.TryCreate(urlText, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static async Task<StructureQueryResult> SearchMolfile(string molfile, string serverUrl,
            Range cellForResults = null)
        {
            molfile = Regex.Replace(molfile, "[^\x0d\x0a\x20-\x7e\t]", "");
            if (molfile.Length < 100) log.DebugFormat("molfile: {0}", molfile);

            if (RestClient.BaseAddress == null) RestClient.BaseAddress = new Uri(serverUrl);
            string fullUrl = serverUrl + "api/v1/substances/structureSearch?type=exact&sync=true&q=" + molfile;
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, fullUrl);
            message.Content = new StringContent(molfile, Encoding.UTF8);

            using (HttpResponseMessage response = await RestClient.GetAsync(fullUrl))
            //using (HttpResponseMessage response = await RestClient.SendAsync(message))
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        StructureQueryResult r = await response.Content.ReadAsAsync<StructureQueryResult>();
                        if(cellForResults != null)
                        {
                            if (r.Content.Length == 1)
                            {
                                cellForResults.FormulaR1C1 = "structure has 1 duplicate";
                            }
                            else if( r.Content.Length > 1)
                            {
                                cellForResults.FormulaR1C1 = string.Format("structure has {0} duplicates", r.Content.Length);
                            }
                            else
                            {
                                cellForResults.FormulaR1C1 = "unique";
                            }
                        }
                        return r;
                        //log.Debug("Error saving structure: " + response.ReasonPhrase);
                        //return string.Empty;
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

        //public async Task<string> RunStructureQuery(string serverUrl, string[] molfiles)
        //{
        //    string data = null;
        //    try
        //    {
        //        int timeout = 10;
        //        log.DebugFormat("Using timeout: {0}", timeout);
        //        RestClient.Timeout = new TimeSpan(TimeSpan.TicksPerSecond * timeout);
        //        RestClient.BaseAddress = new Uri(serverUrl);
        //        if (serverUrl.StartsWith("https"))
        //        {
        //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        }

        //        string searchUrl = "api/v1/substances/structureSearch/substances/structureSearch";

        //        HttpResponseMessage response = await RestClient.SendAsync
        //            .GetAsync(searchUrl);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            data = await response.Content.ReadAsStringAsync();
        //        }
        //        else
        //        {
        //            data = "error";
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        data = "Error: " + ex.Message;
        //        log.ErrorFormat("Error contacting URL. message: {0}", ex.Message);
        //        try
        //        {
        //            if (ex.InnerException != null)
        //            {
        //                log.ErrorFormat("Inner error: {0}", ex.InnerException.Message);
        //                if (ex.InnerException.InnerException != null)
        //                {
        //                    log.ErrorFormat("Second inner error: {0}",
        //                        ex.InnerException.InnerException.Message);
        //                }
        //            }

        //        }
        //        catch (Exception secondary)
        //        {
        //            log.ErrorFormat("Error while processing other error: {0}", secondary.Message);
        //            log.Debug(secondary.StackTrace);
        //        }
        //    }
        //    return data;
        //}

    }
}
