namespace GSRSExcelTools.UI
{
    partial class RetrievalForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            log.Debug("Dispose");
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RetrievalForm));
            this.buttonResolve = new System.Windows.Forms.Button();
            this.buttonAddStructure = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxSaveDiagnostic = new System.Windows.Forms.CheckBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.checkBoxNewSheet = new System.Windows.Forms.CheckBox();
            this.buttonDebugDOM = new System.Windows.Forms.Button();
            this.labelServerURL = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.webViewGsrs = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewGsrs)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonResolve
            // 
            this.buttonResolve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResolve.Location = new System.Drawing.Point(131, 347);
            this.buttonResolve.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResolve.Name = "buttonResolve";
            this.buttonResolve.Size = new System.Drawing.Size(71, 19);
            this.buttonResolve.TabIndex = 1;
            this.buttonResolve.Text = "Resolve";
            this.buttonResolve.UseVisualStyleBackColor = true;
            this.buttonResolve.Click += new System.EventHandler(this.buttonResolve_Click);
            // 
            // buttonAddStructure
            // 
            this.buttonAddStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddStructure.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAddStructure.Enabled = false;
            this.buttonAddStructure.Location = new System.Drawing.Point(257, 347);
            this.buttonAddStructure.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAddStructure.Name = "buttonAddStructure";
            this.buttonAddStructure.Size = new System.Drawing.Size(93, 19);
            this.buttonAddStructure.TabIndex = 2;
            this.buttonAddStructure.Text = "Add structure(s)";
            this.buttonAddStructure.UseVisualStyleBackColor = true;
            this.buttonAddStructure.Visible = false;
            this.buttonAddStructure.Click += new System.EventHandler(this.buttonAddStructure_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(405, 347);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(56, 19);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // checkBoxSaveDiagnostic
            // 
            this.checkBoxSaveDiagnostic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSaveDiagnostic.AutoSize = true;
            this.checkBoxSaveDiagnostic.Location = new System.Drawing.Point(124, 421);
            this.checkBoxSaveDiagnostic.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSaveDiagnostic.Name = "checkBoxSaveDiagnostic";
            this.checkBoxSaveDiagnostic.Size = new System.Drawing.Size(202, 17);
            this.checkBoxSaveDiagnostic.TabIndex = 4;
            this.checkBoxSaveDiagnostic.Text = "Save diagnostic info when finished?  ";
            this.checkBoxSaveDiagnostic.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(270, 394);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(69, 13);
            this.labelStatus.TabIndex = 5;
            this.labelStatus.Text = "Status: ready";
            // 
            // checkBoxNewSheet
            // 
            this.checkBoxNewSheet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxNewSheet.AutoSize = true;
            this.checkBoxNewSheet.Location = new System.Drawing.Point(375, 421);
            this.checkBoxNewSheet.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxNewSheet.Name = "checkBoxNewSheet";
            this.checkBoxNewSheet.Size = new System.Drawing.Size(135, 17);
            this.checkBoxNewSheet.TabIndex = 6;
            this.checkBoxNewSheet.Text = "Resolve to new sheet?";
            this.checkBoxNewSheet.UseVisualStyleBackColor = true;
            // 
            // buttonDebugDOM
            // 
            this.buttonDebugDOM.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDebugDOM.Location = new System.Drawing.Point(492, 394);
            this.buttonDebugDOM.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDebugDOM.Name = "buttonDebugDOM";
            this.buttonDebugDOM.Size = new System.Drawing.Size(88, 19);
            this.buttonDebugDOM.TabIndex = 7;
            this.buttonDebugDOM.Text = "Debug DOM";
            this.buttonDebugDOM.UseVisualStyleBackColor = true;
            this.buttonDebugDOM.Click += new System.EventHandler(this.buttonDebugDOM_Click);
            // 
            // labelServerURL
            // 
            this.labelServerURL.AutoSize = true;
            this.labelServerURL.Location = new System.Drawing.Point(13, 372);
            this.labelServerURL.Name = "labelServerURL";
            this.labelServerURL.Size = new System.Drawing.Size(0, 13);
            this.labelServerURL.TabIndex = 8;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(19, 366);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 19);
            this.button1.TabIndex = 9;
            this.button1.Text = "Load...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // webViewGsrs
            // 
            this.webViewGsrs.AllowExternalDrop = true;
            this.webViewGsrs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webViewGsrs.CreationProperties = null;
            this.webViewGsrs.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewGsrs.Location = new System.Drawing.Point(19, 12);
            this.webViewGsrs.Name = "webViewGsrs";
            this.webViewGsrs.Size = new System.Drawing.Size(579, 315);
            this.webViewGsrs.TabIndex = 10;
            this.webViewGsrs.ZoomFactor = 1D;
            // 
            // RetrievalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(621, 442);
            this.Controls.Add(this.buttonDebugDOM);
            this.Controls.Add(this.checkBoxSaveDiagnostic);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelServerURL);
            this.Controls.Add(this.checkBoxNewSheet);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAddStructure);
            this.Controls.Add(this.buttonResolve);
            this.Controls.Add(this.webViewGsrs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RetrievalForm";
            this.Text = "RetrievalForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RetrievalForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.webViewGsrs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonResolve;
        private System.Windows.Forms.Button buttonAddStructure;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxSaveDiagnostic;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.CheckBox checkBoxNewSheet;
        private System.Windows.Forms.Button buttonDebugDOM;
        private System.Windows.Forms.Label labelServerURL;
        private System.Windows.Forms.Button button1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewGsrs;
    }
}