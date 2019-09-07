using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

namespace gov.ncats.ginas.excel.tools.Model.Callbacks
{
    public class Update2Callback :UpdateCallback
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Range molfileRange;
        new const XlRgbColor COLOR_STARTING = XlRgbColor.rgbGreen;
        new const XlRgbColor COLOR_ERROR = XlRgbColor.rgbAntiqueWhite;
        const XlRgbColor COLOR_HAS_MATCHES = XlRgbColor.rgbIndianRed;
        new const XlRgbColor COLOR_SUCCESS = XlRgbColor.rgbAquamarine;

        public GinasToolsConfiguration GinasConfiguration
        {
            get;
            set;
        }

        public Update2Callback(Range statusRange) : base(statusRange)
        {
            ParameterValues = new Dictionary<string, string>();
        }

        public Update2Callback(Range statusRange, Range molfileRangeParm) : base(statusRange)
        {
            ParameterValues = new Dictionary<string, string>();
            molfileRange = molfileRangeParm;
        }

        public override void Execute(dynamic o)
        {
            if (o is GinasResult result)
            {
                string message = result.message;
                string structIdIntro = "structureid=";
                int pos = message.IndexOf(structIdIntro);
                if (pos > -1)
                {
                    string structureId = message.Substring(pos + structIdIntro.Length);
                    log.DebugFormat("Isolated structure ID: {0}", structureId);

                    string structureImageUrl = GinasConfiguration.SelectedServer.ServerUrl 
                        + "img/" + structureId + ".png";
                    log.DebugFormat("using structure URL {0}", structureImageUrl);
                    int maxAttempts = 3;
                    int attempt = 0;
                    bool addedStructure = false;
                    while(!addedStructure && attempt < maxAttempts)
                    {
                        try
                        {
                            ImageOps.AddImageCaption(molfileRange, structureImageUrl,
                                GinasConfiguration.StructureImageSize);
                            addedStructure = true;
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.StackTrace);
                            if( attempt>= maxAttempts)
                            {
                                throw ex;
                            }
                            else
                            {
                                log.Error("Error adding structure image to cell. Will try again");
                                System.Threading.Thread.Sleep(3000);
                                attempt++;
                            }
                        }

                    }
                    if (result.matches == null || result.matches.Length == 0)
                    {
                        base.Execute("Unique");
                        molfileRange.EntireRow.Interior.Color = COLOR_SUCCESS;
                    }
                    else
                    {
                        base.Execute("Duplicate(s) found");
                        molfileRange.EntireRow.Interior.Color = COLOR_HAS_MATCHES;
                    }
                }
                else
                {
                    base.Execute("Error looking up this structure");
                    molfileRange.EntireRow.Interior.Color = COLOR_ERROR;
                }
            }

        }
    }
}
