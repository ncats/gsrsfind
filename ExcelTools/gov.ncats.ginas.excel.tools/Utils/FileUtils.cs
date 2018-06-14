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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetJavaScript()
        {
            //String javascriptFilePath = @"C:\ginas_source\Excel\CSharpTest2\gov.ncats.ginas.excel.tools\etc\ginas_controller.js";
            string javascriptFilePath = GetCurrentFolder() + @"\etc\ginas_controller.js";
            return File.ReadAllText(javascriptFilePath);//.Replace("  ", "").Replace("\r\n", "");
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

        public static string GetCss()
        {
            string styleFilePath = GetCurrentFolder() + @"\etc\ginas_controller.css";
            return File.ReadAllText(styleFilePath);
        }

        public static void WriteToFile(string filePath, string stuff)
        {
            File.WriteAllText(filePath, stuff);
        }

        public static GinasToolsConfiguration GetGinasConfiguration()
        {
            log.Debug("Starting in " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            string userConfigPath = GetUserFolder() + @"\ginas.config.json";
            string configFilePath = userConfigPath;
            if (!File.Exists(userConfigPath))
            {
                log.Debug("Unable to located user configuration file " + configFilePath);
                configFilePath = GetCurrentFolder() + @"\etc\ginas.config.json";
            }
            string configString = File.ReadAllText(configFilePath);
            log.Debug("configString: " + configString);
            GinasToolsConfiguration config = null;
            try
            {
                config = JSTools.GetGinasToolsConfigurationFromString(configString);
                log.Debug("converted config object: " + config.ToString());
            }
            catch (Exception ex)
            {
                log.Fatal("Error loading configuration: " + ex.Message, ex);
            }


            log.Debug("converted config string to config object");
            return config;
        }

        public static void SaveGinasConfiguration(GinasToolsConfiguration config)
        {
            log.Debug("Starting in " + System.Reflection.MethodBase.GetCurrentMethod().Name);

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
            log.Debug("Starting in " + System.Reflection.MethodBase.GetCurrentMethod().Name);

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
