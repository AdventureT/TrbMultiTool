using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
	public class Tag
	{
		public string Label { get; set; }
		public uint Size { get; set; }

		public Tag()
		{
			Label = new string(Trb._f.ReadChars(4));
			Size = Trb._f.ReadUInt32();
		}
	}
}
