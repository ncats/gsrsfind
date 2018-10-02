namespace gov.ncats.ginas.excel.tools.UI
{
    partial class ConfigurationForm
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
            this.components = new System.ComponentModel.Container();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxBatchSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxDebugInfo = new System.Windows.Forms.CheckBox();
            this.comboBoxURLs = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.textBoxKey = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxExpirationOffset = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.toolTipUrl = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxSortVocabs = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(201, 326);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.toolTipUrl.SetToolTip(this.buttonOK, "Proceed with operation");
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(377, 326);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.toolTipUrl.SetToolTip(this.buttonCancel, "Close this dialog with no further processing");
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Server URL:";
            // 
            // textBoxBatchSize
            // 
            this.textBoxBatchSize.Location = new System.Drawing.Point(87, 79);
            this.textBoxBatchSize.Name = "textBoxBatchSize";
            this.textBoxBatchSize.Size = new System.Drawing.Size(64, 20);
            this.textBoxBatchSize.TabIndex = 3;
            this.toolTipUrl.SetToolTip(this.textBoxBatchSize, "Number of records to process in each set");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Batch Size:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Debug?";
            // 
            // checkBoxDebugInfo
            // 
            this.checkBoxDebugInfo.AutoSize = true;
            this.checkBoxDebugInfo.Location = new System.Drawing.Point(85, 116);
            this.checkBoxDebugInfo.Name = "checkBoxDebugInfo";
            this.checkBoxDebugInfo.Size = new System.Drawing.Size(122, 17);
            this.checkBoxDebugInfo.TabIndex = 4;
            this.checkBoxDebugInfo.Text = "Display Debug Info?";
            this.toolTipUrl.SetToolTip(this.checkBoxDebugInfo, "Gives you the option to capture information that may be useful to developers");
            this.checkBoxDebugInfo.UseVisualStyleBackColor = true;
            // 
            // comboBoxURLs
            // 
            this.comboBoxURLs.FormattingEnabled = true;
            this.comboBoxURLs.Location = new System.Drawing.Point(87, 23);
            this.comboBoxURLs.Name = "comboBoxURLs";
            this.comboBoxURLs.Size = new System.Drawing.Size(365, 21);
            this.comboBoxURLs.TabIndex = 5;
            this.toolTipUrl.SetToolTip(this.comboBoxURLs, "Web address of g-srs server for information retrieval and submission");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(91, 52);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Username:";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(154, 50);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(113, 20);
            this.textBoxUsername.TabIndex = 1;
            this.toolTipUrl.SetToolTip(this.textBoxUsername, "user name when signing into the abover application (not necessary with single-sig" +
        "non)");
            // 
            // textBoxKey
            // 
            this.textBoxKey.Location = new System.Drawing.Point(337, 51);
            this.textBoxKey.Name = "textBoxKey";
            this.textBoxKey.Size = new System.Drawing.Size(113, 20);
            this.textBoxKey.TabIndex = 2;
            this.toolTipUrl.SetToolTip(this.textBoxKey, "(not necessary with single-signon)");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(290, 54);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Key:";
            // 
            // textBoxExpirationOffset
            // 
            this.textBoxExpirationOffset.Location = new System.Drawing.Point(315, 78);
            this.textBoxExpirationOffset.Name = "textBoxExpirationOffset";
            this.textBoxExpirationOffset.Size = new System.Drawing.Size(64, 20);
            this.textBoxExpirationOffset.TabIndex = 4;
            this.toolTipUrl.SetToolTip(this.textBoxExpirationOffset, "How long (seconds) to allow each set to run before considering it expired");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(215, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Expiration Delay:";
            // 
            // checkBoxSortVocabs
            // 
            this.checkBoxSortVocabs.AutoSize = true;
            this.checkBoxSortVocabs.Location = new System.Drawing.Point(230, 115);
            this.checkBoxSortVocabs.Name = "checkBoxSortVocabs";
            this.checkBoxSortVocabs.Size = new System.Drawing.Size(140, 17);
            this.checkBoxSortVocabs.TabIndex = 14;
            this.checkBoxSortVocabs.Text = "Sort New Vocabularies?";
            this.toolTipUrl.SetToolTip(this.checkBoxSortVocabs, "Sort newly created lists of terms alphabetically? ");
            this.checkBoxSortVocabs.UseVisualStyleBackColor = true;
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(477, 368);
            this.Controls.Add(this.checkBoxSortVocabs);
            this.Controls.Add(this.textBoxExpirationOffset);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxKey);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxURLs);
            this.Controls.Add(this.checkBoxDebugInfo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxBatchSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Name = "ConfigurationForm";
            this.Text = "g-srs Excel Tools Configuration";
            this.Load += new System.EventHandler(this.ConfigurationForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxBatchSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxDebugInfo;
        private System.Windows.Forms.ComboBox comboBoxURLs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxKey;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxExpirationOffset;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolTip toolTipUrl;
        private System.Windows.Forms.CheckBox checkBoxSortVocabs;
    }
}