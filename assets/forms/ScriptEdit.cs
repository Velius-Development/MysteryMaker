﻿using FastColoredTextBoxNS;
using MysteryMaker.assets.codes;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MysteryMaker.assets.forms
{
    public partial class ScriptEdit : Form
    {
        private string path;

        private AutocompleteMenu autoCompleteMenu;

        private static List<string> keywords = new List<string>
        {
            "end",
            "go",
            "add",
            "remove",
            "run",
            "play",
            "game"
        };

        public ScriptEdit(string pathToEdit)
        {
            InitializeComponent();
            path = pathToEdit;
            fastColoredTextBox1.Text = Globals.Json.SelectToken(path)["action"].ToString();
            autoCompleteMenu = new AutocompleteMenu(fastColoredTextBox1);
            autoCompleteMenu.MinFragmentLength = 1;
            autoCompleteMenu.Items.SetAutocompleteItems(keywords);
            autoCompleteMenu.AppearInterval = 1;
            autoCompleteMenu.AllowTabKey = true;
            autoCompleteMenu.Font = new Font("Courier New", 18, FontStyle.Bold);
        }

        private void fastColoredTextBox1_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            if (path != null)
            {
                Globals.Json.SelectToken(path)["action"] = fastColoredTextBox1.Text;
            }

            e.ChangedRange.ClearStyle(TextStyles.s1, TextStyles.s2, TextStyles.s3, TextStyles.s4, TextStyles.s5);
            e.ChangedRange.SetStyle(TextStyles.s1, "<.*?>");
            e.ChangedRange.SetStyle(TextStyles.s2, @"\b(" + string.Join("|", keywords) + @")\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(TextStyles.s2, @"(\-\>)");
            e.ChangedRange.SetStyle(TextStyles.s3, "\".*?\"");
            e.ChangedRange.SetStyle(TextStyles.s4, "[0-9]");
            e.ChangedRange.SetStyle(TextStyles.s5, @"\#(.*)");
        }

        private void fastColoredTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.Space))
            {
                //forced show (MinFragmentLength will be ignored)
                autoCompleteMenu.Show(true);
                e.Handled = true;
            }
        }

        private void toolStripButton2_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("https://velius.dev");
        }

        private void saveToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Globals.formMain.saveRecentFileAsync();
        }

        private void saveAsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var result = saveFileDialog1.ShowDialog(); if (result != DialogResult.OK) { return; }
            var fileName = saveFileDialog1.FileName;
            var dir = new FileInfo(fileName).DirectoryName;
            var fileLoc = dir;
            if (Directory.Exists(fileLoc))
            {
                Globals.addToLogs("Saving as " + fileName + "...");
                File.WriteAllText(fileName, Globals.Json.ToString());
                Globals.addToLogs("Saved file " + fileName + " successfully!");
            }
            else
            {
                MessageBox.Show("The given path doesn't exist." + fileLoc, "Error");
                Globals.addToLogs("The given path doesn't exist.");
            }
        }
    }
}
