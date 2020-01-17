using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core.Log
{
    [Serializable]
    public class LogItem
    {
        public string Domain
        {
            get;
            set;
        }
        public long MsgId
        {
            get;
            set;
        }
        public DateTime Time
        {
            get;
            set;
        }
        public string Detail
        {
            get;
            set;
        }
        public string LogName
        {
            get;
            set;
        }
        public string RequestUrl
        {
            get;
            set;
        }
        public string Folder
        {
            get;
            set;
        }
    }
}
