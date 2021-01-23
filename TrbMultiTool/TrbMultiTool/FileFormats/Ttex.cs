using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
    class Ttex
    {
        public static void ResourceNameHash(string resourceName)
        {
            var hash = (ulong)0;
            for (var i = 0; i < resourceName.Length; ++i)
            {
                var value = (ulong)(sbyte)resourceName[i];
                hash = (hash << 4) + hash + value;
                hash &= 0xFFFFFFFF;
            }
        }
    }
}
