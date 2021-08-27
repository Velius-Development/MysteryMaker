using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MysteryMaker.assets.forms
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            var settings = Properties.Settings.Default;
            checkBox1.Checked = settings.verboseDebug;
            checkBox2.Checked = settings.errorPopup;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var settings = Properties.Settings.Default;
            settings.verboseDebug = checkBox1.Checked;
            settings.Save();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            var settings = Properties.Settings.Default;
            settings.errorPopup = checkBox2.Checked;
            settings.Save();
        }
    }
}
