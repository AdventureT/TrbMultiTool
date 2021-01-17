using System.Collections.Generic;
using System.IO;

namespace TrbMultiTool.FileFormats
{
	public class Relc : Tag
	{
		public uint Count { get; set; }

		public struct OffsetInfo
		{
			public ushort HdrxIndex1 { get; set; }
			public ushort HdrxIndex2 { get; set; }
			public uint Offset { get; set; }
		};
		public List<OffsetInfo> StructInfos { get; set; } = new();

		public Relc() : base()
		{
			Count = Trb._f.ReadUInt32();

			for (int i = 0; i < Count; i++)
			{
				StructInfos.Add(new OffsetInfo() { HdrxIndex1 = Trb._f.ReadUInt16(), HdrxIndex2 = Trb._f.ReadUInt16(), Offset = Trb._f.ReadUInt32()});
			}
		}
	}
}