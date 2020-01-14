using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL
{
    public static partial class ExtensionMethod
    {

        /// <summary>
        /// 表示大于
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool GreaterThan<T>(this T origin, T args)
        {
            return true;
        }
        /// <summary>
        /// 表示大于等于
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool GreaterThanOrEqual<T>(this T origin, T args)
        {
            return true;
        }
        /// <summary>
        /// 表示小于
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool LessThan<T>(this T origin,T args)
        {
            return true;
        }
        /// <summary>
        /// 表示小于等于
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool LessThanOrEqual<T>(this T origin, T args)
        {
            return true;
        }
    }
}
