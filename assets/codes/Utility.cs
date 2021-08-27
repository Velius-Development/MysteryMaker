using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MysteryMaker
{
    public class Utility
    {
        public static string get_file_name_no_extension(string input)
        {
            var fname = input.Split('.')[0].Split('\\');
                fname = fname[fname.Length - 1].Split('/');
            var filename = fname[fname.Length - 1];
            return filename;
        }

        public static OpenFileDialog get_image()
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Globals.jHandler.filedirpath;
            var result = openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName == "")
                return null;

            if (!File.Exists(openFileDialog1.FileName) | !openFileDialog1.FileName.ToLower().EndsWith(".png") & !openFileDialog1.FileName.ToLower().EndsWith(".jpg"))
            {
                MessageBox.Show("The chosen file has to be an image.", "Error");
                Globals.addToLogs("The chosen file wasn't a .png or .jpeg file.");
            }



            if (!openFileDialog1.FileName.StartsWith(Globals.jHandler.filedirpath))
            {
                var r = MessageBox.Show("The chosen file has to be inside the project folder!", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                return null;
            }
            return openFileDialog1;
        }


        public static IEnumerable<TreeNode> GetAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;

                foreach (var child in GetAllNodes(node.Nodes))
                    yield return child;
            }
        }


        public static List<string> ListFilesRecursive(string path)
        {
            var results = new List<string>();
            foreach (string file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                results.Add(file);
            }
            return results;
        }



        public static void setAttributesNormal(DirectoryInfo dir)
        {
                foreach (var subDir in dir.GetDirectories())
                    setAttributesNormal(subDir);
                foreach (var file in dir.GetFiles())
                {
                    file.Attributes = FileAttributes.Normal;
                }
        }

     }
}
