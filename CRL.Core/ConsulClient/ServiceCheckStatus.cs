using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.ConsulClient
{
    public enum ServiceCheckStatus
    {
        Passing,
        Warning,
        Critical
    }

    public static class ServiceCheckStatusConverter
    {
        /// <summary>
        /// Parses a string to a ServiceCheckStatus
        /// </summary>
        /// <param name="scsString"></param>
        /// <returns>The parsed enum value, or ServiceCheckStatus.Critical if the input value does not match a known enum value</returns>
        public static ServiceCheckStatus FromString(string scsString)
        {
            ServiceCheckStatus result;
            if (Enum.TryParse(scsString, true, out result))
            {
                return result;
            }

            return ServiceCheckStatus.Critical;
        }
    }
}
