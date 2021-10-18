using FastColoredTextBoxNS;
using System.Drawing;

namespace MysteryMaker.assets.codes
{
    static class TextStyles
    {
        public static TextStyle s1 = new TextStyle(new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#A531A3")), null, FontStyle.Regular);
        public static TextStyle s2 = new TextStyle(new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#5A8AF0")), null, FontStyle.Regular);
        public static TextStyle s3 = new TextStyle(new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#53A053")), null, FontStyle.Regular);
        public static TextStyle s4 = new TextStyle(new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#1485BA")), null, FontStyle.Regular);
        public static TextStyle s5 = new TextStyle(new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#976715")), null, FontStyle.Regular);
        public static TextStyle s6 = new TextStyle(new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#E2574E")), null, FontStyle.Regular);

        public static TextStyle error = new TextStyle(null, Brushes.Red, FontStyle.Underline);
    }
}
