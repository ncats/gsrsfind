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
        public GinasToolsConfiguration CurrentConfiguration
        {
            get;
            set;
        }

        public ConfigurationForm()
        {
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
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            comboBoxURLs.Items.Clear();
            CurrentConfiguration.Servers.ForEach(s => comboBoxURLs.Items.Add(s.ServerUrl));
            if(CurrentConfiguration.SelectedServer != null)
            {
                comboBoxURLs.SelectedItem = CurrentConfiguration.SelectedServer.ServerUrl;
            }
            
            textBoxBatchSize.Text = CurrentConfiguration.BatchSize.ToString();
            textBoxKey.Text = CurrentConfiguration.SelectedServer.PrivateKey;
            textBoxUsername.Text = CurrentConfiguration.SelectedServer.Username;
            checkBoxDebugInfo.Checked = CurrentConfiguration.DebugMode;
            comboBoxURLs.SelectedIndexChanged += ComboBoxURLs_SelectedIndexChanged;
            comboBoxURLs.TextChanged += ComboBoxURLs_TextChanged;
        }

        private void ComboBoxURLs_TextChanged(object sender, EventArgs e)
        {
            textBoxKey.Text = string.Empty;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if(comboBoxURLs.SelectedIndex > -1)
            {
                CurrentConfiguration.SelectedServer = CurrentConfiguration.Servers[comboBoxURLs.SelectedIndex];
                CurrentConfiguration.SelectedServer.Username = textBoxUsername.Text;
                CurrentConfiguration.SelectedServer.PrivateKey = textBoxKey.Text;
            }
            else if( !string.IsNullOrWhiteSpace(comboBoxURLs.Text) && comboBoxURLs.Text.Length >0)
            {
                GinasServer newServer = new GinasServer();
                newServer.ServerUrl = comboBoxURLs.Text;
                newServer.Username = textBoxUsername.Text;
                newServer.PrivateKey = textBoxKey.Text;
                newServer.ServerName = newServer.ServerUrl;
                CurrentConfiguration.Servers.Add(newServer);
                CurrentConfiguration.SelectedServer = newServer;
            }
            CurrentConfiguration.DebugMode = checkBoxDebugInfo.Checked;
            CurrentConfiguration.BatchSize = Convert.ToInt32(textBoxBatchSize.Text);
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
            foreach(GinasServer server in CurrentConfiguration.Servers)
            {
                if( server.ServerUrl.Equals(url, StringComparison.CurrentCultureIgnoreCase))
                {
                    return server;
                }
            }
            return null;
        }
    }
}
