using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
namespace CRL.Core.QuartzScheduler
{
    /// <summary>
    /// 任务接口
    /// 优先使用Cron表达式,如果为空,则使用重复规则
    /// </summary>
    public abstract class QuartzJob : IJob
    {
        /// <summary>
        /// 任务数据项
        /// </summary>
        public object TagData;

        //string JobName { get; }
        //string JobGroup { get; }
        /// <summary>
        /// Cron表达式,如果为空,则按重复间隔
        /// </summary>
        public string CronExpression;
        /// <summary>
        /// 重复间隔
        /// </summary>
        public TimeSpan RepeatInterval;
        /// <summary>
        /// 重复次数,-1为不限次数
        /// </summary>
        public int RepeatCount = -1;

        /// <summary>
        /// 执行的任务委托
        /// </summary>
        public abstract void DoWork();
        static object lockObj = new object();
        protected void Log(string message)
        {
            var name = GetType().Name;
            string logName = string.Format("Task_{0}", name);
            EventLog.Log(message, logName);
            Console.WriteLine(message);
        }
        Task IJob.Execute(IJobExecutionContext context)
        {
            var name = context.JobDetail.Key;
            if (QuartzWorker.workCache[name])
            {
                return Task.FromResult(0);
            }
            QuartzWorker.workCache[name] = true;
            try
            {
                var data = context.JobDetail.JobDataMap["TagData"];
                this.TagData = data;
                DoWork();
            }
            catch (Exception ero)
            {
                Log("执行出错:" + ero);
            }
            QuartzWorker.workCache[name] = false;
            return Task.FromResult(0);
        }
    }
}
