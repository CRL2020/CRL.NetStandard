using CRL.DBAccess;
using CRL.LambdaQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRL
{
    #region delegate
    /// <summary>
    /// as bool TransMethod(out string error);
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public delegate bool TransMethod(out string error);
    #endregion

    /// <summary>
    /// 框架部署,请实现委托
    /// </summary>
    public class SettingConfig
    {
        static SettingConfig()
        {
            var builder = DBConfigRegister.GetInstance();
        }

        /// <summary>
        /// 清除所有内置缓存
        /// </summary>
        public static void ClearCache(string dataBase)
        {
            MemoryDataCache.CacheService.Clear(dataBase);
        }
        #region 设置

        /// <summary>
        /// string字段默认长度
        /// </summary>
        public static int StringFieldLength = 30;
        /// <summary>
        /// 是否检测表结构,生产服务器可将此值设为FALSE
        /// </summary>
        public static bool CheckModelTableMaping = true;


        /// <summary>
        /// 是否使用主从读写分离
        /// 启用后,不会自动检查表结构
        /// 在事务范围内,查询按主库
        /// </summary>
        public static bool UseReadSeparation = false;

        /// <summary>
        /// 是否记录SQL语句调用
        /// </summary>
        public static bool LogSql = false;
        /// <summary>
        /// 生成参数是否与字段名一致
        /// </summary>
        public static bool FieldParameName = false;
        /// <summary>
        /// 是否替换SQL拼接参数
        /// </summary>
        public static bool ReplaceSqlParameter = false;//生成存储过程时不能替换
        /// <summary>
        /// 默认nolock
        /// </summary>
        public static bool QueryWithNoLock = true;
        /// <summary>
        /// 分页是否编译存储过程
        /// </summary>
        public static bool CompileSp = true;
        #endregion
    }
}
