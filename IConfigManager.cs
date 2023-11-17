using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checks
{
    internal interface IConfigManager
    {
        /// <summary>
        /// 从配置文件中读取指定键的值，并可指定默认值
        /// </summary>
        /// <param name="section">配置文件中的节（或分组）名称</param>
        /// <param name="key">要读取值的键名</param>
        /// <param name="defaultValue">如果键不存在时返回的默认值</param>
        /// <returns>指定键对应的值，如果不存在则返回默认值</returns>
        string ReadValue(string section, string key, string defaultValue = "");

        /// <summary>
        /// 将指定的值写入配置文件中指定的键
        /// </summary>
        /// <param name="section">配置文件中的节（或分组）名称</param>
        /// <param name="key">要写入值的键名</param>
        /// <param name="value">要写入的值</param>
        /// <param name="overwrite">是否覆盖已存在的值，默认为true</param>
        void WriteValue(string section, string key, string value, bool overwrite = true);

        /// <summary>
        /// 获取配置文件中指定节下的所有键值对
        /// </summary>
        /// <param name="section">配置文件中的节（或分组）名称</param>
        /// <returns>包含指定节下所有键值对的字典，键为键名，值为对应的值</returns>
        Dictionary<string, string> GetSectionValues(string section);

        /// <summary>
        /// 检查指定的键是否存在于配置文件的指定节中
        /// </summary>
        /// <param name="section">配置文件中的节（或分组）名称</param>
        /// <param name="key">要检查的键名</param>
        /// <returns>如果键存在则返回true，否则返回false</returns>
        bool KeyExists(string section, string key);

        /// <summary>
        /// 重新加载配置文件，以便获取最新的配置信息
        /// </summary>
        void ReloadConfig();

        /// <summary>
        /// 删除配置文件中指定节下的指定键
        /// </summary>
        /// <param name="section">配置文件中的节（或分组）名称</param>
        /// <param name="key">要删除的键名</param>
        void DeleteKey(string section, string key);

        /// <summary>
        /// 获取配置文件中所有的节（或分组）名称
        /// </summary>
        /// <returns>包含所有节名称的字符串数组</returns>
        string[] GetAllSections();
    }
}
