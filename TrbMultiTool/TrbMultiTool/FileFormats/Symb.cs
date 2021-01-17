using System.Collections.Generic;
using System.IO;

namespace TrbMultiTool.FileFormats
{
	public class Symb : Tag
	{
		public uint Count { get; set; }

		public struct NameEntry
		{
			public short ID { get; set; }
			public string Name { get; set; }
			public ushort NameID { get; set; }
			public uint DataOffset { get; set; }
		};
		public List<NameEntry> NameEntries { get; set; } = new();


		public Symb() : base()
		{
			Count = Trb._f.ReadUInt32();
			long pos = Trb._f.BaseStream.Position;
			uint nameEntryOffset = (uint)pos + (Count * 12);

			for (int i = 0; i < Count; i++)
			{
				NameEntries.Add(new NameEntry() { ID = Trb._f.ReadInt16(), Name = ReadHelper.ReadStringFromOffset(Trb._f.ReadUInt32() + nameEntryOffset), NameID = Trb._f.ReadUInt16(), DataOffset = Trb._f.ReadUInt32() });
			}
		}
	}
}