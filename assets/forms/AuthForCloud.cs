using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace MysteryMaker.assets.forms
{
    partial class AuthForCloud : Form
    {
        private bool succeeded = false;

        public AuthForCloud()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if(succeeded)
                Close();


            HttpClient client = new HttpClient();
            /// POST ///

            var values = new Dictionary<string, string>
            {
                { "token", textBox1.Text }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://mysterymaker.zap106456-1.plesk05.zap-webspace.com/api/authForCloud.php", content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (responseString.StartsWith("TOKEN="))
            {
                Properties.Settings.Default.cloudPass = responseString.Replace("TOKEN=", "");
                Properties.Settings.Default.Save();
                label2.ForeColor = Color.Green;
                textBox1.Enabled = false;
                label2.Text = "Success: You got registered!";
                button1.Text = "Close this window";
                succeeded = true;
            }
            else
            {
                label2.Text = "Error: " + responseString;
            }
        }

        private void AuthForCloud_Load(object sender, EventArgs e)
        {
            Globals.formMain.Enabled = false;
        }

        private void AuthForCloud_FormClosed(object sender, FormClosedEventArgs e)
        {
            Globals.formMain.Enabled = true;
        }
    }
}
