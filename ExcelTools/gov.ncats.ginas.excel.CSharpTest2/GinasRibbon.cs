using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Excel = Microsoft.Office.Interop.Excel;

using gov.ncats.ginas.excel.CSharpTest2.UI;
using gov.ncats.ginas.excel.CSharpTest2.Model.Callbacks;
using gov.ncats.ginas.excel.CSharpTest2.Utils;
using gov.ncats.ginas.excel.CSharpTest2.Controller;
using gov.ncats.ginas.excel.CSharpTest2.Model;
using System.Reflection;

namespace gov.ncats.ginas.excel.CSharpTest2
{
    public partial class GinasRibbon
    {
        private void ginas_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            Debug.WriteLine("Click handler (debug)");
            Console.WriteLine("Click handler (console)");
            RetrievalForm form = new RetrievalForm();
            Retriever retriever = new Retriever();
            retriever.StatusUpdater = form;
            retriever.CurrentOperationType = OperationType.Resolution;
            retriever.SetScriptExecutor( form);
            Excel.Window window = e.Control.Context;
            retriever.SetExcelWindow(window);
            form.Controller = retriever;
            form.Show();
        }


        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context;
            Retriever retriever = new Retriever();
            retriever.StartOperation(window);
        }

        private void button3_Click(object sender, RibbonControlEventArgs e)
        {
        }

        private void button3_Click_1(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context;
            DataLoader loader = new DataLoader();
            loader.StartOperation(window);
        }

       private void buttonConfigure_Click(object sender, RibbonControlEventArgs e)
       {
            ConfigurationForm form = new ConfigurationForm();
            form.ShowDialog();
        }

        private void buttonAbout_Click(object sender, RibbonControlEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Welcome to ginas Excel Tools!");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            String applicationName = Assembly.GetExecutingAssembly().FullName;
            stringBuilder.AppendLine("Nerdy details:");
            stringBuilder.Append(applicationName);
            UIUtils.ShowMessageToUser(stringBuilder.ToString());
        }

        private void button4_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context;
            DataLoader loader = new DataLoader();
            loader.StartSheetCreation(window);
        }
    }
}
