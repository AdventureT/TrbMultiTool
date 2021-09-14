using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
    public class Tmat
    {
        public string MeshName { get; set; }
        public string TextureName { get; set; }
        public Tmat(uint hdrx)
        {
            var unk = Trb.SectFile.ReadUInt32();
            var unk2 = Trb.SectFile.ReadUInt32();
            var unkOffset = Trb.SectFile.ReadUInt32();
            MeshName = Trb.SectFile.ReadStringFromOffset(Trb.SectFile.ReadUInt32() + hdrx);
            var count = Trb.SectFile.ReadUInt32();
            var offset = Trb.SectFile.ReadUInt32();
            Trb.SectFile.BaseStream.Seek(offset + hdrx, System.IO.SeekOrigin.Begin);
            unk = Trb.SectFile.ReadUInt32();
            TextureName = Trb.SectFile.ReadStringFromOffset(Trb.SectFile.ReadUInt32() + hdrx);
        }
    }
}
