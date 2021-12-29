using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MysteryMaker.assets.forms;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using Ionic.Zip;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using NJsonSchema.Validation;
using NJsonSchema;
using NJsonSchema.Yaml;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using YamlDotNet.Serialization;
using MysteryMaker.assets.codes;

namespace MysteryMaker
{
    public partial class FormMain : System.Windows.Forms.Form
    {
        public bool isVisible = false;

        public string editedPath = "";
        private bool canSave = true;

        public FormMain()
        {
            InitializeComponent();
        }

        private async void Form_Load(object sender, EventArgs e)
        {
            Globals.formMain = this;

            if (!Debugger.IsAttached)
            {
                var spl = new assets.forms.Splash();
                spl.Show();
                await spl.load();
            }
            else
            {
                open();
            }

            Globals.setup();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            toolStrip1.Renderer = new MySR();
            neuToolStripMenuItem_Click(null, null);

            if (!Properties.Settings.Default.seenUpdateLog && !Debugger.IsAttached)
            {
                ChangeLog l = new ChangeLog();
                l.Show();
            }

            webView2.Source = new Uri(Path.GetTempPath() + @"MysteryMaker\json-editor\index.html");
        }

        public void open()
        {
            isVisible = true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout aboutWin = new FormAbout();
            aboutWin.Show();
        }

        private void updatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(new Updater().CheckForUpdates(), "Update");
        }

