using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Excel = Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.UI;
using gov.ncats.ginas.excel.tools.Utils;
using gov.ncats.ginas.excel.tools.Controller;
using gov.ncats.ginas.excel.tools.Model;
using System.Reflection;

namespace gov.ncats.ginas.excel.tools
{
    public partial class GinasRibbon
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private void ginas_Load(object sender, RibbonUIEventArgs e)
        {
           
        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            log.Debug("click on 'Get Data'");
            RetrievalForm form = new RetrievalForm();
            Retriever retriever = new Retriever();
            retriever.SetStatusUpdater(form);
            retriever.CurrentOperationType = OperationType.Resolution;
            retriever.SetScriptExecutor( form);
            Excel.Window window = e.Control.Context;
            retriever.SetExcelWindow(window);
            
            form.CurrentOperationType = OperationType.Resolution;
            form.Controller = retriever;
            form.Visible = false;
            form.ShowDialog();
        }


        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
            log.Debug("click on 'Get Structure'");
            Excel.Window window = e.Control.Context;
            RetrievalForm form = new RetrievalForm();
            form.Visible = false;
            form.CurrentOperationType = OperationType.GetStructures;
            Retriever retriever = new Retriever();
            retriever.CurrentOperationType = OperationType.GetStructures;
            retriever.SetStatusUpdater( form);
            retriever.SetScriptExecutor(form);
            retriever.SetExcelWindow(window);
            form.Controller = retriever;
            retriever.StartOperation();
            //form.ShowDialog();
            log.Debug("end of click handler");
        }

        private void button3_Click(object sender, RibbonControlEventArgs e)
        {
        }

        private void button3_Click_1(object sender, RibbonControlEventArgs e)
        {
            log.Debug("click on 'Load Data'");
            Excel.Window window = e.Control.Context;
            DataLoader loader = new DataLoader();
            loader.SetExcelWindow(window);
            loader.StartOperation();
        }

       private void buttonConfigure_Click(object sender, RibbonControlEventArgs e)
       {
            log.Debug("click on 'Configure'");
            ConfigurationForm form = new ConfigurationForm();
            form.ShowDialog();
        }

        private void buttonAbout_Click(object sender, RibbonControlEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Welcome to g-srs Excel Tools!");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Some icons provided by ");
            stringBuilder.Append("https://www.flaticon.com/");
            //<div>Icons made by <a href="https://www.flaticon.com/authors/vaadin" title="Split">Split</a> from <a href="https://www.flaticon.com/"     title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/"     title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>")
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            String applicationName = Assembly.GetExecutingAssembly().FullName;
            stringBuilder.AppendLine("Technical details:");
            stringBuilder.Append(applicationName);
            stringBuilder.Append(Environment.NewLine);
            
            UIUtils.ShowMessageToUser(stringBuilder.ToString());
        }

        private void button4_Click(object sender, RibbonControlEventArgs e)
        {
            log.Debug("click on 'Create Loading Sheet'");
            Excel.Window window = e.Control.Context;
            DataLoader loader = new DataLoader();
            loader.StartSheetCreation(window);
        }

        private void buttonSdFileImport_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context;
            if ( !SheetUtils.IsSheetBlank( (Excel.Worksheet)window.Application.ActiveSheet))
            { 
                if( !UIUtils.GetUserYesNo("The current sheet already has data. Are you sure you want to overwrite it?"))
                {
                    return;
                }
            }

            string sdFilePath = UIUtils.GetUserFileSelection("SDF files (*.sdf)|*.sdf|SD files (*.sd)|*.sd|All files (*.*)|*.*",
                "Select one SD file");
            
            if (string.IsNullOrEmpty(sdFilePath)) return;

            SDFileProcessor sDFileProcessor = new SDFileProcessor();
            
            RetrievalForm form = new RetrievalForm();
            sDFileProcessor.SetScriptExecutor(form);
            form.CurrentOperationType = OperationType.ProcessSdFile;
            form.Controller = sDFileProcessor;
            form.Visible = false;
            form.SetSize(1);
            form.Show();
            sDFileProcessor.SetStatusUpdater(form);
            sDFileProcessor.HandleSDFileImport(sdFilePath, (Excel.Worksheet) window.Application.ActiveSheet);
        }

        private void buttonSelectPT_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context;
            SheetUtils.SetupPTColumn(window.ActiveCell);
        }

        private void buttonAssureColumns_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context;
            SDFileProcessor sDFileProcessor = new SDFileProcessor();

            RetrievalForm form = new RetrievalForm();
            sDFileProcessor.SetScriptExecutor(form);
            form.CurrentOperationType = OperationType.ProcessSdFile;
            form.Controller = sDFileProcessor;
            //form.Visible = false;
            //form.SetSize(1);
            form.Show();
            sDFileProcessor.SetStatusUpdater(form);
            sDFileProcessor.ManageSetupRemainingColumns();
        }
    }
}
