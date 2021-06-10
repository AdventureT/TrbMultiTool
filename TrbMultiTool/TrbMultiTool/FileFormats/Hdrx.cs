using System.Collections.Generic;

namespace TrbMultiTool.FileFormats
{
	public class Hdrx : Tag
	{
		public ushort Flag1 { get; set; }
		public ushort Flag2 { get; set; }
		public uint Files { get; set; }

		public struct TagInfo
		{
			public ushort Unknown { get; set; }
			public ushort Unknown2 { get; set; }
			public uint TagSize { get; set; }
			public uint Zero { get; set; }
			public uint Flag { get; set; }
            public uint Offset { get; set; }
        };
		public List<TagInfo> TagInfos { get; set; } = new();

		public Hdrx() : base()
		{
			Flag1 = Trb._f.ReadUInt16();
			Flag2 = Trb._f.ReadUInt16();
			Files = Trb._f.ReadUInt32();

			for (int i = 0; i < Files; i++)
			{
				var tagInfo = new TagInfo() { Unknown = Trb._f.ReadUInt16(), Unknown2 = Trb._f.ReadUInt16(), TagSize = Trb._f.ReadUInt32(), Zero = Trb._f.ReadUInt32(), Flag = Trb._f.ReadUInt32() };
				tagInfo.Offset = i == 0 ? 0 : TagInfos[i - 1].Offset + TagInfos[i - 1].TagSize; //Adding Sizes to make Offsets
				TagInfos.Add(tagInfo);
			}
		}
	}
}