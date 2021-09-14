using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TrbMultiTool.FileFormats
{
    struct LocaleString
    {
        public int id;
        public uint pointer;
        public uint offset;
        public string text;
    }

    class LocaleStringsFile
    {
        uint totalData = 0;
        const int DATA_MARGIN = 12;
        const int TEXT_MARGIN = 2;
        List<string> strings = new();
        BinaryWriter binaryWriter;

        private byte[] GetStringBytes(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        private byte[] GetUnicodeStringBytes(string str)
        {
            return Encoding.Unicode.GetBytes(str);
        }

        private int WriteString(ref List<LocaleString> stringsInfos, string str)
        {
            LocaleString lS = new();
            lS.text = str;
            lS.offset = totalData;

            byte[] uBytes = GetUnicodeStringBytes(str);
            binaryWriter.Write(uBytes);
            binaryWriter.Seek(TEXT_MARGIN, SeekOrigin.Current);

            totalData += (uint)(uBytes.Length + TEXT_MARGIN);
            stringsInfos.Add(lS);

            return str.Length + 1;
        }

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

        public void AddString(string str)
        {
            strings.Add(str);
        }
    }


    class LocaleStrings
    {
        public LocaleStrings()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var window = new LocaleStringsWindow();

                window.Title = $"Viewing LocaleStrings: {Trb._fileName}";

                int i = 0;
                foreach (var info in Trb.Tsfl.Relc.StructInfos)
                {
                    Trb.SectFile.BaseStream.Seek(info.Offset, SeekOrigin.Begin);
                    uint stringOffset = Trb.SectFile.ReadUInt32();
                    Trb.SectFile.BaseStream.Seek(stringOffset, SeekOrigin.Begin);
                    string text = Trb.SectFile.ReadUnicodeString();

                    ListViewItem item = new ListViewItem
                    {
                        Content = text
                    };

                    item.Tag = new LocaleString {
                        id = i++,
                        pointer = info.Offset,
                        offset = stringOffset,
                        text = text
                    };

                    window.ListView.Items.Add(item);
                }

                window.ListView.Items.RemoveAt(window.ListView.Items.Count - 1);

                window.Show();
            });
        }
    }
}
