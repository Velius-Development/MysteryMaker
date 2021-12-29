using Renci.SshNet;
using MysteryMaker.assets.forms;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;
using System.Windows;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;

namespace MysteryMaker
{
    static class Globals
    {
        public static readonly string[] debuggerBlackList = { "SCRIPT ERROR:", "At:", "ERROR", "WARNING" };

        public static FormMain formMain;
        public static FormLogs formLog;
        public static FormCloud cloudForm;
        public static FormImageCloud imageCloudForm;
        public static JsonHandler jHandler;
        public static SftpClient client;

        public static bool editingCloudFile;
        public static string logs;

        public static Process gameProcess;

        public static JObject _json { get; set; }


        public static void setup()
        {
            //if (Debugger.IsAttached)
            //    Properties.Settings.Default.Reset();
            createJsonHandler();
            if(!String.IsNullOrEmpty(Properties.Settings.Default.cloudPass))
            {
                connectSFTP();
            }

            // Set/Update JSON edit
            Directory.CreateDirectory(Path.GetTempPath() + "/MysteryMaker/json-editor/");
            File.WriteAllText(Path.GetTempPath() + "/MysteryMaker/json-editor/jsoneditor.min.css", Properties.Resources.jsoneditor_css);
            File.WriteAllText(Path.GetTempPath() + "/MysteryMaker/json-editor/jsoneditor.min.js", Properties.Resources.jsoneditor_js);
            File.WriteAllBytes(Path.GetTempPath() + "/MysteryMaker/json-editor/icons.svg", Properties.Resources.jsoneditor_icons);
            File.WriteAllText(Path.GetTempPath() + "/MysteryMaker/json-editor/index.html", Properties.Resources.jsoneditor_index);
        }

        public static JObject Json
        {
            get {
                    //              try {
                    //                  if (gameProcess != null)
                    //                    if (!Globals.gameProcess.HasExited)
                    //                      updateDebug();
                    //              }
                    //              catch(Exception ignore) { }


                    return _json;
            }
            set {  _json = value; }
        }

        public static async Task updateDebug()
        {
            var path = Path.GetTempPath() + "\\MysteryMaker\\debug.zip";
            await export(path);
        }

        public static async Task export(String export_path)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(10);

                File.WriteAllText(jHandler.filepath, Globals.Json.ToString());



                using (ZipFile zip = new ZipFile())
                {
                    Guid g = Guid.NewGuid();
                    string rnd = Convert.ToBase64String(g.ToByteArray());
                    rnd = rnd.Replace("=", "");
                    rnd = rnd.Replace("+", "");
                    rnd = rnd.Replace("/", "");
                    rnd = rnd.Replace("\\", "");


                    // Fix import issues of theme

                    var tmp_dir = Path.GetTempPath() + "MysteryMaker\\tmp\\";


                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(tmp_dir);
                    if (dir.Exists)
                    {
                        Utility.setAttributesNormal(dir);
                        dir.Delete(true);
                    }


                    foreach (String path in Utility.ListFilesRecursive(jHandler.filedirpath))
                    {
                        if (path.Contains(@"\.git\"))
                            continue;

                        if (path.EndsWith(".tscn") || path.EndsWith(".import") || path.EndsWith(".gd") || path.EndsWith(".tres"))
                        {

                            
                            var text = File.ReadAllText(path);
                            var lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                            
                            
                            var new_lines = lines;
                            var i = -1;
                            foreach (var line in lines)
                            {
                                i++;
                                if (Regex.Matches(line, "/").Count > 2)
                                {
                                    if (line.Split('/')[3] == "theme")
                                    {
                                        int pFrom = line.IndexOf("\"res://") + "\"res://".Length;
                                        int pTo = line.LastIndexOf("/theme/");

                                        new_lines[i] = line.Replace(line.Substring(pFrom, pTo - pFrom) + "/theme/", "");
                                    }
                                }
                                new_lines[i] = new_lines[i].Replace("\"res://", "\"res://" + rnd + "/theme/");
                            }
                            
                            var final_text = String.Join("\n", new_lines);

                            var final_p = tmp_dir + path.Replace(jHandler.filedirpath, "");
                            Directory.CreateDirectory(Path.GetDirectoryName(final_p));
                            File.WriteAllText(final_p, final_text);
                            continue;
                        }

                        var final_p2 = tmp_dir + path.Replace(jHandler.filedirpath, "");
                        Directory.CreateDirectory(Path.GetDirectoryName(final_p2));
                        File.Copy(path, final_p2);
                    }


                    foreach (String path in Directory.GetFiles(tmp_dir))
                    {
                        zip.AddItem(path, "\\" + rnd);
                    }

                    foreach (String path in Directory.GetDirectories(tmp_dir))
                    {
                        if (!path.EndsWith(".git"))
                            zip.AddItem(path, rnd + "\\" + path.Replace(tmp_dir, ""));
                    }

                    zip.Save(export_path);

                    dir = new System.IO.DirectoryInfo(tmp_dir);
                    if (dir.Exists)
                    {
                        Utility.setAttributesNormal(dir);
                        dir.Delete(true);
                    }

                }
            });
        } 

        public static void connectSFTP()
        {
            try { 
                Globals.addToLogs("Stelle Verbindung mit Cloud her...");
                client = new SftpClient("cloud.velius.dev", "vCloud", Properties.Settings.Default.cloudPass);
                client.Connect();
                Globals.addToLogs("Erfolgreich mit Cloud verbunden.");
            }
            catch (Exception ex)
            {
                Globals.addToLogs("Fehler beim Verbinden mit der Cloud: " + ex.Message);
            }
        }

        public static void createJsonHandler()
        {
            jHandler = new JsonHandler();
        }

        public static string getWithJson(string name)
        {
            if (name.ToLower().EndsWith(".json"))
            {
                return name;
            }
            else
            {
                return name + ".json";
            }
        }

        public static string getWithPng(string name)
        {
            if (name.ToLower().EndsWith(".png") || name.ToLower().EndsWith(".jpg"))
            {
                return name;
            }
            else
            {
                return name + ".png";
            }
        }

        public static void addToLogs(string status)
        {   
            // Allow cross-threading
            formMain.BeginInvoke((MethodInvoker)delegate () {
                if (formMain != null)
                {
                    formMain.setStatusLabel(status);
                }
                logs += "[" + System.DateTime.Now.TimeOfDay.ToString().Split('.')[0] + "] | " + status + "\r\n";
                if (formLog != null)
                {
                    formLog.refresh();
                }
            ; ;
            });
        }

        public static void Rename(this JToken token, string newName)
        {
            if (token == null)
                throw new ArgumentNullException("token", "Cannot rename a null token");

            JProperty property;

            if (token.Type == JTokenType.Property)
            {
                if (token.Parent == null)
                    throw new InvalidOperationException("Cannot rename a property with no parent");

                property = (JProperty)token;
            }
            else
            {
                if (token.Parent == null || token.Parent.Type != JTokenType.Property)
                    throw new InvalidOperationException("This token's parent is not a JProperty; cannot rename");

                property = (JProperty)token.Parent;
            }

            // Note: to avoid triggering a clone of the existing property's value,
            // we need to save a reference to it and then null out property.Value
            // before adding the value to the new JProperty.  
            // Thanks to @dbc for the suggestion.

            var existingValue = property.Value;
            property.Value = null;
            var newProperty = new JProperty(newName, existingValue);
            property.Replace(newProperty);
        }
    }
}
