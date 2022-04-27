using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;

namespace Checks
{


    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private string[] times;
        private string ms;
        private bool IsAuto;
        private string lasttime;
        private string inipath = @".\config.ini";
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        //private System.Windows.Forms.NotifyIcon myNotifyIcon = new NotifyIcon();
        public MainWindow()
        {
            InitializeComponent();

            //byte[] msByte = Encoding.UTF8.GetBytes(IniReadValue("Message", "ms", inipath));
            ms = IniReadValue("Message", "ms", inipath);
            //ms = Encoding.UTF8.GetString(msByte);
            this.a.Content = ms;
            this.Hide();


            //System.Windows.Forms.MenuItem MenuItem_About = new System.Windows.Forms.MenuItem("About");
            //MenuItem_About.Click += MenuItem_About_Click;
            //System.Windows.Forms.MenuItem MenuItem_Exit = new System.Windows.Forms.MenuItem("Exit");
            //MenuItem_Exit.Click += MenuItem_Exit_Click;
            //System.Windows.Forms.MenuItem[] m = new System.Windows.Forms.MenuItem[] { MenuItem_About, MenuItem_Exit };
            //this.myNotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(m);

            //myNotifyIcon.Text = "V:2022.3.17.1";
            //// myNotifyIcon.Icon = Properties.Resources.R_C;
            //myNotifyIcon.Visible = true;


            //判断程序是否开机启动
            string[] strArgs = Environment.GetCommandLineArgs();
            if (strArgs.Count()>1&&strArgs[1].ToString()=="-autorun")
            {
                ShowWindow();
            }

            dispatcherTimer.Tick += new EventHandler(check);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 6);
            dispatcherTimer.Start();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Thread.Sleep(2500);
            //this.Ischeck = false;
            Writelog("已确认");
            IniWriteValue("LastTime", "last", DateTime.Now.ToString("HH:mm:ss"), inipath);
            this.Hide();
        }


        /// <summary>
        /// 获取设置的时间
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string[] Readini(string path)//读ini
        {
            string[] time;
            if (File.Exists(path))
            {
                int count = Convert.ToInt32(IniReadValue("check", "count", path));
                time = new string[count];
                for (int i = 0; i < count; i++)
                {
                    string t = IniReadValue("check", "time" + i, path);
                    if (IsDate(t))
                    {
                        time[i] = t;
                    }
                    else
                    {
                        time[i] = "";
                    }
                }
                return time;
            }
            else
            {
                time = new string[] { " " };
                return time;
            }

        }

        private void check(object sender, EventArgs e)
        {
            //SetProcessWorkingSetSize()

            //读取配置文件，获取点检时间
            times = Readini(inipath);
            //获取输出输出信息
            ms = IniReadValue("Message", "ms", inipath);
            lasttime = IniReadValue("LastTime", "last", inipath);

            //在提示时间段内，提示点检，当前时间段提示过不再提示
            //检查当前时间与配置时间是否一致，检查范围为配置时间后10分钟以内
            for (int i = 0; i < times.Count(); i++)
            {
                if (times[i].ToString() != "" && times[i].ToString() != " " && DateTime.Now >= DateTime.Parse(times[i]) && DateTime.Now <= DateTime.Parse(times[i]).AddMinutes(10))
                {
                    lasttime = IniReadValue("LastTime", "last", inipath);
                    if (!IsDate(lasttime))
                    {
                        IniWriteValue("LastTime", "last", DateTime.Now.ToString("HH:mm:ss"), inipath);
                        break;
                    }
                    if (DateTime.Parse(lasttime) > DateTime.Parse(times[i]) && DateTime.Parse(lasttime) < DateTime.Parse(times[i]).AddMinutes(10))
                    {
                        string a = times[i];
                        string b = DateTime.Now.ToString();
                        break;
                    }
                    else
                    {
                        if (!this.IsVisible)
                        {
                            this.a.Content = ms;
                            ShowWindow();
                        }


                    }
                }
            }
        }

        private bool IsDate(string str)
        {
            try
            {
                DateTime.Parse(str);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 写入日志
        /// 日志文件保存三个月,其他文件删除
        /// </summary>
        private void Writelog(string str)
        {
            string logPath = @"Log\";
            string logMonth = DateTime.Now.ToString("yyyyMM");
            string txtlogPath = logPath + logMonth + @"\";
            string txtdate = DateTime.Now.ToString("yyyyMMdd");

            Directory.CreateDirectory(txtlogPath);

            DirectoryInfo dr = new DirectoryInfo(logPath);
            FileSystemInfo[] files = dr.GetFileSystemInfos();
            foreach (FileSystemInfo item in files)
            {
                if (item is DirectoryInfo)
                {
                    string drname = item.ToString();
                    string month1 = DateTime.Now.ToString("yyyyMM");
                    string month2 = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                    string month3 = DateTime.Now.AddMonths(-2).ToString("yyyyMM");

                    if (drname != month1 && drname != month2 && drname != month3)
                    {
                        DelDir(item.FullName);
                        Directory.Delete(item.FullName);
                    }
                }
                else
                {
                    File.Delete(item.FullName);
                }
            }
            //写入日志
            #region  FileStream
            //FileStream fs = new FileStream(logPath+@"log.txt", FileMode.Append);
            //try
            //{
            //    byte[] bytes = Encoding.UTF8.GetBytes(ms+"\r\n ");
            //    fs.Write(bytes,0,bytes.Length);
            //    fs.Flush();
            //    fs.Close();
            //}
            //catch (Exception)
            //{
            //    fs.Flush();
            //    fs.Close();
            //}
            #endregion
            string txt = str + @" ： " + DateTime.Now.ToString();
            File.AppendAllText(txtlogPath + @"\" + txtdate + @".txt", txt + "\r\n");

        }


        /// 为INI文件中指定的节点取得字符串
        /// <param name="section">欲在其中查找关键字的节点名称</param>
        /// <param name="key">欲获取的项名</param>
        /// <param name="def">指定的项没有找到时返回的默认值</param>
        /// <param name="retVal">指定一个字串缓冲区，长度至少为nSize</param>
        /// <param name="size">指定装载到lpReturnedString缓冲区的最大字符数量</param>
        /// <param name="filePath">INI文件完整路径</param>
        /// <returns>复制到lpReturnedString缓冲区的字节数量，其中不包括那些NULL中止字符</returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        /// <summary>
        /// 读取配置ini文件
        /// </summary>
        /// <param name="Section">配置段</param>
        /// <param name="Key">键</param>
        /// <param name="inipath">存放物理路径</param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key, string inipath)
        {
            StringBuilder temp = new StringBuilder(500);
            GetPrivateProfileString(Section, Key, "", temp, 500, inipath);
            return temp.ToString();
        }
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);


        /// <summary>
        /// 写入ini文件的操作
        /// </summary>
        /// <param name="Section">配置段</param>
        /// <param name="Key">键</param>
        /// <param name="Value">键值</param>
        /// <param name="inipath">物理路径</param>
        public void IniWriteValue(string Section, string Key, string Value, string inipath)
        {
            WritePrivateProfileString(Section, Key, Value, inipath);
        }
        //protected override void OnPreviewKeyDown(KeyEventArgs e)
        //{
        //    e.Handled = true;
        //}

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;    //or “ base.OnKeyDown(e);  ”
            }
        }
        private void ShowWindow()
        {
            
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            if (this.WindowState != WindowState.Maximized)
            {
                ms = IniReadValue("Message", "ms", inipath);
                this.a.Content = ms;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
                this.Topmost = true;
                this.Show();
                Writelog("已提醒点检");
            }
        }


        /// <summary>
        /// 删除Log文件夹中不匹配的文件和文件夹
        /// <param name="dir">不匹配的文件夹</param>
        /// </summary>
        /// <param name="dir"></param>
        private void DelDir(string dir)
        {

            try
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                FileSystemInfo[] fileinfo = di.GetFileSystemInfos();
                foreach (FileSystemInfo item in fileinfo)
                {
                    if (item is DirectoryInfo)
                    {
                        DirectoryInfo subdir = new DirectoryInfo(item.FullName);
                        subdir.Delete(true);
                    }
                    else
                    {
                        File.Delete(item.FullName);
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                throw;
            }

        }


        /// <summary>
        /// 检查是否设置开机自启
        /// 若没有就设置成自启
        /// </summary>

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        /////////config.ini////////////////
        //        [check]               ///
        //        count=5               ///
        //        time0=12:20           ///
        //        time1=13:30           ///
        //        time2=16:54           ///
        //        time3=14:50           ///
        //        time4=14:00           ///
        //                              ///
        //        [message]             ///
        //        ms="点检时间到了!"    ///
        //                              ///
        //        [LastTime]            ///
        //        last=15:11:43         ///
        //                              ///
        //保存为unicode或 UTF-16LE      ///
        ///////////////////////////////////

    }
}
