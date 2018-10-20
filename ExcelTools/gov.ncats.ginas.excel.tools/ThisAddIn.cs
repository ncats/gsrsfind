using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;

using gov.ncats.ginas.excel.tools.Utils;

namespace gov.ncats.ginas.excel.tools
{
    public partial class ThisAddIn
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Application.SheetBeforeRightClick += Application_SheetBeforeRightClick;
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
