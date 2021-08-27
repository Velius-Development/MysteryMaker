using FluentFTP;
using System;
using System.Windows.Forms;

namespace MysteryMaker
{
    public partial class FormFileList : Form
    {
        FtpClient client;
        public FormFileList()
        {
            InitializeComponent();
        }

        private void FileList_Load(object sender, EventArgs e)
        {
            client = Globals.client;
            reload();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                var result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    client.DownloadFile(saveFileDialog1.FileName, "cloud/json/" + listBox1.SelectedItem);
                    this.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void reload()
        {
            listBox1.Items.Clear();
            try
            {
                foreach (var file in client.GetListing("cloud/json/", FtpListOption.Recursive))
                {
                    listBox1.Items.Add(file.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
