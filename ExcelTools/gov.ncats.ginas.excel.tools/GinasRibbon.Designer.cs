namespace gov.ncats.ginas.excel.tools
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
            this.ginasTab = this.Factory.CreateRibbonTab();
            this.retrievalgroup = this.Factory.CreateRibbonGroup();
            this.button1 = this.Factory.CreateRibbonButton();
            this.button2 = this.Factory.CreateRibbonButton();
            this.updateGroup = this.Factory.CreateRibbonGroup();
            this.button3 = this.Factory.CreateRibbonButton();
            this.button4 = this.Factory.CreateRibbonButton();
            this.groupSDFile = this.Factory.CreateRibbonGroup();
            this.buttonSdFileImport = this.Factory.CreateRibbonButton();
            this.buttonSelectPT = this.Factory.CreateRibbonButton();
            this.buttonAssureColumns = this.Factory.CreateRibbonButton();
            this.configurationGroup = this.Factory.CreateRibbonGroup();
            this.buttonConfigure = this.Factory.CreateRibbonButton();
            this.buttonAbout = this.Factory.CreateRibbonButton();
            this.ginasTab.SuspendLayout();
            this.retrievalgroup.SuspendLayout();
            this.updateGroup.SuspendLayout();
            this.groupSDFile.SuspendLayout();
            this.configurationGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // ginasTab
            // 
            this.ginasTab.Groups.Add(this.retrievalgroup);
            this.ginasTab.Groups.Add(this.updateGroup);
            this.ginasTab.Groups.Add(this.groupSDFile);
            this.ginasTab.Groups.Add(this.configurationGroup);
            this.ginasTab.Label = "g-srs";
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
            this.button1.Image = global::gov.ncats.ginas.excel.tools.Properties.Resources.SearchIcon;
            this.button1.Label = " Get Data";
            this.button1.Name = "button1";
            this.button1.ShowImage = true;
            this.button1.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Image = global::gov.ncats.ginas.excel.tools.Properties.Resources.StructureIcon;
            this.button2.Label = " Get Structure(s)";
            this.button2.Name = "button2";
            this.button2.ShowImage = true;
            this.button2.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button2_Click);
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
            this.button3.Image = global::gov.ncats.ginas.excel.tools.Properties.Resources.DataLoadingIcon;
            this.button3.Label = " Load Data";
            this.button3.Name = "button3";
            this.button3.ShowImage = true;
            this.button3.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button3_Click_1);
            // 
            // button4
            // 
            this.button4.Image = global::gov.ncats.ginas.excel.tools.Properties.Resources.CreateSheetIcon;
            this.button4.Label = " Create Loading Sheet";
            this.button4.Name = "button4";
            this.button4.ShowImage = true;
            this.button4.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button4_Click);
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
            this.buttonSdFileImport.Image = global::gov.ncats.ginas.excel.tools.Properties.Resources.StructureIcon;
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
            // configurationGroup
            // 
            this.configurationGroup.Items.Add(this.buttonConfigure);
            this.configurationGroup.Items.Add(this.buttonAbout);
            this.configurationGroup.Label = "Configuration";
            this.configurationGroup.Name = "configurationGroup";
            // 
            // buttonConfigure
            // 
            this.buttonConfigure.Image = global::gov.ncats.ginas.excel.tools.Properties.Resources.ConfigurationIcon;
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
            this.ginasTab.ResumeLayout(false);
            this.ginasTab.PerformLayout();
            this.retrievalgroup.ResumeLayout(false);
            this.retrievalgroup.PerformLayout();
            this.updateGroup.ResumeLayout(false);
            this.updateGroup.PerformLayout();
            this.groupSDFile.ResumeLayout(false);
            this.groupSDFile.PerformLayout();
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
    }

    partial class ThisRibbonCollection
    {
        internal GinasRibbon ginas
        {
            get { return this.GetRibbon<GinasRibbon>(); }
        }
    }
}
