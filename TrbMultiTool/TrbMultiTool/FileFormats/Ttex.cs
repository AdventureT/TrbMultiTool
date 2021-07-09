using PrimeWPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
    public class Ttex
    {
        public uint Unknown { get; set; }

        public uint TextureNameOffset { get; set; }

        public string TextureName { get; set; }

        public uint TextureInfoOffset { get; set; }

        public uint DDSSize { get; set; }

        public uint DDSOffset { get; set; }

        public byte[] RawImage { get; set; }

        public DDSImage DDS { get; set; }

        public uint Width { get; set; }

        public uint Height { get; set; }

        public uint MipMapCount { get; set; }

        public ushort Width2 { get; set; }

        public ushort Height2 { get; set; }

        public uint Type { get; set; }

        public uint Unknown2 { get; set; }

        public List<uint> Offsets { get; set; } = new();


        public Ttex(uint offset)
        {
            Unknown = Trb.SectFile.ReadUInt32();
            Offsets.Add((uint)Trb.SectFile.BaseStream.Position - offset);
            TextureNameOffset = Trb.SectFile.ReadUInt32();
            TextureName = ReadHelper.ReadStringFromOffset(Trb.SectFile, TextureNameOffset + offset);
            Offsets.Add((uint)Trb.SectFile.BaseStream.Position - offset);
            TextureInfoOffset = Trb.SectFile.ReadUInt32();
            DDSSize = Trb.SectFile.ReadUInt32();
            Offsets.Add((uint)Trb.SectFile.BaseStream.Position - offset);
            DDSOffset = Trb.SectFile.ReadUInt32();
            RawImage = ReadHelper.ReadFromOffset(Trb.SectFile, DDSSize, DDSOffset + offset);
            DDS = new DDSImage(RawImage);
            Trb.SectFile.BaseStream.Seek(TextureInfoOffset + offset, System.IO.SeekOrigin.Begin);
            Width = Trb.SectFile.ReadUInt32();
            Height = Trb.SectFile.ReadUInt32();
            MipMapCount = Trb.SectFile.ReadUInt32();
            Width2 = Trb.SectFile.ReadUInt16();
            Height2 = Trb.SectFile.ReadUInt16();
            Type = Trb.SectFile.ReadUInt32();
            Unknown = Trb.SectFile.ReadUInt32();
        }

        public static ulong ResourceNameHash(string resourceName)
        {
            var hash = (ulong)0;
            for (var i = 0; i < resourceName.Length; ++i)
            {
                var value = (ulong)(sbyte)resourceName[i];
                hash = (hash << 4) + hash + value;
                hash &= 0xFFFFFFFF;
            }
            return hash;
        }

        private static byte[] GetStringBytes(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        private byte[] Read4Bytes(MemoryStream ms)
        {
            var width = new byte[4];
            ms.Read(width, 0, 4);
            return width;
        }

        private byte[] Read2Bytes(MemoryStream ms)
        {
            var width = new byte[2];
            ms.Read(width, 0, 2);
            return width;
        }

        public MemoryStream Repack(MemoryStream dds)
        {
            //var f = new BinaryWriter(File.Open("C:\\Users\\nepel\\Desktop\\new.trb", FileMode.Create));
            var sect = new MemoryStream();

            sect.Write(BitConverter.GetBytes(Unknown));
            sect.Write(BitConverter.GetBytes(TextureNameOffset));
            sect.Write(BitConverter.GetBytes(TextureInfoOffset));
            sect.Write(BitConverter.GetBytes((int)dds.Length));
            sect.Write(BitConverter.GetBytes(DDSOffset));
            sect.Write(BitConverter.GetBytes(0));
            sect.Write(BitConverter.GetBytes(0));
            sect.Write(BitConverter.GetBytes(1));
            sect.Write(BitConverter.GetBytes(1));
            sect.Write(BitConverter.GetBytes(1));
            sect.Write(BitConverter.GetBytes(0x14));
            sect.Write(BitConverter.GetBytes(0));
            sect.Write(GetStringBytes(new string(TextureName.Append('\0').ToArray())));

            var BytesToSkip = 4 - ((TextureName.Length + 1) % 4);

            sect.Seek(BytesToSkip, SeekOrigin.Current);

            //Height
            dds.Seek(16, SeekOrigin.Begin);
            sect.Write(Read4Bytes(dds));

            //Width
            dds.Seek(12, SeekOrigin.Begin);
            sect.Write(Read4Bytes(dds));

            //MipMapCount
            dds.Seek(28, SeekOrigin.Begin);
            sect.Write(Read4Bytes(dds));

            //Unknown most of the time this is Height
            dds.Seek(16, SeekOrigin.Begin);
            sect.Write(Read2Bytes(dds));

            //Unknown most of the time this is Width
            dds.Seek(12, SeekOrigin.Begin);
            sect.Write(Read2Bytes(dds));

            //Type
            dds.Seek(84, SeekOrigin.Begin);
            byte[] type = Read4Bytes(dds);
            if (BitConverter.ToInt32(type) == 0) sect.Write(BitConverter.GetBytes(15));
            else sect.Write(type);

            sect.Write(BitConverter.GetBytes(0));

            sect.Write(dds.ToArray());

            return sect;

        }

        public MemoryStream Repack()
        {
            //var f = new BinaryWriter(File.Open("C:\\Users\\nepel\\Desktop\\new.trb", FileMode.Create));
            var sect = new MemoryStream();

            sect.Write(BitConverter.GetBytes(Unknown));
            sect.Write(BitConverter.GetBytes(TextureNameOffset));
            sect.Write(BitConverter.GetBytes(TextureInfoOffset));
            sect.Write(BitConverter.GetBytes(DDSSize));
            sect.Write(BitConverter.GetBytes(DDSOffset));
            sect.Write(BitConverter.GetBytes(0));
            sect.Write(BitConverter.GetBytes(0));
            sect.Write(BitConverter.GetBytes(1));
            sect.Write(BitConverter.GetBytes(1));
            sect.Write(BitConverter.GetBytes(1));
            sect.Write(BitConverter.GetBytes(0x14));
            sect.Write(BitConverter.GetBytes(0));
            sect.Write(GetStringBytes(new string(TextureName.Append('\0').ToArray())));

            var BytesToSkip = 4 - ((TextureName.Length + 1) % 4);

            sect.Seek(BytesToSkip, SeekOrigin.Current);

            sect.Write(BitConverter.GetBytes(Height));
            sect.Write(BitConverter.GetBytes(Width));
            sect.Write(BitConverter.GetBytes(MipMapCount));
            sect.Write(BitConverter.GetBytes(Height2));
            sect.Write(BitConverter.GetBytes(Width2));
            sect.Write(BitConverter.GetBytes(Type));
            sect.Write(BitConverter.GetBytes(0));
            sect.Write(RawImage);

            return sect;
        }

        /*
        public void GenerateFile(string path)
        {
            binaryWriter = new(File.Open(path, FileMode.Create));

            // TSFL
            binaryWriter.Write(GetStringBytes("TSFL"));
            binaryWriter.Write(0); // Size of TSFL

            // HDRX
            binaryWriter.Write(GetStringBytes("TRBFHDRX"));
            binaryWriter.Write(24); // Size of HDRX
            binaryWriter.Write((short)1); // Flag 1
            binaryWriter.Write((short)1); // Flag 2
            binaryWriter.Write(1); // Count of Files (only LocaleStrings)

            // HDRX tag infos
            binaryWriter.Write(0); // Unk

            int sectSizePos1 = (int)binaryWriter.BaseStream.Position;
            binaryWriter.Write(0); // Size of SECT
            binaryWriter.Write(0); // Unk1
            binaryWriter.Write(0); // Unk2

            // SECT
            binaryWriter.Write(GetStringBytes("SECT"));
            binaryWriter.Write(0); // Size of SECT
            int sectPos = (int)binaryWriter.BaseStream.Position;
            List<LocaleString> stringsInfos = new();

            // Write strings
            int stringsLen = 0;
            foreach (string str in strings)
            {
                stringsLen += WriteString(ref stringsInfos, str);
            }

            // Check if data length is not even and make it even
            if (stringsLen % 2 != 0)
                WriteString(ref stringsInfos, "");

            binaryWriter.Seek(DATA_MARGIN, SeekOrigin.Current); // Make a margin between strings and it's offsets
            totalData += DATA_MARGIN;

            // Write offsets to strings
            List<uint> pointersToOffsets = new();
            foreach (LocaleString lS in stringsInfos)
            {
                pointersToOffsets.Add((uint)(binaryWriter.BaseStream.Position - sectPos));
                binaryWriter.Write(lS.offset);
            }

            binaryWriter.Write(0); // Unk1
            binaryWriter.Write(0); // Unk2
            binaryWriter.Write(0); // Unk3
            int countOfStringsPos = (int)(binaryWriter.BaseStream.Position - sectPos);
            binaryWriter.Write(strings.Count); // Count of strings
            binaryWriter.Write(totalData); // Size of strings data
            binaryWriter.Write((short)0); // Unk4
            binaryWriter.Write((ushort)57760); // Flag?
            binaryWriter.Write((short)0); // Unk5
            binaryWriter.Write((ushort)57760); // Flag?

            uint sectSize = (uint)(binaryWriter.BaseStream.Position - sectPos);

            // RELC
            binaryWriter.Write(GetStringBytes("RELC"));
            binaryWriter.Write(0); // Size of RELC
            int relcPos = (int)binaryWriter.BaseStream.Position;
            binaryWriter.Write(stringsInfos.Count + 1); // Count of strings + offset to data size

            foreach (uint pointer in pointersToOffsets)
            {
                binaryWriter.Write(0); // Unk
                binaryWriter.Write(pointer); // Pointer to offset of string
            }

            binaryWriter.Write(0); // Unk
            binaryWriter.Write(countOfStringsPos + 4); // Pointer to strings size

            uint relcSize = (uint)(binaryWriter.BaseStream.Position - relcPos);

            // SYMB
            binaryWriter.Write(GetStringBytes("SYMB"));
            binaryWriter.Write(30); // Size of SYMB
            int symbPos = (int)binaryWriter.BaseStream.Position;
            binaryWriter.Write(1); // Count of files
            binaryWriter.Write((short)0); // ID
            binaryWriter.Write(0); // Name Offset
            binaryWriter.Write((ushort)25256); // Name ID
            binaryWriter.Write(countOfStringsPos); // Offset to count of strings
            binaryWriter.Write(GetStringBytes("LocaleStrings"));
            binaryWriter.Write((byte)0);

            // Writing sizes
            uint fileSize = (uint)binaryWriter.BaseStream.Position - 8;
            binaryWriter.Seek(4, SeekOrigin.Begin);
            binaryWriter.Write(fileSize); // TSFL Size

            binaryWriter.Seek(sectSizePos1, SeekOrigin.Begin);
            binaryWriter.Write(sectSize); // SECT Size in HDRX

            binaryWriter.Seek(sectPos - 4, SeekOrigin.Begin);
            binaryWriter.Write(sectSize); // SECT Size

            binaryWriter.Seek(relcPos - 4, SeekOrigin.Begin);
            binaryWriter.Write(relcSize); // RELC Size

            binaryWriter.Close();
        }
        */
    }
}
