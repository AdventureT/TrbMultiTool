using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
    public class Head : Tag
    {

        public uint FileCount { get; set; }

        public struct File
        {
            public byte[] Flags { get; set; }

            public uint Size { get; set; }
        }

        public List<File> Files { get; set; } = new();


        public Head() : base()
        {
            FileCount = Trb._f.ReadUInt32();

            for (int i = 0; i < FileCount; i++)
            {
                Files.Add(new() { Flags = Trb._f.ReadBytes(4), Size = Trb._f.ReadUInt32()});
            }
            Trb._f.BaseStream.Seek(4, System.IO.SeekOrigin.Current); //Not sure if it belongs to a file or if its just padding....
        }
    }
}
