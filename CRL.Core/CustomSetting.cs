using CRL.Core.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CRL.Core
{
    /// <summary>
    /// 获取自定义配置值
    /// /CustomSetting.config
    /// 文本 key=value
    /// </summary>
    public class CustomSetting
    {
        static System.IO.FileSystemWatcher watch;
        static System.IO.FileSystemWatcher watch2;
        static Dictionary<string, string> keyCaches;
        static Dictionary<string, string> GetSettings()
        {
            string confgiFile = "/Config/CustomSetting.config";
            //var cache = System.Web.HttpRuntime.Cache;
            //string configKey = "$CustomSetting";
            //var cacheObj = cache.Get(configKey);
            //Dictionary<string, string> keyCaches;
            //if (cacheObj != null)
            //{
            //    keyCaches = cacheObj as Dictionary<string, string>;
            //}
            if (keyCaches== null)
            {
                keyCaches = new Dictionary<string, string>();
                var path = RequestHelper.GetFilePath("/Config");
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                //string file = System.Web.Hosting.HostingEnvironment.MapPath(confgiFile);
                string file = RequestHelper.GetFilePath(confgiFile);
                if (!System.IO.File.Exists(file))
                {

                    System.IO.File.WriteAllText(file, "key=value");
                    throw new Exception("配置文件不存在:" + file);
                }
                if (watch == null)
                {
                    watch = new System.IO.FileSystemWatcher(path, "CustomSetting.config");
                    watch.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                    watch.Changed += (s, e) =>
                    {
                        keyCaches = null;
                    };
                    watch.EnableRaisingEvents = true;
                }
                var arry = System.IO.File.ReadLines(file);
                foreach (string str in arry)
                {
                    if (str.StartsWith("//"))
                        continue;
                    int index = str.IndexOf("=");
                    if (index == -1)
                        continue;
                    string name = str.Substring(0, index).Trim().ToUpper();
                    string value = str.Substring(index + 1).Trim();
                    try
                    {
                        value = DesString(value);
                        keyCaches.Add(name, value);
                    }
                    catch
                    {
                        throw new Exception(string.Format("键:{0} 加密错误", name));
                    }
                }
                //cache.Insert(configKey, keyCaches, new System.Web.Caching.CacheDependency(file), DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return keyCaches;
        }

        //const string confgiFile = "/Config/CustomSetting.config";

        /// <summary>
        /// 是否包含有键值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            key = key.ToUpper().Trim();
            var keyCaches = GetSettings();
            return keyCaches.ContainsKey(key);
        }
        public static T GetConfigKey<T>(string key)
        {
            string value = GetConfigKey(key);
            return (T)Convert.ChangeType(value, typeof(T));
        }
        /// <summary>
        /// 获取自定义配置值
        /// 如果值用[]包括，则按加密过处理
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfigKey(string key)
        {
            key = key.ToUpper().Trim();
            var keyCaches = GetSettings();
            if (keyCaches.ContainsKey(key))
                return keyCaches[key];
            throw new Exception("CustomSetting 找不到匹配的KEY:" + key + "");
        }
        /// <summary>
        /// DES加密的内容
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDESEncrypValue(string value)
        {
            return StringHelper.Encrypt(value, CoreConfig.EncryptKey);
        }
        /// <summary>
        /// DES解密的内容
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetUnDESEncrypValue(string value)
        {
            return StringHelper.Decrypt(value, CoreConfig.EncryptKey);
        }
        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void Clear()
        {
            //keyCache.Clear();
        }
        static string DesString(string value)
        {
            //按DES加密处理
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                try
                {
                    value = StringHelper.Decrypt(value.Substring(1, value.Length - 2).Trim(), CoreConfig.EncryptKey);
                }
                catch(Exception ero)
                {
                    throw new Exception("解密串时发生错误 ,请检查源文本是否是正确 :" + ero.Message);
                }
            }
            return value;
        }
        static Dictionary<string, string> connectionCaches;
        /// <summary>
        /// 获取数据连接字符串,默认路径d:\DBConnection
        /// 会自动识是否加密过
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static string GetConnectionString(string connectionName)
        {
            connectionName = (connectionName + ".config").ToUpper();
            //var cache = System.Web.HttpRuntime.Cache;
            //string configKey = "$ConnectionString";
            //var cacheObj = cache.Get(configKey);
            //Dictionary<string, string> keyCache;
            //if (cacheObj != null)
            //{
            //    keyCache = cacheObj as Dictionary<string, string>;
            //}
            if (connectionCaches == null)
            {
                connectionCaches = new Dictionary<string, string>();
                string folder = "c:\\DBConnection\\";
                string folder2 = RequestHelper.GetFilePath("/DBConnection");
                //优先以网站当前目录下设置
                if (System.IO.Directory.Exists(folder2))
                    folder = folder2;

                if (watch2 == null)
                {
                    watch2 = new System.IO.FileSystemWatcher(folder, "*.config");
                    watch2.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                    watch2.Changed += (s, e) =>
                    {
                        connectionCaches = null;
                    };
                    watch2.EnableRaisingEvents = true;
                }

                string[] files = System.IO.Directory.GetFiles(folder, "*.config");
                foreach (string file in files)
                {
                    string value = System.IO.File.ReadAllText(file);
                    try
                    {
                        value = DesString(value);
                        string fileName = file.Substring(file.LastIndexOf("\\") + 1).ToUpper();
                        connectionCaches.Add(fileName, value);
                    }
                    catch { }
                }
                //cache.Insert(configKey, keyCache, new System.Web.Caching.CacheDependency(folder), DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            if (!connectionCaches.ContainsKey(connectionName))
                throw new Exception("数据连接文件不存在,或加密错误:" + connectionName);
            return connectionCaches[connectionName];
        }
    }
}
