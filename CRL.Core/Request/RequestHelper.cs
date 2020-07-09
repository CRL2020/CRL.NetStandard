using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Reflection;
using System.Collections.Specialized;

namespace CRL.Core.Request
{
    public class RequestHelper
    {
        /// <summary>
        /// 是否为远程服务器,DEBUG用
        /// </summary>
        public static bool IsRemote
        {
            get
            {
                string address = GetServerIp();
                bool a = !address.Contains("192.168.") && !address.Contains("127.0.") && !address.Contains("10.0.");
                return a;
            }
        }


        /// <summary>
        /// 判断IP地址是否为内网IP地址
        /// </summary>
        /// <param name="ipAddress">IP地址字符串</param>
        /// <returns></returns>
        public static bool IsInnerIP(String ipAddress)
        {
            bool isInnerIp = false;
            long ipNum = GetIpNum(ipAddress);
            /**
私有IP：A类 10.0.0.0-10.255.255.255
B类 172.16.0.0-172.31.255.255
C类 192.168.0.0-192.168.255.255
当然，还有127这个网段是环回地址 
            **/
            long aBegin = GetIpNum("10.0.0.0");
            long aEnd = GetIpNum("10.255.255.255");
            long bBegin = GetIpNum("172.16.0.0");
            long bEnd = GetIpNum("172.31.255.255");
            long cBegin = GetIpNum("192.168.0.0");
            long cEnd = GetIpNum("192.168.255.255");
            isInnerIp = IsInner(ipNum, aBegin, aEnd) || IsInner(ipNum, bBegin, bEnd) || IsInner(ipNum, cBegin, cEnd) || ipAddress.Equals("127.0.0.1");
            return isInnerIp;
        }
        /// <summary>
        /// 把IP地址转换为Long型数字
        /// </summary>
        /// <param name="ipAddress">IP地址字符串</param>
        /// <returns></returns>
        private static long GetIpNum(String ipAddress)
        {
            String[] ip = ipAddress.Split('.');
            long a = int.Parse(ip[0]);
            long b = int.Parse(ip[1]);
            long c = int.Parse(ip[2]);
            long d = int.Parse(ip[3]);

            long ipNum = a * 256 * 256 * 256 + b * 256 * 256 + c * 256 + d;
            return ipNum;
        }
        /// <summary>
        /// 判断用户IP地址转换为Long型后是否在内网IP地址所在范围
        /// </summary>
        /// <param name="userIp"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static bool IsInner(long userIp, long begin, long end)
        {
            return (userIp >= begin) && (userIp <= end);
        }

        static string serverIp;
		/// <summary>
		/// 获取服务器第一个IP
		/// </summary>
		/// <returns></returns>
		public static string GetServerIp()
		{
            if (string.IsNullOrEmpty(serverIp))
            {
                System.Net.IPAddress[] addressList = Dns.GetHostAddresses(Dns.GetHostName());

                string address = "";
                foreach (System.Net.IPAddress a in addressList)
                {
                    if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !a.ToString().Contains("10.0.0"))
                    {
                        if (!IsInnerIP(a.ToString()))
                        {
                            address = a.ToString();
                            break;
                        }
                    }
                }
                serverIp = address;
            }
            return serverIp;
		}
        static string innerIP;
        public static string GetInnerIP()
        {
            if (string.IsNullOrEmpty(innerIP))
            {
                System.Net.IPAddress[] addressList = Dns.GetHostAddresses(Dns.GetHostName());

                string address = "";
                foreach (System.Net.IPAddress a in addressList)
                {
                    if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        if (IsInnerIP(a.ToString()))
                        {
                            address = a.ToString();
                            break;
                        }
                    }
                }
                innerIP = address;
            }
            return innerIP;
        }
        static object lockObj = new object();
        static int index = 1;


        /// <summary>
        /// 返回当前http主机名
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentHost(string url)
        {
            string[] arry = url.Split('/');
            string host = arry[2];
            string url1 = arry[0] + "//" + host;
            return url1;
        }
        

        /// <summary>
        /// 获取工作目录路径,IIS则为网站根目录,程序为程序所在目录
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFilePath(string file)
        {
            string path;
#if NET45
            path = AppDomain.CurrentDomain.BaseDirectory + file.Replace("/",@"\");
#else
            path = AppContext.BaseDirectory + file.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());//file.Replace("/", @"\");
#endif

            return path;
        }
    }
}
