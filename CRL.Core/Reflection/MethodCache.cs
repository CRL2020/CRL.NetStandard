using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core.Reflection
{

    /// <summary>
    /// 方法委托
    /// </summary>
    /// <param name="args"></param>
    public delegate void MethodHandler(params object[] args);

	/// <summary>
	/// 方法缓存实例,包函方法所在的类型,参数
	/// 此缓存只适用用对执行结果要求性不高的逻辑
	/// 能反复执行,执行不成功也不影响业务
	/// </summary>
	[Serializable]
	public class MethodCache
	{
        /// <summary>
        /// 唯一KEY
        /// </summary>
        public string Key
        {
            get;
            set;
        }
        bool needFix = false;

        /// <summary>
        /// 是否需要手动执行
        /// </summary>
        public bool NeedFix
        {
            get { return needFix; }
            set { needFix = value; }
        }
		
		private int maxErrorTimes = 10;
		/// <summary>
		/// 最大出错次数,达到后就移除
        /// 0则不限次数
		/// </summary>
		public int MaxErrorTimes
		{
			get
			{
				return maxErrorTimes;
			}
			set
			{
				maxErrorTimes = value;
			}
		}
        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime NextExecuteTime
        {
            get;
            set;
        }
		/// <summary>
		/// 执行出错次数
		/// </summary>
		public int ErrorTimes
		{
			get;
			set;
		}
        private MethodHandler methodHandler;
        /// <summary>
        /// 方法委托,如果此值不为空,则按此种方式
        /// </summary>
        public MethodHandler MethodHandler
        {
            get
            {
                return methodHandler;
            }
            set
            {
                methodHandler = value;
                MethodName = methodHandler.Method.Name;
                if (methodHandler.Target != null)
                {
                    ClassType = methodHandler.Target.GetType();
                }
                else
                {
                    ClassType=this.GetType();
                }
            }
        }
        /// <summary>
        /// 类型
        /// </summary>
        public Type ClassType
        {
            get;
            set;
        }
        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName
        {
            get;
            set;
        }
		private object[] parameters;
		/// <summary>
		/// 参数集合
		/// </summary>
		public object[] Parameters
		{
			get { return parameters; }
			set { parameters = value; }
		}
		/// <summary>
		/// 获取参数集合字符串
		/// </summary>
		/// <returns></returns>
        public override string ToString()
        {
            string par = "";
            foreach (object obj in Parameters)
            {
                try
                {
                    par += StringHelper.SerializerToJson(obj) + ",";
                }
                catch
                {
                    par += obj + ",";
                }
            }
            if (par.Length > 1)
            {
                par = par.Substring(0, par.Length - 1);
            }
            return par;
        }
	}
}
