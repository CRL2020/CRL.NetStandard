using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.BinaryFormat
{
    static class Extension
    {
        internal static void Append(this byte[] dst, int dstStart, byte[] src)
        {
            Buffer.BlockCopy(src, 0, dst, dstStart, src.Length);
        }
        internal static byte[] JoinData(this List<byte[]> datas, int length)
        {
            var body = new byte[length];
            var len2 = 0;
            var count = datas.Count;
            for (int i = 0; i < count; i++)
            {
                var d = datas[i];
                body.Append(len2, d);
                len2 += d.Length;
            }
            return body;
        }
    }
}
