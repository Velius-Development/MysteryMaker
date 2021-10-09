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
using System.IO;

namespace MysteryMaker
{
    public partial class DialogueEdit : UserControl
    {
        string path;

        private ScriptEdit se;

        public DialogueEdit(string _path)
        {
            InitializeComponent();
            path = _path;
            textBox1.Text = getValue("name");
            textBox2.Text = getValue("tellerName");
            textBox7.Text = getValue("difficulty");
            textBox7.Text = getValue("text");
            button1.Text = Utility.get_file_name_no_extension(getValue("image")) == "" ? button1.Text = "Choose image" : Utility.get_file_name_no_extension(getValue("image"));
            label8.Text = "ID: " + _path.Split('.').Last<string>();
            foreach (JProperty o in Globals.Json.SelectToken(path)["choices"])
            {
                listBox1.Items.Add(o.Value["title"].ToString());
            }

            comboBox1.SelectedIndex = 0;

            // Disable all option-specific elements
            foreach (Control c in panel1.Controls)
            {
                if (c.Tag == "option-specific")
                {
                    c.Enabled = false;
                }
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

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            setValue("tellerName", textBox2.Text);
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            setValue("text", textBox7.Text);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (se != null)
            {
                se.Close();
            }
            if (listBox1.SelectedItem != null)
            {
                var name = Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1) + ".title").Value<string>();
                var desc = Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1) + ".description").Value<string>();
                var img = Utility.get_file_name_no_extension(Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1) + ".image").Value<string>());
                var action = Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1) + ".action").Value<string>();
                textBox5.Text = desc;
                textBox4.Text = action.Replace("->", "");
                textBox3.Text = name;
                if (img != "")
                    button3.Text = img;
                else
                    button3.Text = "Choose image";

                if (Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1) + ".action") != null)
                {
                    if (Globals.Json.SelectToken(path + ".choices." + (listBox1.SelectedIndex + 1) + ".action").Value<string>().StartsWith("->"))
                        comboBox1.SelectedIndex = 0;
                    else
                        comboBox1.SelectedIndex = 1;
                }

                // Enable all option-specific elements
                foreach (Control c in panel1.Controls)
                {
                    if (c.Tag == "option-specific")
                    {
                        c.Enabled = true;
                    }
                }

            }
            else
            {
                // Disable all option-specific elements
                foreach (Control c in panel1.Controls)
                {
                    if (c.Tag == "option-specific")
                    {
                        c.Enabled = false;
                    }
                }
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

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            var openFileDialog1 = Utility.get_image();

            if (openFileDialog1 == null)
                return;

            button3.Text = Utility.get_file_name_no_extension(openFileDialog1.FileName);

            Globals.Json.SelectToken(path)["choices"][(listBox1.SelectedIndex + 1).ToString()]["image"] = openFileDialog1.FileName.Replace(Globals.jHandler.filedirpath, "").Replace("\\", "/").Trim('/'); // If possible, save with local path
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var openFileDialog1 = Utility.get_image();

            if (openFileDialog1 == null)
                return;

            button1.Text = Utility.get_file_name_no_extension(openFileDialog1.FileName);

            setValue("image", openFileDialog1.FileName.Replace(Globals.jHandler.filedirpath, "").Replace("\\", "/").Trim('/'));  // If possible, save with local path
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(comboBox1.SelectedIndex)
            {
                case 0:
                    button2.Hide();
                    textBox4.Show();
                    comboBox1.Size = new Size(110, 30);
                    break;
                case 1:
                    button2.Show();
                    textBox4.Hide();
                    comboBox1.Size = new Size(165, 30);
                    button2.BringToFront();
                    break;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Globals.Json.SelectToken(path)["choices"][(listBox1.SelectedIndex + 1).ToString()]["action"] = "->" + textBox4.Text;
        }
    }
}
