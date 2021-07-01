﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TrbMultiTool.FileFormats
{
    public class XUI
    {
        public uint XuiOffset { get; set; }
        public int Unk { get; set; }

        public string FileName { get; set; }

        public uint XUIFileOffset { get; set; }

        public List<string> Strings { get; set; } = new();

        public List<ListViewItem> XuiItems { get; set; }


        public record Section(string Name, uint DataOffset, uint DataSize);

        public List<Section> Sections { get; set; } = new();

        public XUI(uint xuiOffset, uint hdrx)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                XuiOffset = xuiOffset;
                Unk = Trb.SectFile.ReadInt32();
                FileName = ReadHelper.ReadStringFromOffset(Trb.SectFile, Trb.SectFile.ReadUInt32() + hdrx);
                XUIFileOffset = Trb.SectFile.ReadUInt32();
                Trb.SectFile.BaseStream.Seek(xuiOffset, SeekOrigin.Begin);
                var name = Encoding.Default.GetString(Trb.SectFile.ReadBytes(3));
                var endianess = Trb.SectFile.ReadChar();
                if (endianess == 'L')
                {
                    MessageBox.Show("Contact me and send me your file, Discord: AdventureT#5879");
                    return;
                }
                var unknown1 = ReadHelper.ReadUInt32B(Trb.SectFile);
                var unknown2 = ReadHelper.ReadUInt32B(Trb.SectFile);
                var unknown3 = ReadHelper.ReadUInt16B(Trb.SectFile);
                var unknown4 = ReadHelper.ReadUInt16B(Trb.SectFile);
                var xuibSize = ReadHelper.ReadUInt16B(Trb.SectFile);
                var subLabelCount = ReadHelper.ReadUInt16B(Trb.SectFile);
                if (unknown2 != 0) Trb.SectFile.BaseStream.Seek(40, SeekOrigin.Current);
                for (int i = 0; i < subLabelCount; i++)
                {
                    Sections.Add(new(Encoding.Default.GetString(Trb.SectFile.ReadBytes(4)), ReadHelper.ReadUInt32B(Trb.SectFile), ReadHelper.ReadUInt32B(Trb.SectFile)));
                }
                foreach (var item in Sections)
                {
                    Trb.SectFile.BaseStream.Seek(xuiOffset + item.DataOffset, SeekOrigin.Begin);
                    switch (item.Name)
                    {
                        case "STRN":
                            ReadSTRN(item);
                            break;
                        case "VECT":
                            ReadVECT(item);
                            break;
                        default:
                            //MessageBox.Show("Contact me and send me your file, Discord: AdventureT#5879");
                            break;
                    }
                }
            });
        }

        private void ReadSTRN(Section sec)
        {
            do
            {
                var count = ReadHelper.ReadUInt16B(Trb.SectFile);
                Strings.Add(ReadHelper.ReadUnicodeStringB(Trb.SectFile, count));
                
            } while (sec.DataSize > Trb.SectFile.BaseStream.Position - XuiOffset - sec.DataOffset);
        }

        private void ReadVECT(Section sec)
        {
            do
            {
                //var vec = ReadHelper.ReadUInt16B(Trb.SectFile); //Vector 12 bytes
                //var vec2 = ReadHelper.ReadUInt16B(Trb.SectFile); //Vector 12 bytes

            } while (sec.DataSize > Trb.SectFile.BaseStream.Position - XuiOffset - sec.DataOffset);
        }
    }
}
