using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.Utils;

namespace gov.ncats.ginas.excel.tools.Model
{
    public class ImageOps
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        GinasToolsConfiguration configuration = FileUtils.GetGinasConfiguration();

        //public void AddStructureImageToCell(string Val, Range cell, int size)
        //{
        //    string format;
        //    string preset;
        //    format = GetFormat();
        //    preset = getPreset();

        //    string url;
        //    string lfile;
        //    if (!string.IsNullOrWhiteSpace(Val))
        //    {
        //        url = getImageURL(Val, size);
        //        lfile = url;
        //        if (format.Equals("eps"))
        //        {
        //            lfile = getTempFile(url, "eps");
        //        }
        //        else
        //        {
        //            lfile = getTempFile(url, "png");
        //        }
        //        AddImageCaption(cell, lfile, size);
        //    }

        //}


        public static bool hascomment(Range cell)
        {
            try
            {
                string.IsNullOrEmpty(cell.Comment.Text());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void AddImageCaption(Range cell, string url, int size)
        {
            try
            {
                log.Debug("starting in AddImageCaption");
                if (!hascomment(cell)) cell.AddComment();
                cell.Comment.Shape.Fill.UserPicture(url);
                log.Debug("set url");
                cell.Comment.Shape.Fill.ForeColor.SchemeColor = 1;
                cell.Comment.Shape.Height = size / 4 * 3;
                cell.Comment.Shape.Width = size / 4 * 3;
                log.Debug("finished setting up comment");
            }
            catch(Exception ex)
            {
                log.Error(string.Format("Error setting up image URL {0}", url), ex);
            }
        }

        public string getTempFile(String url, String suffix)
        {
            //consider alternative: https://stackoverflow.com/questions/581570/how-can-i-create-a-temp-file-with-a-specific-extension-with-net
            Random random = new Random();
            string str = Environment.GetEnvironmentVariable("Temp") + Path.DirectorySeparatorChar
                + random.Next(500) + "." + suffix;
            return str;
        }

        public bool Download_File(string vWebFile, String vLocalFile)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(vWebFile, vLocalFile);
            }
            return true;
        }

        public static bool IsImageUrl(string url)
        {
            string imageFormat = Properties.Resources.ImageFormat;
            if (url.StartsWith("http") && url.Contains("img/") && url.Contains(imageFormat))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// From https://stackoverflow.com/questions/924679/c-sharp-how-can-i-check-if-a-url-exists-is-valid#3808841
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool RemoteFileExists(string url)
        {
            DateTime start = DateTime.Now;
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                }
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                //Returns TRUE if the Status code == 200
                long contentLength = response.ContentLength;
                foreach( string key in response.Headers.AllKeys)
                {
                    log.DebugFormat("key: {0}; header: {1}", key, response.Headers[key]);
                }
                response.Close();

                TimeSpan elapsed = DateTime.Now.Subtract(start);
                log.DebugFormat("in {0}, length: {1}; duration {2}", System.Reflection.MethodBase.GetCurrentMethod().Name,
                    contentLength, elapsed.Milliseconds);
                return (response.StatusCode == HttpStatusCode.OK /*&& contentLength > 0*/);
            }
            catch (Exception ex)
            {
                log.DebugFormat("Error while looking up URL: {0}. URL: {1}", ex.Message, url);
                log.Debug(ex.StackTrace);
                //Any exception will returns false.
                return false;
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        public void CreateMolfileImage(Range cell, string molfile, Range cellForId)
        {
            string cleanMolfile = molfile.Replace("\r", "");
            string structureImageUrl = string.Empty;
            try
            {
                RestUtils.SaveMolfileAndDisplay(cleanMolfile, cell, configuration.SelectedServer.ServerUrl, cellForId);
                //string structureId = structureidTask.Result;
                //if (!string.IsNullOrEmpty(structureId))
                //{
                //    // image displayed by SaveMolfile
                //    //structureImageUrl = configuration.SelectedServer.ServerUrl + "img/" + structureId + ".png";
                //    //log.DebugFormat("using structure URL {0}", structureImageUrl);
                //    //AddImageCaption(cell, structureImageUrl, 300);
                //}
                //else
                //{
                //    log.Debug("Structure not saved");
                //}
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in CreateMolfileImage: {0}", ex.Message);
                log.Debug(ex.StackTrace);
            }
        }
    }

}
