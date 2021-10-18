using FastColoredTextBoxNS;
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

        private static string[] keywords = @"and,in,not,or,self,void,as,assert,breakpoint,class,class_name,
extends,is,func,setget,signal,tool,yield,const,enum,export,
onready,static,var,break,continue,if,elif,else,for,pass,return,
match,while,remote,sync,master,puppet,remotesync,mastersync,
puppetsync".Replace(@"\n", "").Split(',');

        private static string[] keywords2 = @"Color8,ColorN,abs,acos,asin,atan,atan2,bytes2var,
cartesian2polar,ceil,char,clamp,convert,cos,cosh,db2linear,
decimals,dectime,deg2rad,dict2inst,ease,exp,floor,fmod,fposmod,
funcref,get_stack,hash,inst2dict,instance_from_id,inverse_lerp,
is_equal_approx,is_inf,is_instance_valid,is_nan,is_zero_approx,
len,lerp,lerp_angle,linear2db,load,log,max,min,move_toward,
nearest_po2,ord,parse_json,polar2cartesian,posmod,pow,preload,
print_stack,push_error,push_warning,rad2deg,rand_range,
rand_seed,randf,randi,randomize,range_lerp,round,seed,sign,sin,
sinh,smoothstep,sqrt,step_decimals,stepify,str,str2var,tan,tanh,
to_json,type_exists,typeof,validate_json,var2bytes,var2str,
weakref,wrapf,wrapi,bool,int,float,String,NodePath,
Vector2,Rect2,Transform2D,Vector3,Rect3,Plane,
Quat,Basis,Transform,Color,RID,Object,NodePath,
Dictionary,Array,PoolByteArray,PoolIntArray,
PoolRealArray,PoolStringArray,PoolVector2Array,
PoolVector3Array,PoolColorArray".Replace(@"\n", "").Split(',');



        private static string[] keywords3 = @"true,false,null".Replace(@"\n", "").Split(',');

        private static string[] keywords4 = @"Velius".Replace(@"\n", "").Split(',');

        private static string[] keywords5 = @"print,go".Replace(@"\n", "").Split(',');

        public ScriptEdit(string pathToEdit)
        {
            InitializeComponent();
            path = pathToEdit;
            fastColoredTextBox1.Text = Globals.Json.SelectToken(path)["action"].ToString();
            autoCompleteMenu = new AutocompleteMenu(fastColoredTextBox1);
            autoCompleteMenu.MinFragmentLength = 1;
            autoCompleteMenu.Items.SetAutocompleteItems(Utility.ConcatArrays(keywords, keywords2, keywords3, keywords4, keywords5));
            autoCompleteMenu.AppearInterval = 1;
            autoCompleteMenu.AllowTabKey = true;
            autoCompleteMenu.Font = new Font("Consolas", 18, FontStyle.Bold);
        }

        private void fastColoredTextBox1_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            if (path != null)
            {
                Globals.Json.SelectToken(path)["action"] = fastColoredTextBox1.Text;
            }

            e.ChangedRange.ClearStyle(TextStyles.s1, TextStyles.s2, TextStyles.s3, TextStyles.s4, TextStyles.s5, TextStyles.s6);

            e.ChangedRange.SetStyle(TextStyles.s1, @"\b(" + string.Join("|", Utility.ConcatArrays(keywords, keywords4)) + @")\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(TextStyles.s2, @"\b(" + string.Join("|", keywords2) + @")\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(TextStyles.s6, @"\b(" + string.Join("|", keywords3) + @")\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(TextStyles.s3, "\".*?\"");
            e.ChangedRange.SetStyle(TextStyles.s5, "[0-9]");
            e.ChangedRange.SetStyle(TextStyles.s4, @"/^(\+|-|\*|\/|=|>|<|>=|<=|&|\||%|!|\^|\(|\))$/");
            e.ChangedRange.SetStyle(TextStyles.s4, @"\b(" + string.Join("|", keywords5) + @")\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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
