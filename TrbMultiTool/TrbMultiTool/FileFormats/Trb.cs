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
        public static Game _game;
		public static BinaryReader _f;
		public static BinaryReader SectFile;
		public static string _fileName;
        public bool finishedLoading = false;

        public List<Ttex> ttexes = new();
        public List<Ttl> ttls = new();
        public List<Tmdl> tmdls = new();
        public List<Tmat> tmats = new();

        public static Tsfl Tsfl { get; set; }

		public Trb(string fileName, Game game)
		{
			_fileName = fileName;
			_safeFileName = fileName.Split("\\").Last();
            _game = game;
            //SectFile = new BinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
            //SectFile.BaseStream.Seek(0, SeekOrigin.Begin);
            //var Quest = new Quest();
            //return;
            _f = new BinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
            Tsfl = new Tsfl();
			SectFile = new BinaryReader(new MemoryStream(Tsfl.Sect.Data.ToArray()));
			uint hdrx = 0;

            //var TTLWindow = new TtlWindow();
            //var TMDLWindow = new TmdlWindow();

            var groupByIds = Tsfl.Symb.NameEntries.GroupBy(e => e.ID); //Group by IDs
            foreach (var item in groupByIds)
            {
                hdrx = Tsfl.Hdrx.TagInfos[item.Key].Offset;

                if (item.FirstOrDefault().Name.Contains("FileHeader"))
                {
                    var Tmdl = new Tmdl(item.ToList(), hdrx);
                    tmdls.Add(Tmdl);
                    //TMDLWindow.AddTmdl(Tmdl);
                }
                else if (item.FirstOrDefault().Name.Contains("TTL"))
                {
                    var Ttl = new Ttl(item.FirstOrDefault().DataOffset + hdrx, item.FirstOrDefault().Name);
                    ttls.Add(Ttl);
                }
                else if (item.FirstOrDefault().Name.Contains("Main"))
                {
                    _f.BaseStream.Seek(item.FirstOrDefault().DataOffset, SeekOrigin.Begin);
                    var Quest = new PProperty();
                }
                else if (item.FirstOrDefault().Name.Contains("ttex"))
                {
                    SectFile.BaseStream.Seek(item.FirstOrDefault().DataOffset + hdrx, SeekOrigin.Begin);
                    ttexes.Add(new Ttex(item.FirstOrDefault().DataOffset + hdrx));
                }
                else if (item.FirstOrDefault().Name.Contains("tmat"))
                {
                    SectFile.BaseStream.Seek(item.FirstOrDefault().DataOffset + hdrx, SeekOrigin.Begin);
                    tmats.Add(new Tmat(item.FirstOrDefault().DataOffset + hdrx));
                }
            }
            _f.Close();
            finishedLoading = true;
		}
	}
}
