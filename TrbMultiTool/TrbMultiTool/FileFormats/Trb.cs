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
        public static BinaryReader _f;
		private string _fileName;

        public static Tsfl Tsfl { get; set; }

        public Trb(string fileName)
		{
			_fileName = fileName;
			_f = new BinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
			Tsfl = new Tsfl();
            var hdrx = Tsfl.Sect.Offset;
            var previousHdrxIndex = 0;
            foreach (var nameEntry in Tsfl.Symb.NameEntries)
            {
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
                        var Ttl = new Ttl(nameEntry.DataOffset + (uint)hdrx);
                        break;
                    default:
                        break;
                }
                //           switch (nameEntry.NameID) // Good idea, but hash hasn't been discovered yet :(
                //           {
                //case 17868:
                //	var Ttl = new Ttl(nameEntry.DataOffset);
                //	break;
                //               default:
                //                   break;
                //           }

            }
			_f.Close();
		}
	}
}
