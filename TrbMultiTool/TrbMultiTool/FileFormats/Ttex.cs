using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
    class Ttex
    {
        public uint Unknown { get; set; }

        public string TextureName { get; set; }

        public Ttex()
        {
            Unknown = Trb.SectFile.ReadUInt32();
            TextureName = ReadHelper.ReadStringFromOffset(Trb.SectFile, Trb.SectFile.ReadUInt32());
            Debug.WriteLine(TextureName);
        }

        public static ulong ResourceNameHash(string resourceName)
        {
            var hash = (ulong)0;
            for (var i = 0; i < resourceName.Length; ++i)
            {
                var value = (ulong)(sbyte)resourceName[i];
                hash = (hash << 4) + hash + value;
                hash &= 0xFFFFFFFF;
            }
            return hash;
        }
    }
}
