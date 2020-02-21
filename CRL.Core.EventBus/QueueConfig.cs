using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.EventBus
{
    public class QueueConfig
    {
        static QueueConfig Instance;
        public static void SetConfig(QueueConfig config)
        {
            Instance = config;
        }
        public string Host;
        public string User;
        public string Pass;
        public string QueueName= "EventBusQueue";
        internal static QueueConfig GetConfig()
        {
            return Instance;
        }
    }
}
