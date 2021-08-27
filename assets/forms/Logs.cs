using System;
using System.Windows.Forms;

namespace MysteryMaker.assets.forms
{
    public partial class FormLogs : Form
    {
        private string oldLogs;

        public FormLogs()
        {
            InitializeComponent();
            Globals.formLog = this;
            refresh();
        }

        public void refresh()
        {
            try
            {
                if(oldLogs != Globals.logs)
                {
                    var newLine = "";
                    if (oldLogs != null)
                        newLine = Globals.logs.Replace(oldLogs, "");
                    else
                        newLine = Globals.logs;

                    textBox1.AppendText(newLine);
                    textBox1.SelectionStart = textBox1.Text.Length;
                    textBox1.ScrollToCaret();

                    oldLogs = Globals.logs;
                }
            }
            catch(Exception ignore) { }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                textBox1.Text += "\r\n" + Globals.Json.ToString();
            }
        }

        private void FormLogs_FormClosed(object sender, FormClosedEventArgs e)
        {
            Globals.formLog = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            refresh();
        }
    }
}