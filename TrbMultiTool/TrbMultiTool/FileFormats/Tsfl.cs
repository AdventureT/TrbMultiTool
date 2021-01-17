using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
	class Tsfl : Tag
	{
        public string Trbf { get; set; }
        public Hdrx Hdrx { get; set; }
		public Sect Sect { get; set; }
		public Relc Relc { get; set; }
		public Symb Symb { get; set; }

		public Tsfl() : base()
		{
			Trbf = new string(Trb._f.ReadChars(4));
			Hdrx = new();
			Sect = new();
			Relc = new();
			Symb = new();

		}
	}
}
