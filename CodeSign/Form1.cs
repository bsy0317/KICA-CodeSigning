using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using signPri;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace CodeSign
{
    public partial class Form1 : MaterialForm
    {
        string signCert = "";
        string signPri = "";
        public string pass = "";
        public string Version = "1.0";
        
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;

            InitializeComponent();

            label5.Text = "Ver. "+Version;
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                string tmp = drive.Name + ",";
                if(drive.DriveType.ToString() == "Fixed")
                {
                    tmp += "HDD";
                }
                else if(drive.DriveType.ToString() == "Removable")
                {
                    tmp += "USB";
                }
                else
                {
                    tmp += "ETC";
                }
                comboBox1.Items.Add(tmp);
                
            }
            logadd(Color.Green, "[#] 장치 목록 로드완료.");
        }

        private void materialTabSelector1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            comboBox2.Text = "";
            if (comboBox1.SelectedItem.ToString() != "선택")
            {
                try
                {
                    foreach (string str in Directory.GetDirectories(comboBox1.SelectedItem.ToString().Split(',')[0] + @"NPKI\yessign\USER"))
                    {
                        comboBox2.Items.Add(str.Split('\\')[str.Split('\\').Length-1]);
                        
                    }
                }
                catch { }
            }
        }
        public void logadd(Color cl, string msg)
        {
            try
            {
                log.Update();
                log.BeginInvoke(new MethodInvoker(delegate
                {
                    log.SelectionColor = cl;
                    log.AppendText(msg + "\r\n");
                }));
                log.ScrollToCaret();
            }
            catch { }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            logadd(Color.Green, "[#] " + comboBox2.SelectedItem.ToString() + " 인증서 선택 완료");
            signCert = comboBox1.SelectedItem.ToString().Split(',')[0] + @"NPKI\yessign\USER\"+ comboBox2.SelectedItem.ToString()+ @"\signCert.der";
            signPri = comboBox1.SelectedItem.ToString().Split(',')[0] + @"NPKI\yessign\USER\" + comboBox2.SelectedItem.ToString() + @"\signPri.key";
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "응용 프로그램(*.exe, *.dll)|*.exe;*.dll";
            ofd.ShowDialog();
            if(ofd.FileName != "")
            {
                textBox1.Text = ofd.FileName;
                textBox1.ReadOnly = true;
            }

        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            Password pw = new Password(this);
            pw.ShowDialog();

            Thread ENG = new Thread(Sign);
            ENG.Start();
            logadd(Color.Purple,"[!] 코드사인 시작");
        }
        private void Sign()
        {
            //privkey.pk8
            Key key = new Key();
            logadd(Color.Purple, "[!] 인증서 변환 작업중...(1/3)");
            key.derTopem(signCert,Environment.CurrentDirectory+"\\signCert.crt");
            Thread.Sleep(1500);
            logadd(Color.Purple, "[!] 인증서 변환 작업중...(2/3)");
            key.GetKey(signPri,pass);
            Thread.Sleep(2000);
            if (File.Exists("privkey.pk8"))
            {
                logadd(Color.Purple, "[!] 인증서 변환 작업중...(3/3)");
                key.pemTopfx(Environment.CurrentDirectory + "\\signCert.crt", "privkey.pk8", Environment.CurrentDirectory + "\\signCert.pfx", pass);
                Thread.Sleep(1500);
                //END EXPORT PFX

                if (materialCheckBox1.Checked == true)
                {
                    //Console.WriteLine("sign /f " + Environment.CurrentDirectory + "\\signCert.pfx" + " /t http://timestamp.verisign.com/scripts/timstamp.dll /p " + pass + " " + textBox1.Text);
                    Process.Start(@"openssl\bin\signtool.exe", "sign /f " + Environment.CurrentDirectory + "\\signCert.pfx" + " /t http://timestamp.verisign.com/scripts/timstamp.dll /p " + pass + " " + textBox1.Text);
                    logadd(Color.Purple, "[!] 코드사인 시작...");
                }
                else
                {
                    Process.Start(@"openssl\bin\signtool.exe", "sign /f " + Environment.CurrentDirectory + "\\signCert.pfx" + " /p " + pass + " " + textBox1.Text);
                    logadd(Color.Purple, "[!] 코드사인 시작...");
                }
                Thread.Sleep(2800);
                logadd(Color.Green, "[#] 코드사인 성공");
                //Process.Start(@"openssl\bin\signtool.exe", "sign /f MyCert.pfx /p MyPassword MyFile.exe");
                File.Delete(Environment.CurrentDirectory + @"\privkey.pk8");
                File.Delete(Environment.CurrentDirectory + @"\signCert.crt");
                File.Delete(Environment.CurrentDirectory + @"\signCert.pfx");

                InstallCertificate(@"openssl\bin\CA\KISA ROOT CA 4.crt");
                InstallCertificate(@"openssl\bin\CA\ROOT CA.crt");
                InstallCertificate(@"openssl\bin\CA\yessign CA CLASS 2.crt");
            }
            else
            {
                logadd(Color.Red, "[!] 인증서 암호 오류");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private static void InstallCertificate(string cerFileName)
        {
            X509Certificate2 certificate = new X509Certificate2(cerFileName);
            X509Store store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();
        }
    }
}
