using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;

using Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Model;
using System.Text;
using System.IO;

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
            log.DebugFormat("{0} using URL {1}", MethodBase.GetCurrentMethod().Name, fullUrl);
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
                            else idCell.FormulaR1C1 = "Error processing this structure (it may still register OK)";
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
                        if (cellForResults != null)
                        {
                            if (r.Content.Length == 1)
                            {
                                cellForResults.FormulaR1C1 = "structure has 1 duplicate";
                            }
                            else if (r.Content.Length > 1)
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

        public static async Task<string> SaveBinaryFileAndDisplay(string filePath, Range cell,
            string serverUrl, string userName, string authKey)
        {

            if (RestClient.BaseAddress == null) RestClient.BaseAddress = new Uri(serverUrl);
            string fullUrl = serverUrl + "upload";
            log.DebugFormat("{0} using URL {1}", MethodBase.GetCurrentMethod().Name, fullUrl);
            //HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, fullUrl);
            //MultipartContent multipartContent = new MultipartContent();

            string boundary = "Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture);
            MultipartFormDataContent multipartForm = new MultipartFormDataContent(boundary);

            //multipartForm.Headers.ContentType.MediaType = "multipart/form-data";
            //message.Headers.Add("auth-userName", userName);
            //message.Headers.Add("auth-key", authKey);
            Stream fileStream = File.OpenRead(filePath);
            FileInfo file = new FileInfo(filePath);
            log.DebugFormat("Uploading file with name: {0}", file.Name);
            multipartForm.Add(new StreamContent(fileStream), "file-name", file.Name);
            //byte[] data = FileUtils.GetFileData(filePath);
            //ByteArrayContent byteArrayContent = new ByteArrayContent(data);
            //multipartForm.Add(byteArrayContent, "file-name");
            multipartForm.Add(new StringContent("application/octet-stream"), "file-type");
            multipartForm.Headers.Add("auth-userName", userName);
            multipartForm.Headers.Add("auth-key", authKey);
            //message.Content = multipartForm;

            RestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
            using (HttpResponseMessage response = await RestClient.PostAsync(fullUrl, multipartForm))
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        FilePostReturn r = await response.Content.ReadAsAsync<FilePostReturn>();
                        if (!string.IsNullOrWhiteSpace(r.url))
                        {
                            if (cell != null)
                            {
                                cell.FormulaR1C1 = r.url;
                            }
                            multipartForm.Dispose();
                            return r.url;
                        }
                        log.Debug("Error saving processing file: " + response.ReasonPhrase);
                        return string.Empty;
                    }
                    catch (Exception e2)
                    {
                        log.ErrorFormat("Error during file post: " + e2.Message);
                        throw e2;
                    }
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<string> UploadFile(string path, string serverUrl)
        {
            string fullUrl = serverUrl + "upload";
            Console.WriteLine("Uploading {0}", path);
            try
            {
                using (var client = new HttpClient())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        var content = new MultipartFormDataContent();
                        var file_content = new ByteArrayContent(new StreamContent(stream).ReadAsByteArrayAsync().Result);
                        file_content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        file_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = fileInfo.Name,
                            Name = "file-name",
                        };
                        content.Add(file_content, "file-name");
                        client.BaseAddress = new Uri(serverUrl);
                        var response = await client.PostAsync("upload", content);
                        response.EnsureSuccessStatusCode();
                        Console.WriteLine("Done");
                        return "OK";

                    }
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong while uploading the image");
            }
            return string.Empty;
        }

        internal static async Task<string> UploadFile(string userName, string authKey,
            string baseAddress, string path, string filePath, string contentType)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.Method = HttpMethod.Post;
                message.RequestUri = new Uri(baseAddress + path);
                message.Headers.Add("auth-username", userName);
                message.Headers.Add("auth-key", authKey);
                //Authorization = new AuthenticationHeaderValue("auth-username", userName);
                //message.Headers.Authorization = new AuthenticationHeaderValue("auth-key", authKey);

                var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));

                var streamContent = new StreamContent(File.OpenRead(filePath));
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(streamContent, "file-name");
                content.Add(new StringContent(contentType), "file-type");
                message.Content = content;

                var response =
                    await client.SendAsync(message);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();

                }
                else
                {
                    throw new ApplicationException($"{response.StatusCode} {response.ReasonPhrase}");
                }
            }

        }


        public static FilePostReturn ProcessFileSaveRequest(string url, string method, string filePath,
            string domain, string mimeType, Dictionary<string, string> headerData,
            bool binaryData)
        {
            string logMessage= string.Format(
                "Starting in Process Request. url: {0}, method: {1}, filePath: {2}, domain: {3}, mimetype: {4}, binaryData: {5}",
                url, method, filePath, domain, mimeType, binaryData);
            log.Debug(logMessage);
            HttpWebRequest request = BuildRequest(url, method, filePath, 
                "*/*", domain, mimeType, headerData, binaryData);
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException ex)
            {
                log.Error("Error posting file: " + ex.Message);
                log.Debug(ex.StackTrace);
                return new FilePostReturn
                {
                    id = "ERROR",
                    name = "Error: " + ex.Message
                };
            }
            StreamReader responseReader = new StreamReader(response.GetResponseStream());
            // Read the content.
            string output = responseReader.ReadToEnd();
            responseReader.Close();
            log.Debug("At end of Process Request");
            FilePostReturn postReturn = JSTools.GetFilePostReturnFromString(output);
            return postReturn;
        }

        public static HttpWebRequest BuildRequest(string url, string method, string filePath,
            string accept, string domain, string mimeType,
            Dictionary<string, string> headerData, bool binaryData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            foreach (string key in headerData.Keys)
            {
                request.Headers.Add(key, headerData[key]);
            }

            request.Method = method;
            string fileParameterName = "file-name";
            string uniquePart = Guid.NewGuid().ToString();
            string header = string.Format("--{0}", uniquePart);
            string footer = string.Format("--{0}--", uniquePart);
            string fileName = Path.GetFileName(filePath);
            
            request.ContentType = "multipart/form-data; boundary=" + uniquePart;
            //request.ContentLength = requestContents.ToString().Length + 20000;
            request.CookieContainer = new CookieContainer();
            //request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
            request.Headers.Add("DNT", "1");
            request.ServicePoint.Expect100Continue = false;
            string userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36";
            request.UserAgent = userAgent;
            request.Accept = accept;
            request.KeepAlive = true;
            request.ReadWriteTimeout = 3600000;
            request.Timeout = 3600000;

            using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.AutoFlush = true;
                requestWriter.WriteLine(header);
                requestWriter.WriteLine(String.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"", fileParameterName,
                        fileName));
                requestWriter.WriteLine(String.Format("Content-Type: {0}", mimeType));
                requestWriter.WriteLine();
                if (binaryData)
                {
                    byte[] fileContents = File.ReadAllBytes(filePath);
                    requestWriter.BaseStream.Write(fileContents, 0, fileContents.Length);
                    requestWriter.WriteLine();
                }
                else
                {
                    
                    string fileContents = File.ReadAllText(filePath);
                    requestWriter.WriteLine(fileContents);
                }
                Console.WriteLine("wrote file contents to stream");
                requestWriter.WriteLine(footer);
                requestWriter.Flush();
            }

            return request;
        }
    }
}
