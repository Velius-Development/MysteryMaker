using CoreLibrary.Extenders.StringManipulation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MysteryMaker
{
    class JsonHandler
    {
        public string filedirpath;
        public string filepath;
        public string filename;

        public void load(string _filename)
        {
            Globals.addToLogs("Opening file...");
            string text = File.ReadAllText(_filename);
            if (isValidJson(text))
            {
                Globals.Json = JObject.Parse(text);
                filepath = _filename;
                filename = Path.GetFileName(filepath);
                filedirpath = filepath.Substring(0, filepath.Length - filepath.Split("\\")[filepath.Split("\\").Length - 1].Length);

                Globals.formMain.setComponents();
                Globals.addToLogs(filename + " was successfully loaded!");
            }
            else
            {
                MessageBox.Show("JSON file could not be loaded.", "Error");
                Globals.addToLogs("JSON file could not be read.");
            }
        }
        private static bool isValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (Exception) //some other exception
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private string getPattern(string type)
        {
            var r = Properties.Resources.pattern;
            var j = JObject.Parse(r);
            var result = j[type].ToString(Formatting.None);
            return result;
        }

        public void createRootData()
        {
            Globals.Json.Add("name", JToken.Parse(getPattern("root")).Value<String>("name"));
            Globals.Json.Add("type", JToken.Parse(getPattern("root")).Value<String>("type"));
            Globals.formMain.setComponents();
        }

        public void createChapter()
        {
            var mysteriesCount = Globals.Json.Count - 3;
            var newID = (mysteriesCount + 1).ToString();
            Globals.Json.Add(newID, JToken.Parse(getPattern("chapter")));
            Globals.formMain.setComponents();
        }

        public void createDialogue(string chapterID)
        {
            var dialoguesCount = Globals.Json[chapterID]["dialogues"].Count();
            var newID = (dialoguesCount).ToString();
            var obj = (JObject)Globals.Json[chapterID]["dialogues"];
            obj.Add(newID, JToken.Parse(getPattern("dialogue")));
            Globals.formMain.setComponents();
        }

        public void createItem(string chapterID)
        {
            var itemsCount = Globals.Json[chapterID]["items"].Count();
            var newID = (itemsCount).ToString();
            var obj = (JObject)Globals.Json[chapterID]["items"];
            obj.Add(newID, JToken.Parse(getPattern("item")));
            Globals.formMain.setComponents();
        }

        public void createLocation(string chapterID)
        {
            var locationsCount = Globals.Json[chapterID]["locations"].Count();
            var newID = (locationsCount).ToString();
            var obj = (JObject)Globals.Json[chapterID]["locations"];
            obj.Add(newID, JToken.Parse(getPattern("location")));
            Globals.formMain.setComponents();
        }

        public void createStat(string pathToItem)
        {
            var p = pathToItem.Split('.');
            var statsCount = Globals.Json[p[0]][p[1]][p[2]]["stats"].Count();
            var newID = (statsCount+1).ToString();
            var obj = (JObject)Globals.Json[p[0]][p[1]][p[2]]["stats"];
            obj.Add(newID, JToken.Parse(getPattern("stat")));
        }

        public void createDialogueChoice(string pathToDialogue)
        {
            var p = pathToDialogue.Split('.');
            var statsCount = Globals.Json[p[0]][p[1]][p[2]]["choices"].Count();
            var newID = (statsCount + 1).ToString();
            var obj = (JObject)Globals.Json[p[0]][p[1]][p[2]]["choices"];
            obj.Add(newID, JToken.Parse(getPattern("choice")));
        }

        public void createLocationChoice()
        {

        }
        public void remove(string path)
        {
            Globals.Json.SelectToken(path).Parent.Remove();
            Globals.formMain.setComponents();
        }
    }
}