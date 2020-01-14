using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading;
namespace CRL.Core.Reflection
{
    /// <summary>
    /// 动态访问执行者
    /// 实现委托或反射形式执行方法
    /// </summary>
    public static class DynamicVisitor
    {

        #region 动态执行方法
        /// <summary>
        /// 执行一个方法缓存
        /// </summary>
        /// <param name="methodCache"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool Execute(MethodCache methodCache, out string error)
        {
            error = "";
            try
            {
                if (methodCache.MethodHandler != null)
                {
                    methodCache.MethodHandler(methodCache.Parameters);

                }
                else
                {
                    Execute(methodCache.ClassType, methodCache.MethodName, methodCache.Parameters);
                }
                lock (lockObj)
                {
                    MethodCaches.Remove(methodCache);
                }
                return true;
            }
            catch (Exception ero)
            {
                error = ero.Message;
            }
            return false;
        }
        /// <summary>
        /// 通过类型实例执行方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        static object Execute(Type type, string methodName, params object[] parameters)
        {

            object oleObject = System.Activator.CreateInstance(type);
            //CRL.Core.EventLog.Log(string.Format("开始执行缓存方法 {0},{1}", type, methodName));
            return type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, oleObject, parameters);

        }
        /// <summary>
        /// 执行后并移除一个方法缓存
        /// </summary>
        static void ExecuteAndRemoveCache(MethodCache methodCache)
        {
            if (methodCache.NeedFix)
                return;
            if (methodCache.NextExecuteTime > DateTime.Now)
            {
                return;
            }

            string error;
            bool result = Execute(methodCache, out error);
            if (result)
            {
                //MechodCaches.Remove(methodCache);
                string log = string.Format("执行方法缓存成功 {0}", methodCache.ClassType + "." + methodCache.MethodName);
                CRL.Core.EventLog.Log(log, "MethodCache");
       
            }
            else
            {
                methodCache.ErrorTimes += 1;
                int a = (methodCache.ErrorTimes + 1) * methodCache.ErrorTimes;
                methodCache.NextExecuteTime = DateTime.Now.AddSeconds(a);
                bool stop = false;
                if (methodCache.ErrorTimes >= methodCache.MaxErrorTimes && methodCache.MaxErrorTimes > 0)//超过最大执行次数
                {
                    stop = true;
                }
                string message = methodCache.ClassType + "." + methodCache.MethodName + "(" + methodCache.ToString() + ")";
                string log = string.Format("执行方法缓存出错 {0} S:{1}\r\n{2} ", message, a, error);
                CRL.Core.EventLog.Log(log, "MethodCache");

                if (stop)
                {
                    //MechodCaches.Remove(key);
                    methodCache.NeedFix = true;
                    SaveMethodCache();
                    log = "方法缓存达到最大出错次数,状态改为NeedFix " + methodCache.ErrorTimes;
                    CRL.Core.EventLog.Log(log, "MethodCache");
     
                    string path = CRL.Core.EventLog.GetLogFolder();
                    CRL.Core.EventLog.WriteLog(path, new EventLog.LogItem() { Title = log, Detail = message, Time = DateTime.Now }, "Methods");
                }
            }

        }
        #endregion

        /// <summary>
        /// 方法缓存集合
        /// </summary>
        static List<MethodCache> MethodCaches = Reflection.MethodCaches.Instance.MechodCaches;
        public static void AddMechodCacheByHandler(MethodHandler handler, object[] parames, int maxErrorTimes = 10)
        {
            AddMethodCacheByHandler(handler, parames, maxErrorTimes);
        }
        /// <summary>
        /// 通过委托方法添加方法缓存
        /// </summary>
        /// <param name="handler">委托</param>
        /// <param name="parames">委托的参数,按顺序</param>
        /// <param name="maxErrorTimes">重复执行次数,如果超过则需手动处理</param>
        public static void AddMethodCacheByHandler(MethodHandler handler, object[] parames, int maxErrorTimes = 10)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                MethodCache methodCache = new MethodCache()
                {
                    MethodHandler = handler,
                    Parameters = parames,
                    MaxErrorTimes = maxErrorTimes
                };
                string error;
                bool result = Execute(methodCache, out error);
                if(!result)
                {
                    AddMethodCache(methodCache);
                }
            });
        }
        /// <summary>
        /// 添加一个方法缓存
        /// </summary>
        /// <param name="methodCache"></param>
        public static void AddMethodCache(MethodCache methodCache)
        {
            if (methodCache.ClassType != null)
            {
                try
                {
                    System.Activator.CreateInstance(methodCache.ClassType);
                }
                catch
                {
                    CRL.Core.EventLog.Log("插入方法缓存时出现错误," + methodCache.ClassType + "." + methodCache.MethodName + "不可实例化");
                    return;
                }
            }
            CRL.Core.EventLog.Log(string.Format("插入方法缓存 From [{0}] {1} {2}", methodCache.MethodHandler == null ? "反射" : "委托", methodCache.ClassType + "." + methodCache.MethodName, "(" + methodCache.ToString() + ")"));
            string key = System.Guid.NewGuid().ToString().Replace("-", "");
            methodCache.NextExecuteTime = DateTime.Now.AddSeconds(1);
            methodCache.Key = key;

            MethodCaches.Add(methodCache);
            SaveMethodCache();

            if (thread == null)
            {
                thread = new Thread(new ThreadStart(threadStart));
                thread.Start();
            }
        }
        static Thread thread;
        static bool runing = false;
        private static void threadStart()
        {
            while (true)
            {
                DoWork();
                var ts = DateTime.Now - lastSaveTime;
                if (needSave)
                {
                    lock (lockObj)
                    {
                        Reflection.MethodCaches.Instance.Save();
                    }
                    needSave = false;
                }
                Thread.Sleep(1000);
            }
        }
        static void DoWork()
        {
            if (runing)
            {
                return;
            }
            if (MethodCaches.Count == 0)
            {
                return;
            }
            var list = new List<MethodCache>(MethodCaches);
            var n = 5;
            if (list.Count > 50)//最少5个线程,每线程10
            {
                n = list.Count / 10;
            }
            if (n > 100)
            {
                n = 100;
            }
            runing = true;
            var threadSplit = new CRL.Core.ThreadSplit<MethodCache>(list, n);
            threadSplit.OnWork = (b) =>
            {
                var list2 = b as List<MethodCache>;
                foreach (var entry in list2)
                {
                    ExecuteAndRemoveCache(entry);
                }
            };
            //任务执行完成
            threadSplit.OnFinish += (b, e2) =>
            {
                runing = false;
            };
            threadSplit.Start();
        }
        static object lockObj = new object();
        static DateTime lastSaveTime = DateTime.Now;
        static bool needSave = false;
        public static void SaveMethodCache()
        {
            needSave = true;
            lastSaveTime = DateTime.Now;
        }
    }
}
