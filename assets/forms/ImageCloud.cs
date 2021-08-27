using Renci.SshNet;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json.Linq;

namespace MysteryMaker
{
    public partial class FormImageCloud : Form
    {
        SftpClient client;
        public FormImageCloud()
        {
            InitializeComponent();
        }

        private void FormFileListAdvanced_Load(object sender, EventArgs e)
        {
            client = Globals.client;
            //Globals.imageCloudForm = this;
            refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (imageListView1.SelectedItems.Count == 0)
                return;

            var oldName = imageListView1.SelectedItems[0].FileName;
            var input = Interaction.InputBox("Gebe den neuen Datei-Namen ein:", "Umbenennen", oldName);
            if (string.IsNullOrEmpty(input))
                    return;
            var newName = Globals.getWithPng(input);
            //Kann nicht returnen bei abbruch
            client.RenameFile("img/" + oldName, "img/" + newName);
            refresh();
        }

        private void refresh()
        {
            imageListView1.Items.Clear();
            try
            {
                foreach (var file in client.ListDirectory("img/"))
                {
                    if (file.IsRegularFile  &! file.Name.EndsWith(".json"))
                    {
                        loadInCache(file.Name);
                        imageListView1.Items.Add(file.Name, getFromCache(file.Name));
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (imageListView1.SelectedItems.Count == 0)
                return;

            string message = "Soll " + imageListView1.SelectedItems[0].FileName + " wirklich gelöscht werden?";
            string title = "Löschen";
            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.OK)
            {
                client.Get("img/" + imageListView1.SelectedItems[0].FileName).MoveTo("img/" + "recycler/" + imageListView1.SelectedItems[0].FileName + " - " + DateTime.Now.ToString());
                removeFromToC(imageListView1.SelectedItems[0].FileName);
                refresh();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog(); if (result != DialogResult.OK) { return; }
            if (!File.Exists(openFileDialog1.FileName) && openFileDialog1.FileName.ToLower().EndsWith(".jpg") || openFileDialog1.FileName.ToLower().EndsWith(".png"))
            {
                
                MessageBox.Show("Die ausgewählte Datei ist keine jpg oder png-Datei.", "Fehler");
                Globals.addToLogs("Die ausgewählte Datei ist keine jpg oder png-Datei.");
                return;
            }

            using (var file = File.OpenRead(openFileDialog1.FileName))
            {
                var name = Globals.getWithPng(Interaction.InputBox("Please enter the name of the image:", "Enter name"));
                client.UploadFile(file, "img/" + name);
                file.Close();
                addToToC(name);
            }
            

            refresh();
        }

        private void addToToC(String name)
        {
            var fileName = Path.GetTempPath() + "/MysteryMaker/ToC.json";

            var stream = File.Create(fileName);
            client.DownloadFile("img/ToC.json", stream);
            stream.Close();

            var text = File.ReadAllText(fileName);

            var json = JObject.Parse(text);

            var imgsArray = (JArray) json["images"];

            foreach (JObject entry in imgsArray)
            {
                if (entry["name"].Value<String>() == name)
                {
                    MessageBox.Show("This name is already taken.");
                    return;
                }
            }

            var newEntry = new JObject(new JProperty("name", name));

            imgsArray.Add(newEntry);
            File.WriteAllText(fileName, Newtonsoft.Json.JsonConvert.SerializeObject(json));

            var stream2 = File.OpenRead(fileName);
            client.UploadFile(stream2, "img/ToC.json");
            stream2.Close();
        }

        private void removeFromToC(String name)
        {
            var fileName = Path.GetTempPath() + "/MysteryMaker/ToC.json";

            var stream = File.Create(fileName);
            client.DownloadFile("img/ToC.json", stream);
            stream.Close();

            var text = File.ReadAllText(fileName);

            var json = JObject.Parse(text);

            var imgsArray = (JArray)json["images"];
            
            foreach(JObject entry in imgsArray)
            {
                if (entry["name"].Value<String>() == name)
                {
                    entry.Remove();
                    break;
                }
            }

            File.WriteAllText(fileName, Newtonsoft.Json.JsonConvert.SerializeObject(json));
            
            var stream2 = File.OpenRead(fileName);
            client.UploadFile(stream2, "img/ToC.json");
            stream2.Close();
        }

        private void FormCloud_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Globals.cloudForm = null;
        }

        private String loadInCache(String name)
        {
            var fileName = Path.GetTempPath() + "/MysteryMaker/" + name;

            using (var stream = File.Create(fileName))
            {
                client.DownloadFile("img/" + name, stream);
                stream.Close();
            }

            return fileName;
        }

        private Image getFromCache(String name)
        {
            var fileName = Path.GetTempPath() + "/MysteryMaker/" + name;

            Image img;
            using (var bmpTemp = new Bitmap(fileName))
            {
                img = new Bitmap(bmpTemp);
            }

            return img;
        }

        private void imageListView1_ItemClick(object sender, Manina.Windows.Forms.ItemClickEventArgs e)
        {
            pictureBox1.Image = getFromCache(imageListView1.SelectedItems[0].FileName);
            label1.Text = imageListView1.SelectedItems[0].FileName;
        }
    }
}
