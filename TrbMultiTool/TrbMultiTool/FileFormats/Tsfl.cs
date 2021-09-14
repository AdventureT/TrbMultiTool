namespace TrbMultiTool.FileFormats
{
	public class Tsfl
	{
        public string Tsf { get; set; }
        public uint Size { get; set; }
        public string Trbf { get; set; }
		public Hdrx Hdrx { get; set; }
		public Head Head { get; set; }
		public Sect Sect { get; set; }
		public Relc Relc { get; set; }
		public Symb Symb { get; set; }

		public Tsfl()
		{
			Tsf = new string(Trb._f.ReadChars(4));
			if (Tsf[Tsf.Length-1] == 'B') Trb._f._endianness = EndiannessAwareBinaryReader.Endianness.Big;
			Size = Trb._f.ReadUInt32();
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
