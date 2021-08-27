using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MysteryMaker
{
    public partial class MysteriumEdit : UserControl
    {
        string path;
        public MysteriumEdit(string _path)
        {
            InitializeComponent();
            path = _path;
            textBox1.Text = getValue("name");
            textBox4.Text = getValue("image");
            textBox7.Text = getValue("description");
            label8.Text = "ID: " + _path.Split('.').Last<string>();
        }
        private string getValue(string propName)
        {
            return Globals.Json.SelectToken(path).Value<string>(propName);
        }
        private void setValue(string propName, string value)
        {
            Globals.Json.SelectToken(path)[propName] = value;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            setValue("name", textBox1.Text);
            if(Globals.formMain.treeView_Chpters.SelectedNode != null)
            {
                Globals.formMain.treeView_Chpters.SelectedNode.Text = textBox1.Text;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            setValue("image", textBox4.Text);
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            setValue("description", textBox7.Text);
        }
    }
}
