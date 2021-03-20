using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemarkableSync.OnenoteAddin
{
    public partial class SettingsForm : Form
    {
        private IConfigStore _configStore;

        public SettingsForm(string settingsRegPath)
        {
            _configStore = new WinRegistryConfigStore(settingsRegPath);
            InitializeComponent();
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
                btnRemarkableApply.Enabled = false;
            }
            else
            {
                Cursor.Current = Cursors.Default;
                btnMyScriptApply.Enabled = true;
                btnRemarkableApply.Enabled = true;
            }
        }
    }
}
