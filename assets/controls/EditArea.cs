using Newtonsoft.Json.Linq;
using System.Linq;
using System.Windows.Forms;

namespace MysteryMaker
{
    public partial class EditArea : UserControl
    {
        assets.controls.LogoView lv;

        public EditArea()
        {
            InitializeComponent();
            lv = new assets.controls.LogoView();
            this.Controls.Add(lv);
            lv.Dock = DockStyle.Fill;
        }

        public void load(string path)
        {

            var token = Globals.Json.SelectToken(path);

            JObject jO = token.ToObject<JObject>();


            if (jO.Property("type") != null)
            {
                reset();
                var type = token.Value<string>("type");
                switch (type)
                {
                    case "chapter":
                        MysteriumEdit m = new MysteriumEdit(path);
                        this.Controls.Add(m);
                        m.Dock = DockStyle.Fill;
                        break;
                    case "dialogue":
                        DialogueEdit d = new DialogueEdit(path);
                        this.Controls.Add(d);
                        d.Dock = DockStyle.Fill;
                        break;
                    case "card":
                        CardEdit c = new CardEdit(path);
                        this.Controls.Add(c);
                        c.Dock = DockStyle.Fill;
                        break;
                    case "location":
                        LocationEdit l = new LocationEdit(path);
                        this.Controls.Add(l);
                        l.Dock = DockStyle.Fill;
                        break;
                }
                this.Controls.SetChildIndex(lv, -1);
            }
        }
        public void reset()
        {
            foreach (Control c in this.Controls)
            {
                if(c != lv)
                {
                    this.Controls.Remove(c);
                }
            }
        }
    }
}
