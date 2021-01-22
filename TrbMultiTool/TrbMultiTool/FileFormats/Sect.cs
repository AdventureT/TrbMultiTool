using System.IO;

namespace TrbMultiTool.FileFormats
{
	public class Sect : Tag
	{
        public long Offset { get; set; }
        public Sect() : base()
		{
			Offset = Trb._f.BaseStream.Position;
			Trb._f.BaseStream.Seek(Size, SeekOrigin.Current);
		}
	}
}