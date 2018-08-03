namespace gov.ncats.ginas.excel.tools.UI
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
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.buttonResolve = new System.Windows.Forms.Button();
            this.buttonAddStructure = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxSaveDiagnostic = new System.Windows.Forms.CheckBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.checkBoxNewSheet = new System.Windows.Forms.CheckBox();
            this.buttonDebugDOM = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.AllowWebBrowserDrop = false;
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Top;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(593, 339);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.Visible = false;
            // 
            // buttonResolve
            // 
            this.buttonResolve.Location = new System.Drawing.Point(125, 349);
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
            this.buttonAddStructure.Enabled = false;
            this.buttonAddStructure.Location = new System.Drawing.Point(250, 349);
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
            this.buttonCancel.Location = new System.Drawing.Point(400, 349);
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
            this.checkBoxSaveDiagnostic.AutoSize = true;
            this.checkBoxSaveDiagnostic.Location = new System.Drawing.Point(106, 407);
            this.checkBoxSaveDiagnostic.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSaveDiagnostic.Name = "checkBoxSaveDiagnostic";
            this.checkBoxSaveDiagnostic.Size = new System.Drawing.Size(196, 17);
            this.checkBoxSaveDiagnostic.TabIndex = 4;
            this.checkBoxSaveDiagnostic.Text = "Save diagnostic info when finished?";
            this.checkBoxSaveDiagnostic.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(270, 381);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(69, 13);
            this.labelStatus.TabIndex = 5;
            this.labelStatus.Text = "Status: ready";
            // 
            // checkBoxNewSheet
            // 
            this.checkBoxNewSheet.AutoSize = true;
            this.checkBoxNewSheet.Location = new System.Drawing.Point(357, 407);
            this.checkBoxNewSheet.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxNewSheet.Name = "checkBoxNewSheet";
            this.checkBoxNewSheet.Size = new System.Drawing.Size(135, 17);
            this.checkBoxNewSheet.TabIndex = 6;
            this.checkBoxNewSheet.Text = "Resolve to new sheet?";
            this.checkBoxNewSheet.UseVisualStyleBackColor = true;
            // 
            // buttonDebugDOM
            // 
            this.buttonDebugDOM.Enabled = false;
            this.buttonDebugDOM.Location = new System.Drawing.Point(492, 381);
            this.buttonDebugDOM.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDebugDOM.Name = "buttonDebugDOM";
            this.buttonDebugDOM.Size = new System.Drawing.Size(88, 19);
            this.buttonDebugDOM.TabIndex = 7;
            this.buttonDebugDOM.Text = "Debug DOM";
            this.buttonDebugDOM.UseVisualStyleBackColor = true;
            this.buttonDebugDOM.Visible = false;
            this.buttonDebugDOM.Click += new System.EventHandler(this.buttonDebugDOM_Click);
            // 
            // RetrievalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(593, 431);
            this.Controls.Add(this.buttonDebugDOM);
            this.Controls.Add(this.checkBoxNewSheet);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.checkBoxSaveDiagnostic);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAddStructure);
            this.Controls.Add(this.buttonResolve);
            this.Controls.Add(this.webBrowser1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RetrievalForm";
            this.Text = "RetrievalForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RetrievalForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button buttonResolve;
        private System.Windows.Forms.Button buttonAddStructure;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxSaveDiagnostic;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.CheckBox checkBoxNewSheet;
        private System.Windows.Forms.Button buttonDebugDOM;
    }
}