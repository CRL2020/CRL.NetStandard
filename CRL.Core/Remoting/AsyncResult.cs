using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    public abstract class AsyncResult
    {
        public abstract Task InvokeAsync();
        public Func<Task<object>> ResultCreater;
    }
    public class AsyncResult<T> : AsyncResult
    {
        public async Task<T> InvokeAsync2()
        {
            var result = await ResultCreater();
            return (T)result;
        }

        public override Task InvokeAsync()
        {
            return InvokeAsync2();
        }
    }
}
