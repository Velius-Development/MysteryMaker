using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace MysteryMaker
{
    public partial class ItemEdit : UserControl
    {
        string path;

        public ItemEdit(string _path)
        {
            InitializeComponent();
            path = _path;
            textBox1.Text = getValue("name");
            textBox7.Text = getValue("description");
            label8.Text = "ID: " + _path.Split('.').Last<string>();
            loadListBox();
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
            Globals.formMain.treeView_Chpters.SelectedNode.Text = textBox1.Text;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            setValue("description", textBox7.Text);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                var name = Globals.Json.SelectToken(path + ".stats." + (listBox1.SelectedIndex+1) + ".name").Value<string>();
                var value = Globals.Json.SelectToken(path + ".stats." + (listBox1.SelectedIndex+1) + ".value").Value<string>();
                textBox2.Text = value;
                textBox3.Text = name;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if(listBox1.SelectedItem != null) { 
                Globals.Json.SelectToken(path)["stats"][(listBox1.SelectedIndex+1).ToString()]["value"] = textBox2.Text;
            }
            loadListBox(listBox1.SelectedIndex);
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Add")
            {
                Globals.jHandler.createStat(path);
                loadListBox(listBox1.Items.Count);
            }
            else if(e.ClickedItem.Text == "Remove")
            {
                Globals.Json.SelectToken(path + ".stats." + (listBox1.SelectedIndex + 1)).Parent.Remove();


                
                var props = new List<JProperty>();
                foreach (JProperty o in Globals.Json.SelectToken(path + ".stats"))
                {
                    props.Add(o);
                }

                var count = 0;
                foreach (JProperty o in props)
                {
                    count += 1;
                    Globals.Rename(o, count.ToString());
                }


                loadListBox();
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                listBox1.SelectedIndex = listBox1.IndexFromPoint(e.X, e.Y);
                contextMenuStrip1.Items[0].Enabled = true;
                contextMenuStrip1.Items[1].Enabled = true;
                if (listBox1.SelectedItem == null) {
                    contextMenuStrip1.Items[1].Enabled = false;
                }
                contextMenuStrip1.Show(MousePosition);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                Globals.Json.SelectToken(path)["stats"][(listBox1.SelectedIndex+1).ToString()]["name"] = textBox3.Text;
            }
            loadListBox(listBox1.SelectedIndex);
        }
        private void loadListBox(int index=0)
        {
            listBox1.Items.Clear();
            foreach (JProperty p in Globals.Json.SelectToken(path + ".stats").Values<JProperty>())
            {
                listBox1.Items.Add(p.Value["name"] + " : " + p.Value["value"]);
            }
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = index;
            }
        }
    }
}
