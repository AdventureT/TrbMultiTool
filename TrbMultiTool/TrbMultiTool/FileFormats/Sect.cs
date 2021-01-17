using System.IO;

namespace TrbMultiTool.FileFormats
{
	public class Sect : Tag
	{
		public Sect() : base()
		{
			Trb._f.BaseStream.Seek(Size, SeekOrigin.Current);
		}
	}
}