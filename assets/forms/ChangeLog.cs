using System;
using System.Drawing;
using System.Windows.Forms;

namespace MysteryMaker.assets.forms
{
    public partial class ChangeLog : Form
    {
        public bool isVisible = false;

        public ChangeLog()
        {
            InitializeComponent();
        }

        private void ChangeLog_Load(object sender, EventArgs e)
        {
            Globals.formMain.Enabled = false;


            this.BackColor = ColorTranslator.FromHtml("#2D3142");
            webView.BackColor = ColorTranslator.FromHtml("#2D3142");

            Properties.Settings.Default.seenUpdateLog = true;
            Properties.Settings.Default.Save();
        }

        private void ChangeLog_FormClosed(object sender, FormClosedEventArgs e)
        {
            Globals.formMain.Enabled = true;
        }

        private void ChangeLog_Shown(object sender, EventArgs e)
        {
            this.BringToFront();
            this.Focus();
            this.Select();
        }

        private void opacityAnimTimer_Tick(object sender, EventArgs e)
        {
            if (isVisible)
            {
                if (Opacity < 1)
                {
                    Opacity += 0.03 + Opacity / 5;
                }
            }
            else
            {
                if (Opacity > 0)
                {
                    Opacity -= 0.03 + Opacity / 5;
                }
            }
        }

        private void webView_ContentLoading(object sender, Microsoft.Web.WebView2.Core.CoreWebView2ContentLoadingEventArgs e)
        {           //Why does content loadED event not exist ?
            isVisible = true;
        }
    }
}
