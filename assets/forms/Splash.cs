using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MysteryMaker.assets.forms
{
    public partial class Splash : Form
    {
        public bool isVisible = false;

        public Splash()
        {
            InitializeComponent();
            Updater.splashForm = this;
        }

        private void Splash_Load(object sender, EventArgs e)
        {
            BackColor = ColorTranslator.FromHtml("#2D3142");
            pictureBox1.BackColor = ColorTranslator.FromHtml("#2D3142");
            label1.ForeColor = ColorTranslator.FromHtml("#C1C2C7");
            isVisible = true;
        }

        public async Task load()
        {
            await Task.Run(() =>
            {
                while(Opacity < 1)
                    Thread.Sleep(1);


                setStatus("Checking for updates...");
                
                Globals.addToLogs(new Updater().CheckForUpdates());


                setStatus("Checking Temp-path's existance...");

                if (!Directory.Exists(Path.GetTempPath() + "/MysteryMaker"))
                {
                    setStatus("Creating Temp-path...");
                    Directory.CreateDirectory(Path.GetTempPath() + "/MysteryMaker");
                }

                setStatus("Checking if WebView drivers are installed...");

                if (!WebViewIsInstalled())
                {

                    setStatus("Storing WebView driver's installer...");

                    File.WriteAllBytes(Path.GetTempPath() + "/MysteryMaker/WebviewSetup.exe", Properties.Resources.WebviewSetup);
                    var installationProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            Verb = "runas",     //Administrator Permissions
                            FileName = Path.GetTempPath() + "/MysteryMaker/WebviewSetup.exe",
                            Arguments = "/silent /install",
                            UseShellExecute = true,
                            RedirectStandardOutput = false,
                            CreateNoWindow = false
                        }
                    };

                    setStatus("Installing WebView runtime...");

                    installationProcess.Start();
                    installationProcess.WaitForExit();

                    setStatus("Checking for successfull runtime install...");

                    if (!WebViewIsInstalled())  //if installation was cancelled
                    {
                        MessageBox.Show("The webview drivers have to be installed in order to run MysteryMaker", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        Application.Exit();
                        return;
                    }

                    setStatus("WebView runtime was successfully installed!");
                }


                isVisible = false;
                Globals.formMain.open();
                setStatus("Starting...");
            });
        }



        private bool WebViewIsInstalled()
        {

            try
            {
                var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                return true;

            }
            catch (WebView2RuntimeNotFoundException exception)
            {
                return false;
            }
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
                else
                {
                    this.Close();
                }
            }
        }

        public void setStatus(String status)
        {
            // Allow cross-threading
            this.label1.BeginInvoke((MethodInvoker)delegate () {
                this.label1.Text = status
            ; ;});
        }

    }
}
