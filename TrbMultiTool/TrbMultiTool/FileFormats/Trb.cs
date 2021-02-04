using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrbMultiTool.FileFormats;

namespace TrbMultiTool
{
	public class Trb
	{
		public static string _safeFileName;
		public static BinaryReader _f;
		public static string _fileName;
		public static Tsfl Tsfl { get; set; }

		public Trb(string fileName)
		{
			_fileName = fileName;
			_safeFileName = fileName.Split("\\").Last();
			_f = new BinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
			Tsfl = new Tsfl();
			var hdrx = Tsfl.Sect.Offset;
			var previousHdrxIndex = 0;
			var TTLWindow = new TtlWindow();
			for (int i = 0; i < Tsfl.Symb.NameEntries.Count; i++)
			{
				var nameEntry = Tsfl.Symb.NameEntries[i];
				if (previousHdrxIndex != nameEntry.ID)
				{
					hdrx += Tsfl.Hdrx.TagInfos[previousHdrxIndex].TagSize;
					previousHdrxIndex++;
				}
				var name = nameEntry.Name;
				if (nameEntry.Name.Contains('_')) //Pretty dirty
				{
					var splittedNameEntry = nameEntry.Name.Split('_');
					name = splittedNameEntry.Last();
				}
				switch (name)
				{
					case "TTL":
						var Ttl = new Ttl(nameEntry.DataOffset + (uint)hdrx, nameEntry.Name);
						TTLWindow.AddTtl(Ttl);
						break;
					case "FileHeader":
						var Tmdl = new Tmdl(Tsfl.Symb.NameEntries, i, hdrx);
						break;
					case "Main":
						_f.BaseStream.Seek(nameEntry.DataOffset + (uint)hdrx, SeekOrigin.Begin);
						var Quest = new Quest();
						break;
					default:
						break;
				}
			}
			if (TTLWindow.Ttls.Any()) TTLWindow.ShowDialog();
			_f.Close();
		}
	}
}
