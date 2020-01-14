using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
namespace CRL.Core.QuartzScheduler
{
    /// <summary>
    /// QuartzWorker自动任务
    /// </summary>
	public class QuartzWorker
	{
        //https://www.quartz-scheduler.net/documentation/quartz-3.x/quick-start.html
        IScheduler scheduler;
		public QuartzWorker()
		{
			// 创建一个工作调度器工场
			ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
			// 获取一个任务调度器
			scheduler = schedulerFactory.GetScheduler().Result;
		}
        public static Dictionary<JobKey, bool> workCache = new Dictionary<JobKey, bool>();


        /// <summary>
        /// 添加一个任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="work"></param>
        public void AddWork<T>(T work, string name = "") where T : QuartzJob, new()
        {
            Type type = work.GetType();
            string jobName = "JobName_" + type + "_" + name;
            string jobGroup = "JobGroup_" + type;
            var dic = new JobDataMap();
            dic.Add("TagData", work.TagData);
            IJobDetail job = JobBuilder.Create<T>().WithIdentity(jobName, jobGroup).SetJobData(dic).Build();

            // Trigger the job to run now, and then repeat every 10 seconds
            var build = TriggerBuilder.Create()
                .WithIdentity("trigger" + jobName, jobGroup)
                .StartNow();

            if (!string.IsNullOrEmpty(work.CronExpression))
            {
                build.WithCronSchedule(work.CronExpression);
            }
            else
            {
                build.WithSimpleSchedule(x => x
                    .WithInterval(work.RepeatInterval)
                    .RepeatForever());
            }
            var trigger = build.Build();

            scheduler.ScheduleJob(job, trigger);
            workCache.Add(job.Key, false);
        }
		/// <summary>
		/// 开始运行
		/// </summary>
		public void Start()
		{
			scheduler.Start();
            CRL.Core.EventLog.Log(GetType() + "已启动");
		}
		/// <summary>
		/// 停止运行
		/// </summary>
		public void Stop()
        {
            scheduler.Shutdown(false);
            try
            {
                scheduler.Clear();
            }
            catch { }
            workCache.Clear();
            CRL.Core.EventLog.Log(GetType() + "已停止");
		}
	}
}
