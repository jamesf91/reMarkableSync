using RemarkableSync.MyScript;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemarkableSync.OnenoteAddin
{
    public partial class SettingsForm : Form
    {
        public enum RmConnectionMethod
        {
            RmCloud = 0,
            Ssh = 1
        }

        public static readonly string RmConnectionMethodConfig = "RmConnectionMethod";

        private IConfigStore _configStore;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public SettingsForm(string settingsRegPath)
        {
            _configStore = new WinRegistryConfigStore(settingsRegPath);
            InitializeComponent();
            getConnectionMethodFromConfig();
        }

        private async void btnRemarkableApply_Click(object sender, EventArgs e)
        {
            if (textOtc.Text.Length == 0)
            {
                MessageBox.Show(this, "Please enter One Time Code for setting up a new reMarkable client.", "Enter One Time Code");
                return;
            }

            var dialogResult = MessageBox.Show(this, "Are you sure you want to register a new reMarkable tablet to import from and replace any existing devices?",
                                                "Confirm", MessageBoxButtons.YesNo);

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            ToggleLoadingIcon(true);

            string otc = textOtc.Text;
            RmCloudDataSource rmClient = new RmCloudDataSource(_configStore);
            bool registerResult = await rmClient.RegisterWithOneTimeCode(otc);

            ToggleLoadingIcon(false);

            if (registerResult)
            {
                MessageBox.Show(this, "New reMarkable device registered successfully", "Success");
                textOtc.Text = "";
                return;
            }
            else
            {
                MessageBox.Show(this, "Error registering new reMarkable device", "Failed");
                textOtc.Text = "";
                return;
            }
        }

        private async void btnMyScriptApply_Click(object sender, EventArgs e)
        {
            if (textAppKey.Text.Length == 0 || textHmacKey.Text.Length == 0)
            {
                MessageBox.Show(this, "Please enter your MyScript App key and HMAC key", "Enter MyScript Keys");
                return;
            }

            var dialogResult = MessageBox.Show(this, "Are you sure you want to replace any existing MyScript configuration?",
                                               "Confirm", MessageBoxButtons.YesNo);

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            string appKey = textAppKey.Text;
            string hmacKey = textHmacKey.Text;

            ToggleLoadingIcon(true);
            await Task.Run(() =>
            {
                MyScriptClient myScriptClient = new MyScriptClient(_configStore);
                myScriptClient.SetConfig(appKey, hmacKey);
            });
            ToggleLoadingIcon(false);

            MessageBox.Show(this, "MyScript configuration updated", "Success");
            textAppKey.Text = "";
            textHmacKey.Text = "";
        }

        private void ToggleLoadingIcon(bool show)
        {
            if (show)
            {
                Cursor.Current = Cursors.WaitCursor;
                btnMyScriptApply.Enabled = false;
                btnRemarkableCloudApply.Enabled = false;
            }
            else
            {
                Cursor.Current = Cursors.Default;
                btnMyScriptApply.Enabled = true;
                btnRemarkableCloudApply.Enabled = true;
            }
        }

        private void radioButtonRmCloud_CheckedChanged(object sender, EventArgs e)
        {
            tableLayoutRmCloud.Enabled = radioButtonRmCloud.Checked;
            if (radioButtonRmCloud.Checked && sender != null)
            {
                setRmConnectionMethod(RmConnectionMethod.RmCloud);
            }
        }

        private void radioButtonRmSsh_CheckedChanged(object sender, EventArgs e)
        {
            tableLayoutRmSsh.Enabled = radioButtonRmSsh.Checked;
            if (radioButtonRmSsh.Checked && sender != null)
            {
                setRmConnectionMethod(RmConnectionMethod.Ssh);
            }
        }

        private void btnRemarkableSshApply_Click(object sender, EventArgs e)
        {
            if (textSshPassword.Text.Length == 0)
            {
                MessageBox.Show(this, "Please enter your reMarkable SSH password", "Enter SSH Password");
                return;
            }

            Dictionary<string, string> mapConfigs = new Dictionary<string, string>();
            mapConfigs[RmSftpDataSource.SshPasswordConfig] = textSshPassword.Text;
            mapConfigs[RmSftpDataSource.SshHostConfig] = "10.11.99.1";      // hard-coded to USB connection IP

            if (_configStore.SetConfigs(mapConfigs))
            {
                MessageBox.Show(this, "SSH password saved.", "Success");
                return;
            }
            else
            {
                MessageBox.Show(this, "Error saving SSH password.", "Error");
                return;
            }
        }

        private void setRmConnectionMethod(RmConnectionMethod connectionMethod)
        {
            Logger.Debug($"Setting connection method to {connectionMethod.ToString()}");
            Dictionary<string, string> mapConfigs = new Dictionary<string, string>();
            mapConfigs[RmConnectionMethodConfig] = connectionMethod.ToString("d");
            _configStore.SetConfigs(mapConfigs);
        }

        private void getConnectionMethodFromConfig()
        {
            int connMethod = -1;
            try
            {
                string connMethodString = _configStore.GetConfig(RmConnectionMethodConfig);
                connMethod = Convert.ToInt32(connMethodString);
            }
            catch (Exception err)
            {
                Logger.Error($"Failed to get RmConnectionMethod config with err: {err.Message}");
                // will default to cloud
            }

            switch (connMethod)
            {
                case (int)SettingsForm.RmConnectionMethod.Ssh:
                    radioButtonRmCloud.Checked = false;
                    radioButtonRmSsh.Checked = true;
                    break;
                case (int)SettingsForm.RmConnectionMethod.RmCloud:
                default:
                    radioButtonRmCloud.Checked = true;
                    radioButtonRmSsh.Checked = false;
                    break;
            }
            radioButtonRmCloud_CheckedChanged(null, null);
            radioButtonRmSsh_CheckedChanged(null, null);
        }
    }
}
