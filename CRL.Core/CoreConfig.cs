using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    internal class CoreConfig : ICoreConfig<CoreConfig>
    {
        #region 属性

        List<string> uploadFolderMapping = new List<string>();

        /// <summary>
        /// 上传目录映射
        /// </summary>
        public List<string> UploadFolderMapping
        {
            get
            {
                if (uploadFolderMapping == null)
                {
                    uploadFolderMapping = new List<string>();
                }
                return uploadFolderMapping;
            }
            set { uploadFolderMapping = value; }
        }
        private DateTime lastUpdateTime = DateTime.Now;
        /// <summary>
        /// 上次更新时间
        /// </summary>
        public DateTime LastUpdateTime
        {
            get
            {
                return lastUpdateTime;
            }
            set
            {
                lastUpdateTime = value;
            }
        }
        /// <summary>
        /// 日志消息ID
        /// </summary>
        public long LogMsgId
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// Encrypt密钥
        /// </summary>
        public const string EncryptKey = "2qeecf73";//S8S7FLDL
 
    }
}
