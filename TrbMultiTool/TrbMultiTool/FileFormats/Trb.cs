using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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

        public static Tsfl Tsfl
        {
            get;
            set;
        }

        public Trb(string fileName, Game game, bool onlyExtract = false)
        {
            //SectFile = new BinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
            //SectFile.BaseStream.Seek(0, SeekOrigin.Begin);
            //var Quest = new Quest();
            //return;

            _fileName = fileName;
            _safeFileName = fileName.Split("\\").Last();
            _game = game;

            _f = new BinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
            Tsfl = new Tsfl();
            SectFile = new BinaryReader(new MemoryStream(Tsfl.Sect.Data.ToArray()));
            uint hdrx = 0;

            if (!onlyExtract)
            {
                // Open file and display info

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
            }
            else if (onlyExtract && Tsfl.Sect.Label == "SECC")
            {
                // EXTRACT BTEC
                _f.Close();

                // Reading all bytes from the original file and making new reader to make file not used (for rewriting)
                byte[] originalFileData = File.ReadAllBytes(_fileName);
                MemoryStream originalFileStream = new MemoryStream(originalFileData);

                _f = new BinaryReader(originalFileStream);

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = _safeFileName;
                dlg.Filter = $"Original Extension|*.{_safeFileName.Split('.').Last()}";

                if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.FileName))
                {
                    using (var writer = new BinaryWriter(File.Open(dlg.FileName, FileMode.Create, FileAccess.Write)))
                    {
                        _f.BaseStream.Seek(0, SeekOrigin.Begin);
                        byte[] originalHeader = _f.ReadBytes((int)Tsfl.Sect.Offset - 8);

                        writer.Write(originalHeader);
                        writer.Write(Encoding.Default.GetBytes("SECT")); // Magic
                        writer.Write(Tsfl.Sect.Data.Count); // Write size of SECT
                        writer.Write(Tsfl.Sect.Data.ToArray()); // Decompressed Data

                        // Write RELC
                        _f.BaseStream.Seek(Tsfl.Relc.Offset, SeekOrigin.Begin);
                        writer.Write(Encoding.Default.GetBytes("RELC")); // Magic
                        writer.Write((int)Tsfl.Relc.Size); // Write size of RELC
                        byte[] originalRELC = _f.ReadBytes((int)Tsfl.Relc.Size);
                        writer.Write(originalRELC);

                        // Write SYMB
                        _f.BaseStream.Seek(Tsfl.Symb.Offset, SeekOrigin.Begin);
                        writer.Write(Encoding.Default.GetBytes("SYMB")); // Magic
                        writer.Write((int)Tsfl.Symb.Size); // Write size of SYMB
                        byte[] originalSYMB = _f.ReadBytes((int)Tsfl.Symb.Size);
                        writer.Write(originalSYMB);

                        // Write File Size
                        long fileSize = writer.BaseStream.Position - 8;
                        writer.Seek(4, SeekOrigin.Begin);
                        writer.Write((int)fileSize);

                        writer.Close();
                    }
                }

                originalFileStream.Close();
            }

            _f.Close();
            finishedLoading = true;
        }
    }
}