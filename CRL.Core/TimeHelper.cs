using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
namespace CRL.Core
{
    /// <summary>
    /// 时间操作
    /// </summary>
    public class TimeHelper
    {
        /// <summary>
        /// 时间对比间隔枚举
        /// </summary>
        public enum DateInterval
        {
            /// <summary>
            /// 年
            /// </summary>
            Year = 0,
            /// <summary>
            /// 季度
            /// </summary>
            Quarter = 1,
            /// <summary>
            /// 月
            /// </summary>
            Month = 2,
            /// <summary>
            /// 一年第几天
            /// </summary>
            DayOfYear = 3,
            /// <summary>
            /// 天
            /// </summary>
            Day = 4,
            /// <summary>
            /// 一年第几周
            /// </summary>
            WeekOfYear = 5,
            /// <summary>
            /// 一周第几天
            /// </summary>
            Weekday = 6,
            /// <summary>
            /// 时
            /// </summary>
            Hour = 7,
            /// <summary>
            /// 分
            /// </summary>
            Minute = 8,
            /// <summary>
            /// 秒
            /// </summary>
            Second = 9,
        }
        /// <summary>
        /// 比较时间差,同SQL,ASP里datediff
        /// </summary>
        /// <param name="Interval"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static long DateDiff(DateInterval Interval, System.DateTime date1, System.DateTime date2)
        {
            TimeSpan ts = date2 - date1;
            long diff = 0;
            int dayDiff = (date2.DayOfYear + date2.Year * 365) - (date1.DayOfYear + date1.Year * 365);
            switch (Interval)
            {
                case DateInterval.Year:
                    diff = date2.Year - date1.Year;
                    break;
                case DateInterval.Quarter:
                    diff = date2.Month / 3 - date1.Month / 3;
                    break;
                case DateInterval.Month:
                    diff = (date2.Month + date2.Year * 12) - (date1.Month + date1.Year * 12);
                    break;
                case DateInterval.DayOfYear:
                    diff = date2.DayOfYear - date1.DayOfYear;
                    break;
                case DateInterval.Day:
                    diff = dayDiff;
                    break;
                case DateInterval.WeekOfYear:
                    diff = date2.DayOfYear / 7 - date1.DayOfYear / 7;
                    break;
                case DateInterval.Weekday:
                    diff = date2.DayOfWeek - date1.DayOfWeek;
                    break;
                case DateInterval.Hour:
                    diff = (date2.Hour - date1.Hour) + dayDiff * 24;
                    break;
                case DateInterval.Minute:
                    diff = (date2.Minute - date1.Minute) + ((date2.Hour - date1.Hour) + dayDiff * 24) * 60;
                    break;
                case DateInterval.Second:
                    diff = Convert.ToInt32(ts.TotalSeconds);
                    break;
            }
            return diff;
        }//end of DateDiff
    }
}
