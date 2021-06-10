using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TrbMultiTool.FileFormats
{
	public class Sect : Tag
	{
		public List<byte> Data { get; set; } = new();
        public Sect() : base()
		{
			Offset = Trb._f.BaseStream.Position;
			if (Label == "SECC") //Compressed
			{
				var btec = new Btec();
				btec.Decompress();
				Data = btec.DecompressedData;
				var padding = 0;
				var tempSize = Size;
                while (tempSize % 4 != 0)
                {
					padding++;
					tempSize++;
                }

				Trb._f.BaseStream.Seek(Offset + Size + padding, SeekOrigin.Begin);
			}
            else
            {
				Data.AddRange(Trb._f.ReadBytes((int)Size));
			}
		}
	}
}