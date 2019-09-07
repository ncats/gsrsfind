using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;

using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Controller;

namespace gov.ncats.ginas.excel.tools
{
    public partial class ThisAddIn
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IController sheetMonitor = new ApplicationSheetCreator();
        private Model.IScriptExecutor scriptExecutor;
        private bool listening = false;

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
            log.DebugFormat("ThisAddIn_Startup. Exe: {0}; Process: {1}; Module: {2}",
                System.AppDomain.CurrentDomain.FriendlyName, 
                System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Application.WorkbookActivate += Application_WorkbookActivate;
            Application.SheetBeforeRightClick += Application_SheetBeforeRightClick;
            Application.SheetActivate += Application_SheetActivate;
        }

        private void Application_WorkbookActivate(Excel.Workbook Wb)
        {
            if(listening)
            {
                HandleSheet(Wb.ActiveSheet);
            }
            else
            {
                StopHandlingSheet(Wb.ActiveSheet);
            }
        }

        private void Application_SheetActivate(object sheet)
        {
            log.Debug("starting in Application_SheetActivate");
            if (listening)
            {
                HandleSheet(sheet);
            }
            else
            {
                StopHandlingSheet(sheet);
            }
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
            SheetUtils.SetupPTColumn(Ctrl.Application.Selection);
        }

        private Office.CommandBar GetCellContextMenu()
        {
            //foreach( Microsoft.Office.Core.CommandBar bar in Application.CommandBars)
            //{
            //    log.DebugFormat("CommandBar Name: {0}", bar.Name);
            //}
            return this.Application.CommandBars["Column"];
        }
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
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
