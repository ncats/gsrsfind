namespace ginasExcelUnitTests
{
    partial class TestRetrievalForm
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
            // Add a breakpoint or log statement here
            string sourceInfo = Environment.StackTrace;
            log.InfoFormat("Dispose called! Stack trace:{0} ", sourceInfo);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            try
            {
                base.Dispose(disposing);
            }
            catch (ObjectDisposedException)
            {
                // Ignore the exception if the object is already disposed

            }
        }

        #region Windows Form Designer generated code

            /// <summary>
            /// Required method for Designer support - do not modify
            /// the contents of this method with the code editor.
            /// </summary>
        private void InitializeComponent()
        {
            this.labelStatus = new System.Windows.Forms.Label();
            this.webViewTestForm = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(121, 35);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(87, 13);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "Status goes here";
            // 
            // webViewTestForm
            // 
            this.webViewTestForm.CreationProperties = null;
            this.webViewTestForm.Location = new System.Drawing.Point(0, 0);
            this.webViewTestForm.Name = "webViewTestForm";
            this.webViewTestForm.Size = new System.Drawing.Size(597, 375);
            this.webViewTestForm.TabIndex = 2;
            this.webViewTestForm.ZoomFactor = 1D;
            this.webViewTestForm.DefaultBackgroundColor = System.Drawing.Color.Orange;
            this.webViewTestForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // TestRetrievalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 366);
            this.Controls.Add(this.webViewTestForm);
            this.Controls.Add(this.labelStatus);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "TestRetrievalForm";
            this.Text = "MockRetrievalForm";
            this.ResumeLayout(false);
            this.PerformLayout();
            this.FormClosing += TestRetrievalForm_FormClosing;
        }

        #endregion
        private System.Windows.Forms.Label labelStatus;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewTestForm;
    }
}