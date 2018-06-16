using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Excel = Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.tools.UI;
using gov.ncats.ginas.excel.tools.Model.Callbacks;
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
            stringBuilder.Append("Welcome to ginas Excel Tools!");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Some icons provided by ");
            stringBuilder.Append("https://www.flaticon.com/");
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
    }
}
