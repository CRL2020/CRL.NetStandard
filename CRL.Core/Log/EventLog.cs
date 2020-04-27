 using System;
using System.Net;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Configuration;
using System.Net.Sockets;
namespace CRL.Core
{
    /// <summary>
    /// 写日志
    /// 不想自动记录Context信息请调用Log(string message, string typeName, false)
    /// </summary>
	public class EventLog
    {
        static ThreadWork thread;
        static EventLog()
        {
            thread = new ThreadWork();
            thread.Start("eventLog",() =>
            {
                WriteLogFromCache();
                return true;
            }, 0.5);
        }
        /// <summary>
        /// 是否使用上下文信息写日志
        /// </summary>
        public static bool UseContext = true;
        #region LogItem
        [Serializable]
        public class LogItem
        {
            internal string Path;
            internal string Id;
            public DateTime Time
            {
                get;
                set;
            }
            public string Title
            {
                get;
                set;
            }
            public string Detail
            {
                get;
                set;
            }
            public string RequestUrl
            {
                get;
                set;
            }
            public string UrlReferrer
            {
                get;
                set;
            }
            public string HostIP
            {
                get;
                set;
            }
            public string UserAgent
            {
                get;
                set;
            }
            public string Post
            {
                get;
                set;
            }
            public string Method
            {
                get;set;
            }
            public override string ToString()
            {
                string s = Time.ToString("yy-MM-dd HH:mm:ss fffff");
                if (string.IsNullOrEmpty(Title))
                {
                    Title = Detail;
                    Detail = "";
                }
                if (!string.IsNullOrEmpty(Title))
                {
                    s += "  " + Title;
                }
                if (!string.IsNullOrEmpty(RequestUrl))
                {
                    s += "\r\nUrl:" + RequestUrl;
                }
                if (!string.IsNullOrEmpty(Method))
                {
                    s += "\r\nMethod:" + Method;
                }
                if (!string.IsNullOrEmpty(UrlReferrer))
                {
                    s += "\r\nUrlReferrer:" + UrlReferrer;
                }
                if (!string.IsNullOrEmpty(HostIP))
                {
                    s += "\r\nHostIP:" + HostIP;
                }
                if (!string.IsNullOrEmpty(UserAgent))
                {
                    s += "\r\n" + UserAgent;
                }
                if (!string.IsNullOrEmpty(Detail))
                {
                    s += "\r\n" + Detail;
                }
                s += "\r\n";
                return s;
            }
        }
        #endregion
        static object lockObj = new object();
        /// <summary>
        /// 检查目录并建立
        /// </summary>
        /// <param name="path"></param>
		public static void CreateFolder(string path)
		{
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
		}
		/// <summary>
        /// 自定义文件名前辍写入日志
		/// </summary>
		/// <param name="message"></param>
		/// <param name="typeName"></param>
		/// <param name="useContext"></param>
		/// <returns></returns>
        public static bool Log(string message, string typeName, bool useContext)
        {
            LogItem logItem = new LogItem();
            logItem.Detail = message;
            return Log(logItem, typeName, useContext);
        }
        public static bool Log(string message, string typeName)
        {
            return Log(message, typeName, true);
        }
        /// <summary>
        /// 指定日志类型名生成日志
        /// </summary>
        /// <param name="logItem"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool Log(LogItem logItem, string typeName)
        {
            return Log(logItem, typeName, true);
        }
        /// <summary>
        /// 指定日志类型名生成日志
        /// </summary>
        /// <param name="logItem"></param>
        /// <param name="typeName"></param>
        /// <param name="useContext">是否使用当前上下文信息</param>
        /// <returns></returns>
        public static bool Log(LogItem logItem, string typeName,bool useContext)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(typeName))
            {
                fileName += "." + typeName;
            }
            //HttpContext context = HttpContext.Current;
            //logItem.Time = DateTime.Now;

            //if (context != null)
            //{
            //    try
            //    {
            //        if (string.IsNullOrEmpty(thisDomain))
            //        {
            //            thisDomain = context.Request.Url.Host;
            //        }
            //        if (UseContext)
            //        {
            //            if (useContext)
            //            {
            //                logItem.HostIP = Request.RequestHelper.GetIP();

