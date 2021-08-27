using Renci.SshNet;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Windows.Forms;

namespace MysteryMaker
{
    public partial class FormCloud : Form
    {
        SftpClient client;
        public FormCloud()
        {
            InitializeComponent();
        }

        private void FormFileListAdvanced_Load(object sender, EventArgs e)
        {
            client = Globals.client;
            Globals.cloudForm = this;
            
            refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var oldName = (string)listBox1.SelectedItem;
            var newName = Globals.getWithJson(Interaction.InputBox("Gebe den neuen Datei-Namen ein:", "Umbenennen", oldName));
            //Kann nicht returnen bei abbruch
            client.RenameFile("json/" + oldName, "json/" + newName);

            if (Globals.jHandler.filename == oldName)
            {
                var newDir = new FileInfo(Globals.jHandler.filepath).DirectoryName;
                Globals.jHandler.filepath = newDir + "/" + newName;
                Globals.jHandler.filename = newName;
            }

            refresh();
        }
        private void refresh()
        {
            listBox1.Items.Clear();
            try
            {
                foreach (var file in client.ListDirectory("json/"))
                {
                    if (file.IsRegularFile)
                    {
                        listBox1.Items.Add(file.Name);
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
            string message = "Soll " + (string)listBox1.SelectedItem + " wirklich gelöscht werden?";
            string title = "Löschen";
            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.OK)
            {
                var newName = listBox1.SelectedItem.ToString().Replace(".json", "") + " - " + DateAndTime.TimeString + ".json";

                client.Get("json/" + listBox1.SelectedItem).MoveTo("json/recycler/" + newName + " - " + DateTime.Now.ToString());
                refresh();
            }
        }

        private object GetTimestamp(DateTime now)
        {
            throw new NotImplementedException();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            button4_Click(null, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dialogBox = Interaction.InputBox("Gebe den Namen der neuen Datei ein:", "Umbenennen", "Unbenannt");
            var name = Globals.getWithJson(dialogBox); 

            //Kann nicht returnen bei abbruch -> (Eigene rename Form erstellen...)

            var fileName = Path.GetTempPath() + "/MysteryMaker/" + name;
            File.WriteAllText(fileName, "{}");

            var stream = File.Open(fileName, FileMode.Open);
            client.UploadFile(stream, "json/" + name);
            stream.Close();

            File.Delete(fileName);
            refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Globals.addToLogs("Lade Datei in den Cache...");
            var fileName = Path.GetTempPath() + "/MysteryMaker/" + listBox1.SelectedItem.ToString();

            var stream = File.Create(fileName);
            client.DownloadFile("json/" + listBox1.SelectedItem.ToString(), stream);
            stream.Close();
            
            Globals.jHandler.load(fileName);
            Globals.editingCloudFile = true;
            File.Delete(fileName);
            Close();
        }

        private void FormCloud_FormClosed(object sender, FormClosedEventArgs e)
        {
            Globals.cloudForm = null;
        }
    }
}
