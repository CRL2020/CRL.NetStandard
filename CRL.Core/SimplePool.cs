using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CRL.Core
{
    /// <summary>
    /// 简单对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimplePool<T>: IDisposable where T: IDisposable
    {
        Func<T> _objCreater;
        ThreadWork clearWork;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objCreater">对象实例化</param>
        /// <param name="maxSize">池最大值</param>
        /// <param name="poolClearMinute">不使用时，多少分钟后清空</param>
        public SimplePool(Func<T> objCreater, int maxSize = 20, int poolClearMinute = 0)
        {
            _objCreater = objCreater;
            _maxSize = maxSize;
            if (poolClearMinute > 0)
            {
                clearWork = new ThreadWork();
                clearWork.Start("SimplePoolClear", b =>
                 {
                     var ts = DateTime.Now - useTime;
                     if (ts.TotalMinutes > poolClearMinute)
                     {
                         Clear();
                     }
                     return true;
                 }, 30);
            }
        }
        DateTime useTime = DateTime.Now;
        static readonly object lockObj = new object();
        ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();
        int _count;
        int _maxSize = 20;
        /// <summary>
        /// 租用对象
        /// </summary>
        /// <returns></returns>
        public T Rent()
        {
            lock (lockObj)
            {
                while (_count > _maxSize)
                {
                    Thread.SpinWait(1);
                }
                useTime = DateTime.Now;
                return RentBase();
            }
        }
        T RentBase()
        {
            if (_pool.TryDequeue(out var model))
            {
                Interlocked.Decrement(ref _count);

                return model;
            }
            try
            {
                model = _objCreater();
            }
            catch (Exception e)
            {
                throw new Exception("池实例化时发生错误:" + e.Message);
            }

            return model;
        }
        /// <summary>
        /// 返还对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Return(T obj)
        {
            useTime = DateTime.Now;
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                _pool.Enqueue(obj);

                return true;
            }

            Interlocked.Decrement(ref _count);
            return false;
        }

        void Clear()
        {
            while (_pool.TryDequeue(out var item))
            {
                item.Dispose();
            }
            //Console.WriteLine("_pool clear");
        }
        public void Dispose()
        {
            clearWork?.Stop();
            Clear();
        }
    }
}
