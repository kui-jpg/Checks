using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HandyControl;

namespace Checks.View
{
    /// <summary>
    /// View_Config.xaml 的交互逻辑
    /// </summary>
    public partial class View_Config : System.Windows.Window
    {
        public IniConfigManager _configManager;
        public DataGridView dataGridView;
        public Dictionary<string, string> section;
        public List<Times> TimeList=new List<Times>();
        public View_Config(IniConfigManager configaMager)
        {
            InitializeComponent();
            _configManager = configaMager;
            
            string checkcount = _configManager.ReadValue("check", "count");
            if (checkcount != null || checkcount != "")
            {

                section = _configManager.GetSectionValues("check");
                section.Remove("count");
                foreach (var item in section)
                {
                    Times t=new Times();
                    t.Name = item.Key;
                    t.checktimes = item.Value;
                    TimeList.Add(t);
                }
            }

            ItemsControltimes.ItemsSource = TimeList;

        }

        private string[] getConfigList()
        {
            string[] tese = new string[4] { "1.5S整理（部品、治工具等）、部品准备", "2.始业点检及表格作成", "3.腕带、指套、防护眼镜按标准佩戴", "4.粘接工位胶水排胶" };
            return tese;
        }
    }
}
