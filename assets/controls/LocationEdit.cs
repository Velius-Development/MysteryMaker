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
using MysteryMaker.assets.forms;

namespace MysteryMaker
{
    public partial class LocationEdit : UserControl
    {
        string path;

        private ScriptEdit se;

        public LocationEdit(string _path)
        {
            InitializeComponent();
            path = _path;
            textBox1.Text = getValue("name");
            textBox7.Text = getValue("description");
            label8.Text = "ID: " + _path.Split('.').Last<string>();
            foreach (JProperty o in Globals.Json.SelectToken(path)["choices"])
            {
                listBox1.Items.Add(o.Value["title"].ToString());
            }
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
            setValue("description", textBox1.Text);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(se != null)
            {
                se.Close();
            }
            if (listBox1.SelectedItem != null)
            {
                var name = Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1) + ".title").Value<string>();
                var desc = Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1) + ".description").Value<string>();
                textBox5.Text = desc;
                textBox3.Text = name;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                Globals.Json.SelectToken(path)["choices"][(listBox1.SelectedIndex + 1).ToString()]["title"] = textBox3.Text;
            }
            loadListBox(listBox1.SelectedIndex);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                Globals.Json.SelectToken(path)["choices"][(listBox1.SelectedIndex + 1).ToString()]["description"] = textBox5.Text;
            }
            loadListBox(listBox1.SelectedIndex);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                se = new ScriptEdit(path + ".choices." + (listBox1.SelectedIndex + 1));
                se.Show();
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Add")
            {
                Globals.jHandler.createDialogueChoice(path);
                loadListBox(listBox1.Items.Count);
            }
            else if (e.ClickedItem.Text == "Remove")
            {
                Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1)).Parent.Remove();



                var props = new List<JProperty>();
                foreach (JProperty o in Globals.Json.SelectToken(path + ".choices"))
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
                if (listBox1.SelectedItem == null)
                {
                    contextMenuStrip1.Items[1].Enabled = false;
                }
                contextMenuStrip1.Show(MousePosition);
            }
        }

        private void loadListBox(int index = 0)
        {
            listBox1.Items.Clear();
            foreach (JProperty p in Globals.Json.SelectToken(path + ".choices").Values<JProperty>())
            {
                listBox1.Items.Add(p.Value["title"]);
            }
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = index;
            }
        }
    }
}
