using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GSRSExcelTools.Model;

namespace GSRSExcelTools.UI
{
    public partial class ExternalSourceRetrievalProgress : Form, IStatusUpdater
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool userHasCancelled = false;

        public GinasToolsConfiguration CurrentConfiguration
        {
            get;
            set;
        }

        public ExternalSourceRetrievalProgress()
        {
            log.Debug("Starting in ExternalSourceRetrievalProgress");
            InitializeComponent();
        }



        private void ExternalSourceRetrievalProgress_Load(object sender, EventArgs e)
        {
            CurrentConfiguration = Utils.FileUtils.GetGinasConfiguration();
            log.Debug("loaded configuration: " + CurrentConfiguration.ToString());
            try
            {
            }
            catch (Exception ex)
            {
                log.Error("Error loading configuration: " + ex.Message, ex);
            }

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            userHasCancelled = true;
            Close();
        }

        public void UpdateStatus(string message)
        {
            labelStatusText.Text = message;
        }

        public void Complete()
        {
            log.Debug("Will close dialog");
            buttonOK.Text = "Close"; 
            buttonOK.Visible = true;
            buttonOK.Enabled = true;
        }

        public bool GetDebugSetting()
        {
            return CurrentConfiguration.DebugMode;
        }

        public void SetSourceText(string sourceText)
        {
            labelDataSource.Text = sourceText;
        }

        public bool HasUserCancelled()
        {
            return userHasCancelled;
        }
    }
}
