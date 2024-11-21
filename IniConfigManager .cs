using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Checks
{
    public class IniConfigManager:IConfigManager
    {
        private string _iniFilePath;
        public IniConfigManager(string iniFilePath) 
        { 
            _iniFilePath = iniFilePath; 
        }

        public string ReadValue(string section, string key, string defaultValue = "")
        {
            StringBuilder temp = new StringBuilder(500);
            int result = GetPrivateProfileString(section, key, defaultValue, temp, 500, _iniFilePath);
            if (result == 0)
            {
                return defaultValue;
            }
            return temp.ToString();
        }

        

        public void WriteValue(string section, string key, string value, bool overwrite = true)
        {
            if (!overwrite && KeyExists(section, key))
            {
                return;
            }
            WritePrivateProfileString(section, key, value, _iniFilePath);
        }

        public Dictionary<string, string> GetSectionValues(string section)
        {
            byte[] buffer = new byte[2048];
            int result = GetPrivateProfileSection(section, buffer, 2048, _iniFilePath);
            if (result == 0)
            {
                return new Dictionary<string, string>();
            }
            String[] tmp = Encoding.Unicode.GetString(buffer).Trim('\0').Split('\0');
            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            foreach (String entry in tmp)
            {
                string[] v = entry.Split('=');
                resultDict.Add(v[0], v[1]);
            }
            return resultDict;
        }

        public bool KeyExists(string section, string key)
        {
            string value = ReadValue(section, key);
            return !string.IsNullOrEmpty(value);
        }

        public void ReloadConfig()
        {
            // 对于INI文件，这里可根据实际需求决定是否真正需要重新加载操作
            // 例如，如果有其他程序可能修改了INI文件，可在此添加重新读取文件等逻辑
        }

        public void DeleteKey(string section, string key)
        {
            WriteValue(section, key, "", false);
        }

        public string[] GetAllSections()
        {
            // 实现获取INI文件所有节的逻辑，这里暂未详细实现，可根据具体需求进一步完善
            return new string[] { };
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);


    }
}
