using CRL.Core.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CRL.Core.Extension;
namespace CRL.Core
{
    /// <summary>
    /// 可序列化的配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class ICoreConfig<T> where T : class,new()
    {
        static string confgiFile
        {
            get
            {
                return string.Format("/Config/{0}.ser", typeof(T).Name);
            }
        }

        private static T instance;
        /// <summary>
        /// 实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = FromFile();
                return instance;
            }
            set
            {
                instance = value;
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <returns></returns>
        static T FromFile()
        {
            string file = RequestHelper.GetFilePath(confgiFile);
            var path = RequestHelper.GetFilePath("/Config");
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            T obj = new T();
            if (System.IO.File.Exists(file))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(file);
                    obj = json.ToObject<T>();
                }
                catch { }
            }
            if (obj == null)
                obj = new T();
            return obj;
        }
        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            string file = RequestHelper.GetFilePath(confgiFile);
            System.IO.File.WriteAllText(file, this.ToJson());
        }
    }
}
