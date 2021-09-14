using PrimeWPF;
using System;
using System.Collections.Generic;
using System.IO;
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

        public List<uint> Offsets { get; set; } = new();

        public short Idx { get; set; }


        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource LoadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        private static byte[] GetStringBytes(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        public MemoryStream MakeSectStart()
        {
            MemoryStream sect = new();
            
            sect.Write(BitConverter.GetBytes(TextureInfoCount));
            Offsets.Add((uint)sect.Position);
            sect.Write(BitConverter.GetBytes(28)); // offset to first TTL info (in Barnyard it's same everytime)
            Offsets.Add((uint)sect.Position);
            sect.Write(BitConverter.GetBytes(12)); // offset to TTPackTexLib (it's same too)
            sect.Write(GetStringBytes("TTPackTexLib"));
            sect.Write(BitConverter.GetBytes(0));

            return sect;
        }

        public MemoryStream RepackSECT(string texName, MemoryStream data)
        {
            Offsets = new(); // Reset all previous offsets
            MemoryStream sect = MakeSectStart();

            uint dataSize = (uint)(sect.Position + 16 * TextureInfoCount);

            // Writing all info about images
            foreach (var tInfo in TextureInfos)
            {
                if (tInfo.FileName == texName)
                {
                    byte[] bytesData = data.ToArray();

                    tInfo.RawImage = bytesData;
                    tInfo.Dds = new DDSImage(bytesData);
                    tInfo.DdsSize = (uint)bytesData.Length;
                }

                tInfo.FileNameOffset = dataSize;
                tInfo.BytesToSkip = tInfo.FileName.Length + (4 - tInfo.FileName.Length % 4);
                tInfo.DdsOffset = (uint)(tInfo.FileNameOffset + tInfo.BytesToSkip);

                sect.Write(BitConverter.GetBytes(tInfo.Flag));
                Offsets.Add((uint)sect.Position);
                sect.Write(BitConverter.GetBytes(tInfo.FileNameOffset));
                sect.Write(BitConverter.GetBytes(tInfo.DdsSize));
                Offsets.Add((uint)sect.Position);
                sect.Write(BitConverter.GetBytes(tInfo.DdsOffset));

                dataSize += (uint)(tInfo.BytesToSkip + tInfo.RawImage.Length);
            }

            // Writing images data
            foreach (var tInfo in TextureInfos)
            {
                // File Name
                sect.Seek(tInfo.FileNameOffset, SeekOrigin.Begin);
                sect.Write(GetStringBytes(tInfo.FileName));

                // Data
                sect.Seek(tInfo.DdsOffset, SeekOrigin.Begin);
                sect.Write(tInfo.RawImage);
            }

            return sect;
        }

        public Ttl(uint offset, string ttlName, short idx)
        {
            Idx = idx;
            TtlName = ttlName;
            Offset = offset;
            Trb.SectFile.BaseStream.Seek(Offset, System.IO.SeekOrigin.Begin);
            TextureInfoCount = Trb.SectFile.ReadUInt32();
            var pos = Trb.SectFile.BaseStream.Position;
            Trb.SectFile.BaseStream.Seek(Offset + Trb.SectFile.ReadUInt32(), System.IO.SeekOrigin.Begin);
            for (int i = 0; i < TextureInfoCount; i++)
            {
                TextureInfos.Add(new TextureInfo(Offset));
            }
            Trb.SectFile.BaseStream.Seek(pos + 4, System.IO.SeekOrigin.Begin);
            TtlType = Trb.SectFile.ReadStringFromOffset(Offset + Trb.SectFile.ReadUInt32());
            //var TTLWindow = new TtlWindow(this);
            //TTLWindow.Show();
        }
    }
}
