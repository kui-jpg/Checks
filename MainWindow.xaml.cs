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
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using Checks.View;

namespace Checks
{


    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsAuto; //是否开机启动
        private IniConfigManager _configManager;//配置文件操作类实例
        private string[] times;
        private string ms;
        private string lasttime;
        public string inipath = @".\config.ini";
        private DateTime msTime;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private int ClickCount = 0;//判断点击次数
        private string half;  //用于确定窗口模式（全屏或半屏）
        private string ischroose;//分屏
        private NotifyIcon notifyIcon;
        public MainWindow()
        {
            InitializeComponent();

            _configManager = new IniConfigManager(inipath);
            int pin = Screen.AllScreens.Count();
            this.Topmost = true;
            this.Hide();
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            ///设置窗口模式
            SetWindowDisplayMode();


            Writelog("程序启动 ");

            //判断程序是否开机启动
            string[] strArgs = Environment.GetCommandLineArgs();
            if (strArgs.Count() > 1 && strArgs[1].ToString() == "-auto")
            {
                Writelog("开机启动，提示点检");
                string[] msg = GetMessageByKey("PC");
                ShowWindow(msg);
            }


            //添加任务栏图标
            InitializeComponent();
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/R-C.ico", UriKind.RelativeOrAbsolute)).Stream);
            notifyIcon.Text = "Check";
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add("Open Window", OpenWindow_Click);
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Visible = true;

            //设置time检查时间
            dispatcherTimer.Tick += new EventHandler(check);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 6);
            dispatcherTimer.Start();

        }

        /// <summary>
        /// 设置窗口显示模式
        /// </summary>
        private void SetWindowDisplayMode()
        {
            //检查是否全屏显示
            try
            {
                half = IniReadValue("Visible", "Ishalf", inipath);
                switch (half)
                {
                    case "0"://全屏显示
                        this.WindowState = WindowState.Maximized;
                        this.Topmost = true;
                        break;

                    case "1"://半屏显示
                        int xWidth = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width;
                        int yHeight = SystemInformation.PrimaryMonitorSize.Height;
                        this.Height = SystemParameters.PrimaryScreenHeight / 2;
                        this.Left = 0;
                        this.Top = SystemParameters.PrimaryScreenHeight - this.Height;
                        this.Topmost = true;
                        break;

                    default:
                        this.WindowState = WindowState.Maximized;
                        this.Topmost = true;
                        break;
                }
            }
            catch (Exception)
            {

                this.WindowState = WindowState.Maximized;
                this.Topmost = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Thread.Sleep(2500);
            //this.Ischeck = false;
            Writelog("已确认");
            IniWriteValue("LastTime", "last", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), inipath);
            this.Hide();
            dispatcherTimer.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ClickCount++;
            int requiredClicks = itemsControl.Items.Count;
            if (sender is System.Windows.Controls.Button button)
            {
                string itemText = button.Content as string;

                if (itemText != null)
                {
                    button.Content = "已确认";
                    button.IsEnabled = false;
                }

            }
            if (requiredClicks == ClickCount)
            {
                Y.IsEnabled = true;
                ClickCount = 0;
            }
        }


        private void OpenWindow_Click(object sender, EventArgs e)
        {
            View_Config view_config = new View_Config(_configManager);
            if (view_config.WindowState == WindowState.Minimized || !this.IsVisible)
            {
                view_config.Show();
                view_config.WindowState = WindowState.Normal;
            }
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


        /// <summary>
        /// 读取节点,获取提示信息
        /// </summary>
        /// <param name="setcionname">节点名称</param>
        /// <returns></returns>
        private string[] GetMessageByKey(string setcionname)
        {
            Dictionary<string, string> m = GetKeys(inipath, setcionname);
            string[] msg = m.Values.ToArray();

            for (int i = 0; i < msg.Length; i++)
            {
                msg[i] = msg[i].ToString().Trim('\'', '"');
            }
            return msg;
        }

        /// <summary>
        /// 检查是否提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void check(object sender, EventArgs e)
        {
            //SetProcessWorkingSetSize()
            //Writelog("开始检查时间配置");
            //读取配置文件，获取点检时间
            times = Readini(inipath);
            //Writelog("进入check");
            //获取输出输出信息
            lasttime = IniReadValue("LastTime", "last", inipath);
            DateTime last = DateTime.Parse(lasttime);
            DateTime now = DateTime.Now;
            try
            {
                //在提示时间段内，提示点检，如果当前 时间段提示过不再提示

                //提醒条件
                //1、设置距离上一次提醒时间超过十分钟
                //2、当前时间不超过设置时间十分钟
                for (int i = 0; i < times.Count(); i++)
                {


                    DateTime check = DateTime.Parse(DateTime.Now.ToShortDateString() + " " + times[i].ToString());
                    //当前时间不超过设定时间10分钟可以提醒
                    if (CheckTimeConditions(last, check, now))
                    {
                        //Writelog("在设定时间内");
                        if (WindowState != WindowState.Maximized || this.Visibility == Visibility.Hidden)
                        {
                            //Writelog("获取提示内容");
                            string[] msg = GetMessageByKey("message" + i);
                            ShowWindow(msg);

                        }

                    }
                }
            }
            catch (Exception ex)
            {

                Writelog(ex.ToString());
            }

        }

        /// <summary>
        /// 检查是否提醒
        /// </summary>
        /// <param name="lastRemindTime">最后提醒时间</param>
        /// <param name="setRemindTime">设置提醒时间</param>
        /// <param name="currentTime">当前时间</param>
        /// <returns></returns>
        private bool CheckTimeConditions(DateTime lastRemindTime, DateTime setRemindTime, DateTime currentTime)
        {
            
            //计算当前时间与上次提醒时间的时间差，是否超过十分钟
            TimeSpan timeSinceLastRemind = currentTime - lastRemindTime;
            bool isTimeSinceLastRemindEnough = timeSinceLastRemind.TotalMinutes >= 10;


            //计算当前时间与设置时间的时间差，判断当前时间是否不超过设置时间10分钟
            TimeSpan timeToSetRemind =  currentTime - setRemindTime;
            bool isCurrenTimeWithinRange = timeToSetRemind.TotalMinutes <= 10 && timeToSetRemind.TotalMinutes >= 0;


            //只有当距离上一次提醒时间超过十分钟切当前时间在设置提醒时间的十分钟范围内时，才满足提醒条件
            return isTimeSinceLastRemindEnough && isCurrenTimeWithinRange;

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        /// <summary>
        /// 通过节，获取节点下所有键和值
        /// </summary>
        /// <param name="iniFile">配置文件路径</param>
        /// <param name="category">节点名称</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetKeys(string iniFile, string category)
        {

            byte[] buffer = new byte[2048];

            GetPrivateProfileSection(category, buffer, 2048, iniFile);
            String[] tmp = Encoding.Unicode.GetString(buffer).Trim('\0').Split('\0');
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (String entry in tmp)
            {
                string[] v = entry.Split('=');
                result.Add(v[0], v[1]);
            }
            return result;
        }


        /// <summary>
        /// 读取配置ini文件，返回提示信息
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

        /// <summary>
        /// 显示主窗体
        /// </summary>
        private void ShowWindow(string[] ms)
        {

            //Writelog("全屏提示");
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            if (this.WindowState != WindowState.Maximized || this.Visibility == Visibility.Hidden)
            {
                ////ms = IniReadValue("Message", "ms", inipath);
                itemsControl.ItemsSource = ms;
                //this.lastRemindTime.Content = string.Join(Environment.NewLine, ms);

                this.Show();
                Writelog("已提醒点检");
                Y.IsEnabled = false;
                dispatcherTimer.Stop();
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
