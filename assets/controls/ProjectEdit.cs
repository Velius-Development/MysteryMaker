using System;
using System.Linq;
using System.Windows.Forms;

namespace MysteryMaker
{
    public partial class ProjectEdit : UserControl
    {
        string path;
        public ProjectEdit()
        {
            InitializeComponent();
            textBox1.Text = getValue("name");
            textBox4.Text = getValue("author");
            textBox7.Text = getValue("description");
        }

        private string getValue(string propName)
        {
            return Globals.Json.Value<string>(propName);
        }

        private void setValue(string propName, string value)
        {
            Globals.Json[propName] = value;
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
            setValue("author", textBox4.Text);
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            setValue("description", textBox7.Text);
        }
    }
}
