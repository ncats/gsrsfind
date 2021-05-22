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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.toolTipUrl = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxDebugInfo = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxExpirationOffset = new System.Windows.Forms.TextBox();
            this.textBoxBatchSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxSortVocabs = new System.Windows.Forms.CheckBox();
            this.textBoxImageSize = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxURLs = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxKey = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(201, 287);
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
            this.buttonCancel.Location = new System.Drawing.Point(377, 287);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.toolTipUrl.SetToolTip(this.buttonCancel, "Close this dialog with no further processing");
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxSortVocabs);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBoxBatchSize);
            this.groupBox2.Controls.Add(this.textBoxExpirationOffset);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.checkBoxDebugInfo);
            this.groupBox2.Location = new System.Drawing.Point(12, 129);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(458, 100);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General";
            // 
            // checkBoxDebugInfo
            // 
            this.checkBoxDebugInfo.AutoSize = true;
            this.checkBoxDebugInfo.Location = new System.Drawing.Point(78, 64);
            this.checkBoxDebugInfo.Name = "checkBoxDebugInfo";
            this.checkBoxDebugInfo.Size = new System.Drawing.Size(122, 17);
            this.checkBoxDebugInfo.TabIndex = 4;
            this.checkBoxDebugInfo.Text = "Display Debug Info?";
            this.toolTipUrl.SetToolTip(this.checkBoxDebugInfo, "Gives you the option to capture information that may be useful to developers");
            this.checkBoxDebugInfo.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(208, 30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Expiration Delay:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Debug?";
            // 
            // textBoxExpirationOffset
            // 
            this.textBoxExpirationOffset.Location = new System.Drawing.Point(308, 26);
            this.textBoxExpirationOffset.Name = "textBoxExpirationOffset";
            this.textBoxExpirationOffset.Size = new System.Drawing.Size(64, 20);
            this.textBoxExpirationOffset.TabIndex = 4;
            this.toolTipUrl.SetToolTip(this.textBoxExpirationOffset, "How long (seconds) to allow each set to run before considering it expired");
            // 
            // textBoxBatchSize
            // 
            this.textBoxBatchSize.Location = new System.Drawing.Point(82, 27);
            this.textBoxBatchSize.Name = "textBoxBatchSize";
            this.textBoxBatchSize.Size = new System.Drawing.Size(64, 20);
            this.textBoxBatchSize.TabIndex = 3;
            this.toolTipUrl.SetToolTip(this.textBoxBatchSize, "Number of records to process in each set");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Batch Size:";
            // 
            // checkBoxSortVocabs
            // 
            this.checkBoxSortVocabs.AutoSize = true;
            this.checkBoxSortVocabs.Location = new System.Drawing.Point(223, 63);
            this.checkBoxSortVocabs.Name = "checkBoxSortVocabs";
            this.checkBoxSortVocabs.Size = new System.Drawing.Size(140, 17);
            this.checkBoxSortVocabs.TabIndex = 14;
            this.checkBoxSortVocabs.Text = "Sort New Vocabularies?";
            this.toolTipUrl.SetToolTip(this.checkBoxSortVocabs, "Sort newly created lists of terms alphabetically? ");
            this.checkBoxSortVocabs.UseVisualStyleBackColor = true;
            // 
            // textBoxImageSize
            // 
            this.textBoxImageSize.Location = new System.Drawing.Point(82, 25);
            this.textBoxImageSize.Name = "textBoxImageSize";
            this.textBoxImageSize.Size = new System.Drawing.Size(64, 20);
            this.textBoxImageSize.TabIndex = 5;
            this.toolTipUrl.SetToolTip(this.textBoxImageSize, "Number of records to process in each set");
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Image Size:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.textBoxImageSize);
            this.groupBox3.Location = new System.Drawing.Point(12, 236);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(458, 46);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Structures";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(285, 58);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Key:";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(149, 54);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(113, 20);
            this.textBoxUsername.TabIndex = 1;
            this.toolTipUrl.SetToolTip(this.textBoxUsername, "user name when signing into the abover application (not necessary with single-sig" +
        "non)");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(86, 56);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Username:";
            // 
            // comboBoxURLs
            // 
            this.comboBoxURLs.FormattingEnabled = true;
            this.comboBoxURLs.Location = new System.Drawing.Point(82, 27);
            this.comboBoxURLs.Name = "comboBoxURLs";
            this.comboBoxURLs.Size = new System.Drawing.Size(365, 21);
            this.comboBoxURLs.TabIndex = 5;
            this.toolTipUrl.SetToolTip(this.comboBoxURLs, "Web address of g-srs server for information retrieval and submission");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "URL:";
            // 
            // textBoxKey
            // 
            this.textBoxKey.Location = new System.Drawing.Point(332, 55);
            this.textBoxKey.Name = "textBoxKey";
            this.textBoxKey.Size = new System.Drawing.Size(113, 20);
            this.textBoxKey.TabIndex = 2;
            this.toolTipUrl.SetToolTip(this.textBoxKey, "(not necessary with single-signon)");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxKey);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBoxURLs);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxUsername);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(12, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 111);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server";
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(477, 319);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfigurationForm";
            this.Text = "g-srs Excel Tools Configuration";
            this.Load += new System.EventHandler(this.ConfigurationForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ToolTip toolTipUrl;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxSortVocabs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxBatchSize;
        private System.Windows.Forms.TextBox textBoxExpirationOffset;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxDebugInfo;
        private System.Windows.Forms.TextBox textBoxImageSize;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxURLs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxKey;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}