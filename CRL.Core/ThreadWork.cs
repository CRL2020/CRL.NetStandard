using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace CRL.Core
{
    public class ThreadWork
    {
        Thread thread;
        static List<Thread> threads = new List<Thread>();
        //public void Start(string name, Func<bool> action, int second)
        //{
        //    Start(name, action, second);
        //}
        public void Start(string name, Func<bool> action, double second)
        {
            if(second==0)
            {
                throw new Exception("second不能为0");
            }
            if (thread == null)
            {
                thread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ero)
                        {
                            CRL.Core.EventLog.Log("ThreadWork时发生错误" + ero, "ThreadWork_" + name);
                        }

                        Thread.Sleep((int)(1000 * second));
                    }
                });
                threads.Add(thread);
                thread.Start();
                CRL.Core.EventLog.Log(name + "启动", "ThreadWork");
            }
        }
        public void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
            }
        }
        public static void StopAll()
        {
            foreach(var item in threads)
            {
                try
                {
                    item.Abort();
                }
                catch { }
            }
        }
    }
}
