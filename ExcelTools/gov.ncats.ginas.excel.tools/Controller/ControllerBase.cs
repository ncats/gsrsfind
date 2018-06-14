using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using gov.ncats.ginas.excel.tools.Model;

using Excel = Microsoft.Office.Interop.Excel;
using gov.ncats.ginas.excel.tools.Model.Callbacks;

namespace gov.ncats.ginas.excel.tools.Controller
{
    public class ControllerBase : IDisposable
    {
        protected static int _checkInterval = 30;
        protected Timer _timer;
        protected int _totalScripts = 0;
        protected Excel.Range ExcelSelection;
        protected Excel.Window ExcelWindow;
        protected Queue<string> ScriptQueue;

        protected Dictionary<string, Callback> Callbacks;

        public void SetExcelWindow(Excel.Window window)
        {
            ExcelWindow = window;
        }

        public OperationType CurrentOperationType
        {
            get;
            set;
        }

        protected int ItemsPerBatch
        {
            get;
            set;
        }

        public bool KeepCheckingCallbacks
        {
            get;
            set;
        }

        protected IStatusUpdater StatusUpdater;

        public void SetStatusUpdater(IStatusUpdater statusUpdater)
        {
            StatusUpdater = statusUpdater;
        }

        public int GetBatchSize()
        {
            if (ToolsConfiguration != null && ToolsConfiguration.BatchSize > 0)
            {
                return ToolsConfiguration.BatchSize;
            }

            int batchSize;
            string batchSizeRaw = Properties.Resources.DefaultBatchSize;
            if (!int.TryParse(batchSizeRaw, out batchSize))
            {
                batchSize = 30;
            }
            return batchSize;
        }

        public void Dispose()
        {
            try
            {
                _timer.Dispose();
            }
            catch (Exception ignore)
            {
                Debug.WriteLine("Error disposing timer: " + ignore.Message);
            }
        }

        public GinasToolsConfiguration ToolsConfiguration = Utils.FileUtils.GetGinasConfiguration();
    }
}