        private void ladenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog(); if (result != DialogResult.OK) { return; }
            if (File.Exists(openFileDialog1.FileName) && openFileDialog1.FileName.ToLower().EndsWith(".json")) {
                Globals.jHandler.load(openFileDialog1.FileName);
                Globals.editingCloudFile = false;
            }
            else
            {
                MessageBox.Show("The selected file is not a JSON-file.", "Error");
            }
        }

        public void setStatusLabel(string status)
        {
            toolStripStatusLabel_status.Text = status;
        }

        public void setComponents()
        {
            editArea.reset();
            setTreeView(Globals.Json, "root");
        }

        private void setTreeView(JToken root, string rootName)
        {
            List<Object> expanded_nodes = new List<Object>();

            foreach (TreeNode node in Utility.GetAllNodes(treeView_Chpters.Nodes))
            {
                if (node.IsExpanded)
                    expanded_nodes.Add(node.Tag);
            }

            try
            {
                treeView_Chpters.Nodes.Clear();
                var tNode = treeView_Chpters.Nodes[treeView_Chpters.Nodes.Add(new TreeNode(rootName))];
                tNode.Tag = root;

                AddNode(root, tNode);
            }
            catch (Exception e)
            {

            }

            var nodes = treeView_Chpters.Nodes[0].Nodes.Cast<TreeNode>().ToArray();

            foreach (TreeNode n in nodes)
            {
                treeView_Chpters.Nodes.Remove(n);
                treeView_Chpters.Nodes.Add(n);
            }

            treeView_Chpters.Nodes.RemoveAt(0);         //Remove root node


            nodes = treeView_Chpters.Nodes.Cast<TreeNode>().ToArray();

            foreach (TreeNode n in nodes)
            {
                foreach(TreeNode nn in n.Nodes)
                {
                    foreach (TreeNode nnn in nn.Nodes)
                    {
                        if (nnn.Nodes.Count == 0)                 //If dialogue, item or location entry
                        {
                            nnn.ImageIndex = 2;                   //Set page icon
                            nnn.SelectedImageIndex = 2;
                        }
                    }
                }
            }

            if (treeView_Chpters.Nodes.Count == 0)
                return;

            // Expand previous expanded nodes

            if (expanded_nodes != null)
            {
                foreach (TreeNode node in Utility.GetAllNodes(treeView_Chpters.Nodes))
                {
                    if (expanded_nodes.Contains(node.Tag))
                        node.Expand();
                }
            }
        }

        private void AddNode(JToken token, TreeNode inTreeNode)
        {
            if (token == null)
                return;
            if (token is JObject)
            {
                var obj = (JObject)token;
                foreach (var property in obj.Properties())
                {
                    if (property.Value.Type != JTokenType.Object || property.Name == "choices" || property.Name == "stats")
                    {
                        continue;
                    }
                    else
                    {
                        var displayName = (string) property.Value["name"];
                        if (property.Value["name"] == null)
                            displayName = (string)property.Name;

                        var text = "";
                        if (String.IsNullOrEmpty(displayName))
                        {
                            text = property.Name;
                        }
                        else
                        {
                            text = displayName;
                        }
                        TreeNode tn = new TreeNode(text);
                        tn.Name = property.Name;
                        var childNode = inTreeNode.Nodes[inTreeNode.Nodes.Add(tn)];
                        childNode.Tag = property;
                        AddNode(property.Value, childNode);

                    }
                }
            }
            else if (token is JArray)
            {
                var array = (JArray)token;
                for (int i = 0; i < array.Count; i++)
                {
                    var childNode = inTreeNode.Nodes[inTreeNode.Nodes.Add(new TreeNode(i.ToString()))];
                    childNode.Tag = array[i];
                    AddNode(array[i], childNode);
                }
            }
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveRecentFileAsync();
            try
            {
                Globals.addToLogs("Upload to cloud...");

                var stream = File.Open(Globals.jHandler.filepath, FileMode.Open);
                Globals.client.UploadFile(stream, "json/" + Globals.jHandler.filename);
                Globals.addToLogs("Successfully saved to cloud!");
                stream.Close();
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    MessageBox.Show("To upload a project, it has to be opened", "Error");
                }
                else
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        public async Task saveRecentFileAsync(bool locally = false)
        {
            if (!canSave)
                MessageBox.Show("Project can't be saved with errors!");

            await webView2.ExecuteScriptAsync("getJSON();");

            if (Globals.editingCloudFile &! locally)
            {
                Globals.addToLogs("Save to Cloud...");
                var localPath = Path.GetTempPath() + "/" + Globals.jHandler.filename;
                File.WriteAllText(localPath, Globals.Json.ToString(Formatting.None));

                var stream = File.Open(Globals.jHandler.filepath, FileMode.Open);
                Globals.client.UploadFile(stream, "json/" + Globals.jHandler.filename);
                stream.Close();

                Globals.addToLogs("Successfully saved to cloud!");
                return;
            }

            var fileName = Globals.jHandler.filepath;
            if (File.Exists(fileName))
            {
                Globals.addToLogs("Saving...");
                File.WriteAllText(fileName, Globals.Json.ToString());
            }
            else
            {
                speichernUnterToolStripMenuItem_Click(null, null);
            }

            if (Globals.gameProcess != null)
                if (!Globals.gameProcess.HasExited)
                    await Globals.updateDebug();

            Globals.addToLogs("Successfully saved!");
        }

        private void dateienToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCloud cloudWin = new FormCloud();
            cloudWin.Show();
        }

        private void neuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Globals.addToLogs("Creating new file...");
            Globals.editingCloudFile = false;
            var fileName = Path.GetTempPath() + "/" + "new.json";
            File.WriteAllText(fileName, "{}");

            //Öffne neu angelegte Datei
            Globals.jHandler.load(fileName);
            Globals.jHandler.filepath = null;
            File.Delete(fileName);

            Globals.jHandler.createRootData();
            Globals.addToLogs("Project was successfully created.");
        }

        private void speichernUnterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = saveFileDialog1.ShowDialog(); if (result != DialogResult.OK) { return; }
            var fileName = saveFileDialog1.FileName;
            var dir = new FileInfo(fileName).DirectoryName;
            var fileLoc = dir;
            if (!Directory.Exists(fileLoc))
            {
                MessageBox.Show("The given path does not exist: " + fileLoc, "Error");
                Globals.addToLogs("The given path does not exist: " + fileLoc);
                return;
            }

            if(Directory.EnumerateFileSystemEntries(fileLoc).Any())
            {
                var r = MessageBox.Show("The chosen directory should be empty due to its role as project folder. Use it anyway?", "Beware", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                switch(r)
                {
                    case DialogResult.Yes: break;
                    case DialogResult.No: return;
                    default: return;
                }

            }
            Globals.addToLogs("Save As " + fileName + "...");
            File.WriteAllText(fileName, Globals.Json.ToString());
            Globals.jHandler.load(fileName);
            Globals.addToLogs("Successfully saved as " + fileName + "!");
        }

        private void speichernToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveRecentFileAsync();
        }

        private void treeView_Chpters_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // GUI
            editedPath = getNodeNamePath(treeView_Chpters.SelectedNode);
            editArea.reset();
            editArea.load(editedPath);

            var newJson = "";
            if (editedPath.Length == 0)
            {
                newJson = Globals.Json.ToString();
                editedPath = "";
            }
            else
            {
                newJson = Globals.Json.SelectToken(editedPath).ToString();
            }


            // YAML
            var expConverter = new ExpandoObjectConverter();
            dynamic deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>(newJson, expConverter);

            var serializer = new YamlDotNet.Serialization.Serializer();
            string yaml = serializer.Serialize(deserializedObject);
            fastColoredTextBox1.Text = yaml;


            // JSON
            newJson = newJson.Replace("\"", "\\\"");
            webView2.ExecuteScriptAsync("setJSON(\"" + newJson + "\");");
        }

        private string getNodeNamePath(TreeNode node)
        {
            if (node == null)
                return "";

            int parentCount = node.FullPath.Count(x => x == '\\');
            List<string> pathList = new List<string>();

            if (parentCount > 0)
            {
                for (int i = parentCount+1; i > 0; i--)
                {
                    pathList.Add(node.Name);
                    node = node.Parent;
                }
            }
            else
            {
                pathList.Add(node.Name);
            }

            pathList.Reverse();
            var path = String.Join(".", pathList);
            return path;
        }

        private void inCloudHochladenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Globals.editingCloudFile)
            {
                MessageBox.Show("You are currently working on a cloud-file. To save it, you can just save it and it will be upload to the cloud automatically.", "Error");
                return;
            }
            Globals.addToLogs("Uploading to cloud...");
            saveRecentFileAsync();
            var dialogBox = Interaction.InputBox("Enter the name for the file to be saved in the cloud:", "Rename", "Unnamed");
            var name = Globals.getWithJson(dialogBox);

            //Kann nicht returnen bei abbruch -> (Eigene rename Form erstellen...)

            var stream = File.Open(Globals.jHandler.filepath, FileMode.Open);
            Globals.client.UploadFile(stream, "json/" + name);
            Globals.addToLogs("Uploaded successfully!");
            stream.Close();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (Globals.formLog == null)
            {
                FormLogs logsWin = new assets.forms.FormLogs();
                logsWin.Show();
            }
        }

        private void treeView_Chpters_MouseDown(object sender, MouseEventArgs e)
        {
            var hit = treeView_Chpters.HitTest(e.X, e.Y);

            if (hit.Node == null)
            {
                treeView_Chpters.SelectedNode = null;
                editArea.reset();
                editedPath = "";

                if (e.Button == MouseButtons.Right)
                {
                    contextMenuStrip1.Items[1].Enabled = false;
                    contextMenuStrip1.Items[0].Enabled = true;
                    contextMenuStrip1.Show(PointToScreen(e.Location));
                }
            }
            else
            {
                treeView_Chpters.SelectedNode = hit.Node;
                if (e.Button == MouseButtons.Right)
                {
                    contextMenuStrip1.Items[1].Enabled = true;

                    var n = treeView_Chpters.SelectedNode.Name;
                    switch (n)
                    {
                        case "dialogues": 
                            contextMenuStrip1.Items[1].Enabled = false;
                            contextMenuStrip1.Items[0].Enabled = true; 
                            break;
                        case "items":
                            contextMenuStrip1.Items[1].Enabled = false;
                            contextMenuStrip1.Items[0].Enabled = true;
                            break;
                        case "locations":
                            contextMenuStrip1.Items[1].Enabled = false;
                            contextMenuStrip1.Items[0].Enabled = true;
                            break;
                    }
                    contextMenuStrip1.Show(PointToScreen(e.Location));
                }
                if (int.TryParse(treeView_Chpters.SelectedNode.Name, out int i))
                {
                    contextMenuStrip1.Items[0].Enabled = false;
                }
            }

            treeView_Chpters_AfterSelect(null, null);

        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Add")
            {
                if (treeView_Chpters.SelectedNode == null)
                {
                    Globals.jHandler.createChapter();
                }
                else
                {
                    var parentName = treeView_Chpters.SelectedNode.Parent.Name;

                    if (parentName == null)
                        return;

                    switch (treeView_Chpters.SelectedNode.Name)
                    {
                        case "dialogues": Globals.jHandler.createDialogue(parentName); break;
                        case "items": Globals.jHandler.createItem(parentName); break;
                        case "locations": Globals.jHandler.createLocation(parentName); break;
                    }
                }
            }
            else if (e.ClickedItem.Text == "Remove")
            {
                Globals.jHandler.remove(getNodeNamePath(treeView_Chpters.SelectedNode));
            }
        }

        private void treeView_Chpters_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.ImageIndex = 0;
            e.Node.SelectedImageIndex = 0;
            treeView_Chpters.SelectedNode = null;
        }

        private void treeView_Chpters_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.ImageIndex = 1;
            e.Node.SelectedImageIndex = 1;
            treeView_Chpters.SelectedNode = null;
        }

        private void mysteriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Globals.cloudForm == null)
            {
                FormCloud cloudWin = new FormCloud();
                cloudWin.Show();
            }
        }

        private void imagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Globals.imageCloudForm == null)
            {
                FormImageCloud imageCloudWin = new FormImageCloud();
                imageCloudWin.Show();
            }
        }

        private void updateLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLog l = new ChangeLog();
            l.Show();
            l.Focus();
        }

        private void toolStripDropDownButton1_Click_1(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.cloudPass))
            {
                AuthForCloud afc = new AuthForCloud();
                afc.Show();
                afc.Focus();
            }
            else
            {
                if (Globals.client != null)
                    if (Globals.client.IsConnected)
                        return;
                
                DialogResult dr = MessageBox.Show("You are not connected to the cloud!\nTry to connect?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (dr == DialogResult.Yes)
                {
                    MsgBox mb = new MsgBox("Please wait", "PLEASE WAIT:\nConnecting to cloud...");
                    mb.Show();
                    Globals.connectSFTP();
                    mb.Close();

                    if (Globals.client != null)
                        return;
                    else if (Globals.client.IsConnected)
                        return;
                    askForReconnect();
                }
            }
        }

        private void askForReconnect()
        {
            DialogResult dr = MessageBox.Show("MysteryMaker couldn't connect to vCloud!\nRetry to connect?", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            if (dr == DialogResult.Yes)
            {
                MsgBox mb = new MsgBox("Please wait", "PLEASE WAIT:\nConnecting to cloud...");
                mb.Show();
                Globals.connectSFTP();
                mb.Close();

                if (Globals.client != null)
                    return;
                else if (Globals.client.IsConnected)
                    return;
                askForReconnect();
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 99;

            if (!File.Exists(Globals.jHandler.filepath))
            {
                MessageBox.Show("For debugging, the project needs to be saved!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            treeView_Chpters.Focus();
            if(button1.Text == "Close")
            {
                Globals.gameProcess.Kill();
            }
            else
            {
                await Globals.updateDebug();
                try
                {
                    File.WriteAllBytes(Path.GetTempPath() + "/MysteryMaker/Game.exe", Properties.Resources.Game);
                    Globals.gameProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Path.GetTempPath() + "/MysteryMaker/Game.exe",
                            Arguments = "MM",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    Globals.gameProcess.Start();
                    button1.Text = "Close";

                    await readOutput(Globals.gameProcess);
                    button1.Text = "Play";
                }
                catch (Exception ex)
                {
                    Globals.addToLogs(ex.Message);
                }
            }
            toolStripProgressBar1.Value = 0;
        }

        async Task readOutput(Process process)
        {
            await Task.Run(() =>
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();

                    if (!Properties.Settings.Default.verboseDebug)
                    {
                        nonVerboseLogging(line);
                    }
                    else
                    {
                        Globals.addToLogs(line);
                    }

                    if (line.ToUpper().Contains("VELIUS-SCRIPT-ERROR"))
                    {
                        if(Properties.Settings.Default.errorPopup)
                            MessageBox.Show("A Velius-Script-Error occurred. Please check the logs for more information.", "Velius-Script-Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            });
        }

        private void nonVerboseLogging(string line)
        {
            if (Globals.debuggerBlackList.Any(line.Contains))
            {
                return;
            }
            Globals.addToLogs(line);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(Globals.gameProcess != null)
                if (!Globals.gameProcess.HasExited)
                    Globals.gameProcess.Kill();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = new Settings();
            settings.Show();
        }

        private void opacityAnimTimer_Tick(object sender, EventArgs e)
        {
            if (isVisible)
            {
                if (Opacity < 1)
                {
                    TopMost = true;
                    Opacity += 0.03 + Opacity / 5;
                }
                else if (TopMost)
                    TopMost = false;
            }
            else
            {
                if (Opacity > 0)
                {
                    Opacity -= 0.03 + Opacity / 5;
                }
            }
        }

        private void fastColoredTextBox1_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            if (fastColoredTextBox1.Text.Length == 0)
                return;

            // Coloring
            fastColoredTextBox1.ClearStyle(FastColoredTextBoxNS.StyleIndex.All);
            fastColoredTextBox1.Range.SetStyle(TextStyles.s1, @"\b(" + "~|true|false|on|off" + @")\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            fastColoredTextBox1.Range.SetStyle(TextStyles.s4, "'(.*?)'");
            fastColoredTextBox1.Range.SetStyle(TextStyles.s2, "[a-zA-Z0-9\"_-]+?(?=:)");
            fastColoredTextBox1.Range.SetStyle(TextStyles.s6, "[0-9\\.]+");

            try
            {
                var deserializer = new Deserializer();
                var yamlObject = deserializer.Deserialize<dynamic>(fastColoredTextBox1.Text);

                var js = new JsonSerializer();

                var w = new StringWriter();
                js.Serialize(w, yamlObject);
                string jsonText = w.ToString();
                Globals.Json.SelectToken(editedPath).Replace(JObject.Parse(jsonText));

                label1.Hide();
                canSave = true;
            }
            catch (Exception ex)
            {
                if (!(ex is YamlDotNet.Core.SemanticErrorException) && !(ex is YamlDotNet.Core.SyntaxErrorException))
                    return;

                fastColoredTextBox1.GetLine(e.ChangedRange.ToLine).ClearStyle();
                fastColoredTextBox1.GetLine(e.ChangedRange.ToLine).SetStyle(TextStyles.error);

                label1.Text = ex.Message;
                label1.Show();

                canSave = false;
            }
        }

        private void webView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Gets newest JSON
            Globals.Json.SelectToken(editedPath).Replace(JObject.Parse(e.TryGetWebMessageAsString()));
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0: // GUI
                    editArea.load(editedPath);
                    return;
                case 1: // YAML
                    var expConverter = new ExpandoObjectConverter();
                    dynamic deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>(Globals.Json.SelectToken(editedPath).ToString(), expConverter);

                    var serializer = new YamlDotNet.Serialization.Serializer();
                    string yaml = serializer.Serialize(deserializedObject);
                    fastColoredTextBox1.Text = yaml;
                    return;
                case 2: // JSON
                    var jsonString = Globals.Json.SelectToken(editedPath).ToString().Replace("\"", "\\\"").Replace("\r\n", "");
                    webView2.ExecuteScriptAsync("setJSON(\"" + jsonString + "\");");
                    return;
            }
        }

        private async void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog2.FileName = Globals.Json.Value<string>("name") + ".zip";

            var result = saveFileDialog2.ShowDialog(); if (result != DialogResult.OK) { return; }
            var fileName = saveFileDialog2.FileName;
            var dir = new FileInfo(fileName).DirectoryName;
            var fileLoc = dir;

            if (!Directory.Exists(fileLoc))
            {
                MessageBox.Show("The given path does not exist: " + fileLoc, "Error");
                Globals.addToLogs("The given path does not exist: " + fileLoc);
                return;
            }

            Globals.addToLogs("Exporting to " + fileName + "...");
            await Globals.export(fileName);
            Globals.addToLogs("Successfully saved as " + fileName + "!");
        }
    }



    public class MySR : ToolStripSystemRenderer
    {
        public MySR() { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(Brushes.MidnightBlue, new Rectangle(Point.Empty, e.Item.Size));
            }
        }
    }
}
