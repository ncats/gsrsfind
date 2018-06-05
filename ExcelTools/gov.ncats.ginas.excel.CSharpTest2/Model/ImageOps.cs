using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.CSharpTest2.Model
{
    public class ImageOps
    {

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


        public bool hascomment(Range cell)
        {
            try
            {
                string.IsNullOrEmpty(cell.Comment.Text());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void AddImageCaption(Range cell, string url, int size)
        {
            if (!hascomment(cell)) cell.AddComment();
            string lfile = url;
            cell.Comment.Shape.Fill.UserPicture(lfile);
            cell.Comment.Shape.Fill.ForeColor.SchemeColor = 1;
            cell.Comment.Shape.Height = size / 4 * 3;
            cell.Comment.Shape.Width = size / 4 * 3;
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
            if(url.StartsWith("http") && url.Contains(imageFormat))
            {
                return true;
            }
            return false;
        }

    }

}
