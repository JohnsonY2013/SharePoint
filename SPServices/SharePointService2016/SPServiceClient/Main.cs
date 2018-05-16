using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace SPServiceClient
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        #region Site Methods

        private void btnGetCentralAdminUrl_Click(object sender, EventArgs e)
        {
            try
            {
                var webClient = new WebClient();
                if (chbJSON.Checked)
                {
                    //webClient.Headers.Add("Content-Type", "application/json");
                    //webClient.Headers.Add("Accept", "application/json");
                    webClient.Headers["Content-type"] = "application/json";
                    webClient.Headers["Accept"] = "application/json";
                }
                //webClient.Encoding = Encoding.UTF8;
                webClient.Credentials = Configuration.Credential;
                using (var reader = new StreamReader(webClient.OpenRead(Configuration.SiteServiceAddress + "/GetCentralAdminUrl")))
                {
                    txtBoxResult.Text = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = ex.Message;
            }
        }

        #endregion

        #region Results

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtBoxResult.Text);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtBoxResult.Clear();
        }

        #endregion

        private void btnGetSiteLock_Click(object sender, EventArgs e)
        {
            var webClient = new WebClient();
            if (chbJSON.Checked)
            {
                //webClient.Headers.Add("Content-Type", "application/json");
                //webClient.Headers.Add("Accept", "application/json");
                webClient.Headers["Content-type"] = "application/json";
                webClient.Headers["Accept"] = "application/json";
            }
            //webClient.Encoding = Encoding.UTF8;
            webClient.Credentials = Configuration.Credential;
            using (var reader = new StreamReader(webClient.OpenRead(Configuration.SiteServiceAddress + "/GetMock")))
            {
                txtBoxResult.Text = reader.ReadToEnd();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
