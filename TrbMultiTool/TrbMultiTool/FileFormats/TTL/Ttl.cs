using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using TrbMultiTool.FileFormats.TTL;

namespace TrbMultiTool.FileFormats
{
    public class Ttl
    {
        public string TtlName { get; set; }
        public uint Offset { get; set; }

        public uint TextureInfoCount { get; set; }
        public List<TextureInfo> TextureInfos { get; set; } = new();
        public string TtlType { get; set; }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource LoadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        public Ttl(uint offset, string ttlName)
        {
            TtlName = ttlName;
            Offset = offset;
            Trb._f.BaseStream.Seek(Offset, System.IO.SeekOrigin.Begin);
            TextureInfoCount = Trb._f.ReadUInt32();
            var pos = Trb._f.BaseStream.Position;
            Trb._f.BaseStream.Seek(Offset + Trb._f.ReadUInt32(), System.IO.SeekOrigin.Begin);
            for (int i = 0; i < TextureInfoCount; i++)
            {
                TextureInfos.Add(new TextureInfo(Offset));
            }
            Trb._f.BaseStream.Seek(pos + 4, System.IO.SeekOrigin.Begin);
            TtlType = ReadHelper.ReadStringFromOffset(Offset + Trb._f.ReadUInt32());
            //var TTLWindow = new TtlWindow(this);
            //TTLWindow.Show();
        }
    }
}
