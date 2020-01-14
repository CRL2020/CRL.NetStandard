using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.QuartzScheduler
{
    public class TestJob : QuartzJob
    {
        public TestJob()
        {
            RepeatInterval = new TimeSpan(0,0,6);
        }
        public override void DoWork()
        {
            EventLog.Log(GetType() + " is runing");
        }
    }
    public class TestJob2 : QuartzJob
    {
        public TestJob2()
        {
            this.CronExpression = "0/5 * * * * ?";
        }
        public override void DoWork()
        {
            EventLog.Log(GetType() + " is runing");
        }
    }
}
