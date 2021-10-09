using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysteryMaker.assets.codes
{
    static class TextStyles
    {
        public static TextStyle s1 = new TextStyle(Brushes.DarkOrange, null, FontStyle.Bold);
        public static TextStyle s2 = new TextStyle(Brushes.DarkBlue, null, FontStyle.Bold);
        public static TextStyle s3 = new TextStyle(Brushes.DimGray, null, FontStyle.Bold);
        public static TextStyle s4 = new TextStyle(Brushes.Orange, null, FontStyle.Bold);
        public static TextStyle s5 = new TextStyle(Brushes.Green, null, FontStyle.Regular);
        public static TextStyle s6 = new TextStyle(Brushes.BlueViolet, null, FontStyle.Bold);

        public static TextStyle error = new TextStyle(null, Brushes.Red, FontStyle.Underline);
    }
}
