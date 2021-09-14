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
        public static EndiannessAwareBinaryReader _f;
        public static EndiannessAwareBinaryReader SectFile;
        public static string _fileName;
        public bool finishedLoading = false;

        public bool ContainsXui;

        public List<Ttex> ttexes = new();
        public List<Ttl> ttls = new();
        public List<Tmdl> tmdls = new();
        public List<Tmat> tmats = new();

        public List<XUI> xuis = new();

        public static Tsfl Tsfl
        {
            get;
            set;
        }

        private static byte[] GetStringBytes(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        public static void GenerateFile(string path, MemoryStream sect, List<uint> filesSizes, List<List<uint>> offsets, List<string> names, List<short> idx)
        {
            List<uint> finalFileSizes = new();

            for (int i = 0; i < Tsfl.Hdrx.Files; i++)
            {
                if (!idx.Contains((short)i))
                {
                    finalFileSizes.Add(Tsfl.Hdrx.TagInfos[i].TagSize);
                }
                else
                {
                    finalFileSizes.Add(filesSizes[i]);
                }
            }


            BinaryWriter binaryWriter = new(File.Open(path, FileMode.Create));

            // TSFL
            binaryWriter.Write(GetStringBytes("TSFL"));
            binaryWriter.Write(0); // Size of TSFL

            // HDRX
            binaryWriter.Write(GetStringBytes("TRBFHDRX"));
            var hdrx = GenerateHDRX(finalFileSizes.Count, finalFileSizes);
            binaryWriter.Write((uint)hdrx.Length); // Size of HDRX
            binaryWriter.Write(hdrx.ToArray());

            // SECT
            binaryWriter.Write(GetStringBytes("SECT"));
            binaryWriter.Write((uint)sect.Length); // Size of SECT
            binaryWriter.Write(sect.ToArray());
            

            // RELC
            if (offsets.Count != 0)
            {
                binaryWriter.Write(GetStringBytes("RELC"));
                var relc = GenerateRELC(offsets);
                binaryWriter.Write((uint)relc.Length);
                binaryWriter.Write(relc.ToArray());
            }
            
            // SYMB
            binaryWriter.Write(GetStringBytes("SYMB"));
            var symb = GenerateSYMB(names);
            binaryWriter.Write((uint)symb.Length);
            binaryWriter.Write(symb.ToArray());

            binaryWriter.BaseStream.Seek(4, SeekOrigin.Begin);
            binaryWriter.Write((uint)binaryWriter.BaseStream.Length - 8);

            binaryWriter.Close();
        }

        private static MemoryStream GenerateHDRX(int countOfFiles, List<uint> filesSizes)
        {
            var hdrx = new MemoryStream();
            
            hdrx.Write(BitConverter.GetBytes((ushort)1)); // Flag 1
            hdrx.Write(BitConverter.GetBytes((ushort)1)); // Flag 2
            hdrx.Write(BitConverter.GetBytes(countOfFiles)); // Count of Files

            for (int i = 0; i < countOfFiles; i++)
            {
                // HDRX tag infos
                hdrx.Write(BitConverter.GetBytes(0)); // Unk
                hdrx.Write(BitConverter.GetBytes(filesSizes[i])); // Size of File
                hdrx.Write(BitConverter.GetBytes(0)); // Unk1
                hdrx.Write(BitConverter.GetBytes(0)); // Unk2
            }

            return hdrx;
        }

        public static void AppendFile(string path, MemoryStream sect, uint fileSize, List<uint> offsets, string name)
        {
            BinaryWriter binaryWriter = new(File.Open(path, FileMode.OpenOrCreate));

            // TSFL
            binaryWriter.Write(GetStringBytes("TSFL"));
            binaryWriter.Write(0); // Size of TSFL

            // HDRX
            binaryWriter.Write(GetStringBytes("TRBFHDRX"));
            using var hdrx = GenerateHDRX((int)Tsfl.Hdrx.Files+1, Tsfl.Hdrx.TagInfos.Select(x => x.TagSize).Append(fileSize).ToList()); //Appending new fileSize
            binaryWriter.Write((uint)hdrx.Length); // Size of HDRX
            binaryWriter.Write(hdrx.ToArray());

            EndiannessAwareBinaryReader binaryReader = new(File.Open(_fileName, FileMode.Open));
            binaryReader.BaseStream.Seek(0x10, SeekOrigin.Begin); //Seek to hdrx size
            binaryReader.BaseStream.Seek(binaryReader.ReadUInt32()+4, SeekOrigin.Current); //skip hdrx size and to sect size
            var oldSectSize = binaryReader.ReadUInt32();
            var oldSect = binaryReader.ReadBytes((int)oldSectSize);

            // SECT
            binaryWriter.Write(GetStringBytes("SECT"));
            binaryWriter.Write((uint)sect.Length + oldSectSize); // Size of SECT
            binaryWriter.Write(oldSect); //Write old Sect
            binaryWriter.Write(sect.ToArray()); //Write new Sect = Combined new SECT

            binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
            var oldRelcSize = binaryReader.ReadUInt32();
            binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
            var oldRelc = binaryReader.ReadBytes((int)oldRelcSize-4);

            // RELC
            if (Tsfl.Relc.Count != 0)
            {
                binaryWriter.Write(GetStringBytes("RELC"));

                using var relc = new MemoryStream();
                relc.Write(oldRelc);
                for (int i = 0; i < offsets.Count; i++)
                {
                    relc.Write(BitConverter.GetBytes((ushort)Tsfl.Hdrx.Files)); //Source offset hdrx index
                    relc.Write(BitConverter.GetBytes((ushort)Tsfl.Hdrx.Files)); //Target offset hdrx index < not correct yet!
                    relc.Write(BitConverter.GetBytes(offsets[i]));
                }
                
                binaryWriter.Write((uint)relc.Length+4);
                binaryWriter.Write((uint)offsets.Count + Tsfl.Relc.Count);
                binaryWriter.Write(relc.ToArray());
            }

            binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
            var oldSymbSize = binaryReader.ReadUInt32();
            var oldSymbCount = binaryReader.ReadUInt32();
            var oldSymbEntries = binaryReader.ReadBytes((int)oldSymbCount * 12);
            var oldSymbNames = binaryReader.ReadBytes((int)((int)oldSymbSize - (oldSymbCount * 12) - 4));

            // SYMB
            binaryWriter.Write(GetStringBytes("SYMB"));

            using var symb = new MemoryStream();
            symb.Write(BitConverter.GetBytes(Tsfl.Symb.Count+1));
            symb.Write(oldSymbEntries);
            symb.Write(BitConverter.GetBytes((ushort)(Tsfl.Hdrx.Files))); // HDRX ID
            symb.Write(BitConverter.GetBytes((ushort)oldSymbNames.Length)); // Name offset
            symb.Write(BitConverter.GetBytes((ushort)0)); // Padding

            //The game doesn't read namehashes...
            if (name == "ttex\0")
            {
                symb.Write(BitConverter.GetBytes((ushort)Ttex.ResourceNameHash(name))); // Namehash
            }
            else if (name.EndsWith("TTL\0"))
            {
                symb.Write(BitConverter.GetBytes((ushort)17868));
            }
            else
            {
                symb.Write(BitConverter.GetBytes((ushort)7365)); // Namehash some random one TODO
            }

            symb.Write(BitConverter.GetBytes((uint)0)); // DataOffset ttex is 0 other ones TODO
            symb.Write(oldSymbNames);
            symb.Write(GetStringBytes(name));

            binaryWriter.Write((uint)symb.Length);
            binaryWriter.Write(symb.ToArray());

            binaryWriter.BaseStream.Seek(4, SeekOrigin.Begin);
            binaryWriter.Write((uint)binaryWriter.BaseStream.Length - 8);

            binaryWriter.Close();
        }

        private static uint GetCountOf2DArray(List<List<uint>> array)
        {
            uint count = 0;
            foreach (var item in array)
            {
                foreach (var item2 in item)
                {
                    count++;
                }
            }
            return count;
        }

        private static MemoryStream GenerateRELC(List<List<uint>> offsets)
        {
            var relc = new MemoryStream();

            relc.Write(BitConverter.GetBytes(GetCountOf2DArray(offsets))); // Count of Relocations

            for (int i = 0; i < offsets.Count; i++)
            {
                foreach (var offset in offsets[i])
                {
                    relc.Write(BitConverter.GetBytes((ushort)i)); //Source offset hdrx index
                    relc.Write(BitConverter.GetBytes((ushort)i)); //Target offset hdrx index < not correct yet!
                    relc.Write(BitConverter.GetBytes(offset));
                }
            }

            return relc;
        }

        private static MemoryStream GenerateSYMB(List<string> names)
        {
            var symb = new MemoryStream();

            symb.Write(BitConverter.GetBytes(names.Count)); // Count of files

            var nameOffsets = new List<uint>();

            for (int i = -1; i < names.Count; i++)
            {
                nameOffsets.Add(i == -1 ? 0 : (uint)names[i].Length + (uint)nameOffsets[i]);
            }

            for (int i = 0; i < names.Count; i++)
            {
                symb.Write(BitConverter.GetBytes((short)i)); // HDRX ID
                symb.Write(BitConverter.GetBytes((ushort)nameOffsets[i])); // Name offset
                symb.Write(BitConverter.GetBytes((ushort)0)); // Padding

                //The game doesn't read namehashes...
                if (names[i] == "ttex\0")
                {
                    symb.Write(BitConverter.GetBytes((ushort)Ttex.ResourceNameHash(names[i]))); // Namehash
                }
                else if (names[i].EndsWith("TTL\0"))
                {
                    symb.Write(BitConverter.GetBytes((ushort)17868));
                }
                else
                {
                    symb.Write(BitConverter.GetBytes((ushort)7365)); // Namehash some random one TODO
                }

                symb.Write(BitConverter.GetBytes((uint)0)); // DataOffset ttex is 0 other ones TODO
            }

            foreach (var name in names)
            {
                symb.Write(GetStringBytes(name)); // NameStr
            }

            return symb;
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

            _f = new EndiannessAwareBinaryReader(File.Open(_fileName, FileMode.Open, FileAccess.Read));
            Tsfl = new Tsfl();
            SectFile = new EndiannessAwareBinaryReader(new MemoryStream(Tsfl.Sect.Data.ToArray()), _f._endianness);
            uint hdrx = 0;

            if (!onlyExtract)
            {
                // Open file and display info

                var groupByIds = Tsfl.Symb.NameEntries.GroupBy(e => e.ID); //Group by IDs
                foreach (var item in groupByIds)
                {
                    if (Tsfl.Hdrx == null) hdrx = 0;
                    else hdrx = Tsfl.Hdrx.TagInfos[item.Key].Offset;

                    if (item.FirstOrDefault().Name.Contains("FileHeader") || item.FirstOrDefault().Name.Contains("Database"))
                    {
                        var Tmdl = new Tmdl(item.ToList(), hdrx);
                        tmdls.Add(Tmdl);
                        //TMDLWindow.AddTmdl(Tmdl);
                    }
                    else if (item.FirstOrDefault().Name.Contains("TTL"))
                    {
                        var Ttl = new Ttl(item.FirstOrDefault().DataOffset + hdrx, item.FirstOrDefault().Name, item.FirstOrDefault().ID);
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
                        ttexes.Add(new Ttex(item.FirstOrDefault().DataOffset + hdrx, item.FirstOrDefault().ID));
                    }
                    else if (item.FirstOrDefault().Name.Contains("tmat"))
                    {
                        SectFile.BaseStream.Seek(item.FirstOrDefault().DataOffset + hdrx, SeekOrigin.Begin);
                        tmats.Add(new Tmat(item.FirstOrDefault().DataOffset + hdrx));
                    }
                    else if (item.FirstOrDefault().Name.Contains("LocaleStrings"))
                    {
                        SectFile.BaseStream.Seek(item.FirstOrDefault().DataOffset + hdrx, SeekOrigin.Begin);
                        var LocaleStrings = new LocaleStrings();
                    }
                    else if (item.FirstOrDefault().Name.Contains("txui"))
                    {
                        SectFile.BaseStream.Seek(item.FirstOrDefault().DataOffset + hdrx, SeekOrigin.Begin);
                        ContainsXui = true;
                        var xuiOffset = Tsfl.Hdrx.TagInfos[item.Key+1].Offset;
                        xuis.Add(new XUI(xuiOffset, hdrx));
                    }
                    else if (item.FirstOrDefault().Name.Contains("terrainvis"))
                    {
                        SectFile.BaseStream.Seek(item.FirstOrDefault().DataOffset + hdrx, SeekOrigin.Begin);
                        new TerrainVIS();
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

                _f = new EndiannessAwareBinaryReader(originalFileStream);

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
            SectFile.Close();
            _f.Close();
            finishedLoading = true;
        }
    }
}