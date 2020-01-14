using DotNetty.Buffers;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CRL.RPC
{
    class ResponseWait
    {
        AutoResetEvent autoReset = new AutoResetEvent(false);
        public ResponseMessage Response;
        public void Set()
        {
            autoReset.Set();
        }
        public void Wait()
        {
            autoReset.WaitOne(3000);
        }
    }
    class ResponseWaits
    {
        private ConcurrentDictionary<string, ResponseWait> waits { get; set; } = new ConcurrentDictionary<string, ResponseWait>();
        public void Add(string id)
        {
            waits[id] = new ResponseWait();
        }
        public void Set(string key, ResponseMessage response)
        {
            var wait = waits[key];
            wait.Response = response;
            wait.Set();
        }
        public ResponseWait Wait(string key)
        {
            var wait = waits[key];
            wait.Wait();
            waits.TryRemove(key, out ResponseWait value);
            return wait;
        }
    }
}