using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using gov.ncats.ginas.excel.tools.Model;
using gov.ncats.ginas.excel.tools.Model.FDAApplication;

namespace gov.ncats.ginas.excel.tools.Utils
{
    public class FileUtils
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetJavaScript()
        {
            //String javascriptFilePath = @"C:\ginas_source\Excel\CSharpTest2\gov.ncats.ginas.excel.tools\etc\ginas_controller.js";
            string javascriptFilePath = GetCurrentFolder() + @"\etc\g-srs_controller.js";
            return File.ReadAllText(javascriptFilePath);//.Replace("  ", "").Replace("\r\n", "");
        }

        public static string GetLodashJavaScript()
        {
            string javascriptFilePath = GetCurrentFolder() + @"\etc\lodash.min.js";
            return File.ReadAllText(javascriptFilePath);
        }

        public static string GetLastJavaScript()
        {
            string javascriptFilePath = GetCurrentFolder() + @"\etc\LastScript.js";
            return File.ReadAllText(javascriptFilePath);
        }

        public static string GetJsonPatchJavaScript()
        {
            string javascriptFilePath = GetCurrentFolder() + @"\etc\json-patch.js";
            return File.ReadAllText(javascriptFilePath);
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
            string styleFilePath = GetCurrentFolder() + @"\etc\g-srs_controller.css";
            return File.ReadAllText(styleFilePath);
        }

        public static void WriteToFile(string filePath, string stuff)
        {
            File.WriteAllText(filePath, stuff);
        }


        public static string ReadFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public static GinasToolsConfiguration GetGinasConfiguration()
        {
            log.Debug("Starting in " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            string userConfigPath = GetUserFolder() + @"\g-srs.config.json";
            log.Debug("userConfigPath: " + userConfigPath);
            string configFilePath = userConfigPath;
            if (!File.Exists(userConfigPath))
            {
                log.Debug("Unable to locate user configuration file " + configFilePath);
                configFilePath = GetCurrentFolder() + @"\etc\g-srs-config.txt";
            }
            string configString = File.ReadAllText(configFilePath);
            GinasToolsConfiguration config = null;
            try
            {
                config = JSTools.GetGinasToolsConfigurationFromString(configString);
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

            string configFilePath = GetUserFolder() + @"\g-srs.config.json";
            log.DebugFormat("config file path: {0}", configFilePath);
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
                + Path.DirectorySeparatorChar + "g-srs";
            DirectoryInfo folderInfo = new DirectoryInfo(folder);
            if (!folderInfo.Exists)
            {
                folderInfo.Create();
            }
            return folder;
        }

        public static string GetTemporaryFilePath(string extension)
        {
            string filePath = Path.GetTempPath() + Guid.NewGuid().ToString() + "."
                + extension;
            return filePath;
        }

        public static bool FolderExists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        public static List<ApplicationField> GetApplicationMetadata()
        {
            string javascriptFilePath = GetCurrentFolder() + @"\etc\application.metadata.json";
            string metadataString= File.ReadAllText(javascriptFilePath);
            return JSTools.GetApplicationMetadataFromString(metadataString);
        }

        public static long GetSize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }

        public static byte[] GetFileData(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public static string GetFileText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        //from https://stackoverflow.com/questions/910873/how-can-i-determine-if-a-file-is-binary-or-text-in-c#910929
        public static bool IsBinary(string path)
        {
            long length = GetSize(path);
            if (length == 0) return false;

            using (StreamReader stream = new StreamReader(path))
            {
                int ch;
                while ((ch = stream.Read()) != -1)
                {
                    if (IsControlChar(ch))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsControlChar(int ch)
        {
            return (ch > Chars.NUL && ch < Chars.BS)
                || (ch > Chars.CR && ch < Chars.SUB);
        }


        public static class Chars
        {
            public static char NUL = (char)0; // Null char
            public static char BS = (char)8; // Back Space
            public static char CR = (char)13; // Carriage Return
            public static char SUB = (char)26; // Substitute
        }

        public static Dictionary<string, string> GetNucleotideMasterData()
        {
            string nucleotideDataFilePath = GetCurrentFolder() + @"\etc\NucleotideData.txt";
            Dictionary<string, string> nucleotideData = new Dictionary<string, string>();
            
            string data= File.ReadAllText(nucleotideDataFilePath);
            string[] lines = data.Split('\n');
            foreach(string line in lines)
            {
                string[] tokens = line.Trim().Split(',');
                nucleotideData.Add(tokens[0], tokens[1]);
                nucleotideData.Add(tokens[2], tokens[3]);
                nucleotideData.Add(tokens[4], tokens[5]);
                nucleotideData.Add(tokens[6], tokens[7]);
            }
            return nucleotideData;
        }

        public static Dictionary<string, string> GetAminoAcidMasterData()
        {
            string nucleotideDataFilePath = GetCurrentFolder() + @"\etc\AminoAcidRepresentations.txt";
            Dictionary<string, string> aminoAcidData = new Dictionary<string, string>();

            string data = File.ReadAllText(nucleotideDataFilePath);
            string[] lines = data.Split('\n');
            foreach (string line in lines)
            {
                string[] tokens = line.Trim().Split('/');
                aminoAcidData.Add(tokens[0], tokens[1]);
            }
            return aminoAcidData;
        }

        public static string GetUniqueFileName(string extension)
        {
            var uniqueFileName = string.Format(@"{0}.{1}", Guid.NewGuid(),
                extension);
            return uniqueFileName;
        }

    }
}
