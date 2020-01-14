using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core
{
    /// <summary>
    /// 并发请求限制
    /// </summary>
    public class RequestLimiter
    {
        List<int> arry = new List<int>();
        object lockObj = new object();
        DateTime time = DateTime.Now;
        TimeSpan timeSpan;
        int count;
        public RequestLimiter(int _count, TimeSpan _timeSpan)
        {
            timeSpan = _timeSpan;
            count = _count;
            fill();
        }
        void fill()
        {
            while (arry.Count < count)
            {
                arry.Add(0);
            }
        }
        public int CheckLimit()
        {
            lock (lockObj)
            {
                if (DateTime.Now - time > timeSpan)
                {
                    fill();
                    time = DateTime.Now;
                }
                if (arry.Count == 0)
                {
                    return 0;
                }
                arry.RemoveAt(0);
            }
            return arry.Count;
        }
    }
}
