using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.EventBus
{
    public class SubscribeAttribute : Attribute
    {
        public SubscribeAttribute()
        {

        }
        public SubscribeAttribute(string name)
        {
            Name = name;
        }
        public string Name
        {
            get; set;
        }
        public int ListTake
        {
            get; set;
        } = 10;
        /// <summary>
        /// 自定义队列名
        /// </summary>
        public string QueueName
        {
            get;set;
        }
        /// <summary>
        /// 轮循线程间隔时间
        /// </summary>
        public double ThreadSleepSecond { get; set; } = 1;
    }
}
