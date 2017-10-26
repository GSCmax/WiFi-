using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.Runtime.InteropServices;

namespace WiFi发射器
{

    public partial class Form1 : Form
    {

        string name = null;
        string password = null;
        int pswd_length;
        string back;
        bool ifStarted = false;
        bool ifFirstRun = false;
        bool ifReg = false;
        string place = Environment.CurrentDirectory;
        Process p = new Process(); //初始化新的CMD进程
        bool ifHide = false;
        //public WebProxy proxy;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wparam, int lparam);
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) //按下的是鼠标左键            
            {
                Capture = false; //释放鼠标使能够手动操作                
                SendMessage(Handle, 0x00A1, 2, 0); //拖动窗体            
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ifStarted = false;
            button2.Text = "退出";
            p.StartInfo.FileName = "CMD.EXE"; //创建CMD.exe进程
            p.StartInfo.RedirectStandardInput = true; //重定向输入
            p.StartInfo.RedirectStandardOutput = true; //重定向输出
            p.StartInfo.UseShellExecute = false; //不调用系统的Shell
            p.StartInfo.RedirectStandardError = true; //重定向Error
            p.StartInfo.CreateNoWindow = true; //不创建窗口
            toolStripMenuItem3.Text = "退出";
            ifHaveRuned(); //载入配置文件
            ifRegistry(); //查看是否已经放入右键菜单

            //proxy = new WebProxy("127.0.0.1:8123");
            //GlobalProxySelection.Select = proxy;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stop();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            ifHide = false;
            notifyIcon1.Text = "WiFi发射器(双击最小化)";
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            start();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (ifStarted == true)
            {
                stop();
            }
            else
            {
                this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing_yes);
                Application.Exit();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (ifHide == true)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                ifHide = false;
                notifyIcon1.Text = "WiFi发射器(双击最小化)";
            }
            else
            {
                this.Hide();
                ifHide = true;
                notifyIcon1.Text = "WiFi发射器(双击最大化)";
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                ifHide = true;
                notifyIcon1.Text = "WiFi发射器(双击最大化)";
            }
        }

        private void Form1_FormClosing_not(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            ifHide = true;
            notifyIcon1.Text = "WiFi发射器(双击最大化)";
        }

        private void Form1_FormClosing_yes(object sender, FormClosingEventArgs e)
        {
            stop();
            e.Cancel = false;
        }

        private void ifRegistryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ifReg == false)
            {
                SetRegistry();
            }
            else
            {
                RemoveRegistry();
            }
        }

        private void start() //核心功能函数
        {
            name = textBox1.Text;
            password = textBox2.Text;
            if (name.Length == 0)
            {
                MessageBox.Show("WiFi名不能为空");
            }
            else
            {
                pswd_length = password.Length;
                p.Start(); //启动进程
                p.StandardInput.WriteLine("netsh wlan set hostednetwork mode=allow ssid=" + name + " key=" + password); //配置
                //back1 = p.StandardOutput.ReadToEnd();
                p.StandardInput.WriteLine("netsh wlan start hostednetwork"); //启动
                //back2 = p.StandardOutput.ReadToEnd();
                p.StandardInput.WriteLine("exit");
                back = p.StandardOutput.ReadToEnd();
                int num = Regex.Matches(back, @"成功").Count; //判断返回字符串中“成功”的数量
                if (num == 3)
                {
                    if (ifFirstRun == false)
                    {
                        label3.Text = "1承载网络模式已设置为允许." + "\n" + "2已成功更改承载网络的SSID." + "\n" + "3已成功更改托管网络的用户" + "\n" + " 密钥密码." + "\n" + "\n" + "4已启动承载网络.";
                    }
                    else
                    {
                    }
                    button1.Text = "已开启   ";
                    button1.BackgroundImage = imageList1.Images[0];
                    button2.Text = "停止WiFi";
                    button1.Enabled = false;
                    ifStarted = true;
                    toolStripMenuItem2.Enabled = false;
                    toolStripMenuItem3.Text = "停止WiFi";
                    toolStripMenuItem3.Enabled = true;
                    textBox2.Text = null;
                    for (int i = 0; i < pswd_length; i++)
                    {
                        textBox2.Text += "*";
                    }
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    notifyIcon1.BalloonTipText = "WiFi已打开"; //提醒
                    notifyIcon1.ShowBalloonTip(5000);
                    writeConfig();
                }
                else
                {
                    label3.Size = new Size(164, 74);
                    label3.Text = "出现问题,请检查" + "\n" + "1.用户安全密钥应为 8 到 63   个ASCII字符组成的字符串" + "\n" + "2.请查看此贴：http://bbs.160.com/thread-90059-1-1.html";
                    this.Height = 188;
                }
            }

        }

        private void stop()
        {
            if (ifStarted == true)
            {
                p.Start(); //启动进程
                p.StandardInput.WriteLine("netsh wlan stop hostednetwork"); //停止
                p.StandardInput.WriteLine("exit");
                label3.Text += "\n" + "\n" + "已停止承载网络。";
                button2.Text = "退出";
                toolStripMenuItem2.Enabled = true;
                toolStripMenuItem3.Text = "退出";
                ifStarted = false;
                button1.Enabled = true;
                button1.Text = "开启WiFi";
                button1.BackgroundImage = null;
                toolStripMenuItem2.Enabled = true;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox2.Text = password;
                notifyIcon1.BalloonTipText = "WiFi已关闭"; //提醒
                notifyIcon1.ShowBalloonTip(5000);
                button1.Focus();
            }
            else
            {
                this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing_yes);
                Application.Exit();
            }
        }

        private void writeConfig()
        {
            /*-------------------------使用注册表保存----------------------------*/
            RegistryKey key = Registry.LocalMachine;
            RegistryKey SOFTWARE = key.OpenSubKey("SOFTWARE", true);
            RegistryKey WiFiLauncher = SOFTWARE.CreateSubKey("WiFiLauncher");
            WiFiLauncher.SetValue("name", name);
            WiFiLauncher.SetValue("password", password);
            WiFiLauncher.Close();

            /*------------------------使用配置文件保存-----------------------------
            FileStream FS = new FileStream(place + "\\configure", FileMode.Create);
            byte[] con = System.Text.Encoding.Default.GetBytes("Name<" + name + "/>Password<" + password + "/>");
            FS.Write(con, 0, con.Length); //开始写入
            FS.Flush(); //清空缓冲区、关闭流
            FS.Close();
            ---------------------------------------------------------------------*/

        }

        private void readConfig()
        {
            /*-------------------------使用注册表读取----------------------------*/
            RegistryKey key = Registry.LocalMachine;
            RegistryKey SOFTWARE = key.OpenSubKey("SOFTWARE", true);
            RegistryKey WiFiLauncher = SOFTWARE.OpenSubKey("WiFiLauncher");
            name = WiFiLauncher.GetValue("name").ToString();
            password = WiFiLauncher.GetValue("password").ToString();
            WiFiLauncher.Close();
            textBox1.Text = name;
            textBox2.Text = password;

            /*------------------------使用配置文件读取-----------------------------
            StreamReader RD = new StreamReader(place + "\\configure", Encoding.Default);
            string read;
            read = RD.ReadLine();
            int name_start = read.IndexOf("e<") + 2;
            int name_stop = read.IndexOf(">") - 1;
            string getname = read.Substring(name_start, name_stop - name_start);
            int password_start = read.IndexOf("d<") + 2;
            int password_stop = read.Length - 2;
            string getpassword = read.Substring(password_start, password_stop - password_start);
            textBox1.Text = getname;
            textBox2.Text = getpassword;
            RD.Close();
            ---------------------------------------------------------------------*/

        }

        private void ifHaveRuned()
        {
            /*-------------------------使用注册表判断----------------------------*/
            RegistryKey key = Registry.LocalMachine;
            RegistryKey SOFTWARE = key.OpenSubKey("SOFTWARE", true);
            string[] SubKeys;
            SubKeys = SOFTWARE.GetSubKeyNames();
            if (SubKeys.Contains("WiFiLauncher"))
            {
                SOFTWARE.Close();
                label3.Text = "点击关闭按钮可以隐藏主界面" + "\n" + "\n" + "右击此窗体可选择添加至[右键菜单]";
                readConfig();
                ifFirstRun = false;
            }
            else
            {
                SOFTWARE.Close();
                label3.Size = new Size(164, 146);
                label3.Text = "首次使用" + "\n" + "1.输入WiFi名称和密码;" + "\n" + "2.点击[开启WiFi]按钮;" + "\n" + "3.在打开的[网络连接]窗口中" + "\n" + "  找到有[Microsoft托管网络" + "\n" + "  虚拟适配器]的那一项,记住" + "\n" + "  其名称;" + "\n" + "4.找到任意一个已连接的有线" + "\n" + "  网络,右击-属性-进入[共享" + "\n" + "  ]标签栏,勾选允许并在下拉" + "\n" + "  菜单中选择刚刚记下的网络" + "\n" + "  名称,点击确定即可;";
                this.Height = 250;
                Process.Start("control.exe", "ncpa.cpl");
                ifFirstRun = true;
            }

            /*------------------------使用配置文件判断-----------------------------
            string file = place + "\\configure";
            if (File.Exists(@file))
            {
                label3.Text = "点击关闭按钮可以隐藏主界面";
                readConfig();
                ifFirstRun = false;
            }
            else
            {
                label3.Size = new Size(214, 124);
                label3.Text = "首次使用" + "\n" + "1.输入WiFi名和密码" + "\n" + "2.点击“开启WiFi”按钮" + "\n" + "3.在打开的[网络连接]窗口中找到" + "\n" + "  有[Microsoft托管网络虚拟适配器]" + "\n" + "  的那一项，并记住其名称" + "\n" + "4.找到任意一个已连接的有线网络" + "\n" + "  右击-属性-进入共享标签栏" + "\n" + "  勾选允许并在下拉菜单中选择刚刚" + "\n" + "  的网络名称，点击确定即可";
                this.Height = 283;
                Process.Start("control.exe", "ncpa.cpl");
                ifFirstRun = true;
            }
            ---------------------------------------------------------------------*/

        }

        private void SetRegistry()
        {
            RegistryKey key = Registry.ClassesRoot;
            RegistryKey Directory = key.OpenSubKey("Directory", true);
            RegistryKey Background = Directory.OpenSubKey("Background", true);
            RegistryKey shell = Background.OpenSubKey("shell", true);
            RegistryKey WiFiLauncher = shell.CreateSubKey("WiFiLauncher");
            WiFiLauncher.SetValue("", "WiFi发射器");
            RegistryKey WiFiLauncher1 = shell.OpenSubKey("WiFiLauncher", true);
            RegistryKey command = WiFiLauncher1.CreateSubKey("command");
            command.SetValue("", place + "\\WiFi发射器.exe");
            ifRegistryToolStripMenuItem.Image = imageList1.Images[1];
            ifRegistryToolStripMenuItem.Text = "已放入，再次点击可撤出右击菜单";
            ifReg = true;
        }

        private void RemoveRegistry()
        {
            RegistryKey key = Registry.ClassesRoot;
            RegistryKey Directory = key.OpenSubKey("Directory", true);
            RegistryKey Background = Directory.OpenSubKey("Background", true);
            RegistryKey shell = Background.OpenSubKey("shell", true);
            shell.DeleteSubKeyTree("WiFiLauncher", true);
            ifRegistryToolStripMenuItem.Image = null;
            ifRegistryToolStripMenuItem.Text = "将WiFi发射器放入右键菜单";
            ifReg = false;
        }

        private void ifRegistry()
        {
            RegistryKey key = Registry.ClassesRoot;
            RegistryKey Directory = key.OpenSubKey("Directory", true);
            RegistryKey Background = Directory.OpenSubKey("Background", true);
            RegistryKey shell = Background.OpenSubKey("shell", true);
            string[] SubKeys;
            SubKeys = shell.GetSubKeyNames();
            if (SubKeys.Contains("WiFiLauncher"))
            {
                shell.Close();
                ifRegistryToolStripMenuItem.Image = imageList1.Images[1];
                ifRegistryToolStripMenuItem.Text = "已放入，再次点击可撤出右击菜单";
                ifReg = true;
            }
            else
            {
                shell.Close();
                ifRegistryToolStripMenuItem.Text = "将WiFi发射器放入右键菜单";
                ifReg = false;
            }
        }

    }

}
