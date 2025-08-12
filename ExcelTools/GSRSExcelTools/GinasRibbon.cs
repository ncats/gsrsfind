using GSRSExcelTools;
using GSRSExcelTools.Controller;
using GSRSExcelTools.Model;
using GSRSExcelTools.UI;
using GSRSExcelTools.Utils;
using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

using System.Reflection;
using System.Threading.Tasks;


namespace GSRSExcelTools
{
    public partial class GinasRibbon
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); // Add an Application Setting.

        private void ginas_Load(object sender, RibbonUIEventArgs e)
        {
            log.Debug("starting ginas_load");
            this.groupFDA.Visible = false;
            if (config.AppSettings.Settings["handleProductApplications"].Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                this.groupFDA.Visible = true;
            } else
            {
                Globals.ThisAddIn.TurnOffMonitoring();
            }
        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            log.Debug("click on 'Get Data'");
            RetrievalForm form = new RetrievalForm();
            Retriever retriever = new Retriever();
            retriever.SetStatusUpdater(form);
            retriever.CurrentOperationType = OperationType.Resolution;
            retriever.SetScriptExecutor( form);
            Excel.Window window = (Excel.Window) e.Control.Context;
            retriever.SetExcelWindow(window);
            
            form.CurrentOperationType = OperationType.Resolution;
            form.Controller = retriever;
            form.Visible = false;
            if (config.AppSettings.Settings["resolutionMode"].Value.Equals("synchronous", 
                StringComparison.InvariantCultureIgnoreCase))
            {
                log.Debug("resolving using modal dialog");
                form.ShowDialog();
            }
            else
            {
                log.Debug("resolving using modeless dialog");
                form.Show();
            }
                
        }


        private async void button2_Click(object sender, RibbonControlEventArgs e)
        {
            log.Debug("click on 'Get Structure'");
            Excel.Window window = e.Control.Context as Excel.Window;
            RetrievalForm form =await RetrievalForm.CreateInstance();
            form.Visible = false;
            form.CurrentOperationType = OperationType.GetStructures;
            Retriever retriever = new Retriever();
            retriever.CurrentOperationType = OperationType.GetStructures;
            retriever.SetStatusUpdater( form);
            retriever.SetScriptExecutor(form);
            retriever.SetExcelWindow(window);
            form.Controller = retriever;
            int delay = 50;
            GinasToolsConfiguration config = FileUtils.GetGinasConfiguration();
            if (config.PageBuildDelayMilliseconds > 0)
            {
                delay = config.PageBuildDelayMilliseconds;
            }

            while (! form.IsReady)
            {
                await Task.Delay(delay);
            }
            retriever.StartOperation();
            log.Debug("end of click handler");
        }


        private void button3_Click_1(object sender, RibbonControlEventArgs e)
        {
            log.Debug("click on 'Load Data'");
            Excel.Window window = e.Control.Context as Excel.Window;
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
            stringBuilder.Append("Welcome to the GSRS Excel Tools!");
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
            stringBuilder.Append("Running on .NET ");
            stringBuilder.Append(Assembly.GetExecutingAssembly().ImageRuntimeVersion);
            
            UIUtils.ShowMessageToUser(stringBuilder.ToString());
        }

        private void button4_Click(object sender, RibbonControlEventArgs e)
        {
            log.Debug("click on 'Create Loading Sheet'");
            Excel.Window window = e.Control.Context as Excel.Window;
            DataLoader loader = new DataLoader();
            loader.StartSheetCreation(window);
        }

        private void buttonSdFileImport_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            if ( !SheetUtils.IsSheetBlank( (Excel.Worksheet)window.Application.ActiveSheet))
            { 
                if( !UIUtils.GetUserYesNo("The current sheet already has data. Are you sure you want to overwrite it?"))
                {
                    return;
                }
            }

            try
            {
                string sdFilePath = UIUtils.GetUserFileSelection("SDF files (*.sdf)|*.sdf|SD files (*.sd)|*.sd|All files (*.*)|*.*",
     "Select one SD file");

                if (string.IsNullOrEmpty(sdFilePath)) return;

                SDFileProcessor sDFileProcessor = new SDFileProcessor();

                RetrievalForm form = new RetrievalForm();
                sDFileProcessor.SetScriptExecutor(form);
                form.CurrentOperationType = OperationType.ProcessSdFile;
                form.Controller = sDFileProcessor;
                sDFileProcessor.SetStatusUpdater(form);
                sDFileProcessor.HandleSDFileImport(sdFilePath, (Excel.Worksheet)window.Application.ActiveSheet);

            }
            catch(Exception ex)
            {
                UIUtils.ShowMessageToUser("Error during SD file import: " + ex.Message);
                log.Debug(ex.StackTrace);
            }
        }

        private void buttonSelectPT_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            SheetUtils.SetupPTColumn(window.ActiveCell);
        }

        private async void buttonAssureColumns_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            SDFileProcessor sDFileProcessor = new SDFileProcessor();

            RetrievalForm form = new RetrievalForm();
            sDFileProcessor.SetScriptExecutor(form);
            form.CurrentOperationType = OperationType.ProcessSdFile;
            form.Controller = sDFileProcessor;
            form.Visible = false;
            form.SetSize(1);
            form.Show();
            sDFileProcessor.SetStatusUpdater(form);
            sDFileProcessor.SetScriptExecutor(form);
            await sDFileProcessor.ManageSetupRemainingColumns();
        }

        private void buttonApplication_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            HandleApplicationProcessing(window, OperationType.ProcessApplication);
        }

        private void HandleApplicationProcessing(Excel.Window window, OperationType operationType)
        {
            ApplicationProcessor appProcessor = new ApplicationProcessor();

            RetrievalForm form = new RetrievalForm();
            appProcessor.SetScriptExecutor(form);
            appProcessor.SetExcelWindow(window);
            form.CurrentOperationType = operationType;
            form.Controller = appProcessor;
            form.Visible = false;
            form.SetSize(1);
            form.Show();
            appProcessor.SetStatusUpdater(form);
            appProcessor.CurrentOperationType = operationType;
            appProcessor.SetScriptExecutor(form);
            log.Debug("before appProcessor.StartOperation()");
            string operationUrl = "addApplication";
            if (operationType== OperationType.AddIngredient)
            { 
                operationUrl = "updateApplicationSave";
            }
            appProcessor.StartOperation(operationUrl);
        }
        private void button5_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            ApplicationSheetCreator sheetCreator = new ApplicationSheetCreator();
            sheetCreator.SetExcelWindow(window);
            RetrievalForm form = new RetrievalForm();
            form.CurrentOperationType = OperationType.ProcessApplication;
            form.Controller = sheetCreator;
            form.Visible = false;
            form.SetSize(1);
            sheetCreator.SetScriptExecutor(form);
            sheetCreator.SetStatusUpdater(form);
            form.Show();
            sheetCreator.CreateApplicationSheet();
        }

        private void buttonAddProduct_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            if( !ApplicationSheetCreator.IsApplicationWorksheet( (Excel.Worksheet) window.ActiveSheet))
            {
                return;
            }
            ApplicationSheetCreator sheetCreator = new ApplicationSheetCreator();
            sheetCreator.SetExcelWindow(window);
            RetrievalForm form = new RetrievalForm();
            form.CurrentOperationType = OperationType.ProcessApplication;
            form.Controller = sheetCreator;
            form.Visible = false;
            form.SetSize(1);
            sheetCreator.SetScriptExecutor(form);
            sheetCreator.SetStatusUpdater(form);
            form.Show();
            sheetCreator.CopySheet((Excel.Worksheet)window.ActiveSheet, true);
        }

        private void buttonAddIngredient_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            HandleApplicationProcessing(window, OperationType.AddIngredient);
        }

        private void CheckBoxMonitorSheets_Click(object sender, RibbonControlEventArgs e)
        {
            if( this.checkBoxMonitorSheets.Checked)
            {
                Globals.ThisAddIn.TurnOnMonitoring();
                log.Debug("turned monitoring on");
            }
            else
            {
                Globals.ThisAddIn.TurnOffMonitoring();
                log.Debug("turned monitoring off");
            }
        }

        private void ButtonGetInfo_Click(object sender, RibbonControlEventArgs e)
        {
            RetrievalForm form = new RetrievalForm();
            form.Show();
            System.Threading.Thread.Sleep(3000);
            string loc = form.ExecuteScript("window.location").ToString();
            UIUtils.ShowMessageToUser(loc);
        }

        private void buttonDnaToProtein_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            SequenceProcessor.StartDnaToProtein(window);
        }

        private void buttonDnaToRetrovirusRna_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            SequenceProcessor.StartDnaToRetrovirusRna(window);
        }

        private void buttonCreateIngredientSheet_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            ApplicationSheetCreator sheetCreator = new ApplicationSheetCreator();
            sheetCreator.SetExcelWindow(window);
            RetrievalForm form = new RetrievalForm();
            form.CurrentOperationType = OperationType.ProcessApplication;
            form.Controller = sheetCreator;
            form.Visible = false;
            form.SetSize(1);
            sheetCreator.SetScriptExecutor(form);
            sheetCreator.SetStatusUpdater(form);
            form.Show();
            sheetCreator.CreateIngredientSheet();
        }

        private async void button5_Click_1(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            PubChemRetriever retriever = new PubChemRetriever();
            ExternalSourceRetrievalProgress externalSourceStatus = new ExternalSourceRetrievalProgress();
            externalSourceStatus.SetSourceText("PubChem");
            externalSourceStatus.Show();
            retriever.SetExcelWindow(window);
            retriever.SetStatusUpdater(externalSourceStatus);
            await retriever.StartResolution();
        }

        private async void buttonGetMolfileFromChemSpider_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            ChemSpiderRetriever retriever = new ChemSpiderRetriever();
            ExternalSourceRetrievalProgress externalSourceStatus = new ExternalSourceRetrievalProgress();
            externalSourceStatus.SetSourceText("ChemSpider");
            externalSourceStatus.Show();
            retriever.SetExcelWindow(window);
            retriever.SetStatusUpdater(externalSourceStatus);
            await retriever.StartResolution();
        }

        private async void buttonLookupChemSpider_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Window window = e.Control.Context as Excel.Window;
            ChemSpiderRetriever retriever = new ChemSpiderRetriever();
            /*ExternalSourceRetrievalProgress externalSourceStatus = new ExternalSourceRetrievalProgress();
            externalSourceStatus.SetSourceText("ChemSpider");
            externalSourceStatus.Show();*/
            retriever.SetExcelWindow(window);
            //retriever.SetStatusUpdater(externalSourceStatus);
            await retriever.StartGeneralResolution();
        }
    }
}
