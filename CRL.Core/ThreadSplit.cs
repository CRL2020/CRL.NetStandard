using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
namespace CRL.Core
{
    public delegate void OnWorkHandler<T>(List<T> sender);
    /// <summary>
    /// 线程分割处理 
    /// </summary>
    public class ThreadSplit<T> where T:class
    {
        /// <summary>
        /// 线程数
        /// </summary>
        int prodress = 0;
        List<T> arry;
        List<T> failed = new List<T>();
        /// <summary>
        /// 每线程任务数
        /// </summary>
        int threadTask = 1000;
        /// <summary>
        /// 一项正在执行
        /// </summary>
        public OnWorkHandler<T> OnWork;
        /// <summary>
        /// 当全部执行完成时
        /// </summary>
        public EventHandler OnFinish;
        public bool UseLog = true;
        Status workStatus = Status.未开始;
        /// <summary>
        /// 构造5个线程分割
        /// </summary>
        /// <param name="_arry"></param>
        public ThreadSplit(List<T> _arry)
            : this(_arry, 5)
        {
        }
        /// <summary>
        /// 构造线程分割
        /// </summary>
        /// <param name="_arry">要处理的数据</param>
        /// <param name="_progress">线程数,如果为0则为5</param>
        public ThreadSplit(List<T> _arry, int _progress)
        {
            Init(_arry, _progress);
        }
        void Init(List<T> _arry, int _progress)
        {
            arry = _arry;
            int count = arry.Count;
            if (count == 0)
                count = 1;

            prodress = _progress;
            if (prodress == 0)
                prodress = 5;
            if (count < prodress)
            {
                prodress = 1;
            }
            threadTask = count / prodress;
            int mod = count % prodress;
            if (mod > 0)
                threadTask += 1;
            if (threadTask == 0)
                threadTask = 1;
        }
        /// <summary>
        /// 启动线程
        /// </summary>
        public void Start()
        {
            if (workStatus == Status.运行中)
            {
                throw new Exception("任务运行中");
            }
            workStatus = Status.运行中;
            AutoResetEvent[] waits = new AutoResetEvent[prodress];
            if (OnWork == null)
            {
                throw new Exception("onEvent为空");
            }
            //ThreadPool.SetMaxThreads(50, 1000);
            if (UseLog)
            {
                CRL.Core.EventLog.WriteLog(string.Format("线程开始,任务数 {0},线程数 {1} 每线程任务数 {2}", arry.Count, prodress, threadTask));
            }
            for (int i = 0; i < prodress; i++)
            {
                int temp = i;
                List<T> list2 = new List<T>();
                for (int a = i * threadTask; a < (i + 1) * threadTask; a++)
                {
                    if (a < arry.Count)
                    {
                        list2.Add(arry[a]);
                    }
                }
                waits[temp] = new AutoResetEvent(false);
                //IIS下用线程池有问题
                //ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork), strings);
                ThreadStart work = () =>
                {
                    DateTime time = DateTime.Now;
                    try
                    {
                        OnWork(list2);
                        double a = (DateTime.Now - time).TotalSeconds;
                        if (UseLog)
                        {
                            CRL.Core.EventLog.Log(string.Format("线程分块{2}处理完成,任务数{0},用时{1}秒", list2.Count, a.ToString("F"), temp));
                        }
                    }
                    catch(Exception ero)
                    {
                        CRL.Core.EventLog.Log(string.Format("执行线程项时发生错误:{0}", ero.Message));
                    }
                    waits[temp].Set();//发送线程执行完毕信号
                };
                Thread thread = new Thread(new ThreadStart(work));
                thread.Start();
            }
            WaitHandle.WaitAll(waits);//等待Waits中的所有对象发出信号
            workStatus = Status.结束;
            //失败任务重新开始,直到完成
            if (failed.Count > 0)
            {
                CRL.Core.EventLog.Log("重新执行失败任务:" + failed.Count);
                Init(failed, prodress);
                Start();
            }
            if (OnFinish != null)
            {
                OnFinish(null, null);
            }
            if (UseLog)
            {
                CRL.Core.EventLog.WriteLog("所有线程执行完成 " + arry.Count + ",线程数 " + prodress);
            }
        }
        /// <summary>
        /// 添加任务到失败队列,主线程执行完成,再次运行
        /// </summary>
        /// <param name="item"></param>
        public void AddFailed(T item)
        {
            failed.Add(item);
        }


        enum Status
        {
            未开始,
            运行中,
            结束
        }
    }
}
