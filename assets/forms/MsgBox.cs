using System;
using System.Windows.Forms;

namespace MysteryMaker.assets.forms
{
    public partial class MsgBox : Form
    {
        public MsgBox(string title, string msg)
        {
            InitializeComponent();
            Text = title;
            richTextLabel1.Text = msg;
        }
    }
}
