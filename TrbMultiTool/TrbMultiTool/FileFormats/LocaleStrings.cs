using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
                    string text = ReadHelper.ReadUnicodeString(Trb.SectFile);

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

                window.Show();
            });
        }
    }
}
