namespace GSRSExcelTools
{
    partial class GinasRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public GinasRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupFDA = this.Factory.CreateRibbonGroup();
            this.buttonCreateAppSheet = this.Factory.CreateRibbonButton();
            this.buttonApplication = this.Factory.CreateRibbonButton();
            this.buttonAddProduct = this.Factory.CreateRibbonButton();
            this.buttonCreateIngredientSheet = this.Factory.CreateRibbonButton();
            this.buttonAddIngredient = this.Factory.CreateRibbonButton();
            this.checkBoxMonitorSheets = this.Factory.CreateRibbonCheckBox();
            this.ginasTab = this.Factory.CreateRibbonTab();
            this.retrievalgroup = this.Factory.CreateRibbonGroup();
            this.button1 = this.Factory.CreateRibbonButton();
            this.button2 = this.Factory.CreateRibbonButton();
            this.groupSDFile = this.Factory.CreateRibbonGroup();
            this.buttonSdFileImport = this.Factory.CreateRibbonButton();
            this.buttonSelectPT = this.Factory.CreateRibbonButton();
            this.buttonAssureColumns = this.Factory.CreateRibbonButton();
            this.updateGroup = this.Factory.CreateRibbonGroup();
            this.button3 = this.Factory.CreateRibbonButton();
            this.button4 = this.Factory.CreateRibbonButton();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.buttonDnaToProtein = this.Factory.CreateRibbonButton();
            this.buttonDnaToRetrovirusRna = this.Factory.CreateRibbonButton();
            this.group2 = this.Factory.CreateRibbonGroup();
            this.buttonGetPubChemIDs = this.Factory.CreateRibbonButton();
            this.buttonGetMolfileFromChemSpider = this.Factory.CreateRibbonButton();
            this.buttonLookupChemSpider = this.Factory.CreateRibbonButton();
            this.configurationGroup = this.Factory.CreateRibbonGroup();
            this.buttonConfigure = this.Factory.CreateRibbonButton();
            this.buttonAbout = this.Factory.CreateRibbonButton();
            this.groupFDA.SuspendLayout();
            this.ginasTab.SuspendLayout();
            this.retrievalgroup.SuspendLayout();
            this.groupSDFile.SuspendLayout();
            this.updateGroup.SuspendLayout();
            this.group1.SuspendLayout();
            this.group2.SuspendLayout();
            this.configurationGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupFDA
            // 
            this.groupFDA.Items.Add(this.buttonCreateAppSheet);
            this.groupFDA.Items.Add(this.buttonApplication);
            this.groupFDA.Items.Add(this.buttonAddProduct);
            this.groupFDA.Items.Add(this.buttonCreateIngredientSheet);
            this.groupFDA.Items.Add(this.buttonAddIngredient);
            this.groupFDA.Items.Add(this.checkBoxMonitorSheets);
            this.groupFDA.Label = "FDA";
            this.groupFDA.Name = "groupFDA";
            this.groupFDA.Visible = false;
            // 
            // buttonCreateAppSheet
            // 
            this.buttonCreateAppSheet.Label = "Create App Sheet";
            this.buttonCreateAppSheet.Name = "buttonCreateAppSheet";
            this.buttonCreateAppSheet.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button5_Click);
            // 
            // buttonApplication
            // 
            this.buttonApplication.Label = "Load Application";
            this.buttonApplication.Name = "buttonApplication";
            this.buttonApplication.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonApplication_Click);
            // 
            // buttonAddProduct
            // 
            this.buttonAddProduct.Enabled = false;
            this.buttonAddProduct.Label = "Add Product";
            this.buttonAddProduct.Name = "buttonAddProduct";
            this.buttonAddProduct.Visible = false;
            this.buttonAddProduct.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonAddProduct_Click);
            // 
            // buttonCreateIngredientSheet
            // 
            this.buttonCreateIngredientSheet.Label = "Create Ingr. Sheet";
            this.buttonCreateIngredientSheet.Name = "buttonCreateIngredientSheet";
            this.buttonCreateIngredientSheet.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonCreateIngredientSheet_Click);
            // 
            // buttonAddIngredient
            // 
            this.buttonAddIngredient.Label = "Add Ingredient";
            this.buttonAddIngredient.Name = "buttonAddIngredient";
            this.buttonAddIngredient.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonAddIngredient_Click);
            // 
            // checkBoxMonitorSheets
            // 
            this.checkBoxMonitorSheets.Checked = true;
            this.checkBoxMonitorSheets.Label = "Monitor Sheets?";
            this.checkBoxMonitorSheets.Name = "checkBoxMonitorSheets";
            this.checkBoxMonitorSheets.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.CheckBoxMonitorSheets_Click);
            // 
            // ginasTab
            // 
            this.ginasTab.Groups.Add(this.retrievalgroup);
            this.ginasTab.Groups.Add(this.groupSDFile);
            this.ginasTab.Groups.Add(this.updateGroup);
            this.ginasTab.Groups.Add(this.group1);
            this.ginasTab.Groups.Add(this.groupFDA);
            this.ginasTab.Groups.Add(this.group2);
            this.ginasTab.Groups.Add(this.configurationGroup);
            this.ginasTab.Label = "GSRS";
            this.ginasTab.Name = "ginasTab";
            // 
            // retrievalgroup
            // 
            this.retrievalgroup.Items.Add(this.button1);
            this.retrievalgroup.Items.Add(this.button2);
            this.retrievalgroup.Label = "Retrieval";
            this.retrievalgroup.Name = "retrievalgroup";
            // 
            // button1
            // 
            this.button1.Image = global::GSRSExcelTools.Properties.Resources.SearchIcon;
            this.button1.Label = " Get Data";
            this.button1.Name = "button1";
            this.button1.ShowImage = true;
            this.button1.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Image = global::GSRSExcelTools.Properties.Resources.StructureIcon;
            this.button2.Label = " Get Structure(s)";
            this.button2.Name = "button2";
            this.button2.ShowImage = true;
            this.button2.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button2_Click);
            // 
            // groupSDFile
            // 
            this.groupSDFile.Items.Add(this.buttonSdFileImport);
            this.groupSDFile.Items.Add(this.buttonSelectPT);
            this.groupSDFile.Items.Add(this.buttonAssureColumns);
            this.groupSDFile.Label = "SD File";
            this.groupSDFile.Name = "groupSDFile";
            // 
            // buttonSdFileImport
            // 
            this.buttonSdFileImport.Image = global::GSRSExcelTools.Properties.Resources.StructureIcon;
            this.buttonSdFileImport.Label = "Import SD File";
            this.buttonSdFileImport.Name = "buttonSdFileImport";
            this.buttonSdFileImport.ShowImage = true;
            this.buttonSdFileImport.SuperTip = "Read a structure data format file into the current sheet";
            this.buttonSdFileImport.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonSdFileImport_Click);
            // 
            // buttonSelectPT
            // 
            this.buttonSelectPT.Label = "Select PT";
            this.buttonSelectPT.Name = "buttonSelectPT";
            this.buttonSelectPT.SuperTip = "Mark a column as Preferred Term for substance creation";
            this.buttonSelectPT.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonSelectPT_Click);
            // 
            // buttonAssureColumns
            // 
            this.buttonAssureColumns.Enabled = false;
            this.buttonAssureColumns.Label = "Assure Required Columns";
            this.buttonAssureColumns.Name = "buttonAssureColumns";
            this.buttonAssureColumns.Visible = false;
            this.buttonAssureColumns.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonAssureColumns_Click);
            // 
            // updateGroup
            // 
            this.updateGroup.Items.Add(this.button3);
            this.updateGroup.Items.Add(this.button4);
            this.updateGroup.Label = "Update";
            this.updateGroup.Name = "updateGroup";
            // 
            // button3
            // 
            this.button3.Image = global::GSRSExcelTools.Properties.Resources.DataLoadingIcon;
            this.button3.Label = " Load/Edit Data";
            this.button3.Name = "button3";
            this.button3.ScreenTip = "Set up a new sheet for data entry";
            this.button3.ShowImage = true;
            this.button3.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button3_Click_1);
            // 
            // button4
            // 
            this.button4.Image = global::GSRSExcelTools.Properties.Resources.CreateSheetIcon;
            this.button4.Label = " Create Editing Sheet";
            this.button4.Name = "button4";
            this.button4.ShowImage = true;
            this.button4.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button4_Click);
            // 
            // group1
            // 
            this.group1.Items.Add(this.buttonDnaToProtein);
            this.group1.Items.Add(this.buttonDnaToRetrovirusRna);
            this.group1.Label = "Sequences";
            this.group1.Name = "group1";
            this.group1.Visible = false;
            // 
            // buttonDnaToProtein
            // 
            this.buttonDnaToProtein.Label = "DNA to Protein";
            this.buttonDnaToProtein.Name = "buttonDnaToProtein";
            this.buttonDnaToProtein.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonDnaToProtein_Click);
            // 
            // buttonDnaToRetrovirusRna
            // 
            this.buttonDnaToRetrovirusRna.Label = "DNA to Retrovirus RNA";
            this.buttonDnaToRetrovirusRna.Name = "buttonDnaToRetrovirusRna";
            this.buttonDnaToRetrovirusRna.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonDnaToRetrovirusRna_Click);
            // 
            // group2
            // 
            this.group2.Items.Add(this.buttonGetPubChemIDs);
            this.group2.Items.Add(this.buttonGetMolfileFromChemSpider);
            this.group2.Items.Add(this.buttonLookupChemSpider);
            this.group2.Label = "External Data Sources";
            this.group2.Name = "group2";
            // 
            // buttonGetPubChemIDs
            // 
            this.buttonGetPubChemIDs.Label = "Get PubChem CIDs";
            this.buttonGetPubChemIDs.Name = "buttonGetPubChemIDs";
            this.buttonGetPubChemIDs.SuperTip = "This button will look up a set of InChIKeys and return the PubChem CID for each";
            this.buttonGetPubChemIDs.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button5_Click_1);
            // 
            // buttonGetMolfileFromChemSpider
            // 
            this.buttonGetMolfileFromChemSpider.Label = "Get Mol from ChemSpider";
            this.buttonGetMolfileFromChemSpider.Name = "buttonGetMolfileFromChemSpider";
            this.buttonGetMolfileFromChemSpider.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonGetMolfileFromChemSpider_Click);
            // 
            // buttonLookupChemSpider
            // 
            this.buttonLookupChemSpider.Label = "Look up in ChemSpider";
            this.buttonLookupChemSpider.Name = "buttonLookupChemSpider";
            this.buttonLookupChemSpider.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonLookupChemSpider_Click);
            // 
            // configurationGroup
            // 
            this.configurationGroup.Items.Add(this.buttonConfigure);
            this.configurationGroup.Items.Add(this.buttonAbout);
            this.configurationGroup.Label = "Configuration";
            this.configurationGroup.Name = "configurationGroup";
            // 
            // buttonConfigure
            // 
            this.buttonConfigure.Image = global::GSRSExcelTools.Properties.Resources.ConfigurationIcon;
            this.buttonConfigure.Label = " Configure";
            this.buttonConfigure.Name = "buttonConfigure";
            this.buttonConfigure.ShowImage = true;
            this.buttonConfigure.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonConfigure_Click);
            // 
            // buttonAbout
            // 
            this.buttonAbout.Label = "About..";
            this.buttonAbout.Name = "buttonAbout";
            this.buttonAbout.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonAbout_Click);
            // 
            // GinasRibbon
            // 
            this.Name = "GinasRibbon";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.ginasTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.ginas_Load);
            this.groupFDA.ResumeLayout(false);
            this.groupFDA.PerformLayout();
            this.ginasTab.ResumeLayout(false);
            this.ginasTab.PerformLayout();
            this.retrievalgroup.ResumeLayout(false);
            this.retrievalgroup.PerformLayout();
            this.groupSDFile.ResumeLayout(false);
            this.groupSDFile.PerformLayout();
            this.updateGroup.ResumeLayout(false);
            this.updateGroup.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.group2.ResumeLayout(false);
            this.group2.PerformLayout();
            this.configurationGroup.ResumeLayout(false);
            this.configurationGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab ginasTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup retrievalgroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button2;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup updateGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button3;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup configurationGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonConfigure;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonAbout;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton button4;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonSdFileImport;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonSelectPT;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupSDFile;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonAssureColumns;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonApplication;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonCreateAppSheet;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonAddProduct;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonAddIngredient;
        internal Microsoft.Office.Tools.Ribbon.RibbonCheckBox checkBoxMonitorSheets;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonDnaToProtein;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonDnaToRetrovirusRna;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonCreateIngredientSheet;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonGetPubChemIDs;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonGetMolfileFromChemSpider;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonLookupChemSpider;
        private Microsoft.Office.Tools.Ribbon.RibbonGroup groupFDA;
    }

    partial class ThisRibbonCollection
    {
        internal GinasRibbon ginas
        {
            get { return this.GetRibbon<GinasRibbon>(); }
        }
    }
}
