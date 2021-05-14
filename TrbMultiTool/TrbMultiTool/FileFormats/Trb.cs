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
		public static BinaryReader SectFile;
		public static string _fileName;
		public static Tsfl Tsfl { get; set; }

		public Trb(string fileName)
		{
			_fileName = fileName;
			_safeFileName = fileName.Split("\\").Last();
            //SectFile = new BinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
            //SectFile.BaseStream.Seek(0, SeekOrigin.Begin);
            //var Quest = new Quest();
            //return;
            _f = new BinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
            Tsfl = new Tsfl();
			SectFile = new BinaryReader(new MemoryStream(Tsfl.Sect.Data.ToArray()));
			uint hdrx = 0;
			var previousHdrxIndex = -1;
            var previousNameEntries = new List<Symb.NameEntry>();

            var TTLWindow = new TtlWindow();
            var TMDLWindow = new TmdlWindow();
            for (int i = 0; i < Tsfl.Symb.NameEntries.Count; i++)
			{
                var nameEntry = Tsfl.Symb.NameEntries[i];
                if (previousHdrxIndex+1 != nameEntry.ID || i == Tsfl.Symb.NameEntries.Count-1)
                {
                    if (i != Tsfl.Symb.NameEntries.Count - 1)
                    {
                        if (previousHdrxIndex != -1)
                        {
                            hdrx += Tsfl.Hdrx.TagInfos[previousHdrxIndex].TagSize;
                        }
                        previousHdrxIndex++;
                    }
                    else
                    {
                        if (previousHdrxIndex == -1) previousHdrxIndex++;
                        hdrx += Tsfl.Hdrx.TagInfos[previousHdrxIndex].TagSize;
                        previousNameEntries.Add(nameEntry);
                    }
                    
                    if (previousNameEntries.FirstOrDefault().Name.Contains("FileHeader"))
                    {
                        var Tmdl = new Tmdl(previousNameEntries, hdrx);
                        TMDLWindow.AddTmdl(Tmdl);
                    }
                    else if (previousNameEntries.FirstOrDefault().Name.Contains("TTL"))
                    {
                        var Ttl = new Ttl(previousNameEntries.FirstOrDefault().DataOffset + (uint)hdrx, previousNameEntries.FirstOrDefault().Name);
                        TTLWindow.AddTtl(Ttl);
                    }
                    else if (previousNameEntries.FirstOrDefault().Name.Contains("Main"))
                    {
                        _f.BaseStream.Seek(previousNameEntries.FirstOrDefault().DataOffset, SeekOrigin.Begin);
                        var Quest = new PProperty();
                    }
                    else if (previousNameEntries.FirstOrDefault().Name.Contains("ttex"))
                    {
                        SectFile.BaseStream.Seek(previousNameEntries.FirstOrDefault().DataOffset + (uint)hdrx, SeekOrigin.Begin);
                        TTLWindow.AddTtex(new Ttex(previousNameEntries.FirstOrDefault().DataOffset + (uint)hdrx));
                    }
                    previousNameEntries.Clear();
                }
                previousNameEntries.Add(nameEntry);
            }
			if (TTLWindow.Ttls.Any() || TTLWindow.Ttex.Any()) TTLWindow.Show();
            if (TMDLWindow.Tmdls.Any()) TMDLWindow.Show();
            _f.Close();
		}
	}
}
