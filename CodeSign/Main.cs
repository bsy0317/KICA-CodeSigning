using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;

namespace CodeSign
{
    public partial class Main : MaterialForm
    {
        public Main()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            try
            {
                WebClient wc = new WebClient();
                string VER = wc.DownloadString("https://raw.githubusercontent.com/bmg0001/Version/master/NPKI/VER");
                if (VER != new Form1().Version)
                {
                    MessageBox.Show("업데이트가 존재합니다.\n업데이트 버전: " + VER + "\n현재 버전: " + new Form1().Version, "Update", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    Process.Start("IExplore.exe", wc.DownloadString("https://raw.githubusercontent.com/bmg0001/Version/master/NPKI/Down"));
                    this.Hide();
                    Form1 frm = new Form1();
                    frm.Show();
                }
                else
                {
                    MessageBox.Show("최신버전 입니다.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    this.Hide();
                    Form1 frm = new Form1();
                    frm.Show();
                }
            }
            catch {
                MessageBox.Show("업데이트 정보를 얻어올 수 없습니다.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Hide();
                Form1 frm = new Form1();
                frm.Show();
            }
        }

        private void materialFlatButton2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 frm = new Form1();
            frm.Show();
        }
    }
}