            //                logItem.RequestUrl = context.Request.Url.ToString();
            //                logItem.UserAgent = context.Request.UserAgent;
            //                logItem.UrlReferrer = context.Request.UrlReferrer + "";
            //                logItem.Post = context.Request.Form.ToString();
            //                logItem.Method = context.Request.HttpMethod;
            //            }
            //        }
            //    }
            //    catch
            //    {
            //    }
            //}
            return WriteLog(GetLogFolder(), logItem, fileName);
        }
		/// <summary>
		/// 生成日志,默认文件名
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool Log(string message)
		{
			return WriteLog(message);
		}
		/// <summary>
		/// 生成日志,文件名以Error开头
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool Error(string message)
		{
			return Log(message,"Error");
		}
		/// <summary>
		/// 生成日志,文件名以Info开头
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool Info(string message)
		{
			return Log(message, "Info");
		}
		/// <summary>
		/// 生成日志,文件名以Debug开头
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool Debug(string message)
		{
			return Log(message, "Debug");
		}
		/// <summary>
		/// 在当前网站目录生成日志
		/// </summary>
        /// <param name="message"></param>
		public static bool WriteLog(string message)
		{
            return Log(message, "");
		}


		static bool Writing = false;
		static DateTime lastWriteTime = DateTime.Now;
		static Dictionary<string, LogItem> logCaches = new Dictionary<string, LogItem>();

        /// <summary>
        /// 指定路径,文件名,写入日志
        /// </summary>
        /// <param name="path"></param>
        /// <param name="logItem"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool WriteLog(string path, LogItem logItem, string fileName)//建立日志
        {
            try
            {
                if (!System.IO.Directory.Exists(path))
                    CreateFolder(path);
                string filePath = "";
                var id = System.Guid.NewGuid().ToString();
                filePath = path + fileName + ".txt";
                logItem.Path = filePath;
                logItem.Id = id;
                logCaches.Add(id, logItem);
                return true;
            }
            catch (Exception ero)
            {
                Console.WriteLine("写入文件时发生错误:" + ero.Message);
                return false;
            }
        }
		public static string LastError;
        public static void WriteLogFromCache()
        {
            lock (lockObj)
            {
                Writing = true;
                //累加上次记录的日志
                if (logCaches.Count > 0)
                {
                    var list = new Dictionary<string, LogItem>(logCaches);
                    var group = list.Values.GroupBy(b => b.Path);
                    var removeKeys = new List<string>();
                    foreach (var g in group)
                    {
                        var logs = new LogItemArry() { logs = g.ToList() };
                        try
                        {
                            WriteLine(logs.ToString(), g.Key);
                            removeKeys.AddRange(g.Select(b => b.Id));
                        }
                        catch (Exception ero)
                        {
                            LastError = ero.ToString();
                        }
                    }
                    foreach (var key in removeKeys)
                    {
                        logCaches.Remove(key);
                    }
                }
                //System.Threading.Thread.Sleep(6000);
                Writing = false;
            }
        }

        static string thisDomain = "";

		private static void WriteLine(string message, string filePath)
		{
            message += "\r\n";
			using (FileStream fs = File.OpenWrite(filePath))
			{
				//根据上面创建的文件流创建写数据流
				StreamWriter w = new StreamWriter(fs, System.Text.Encoding.Default);
				//设置写数据流的起始位置为文件流的末尾
				w.BaseStream.Seek(0, SeekOrigin.End);
				//w.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
				w.Write(message);
				w.Flush();
				w.Close();
			}
			//Console.WriteLine(message);
		}
        static string rootPath = null;
		/// <summary>
		/// 获取日志绝对目录
		/// </summary>
		/// <returns></returns>
		public static string GetLogFolder()
		{
            if (rootPath == null)
            {
                //rootPath = System.Web.Hosting.HostingEnvironment.MapPath(@"\log\");
                //if (string.IsNullOrEmpty(rootPath))
                //{
                //    rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\log\";
                //}
                rootPath = Request.RequestHelper.GetFilePath(@"\log\");
                rootPath += @"\";
                
            }
			return rootPath;
		}
        public static void Stop()
        {
            if (thread != null)
            {
                thread.Stop();
                //thread = null;
            }
        }
        /// <summary>
        /// 项集合
        /// </summary>
		class LogItemArry 
		{
			public string savePath;
            internal List<LogItem> logs = new List<LogItem>();
            public void Add(LogItem log)
			{
				logs.Add(log);
			}
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
                foreach (LogItem item in logs)
				{
                    sb.Append(item.ToString());
				}
				return sb.ToString();
			}
		}
	}
}
