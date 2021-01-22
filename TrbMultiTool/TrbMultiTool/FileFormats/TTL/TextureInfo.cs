using PrimeWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats.TTL
{
    public class TextureInfo
    {
        public uint Flag { get; set; }

        public string FileName { get; set; }

        public uint DdsSize { get; set; }

        public uint DdsOffset { get; set; }

        public byte[] RawImage { get; set; }

        public DDSImage Dds { get; set; }

        public TextureInfo(uint offset)
        {
            Flag = Trb._f.ReadUInt32();
            var FileNameOffset = offset + Trb._f.ReadUInt32();
            FileName = ReadHelper.ReadStringFromOffset(FileNameOffset);
            DdsSize = Trb._f.ReadUInt32();
            DdsOffset = offset + Trb._f.ReadUInt32();
            RawImage = ReadHelper.ReadFromOffset(DdsSize, DdsOffset);
            Dds = new DDSImage(RawImage);
        }
    }
}
