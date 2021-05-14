using PrimeWPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
    public class Ttex
    {
        public uint Unknown { get; set; }

        public string TextureName { get; set; }

        public uint TextureInfoOffset { get; set; }

        public uint DDSSize { get; set; }

        public uint DDSOffset { get; set; }

        public byte[] RawImage { get; set; }

        public DDSImage DDS { get; set; }

        public Ttex(uint offset)
        {
            Unknown = Trb.SectFile.ReadUInt32();
            TextureName = ReadHelper.ReadStringFromOffset(Trb.SectFile, Trb.SectFile.ReadUInt32() + offset);
            Debug.WriteLine(TextureName);
            TextureInfoOffset = Trb.SectFile.ReadUInt32();
            DDSSize = Trb.SectFile.ReadUInt32();
            DDSOffset = Trb.SectFile.ReadUInt32();
            RawImage = ReadHelper.ReadFromOffset(Trb.SectFile, DDSSize, DDSOffset + offset);
            DDS = new DDSImage(RawImage);
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
