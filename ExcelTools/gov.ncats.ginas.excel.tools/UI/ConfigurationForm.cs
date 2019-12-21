using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using gov.ncats.ginas.excel.tools.Model;

namespace gov.ncats.ginas.excel.tools.UI
{
    public partial class ConfigurationForm : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GinasToolsConfiguration CurrentConfiguration
        {
            get;
            set;
        }

        public ConfigurationForm()
        {
            log.Debug("Starting in ConfigurationForm");
            InitializeComponent();
        }

        private void ComboBoxURLs_SelectedIndexChanged(object sender, EventArgs e)
        {
            int newIndex = comboBoxURLs.SelectedIndex;
            if (newIndex > -1)
            {
                GinasServer newServer = FindServerForUrl(comboBoxURLs.Items[newIndex].ToString());
                if (newServer != null)
                {
                    textBoxUsername.Text = newServer.Username;
                    textBoxKey.Text = newServer.PrivateKey;
                }
            }
        }

        private void ConfigurationForm_Load(object sender, EventArgs e)
        {
            CurrentConfiguration = Utils.FileUtils.GetGinasConfiguration();
            log.Debug("loaded configuration: " + CurrentConfiguration.ToString());
            try
            {
                DisplayCurrentConfiguration();
            }
            catch (Exception ex)
            {
                log.Error("Error loading configuration: " + ex.Message, ex);
            }

        }

        private void DisplayCurrentConfiguration()
        {
            log.Debug("DisplayCurrentConfiguration");
            comboBoxURLs.Items.Clear();
            CurrentConfiguration.Servers.ForEach(s => comboBoxURLs.Items.Add(s.ServerUrl));
            if (CurrentConfiguration.SelectedServer != null)
            {
                comboBoxURLs.SelectedItem = CurrentConfiguration.SelectedServer.ServerUrl;
                log.Debug(" set URL to " + CurrentConfiguration.SelectedServer.ServerUrl);
            }
            else
            {
                log.Debug(" selected server null");
            }

            textBoxBatchSize.Text = CurrentConfiguration.BatchSize.ToString();
            textBoxExpirationOffset.Text = CurrentConfiguration.ExpirationOffset.ToString("0.00");
            if (CurrentConfiguration.SelectedServer != null)
            {
                textBoxKey.Text = CurrentConfiguration.SelectedServer.PrivateKey;
                textBoxUsername.Text = CurrentConfiguration.SelectedServer.Username;
            }
            checkBoxDebugInfo.Checked = CurrentConfiguration.DebugMode;
            checkBoxSortVocabs.Checked = CurrentConfiguration.SortVocabsAlphabetically;
            comboBoxURLs.SelectedIndexChanged += ComboBoxURLs_SelectedIndexChanged;
            comboBoxURLs.TextChanged += ComboBoxURLs_TextChanged;

            textBoxImageSize.Text = CurrentConfiguration.StructureImageSize.ToString();
        }

        private void ComboBoxURLs_TextChanged(object sender, EventArgs e)
        {
            textBoxKey.Text = string.Empty;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (comboBoxURLs.SelectedIndex > -1)
            {
                CurrentConfiguration.SelectedServer = CurrentConfiguration.Servers[comboBoxURLs.SelectedIndex];
                CurrentConfiguration.SelectedServer.Username = textBoxUsername.Text;
                CurrentConfiguration.SelectedServer.PrivateKey = textBoxKey.Text;
            }
            else if (!string.IsNullOrWhiteSpace(comboBoxURLs.Text) && comboBoxURLs.Text.Length > 0)
            {
                if (!Utils.RestUtils.IsValidHttpUrl(comboBoxURLs.Text))
                {
                    Utils.UIUtils.ShowMessageToUser("Please make sure the URL starts with 'http://' or 'https://' ");
                    return;
                }
                GinasServer newServer = new GinasServer();
                newServer.ServerUrl = comboBoxURLs.Text;
                if (!newServer.ServerUrl.EndsWith("/")) newServer.ServerUrl = newServer.ServerUrl + "/";
                newServer.Username = textBoxUsername.Text;
                newServer.PrivateKey = textBoxKey.Text;
                newServer.ServerName = newServer.ServerUrl;
                CurrentConfiguration.Servers.Add(newServer);
                CurrentConfiguration.SelectedServer = newServer;
            }
            CurrentConfiguration.DebugMode = checkBoxDebugInfo.Checked;
            CurrentConfiguration.SortVocabsAlphabetically = checkBoxSortVocabs.Checked;
            CurrentConfiguration.BatchSize = Convert.ToInt32(textBoxBatchSize.Text);
            float tempFloat;
            if (float.TryParse(textBoxExpirationOffset.Text, out tempFloat))
            {
                CurrentConfiguration.ExpirationOffset = Convert.ToSingle(textBoxExpirationOffset.Text);
            }
            else
            {
                log.WarnFormat("Unable to get a number from text box value {0}",
                    textBoxExpirationOffset.Text);
            }

            if (float.TryParse(textBoxImageSize.Text, out tempFloat))
            {
                int structureImageSize = Convert.ToInt32(Math.Round(tempFloat));
                CurrentConfiguration.StructureImageSize = structureImageSize;
            }
            Utils.FileUtils.SaveGinasConfiguration(CurrentConfiguration);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        public GinasServer FindServerForUrl(string url)
        {
            foreach (GinasServer server in CurrentConfiguration.Servers)
            {
                if (server.ServerUrl.Equals(url, StringComparison.CurrentCultureIgnoreCase))
                {
                    return server;
                }
            }
            return null;
        }
    }
}
