using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using GSRSExcelTools.Controller;
using GSRSExcelTools.Utils;

namespace GSRSExcelTools
{
    public partial class ThisAddIn
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IController sheetMonitor = new ApplicationSheetCreator();
        private Model.IScriptExecutor scriptExecutor;
        private bool listening = true;

        public bool Listening
        {
            get
            {
                log.Debug("Listening about to return " + listening);
                return listening;
            }
        }
        public void TurnOnMonitoring()
        {
            log.Debug("starting in TurnOnMonitoring");
            scriptExecutor = new UI.RetrievalForm("Unable to connect to server.  GSRS functionality is not available until a connection can be established.",
            "When the GSRS server is available again, please restart Excel");
            listening = true;
            sheetMonitor.SetScriptExecutor(scriptExecutor);
            if (Application.ActiveSheet != null)
            {
                log.Debug("Turning on monitoring for current worksheet");
                HandleSheet(Application.ActiveSheet);
            }
        }

        public void TurnOffMonitoring()
        {
            log.Debug("starting in TurnOffMonitoring");
            listening = false;
            Application.SheetBeforeRightClick -= Application_SheetBeforeRightClick;
            //Application.SheetActivate -= Application_SheetActivate;
            //Application.WorkbookActivate -= Application_WorkbookActivate;
            scriptExecutor = null;
            if (Application.ActiveSheet != null) StopHandlingSheet(Application.ActiveSheet);
        }


        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            log.Debug("starting in ThisAddIn_Startup");
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }
        internal void HandleSheet(object possibleSheet)
        {
            if (!(possibleSheet is Excel.Worksheet)) return;
            Excel.Worksheet worksheet = (Excel.Worksheet)possibleSheet;
            if (ApplicationSheetCreator.IsApplicationWorksheet(worksheet))
            {
                log.Debug("application sheet detected");
                worksheet.Change += Worksheet_Change;
            }
        }

        internal void StopHandlingSheet(object possibleSheet)
        {
            if (!(possibleSheet is Excel.Worksheet)) return;
            Excel.Worksheet worksheet = (Excel.Worksheet)possibleSheet;
            if (ApplicationSheetCreator.IsApplicationWorksheet(worksheet))
            {
                log.Debug("application sheet detected");
                worksheet.Change -= Worksheet_Change;
            }
        }

        private void Worksheet_Change(Excel.Range Target)
        {
            //cheesy cast but it works
            (sheetMonitor as ApplicationSheetCreator).HandleChange(Target);
        }

        private void Application_SheetBeforeRightClick(object Sh, Excel.Range Target, ref bool Cancel)
        {
            GetCellContextMenu().Reset();
            AddMenuItem();
        }

        private void AddMenuItem()
        {
            Office.MsoControlType menuItem = Office.MsoControlType.msoControlButton;

            Office.CommandBarButton exampleMenuItem = (Office.CommandBarButton)GetCellContextMenu().Controls.Add(menuItem, missing, missing, 1, true);
            //Office.CommandBarButton)GetCellContextMenu().Controls.Add()

            exampleMenuItem.Style = Office.MsoButtonStyle.msoButtonCaption;
            exampleMenuItem.Caption = "Mark Column as Preferred Term";
            exampleMenuItem.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(SetPTColumn);
        }

        private void SetPTColumn(Office.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            Excel.Application excelApp = Ctrl.Application as Excel.Application;
            Excel.Range selectionRange = excelApp.Selection as Excel.Range;
            SheetUtils.SetupPTColumn(selectionRange);
        }

        private Office.CommandBar GetCellContextMenu()
        {
            //foreach( Microsoft.Office.Core.CommandBar bar in Application.CommandBars)
            //{
            //    log.DebugFormat("CommandBar Name: {0}", bar.Name);
            //}
            return this.Application.CommandBars["Column"];
        }


        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
