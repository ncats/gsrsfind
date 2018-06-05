using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using gov.ncats.ginas.excel.tools.Model;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class FileUtils
    {
        public static string GetJavaScript()
        {
            //String javascriptFilePath = @"C:\ginas_source\Excel\CSharpTest2\gov.ncats.ginas.excel.tools\etc\ginas_controller.js";
            string javascriptFilePath = GetCurrentFolder() + @"\etc\ginas_controller.js";
            return File.ReadAllText(javascriptFilePath);//.Replace("  ", "").Replace("\r\n", "");
        }

        public static string GetMinJavaScript()
        {
            string javascriptFilePath = GetCurrentFolder() + @"\etc\ginas_min.js";
            return File.ReadAllText(javascriptFilePath).Replace("  ", "").Replace("\r\n", "");
        }

        public static string GetPartialJavaScript()
        {
            string javascriptFilePath = GetCurrentFolder() + @"\etc\ginas_controller p1a.js";
            return File.ReadAllText(javascriptFilePath);
        }

        public static string GetLastJavaScript()
        {
            string javascriptFilePath = GetCurrentFolder() + @"\etc\LastScript.js";
            return File.ReadAllText(javascriptFilePath);
        }

        public static string GetHtml()
        {
            String htmlFilePath = GetCurrentFolder() + @"\etc\ginas_controller.html";
            if (!File.Exists(htmlFilePath))
            {
                System.Windows.Forms.MessageBox.Show("HTML file not found!");
                return "";
            }
            //@"C:\ginas_source\Excel\CSharpTest2\gov.ncats.ginas.excel.tools\etc\ginas_controller.html";
            return File.ReadAllText(htmlFilePath);
        }

        public static string GetErrorHtml()
        {
            String htmlFilePath = GetCurrentFolder() + @"\etc\error page.html";
            if (!File.Exists(htmlFilePath))
            {
                System.Windows.Forms.MessageBox.Show("HTML file not found!");
                return "";
            }
            return File.ReadAllText(htmlFilePath);
        }


        public static string getJQueryCode()
        {
            string filePath = @"C:\downloads\jquery\jquery-1.12.4.js";
            return File.ReadAllText(filePath);
        }

        public static string GetCss()
        {
            string styleFilePath = GetCurrentFolder() + @"\etc\ginas_controller.css";
            return File.ReadAllText(styleFilePath);
        }

        public static void WriteToFile(string filePath, string stuff)
        {
            File.WriteAllText(filePath, stuff);
        }

        public static string BuildScriptFromFile()
        {
            string sourcePath = GetCurrentFolder() + @"\etc\BuildPage.js";
            string[] lines = File.ReadAllLines(sourcePath);
            return string.Join(Environment.NewLine, lines);
        }

        public static GinasToolsConfiguration GetGinasConfiguration()
        {
            string userConfigPath = GetUserFolder() + @"\ginas.config.json";
            string configFilePath = userConfigPath;
            if (!File.Exists(userConfigPath))
            {
                configFilePath = GetCurrentFolder() + @"\etc\ginas.config.json";
            }
            string configString = File.ReadAllText(configFilePath);
            return JSTools.GetGinasToolsConfigurationFromString(configString);
        }

        public static void SaveGinasConfiguration(GinasToolsConfiguration config)
        {
            string configFilePath = GetUserFolder() + @"\ginas.config.json";
            string configString = JSTools.GetStringFromGinasToolsConfiguration(config);
            File.WriteAllText(configFilePath, configString);
        }

        private static string GetCurrentFolder()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetUserFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                + Path.DirectorySeparatorChar + "ginas";
            DirectoryInfo folderInfo = new DirectoryInfo(folder);
            if (!folderInfo.Exists)
            {
                folderInfo.Create();
            }
            return folder;
        }
    }
}
