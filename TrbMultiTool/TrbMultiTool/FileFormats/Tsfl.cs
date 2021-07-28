namespace TrbMultiTool.FileFormats
{
	public class Tsfl : Tag
	{
		public string Trbf { get; set; }
		public Hdrx Hdrx { get; set; }
		public Head Head { get; set; }
		public Sect Sect { get; set; }
		public Relc Relc { get; set; }
		public Symb Symb { get; set; }

		public Tsfl() : base()
		{
			Trbf = new string(Trb._f.ReadChars(4));
			if (new string(Trb._f.ReadChars(4)) == "HEAD")
            {
				Trb._f.BaseStream.Seek(-4, System.IO.SeekOrigin.Current);
				Head = new();
			}
            else
            {
				Trb._f.BaseStream.Seek(-4, System.IO.SeekOrigin.Current);
				Hdrx = new();
			}
			
			Sect = new();
			Relc = new();
			Symb = new();
		}
	}
}
