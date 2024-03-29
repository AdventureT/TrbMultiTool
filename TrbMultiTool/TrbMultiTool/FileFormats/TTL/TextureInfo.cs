﻿using PrimeWPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TrbMultiTool.FileFormats.TTL
{
    public class TextureInfo
    {
        public uint Flag { get; set; }

        public string FileName { get; set; }

        public uint DdsSize { get; set; }

        public uint DdsOffset { get; set; }

        public byte[] RawImage { get; set; }

        public DDSImage Dds { get; set; }
        
        public uint FileNameOffset { get; set; }

        public int BytesToSkip { get; set; }

        public byte[] Pallete { get; set; }

        public Bitmap Bitmap { get; set; }

        public enum TextureFormat
        {
            FORMAT_RGB565,
            FORMAT_RGB5A3,
            FORMAT_Unknown,
            FORMAT_Z8,
            FORMAT_CMPR,
            FORMAT_Z16,
            FORMAT_Z24X8,
            FORMAT_INDEX4,
            FORMAT_INDEX14X2,
            FORMAT_INTENSITY4,
            FORMAT_INTENSITY4A,
            FORMAT_INTENSITY8,
            FORMAT_INTENSITY8A,
            FORMAT_UNKNOWN,
            FORMAT_RGBA8,
            FORMAT_UNKNOWN3,
            FORMAT_INDEX8,
        }

        public TextureFormat textureFormat;

        public uint Width;

        public uint Height;

        public uint BPP;

        public uint PBPP;

        public uint PSIZE;

        private byte[] ReadBytes(MemoryStream ms, int count)
        {
            var width = new byte[count+3];
            ms.Read(width, 0, count);
            return width;
        }

        public TextureInfo(uint offset)
        {
            if (Trb._game == Game.NicktoonsUnite || Trb._game == Game.NicktoonsBattleForVolcanoIsland)
            {
                var flags = Trb.SectFile.ReadBytes(4);
                textureFormat = (TextureFormat)flags[0];
                FileNameOffset = offset + Trb.SectFile.ReadUInt32();
                FileName = Trb.SectFile.ReadStringFromOffset(FileNameOffset);
                Width = Trb.SectFile.ReadUInt32();
                Height = Trb.SectFile.ReadUInt32();
                BPP = offset + Trb.SectFile.ReadUInt32();
                var imageOffset = offset + Trb.SectFile.ReadUInt32();
                var imageSize = offset + Trb.SectFile.ReadUInt32();
                var paletteOffset = offset + Trb.SectFile.ReadUInt32();
                PBPP = offset + Trb.SectFile.ReadUInt32();
                PSIZE = offset + Trb.SectFile.ReadUInt32();
                RawImage = Trb.SectFile.ReadFromOffset(imageSize, imageOffset);
                Pallete = Trb.SectFile.ReadFromOffset(PBPP * PSIZE, paletteOffset);
                //if (flags[0] == 4)
                //{
                //    FileNameOffset = offset + Trb.SectFile.ReadUInt32();
                //    FileName = ReadHelper.ReadStringFromOffset(Trb.SectFile, FileNameOffset);
                //    Width = Trb.SectFile.ReadUInt32();
                //    Height = Trb.SectFile.ReadUInt32();
                //    BPP = offset + Trb.SectFile.ReadUInt32();
                //    var imageOffset = offset + Trb.SectFile.ReadUInt32();
                //    var imageSize = offset + Trb.SectFile.ReadUInt32();
                //    var paletteOffset = offset + Trb.SectFile.ReadUInt32();
                //    PBPP = offset + Trb.SectFile.ReadUInt32();
                //    PSIZE = offset + Trb.SectFile.ReadUInt32();
                //    RawImage = ReadHelper.ReadFromOffset(Trb.SectFile, imageSize, imageOffset);
                //    Pallete = ReadHelper.ReadFromOffset(Trb.SectFile, PBPP * PSIZE, paletteOffset);
                //    //Bitmap = new Bitmap(width, height, pixelformat);
                //    //var palleteStream = new MemoryStream(Pallete);
                //    ////Create ColorPalette
                //    //ColorPalette pal = Bitmap.Palette;
                //    //var stream = new MemoryStream();
                //    //Bitmap = new(width, height, PixelFormat.Format8bppIndexed);

                //    //var tga = new TgaSharp.TGA((ushort)((ushort)width), (ushort)((ushort)height), TgaSharp.TgaPixelDepth.Bpp16, TgaSharp.TgaImageType.Uncompressed_ColorMapped);

                //    //tga.Header.ColorMapSpec.ColorMapEntrySize = TgaSharp.TgaColorMapEntrySize.A1R5G5B5;
                //    //tga.Header.ColorMapSpec.ColorMapLength = (ushort)((ushort)Pallete.Length / 2);
                //    //tga.Header.ColorMapType = TgaSharp.TgaColorMapType.ColorMap;



                //    //tga.ImageOrColorMapArea.ColorMapData = Pallete;
                //    //tga.ImageOrColorMapArea.ImageData = RawImage;
                //    //string error;
                //    //tga.CheckAndUpdateOffsets(out error);
                //    //Bitmap = tga.ToBitmap();


                //    ////SixLabors.ImageSharp.Image.Load()
                //    //int x = 0;
                //    //for (int i = 0; i < PSIZE * PBPP; i+=4)
                //    //{
                //    //    //var test = BitConverter.ToInt32(ReadBytes(palleteStream, (int)bitsPerColor / 8));
                //    //    //var test2 = BitConverter.ToInt32(ReadBytes(palleteStream, (int)bitsPerColor / 8));
                //    //    //var test3 = BitConverter.ToInt32(ReadBytes(palleteStream, (int)bitsPerColor / 8));
                //    //    //var test4 = BitConverter.ToInt32(ReadBytes(palleteStream, (int)bitsPerColor / 8));
                //    //    pal.Entries[x] = Color.FromArgb(Pallete[i], Pallete[i+1], Pallete[i + 2]);
                //    //    x++;
                //    //}
                //    //Bitmap.Palette = pal;

                //    ////Lock Bits with white image
                //    //var BoundsRect = new Rectangle(0, 0, width, height);
                //    //BitmapData bitmapData = Bitmap.LockBits(BoundsRect,
                //    //                                ImageLockMode.WriteOnly,
                //    //                                Bitmap.PixelFormat);
                //    ////Copy PixelData into bitmapData
                //    //IntPtr pointer = bitmapData.Scan0;
                //    //Marshal.Copy(RawImage, 0, pointer, RawImage.Length);

                //    ////Unlock Bits
                //    //Bitmap.UnlockBits(bitmapData);
                //    Trb.SectFile.BaseStream.Seek(4, SeekOrigin.Current);
                //}
                //else
                //{
                //    Trb.SectFile.BaseStream.Seek(40, SeekOrigin.Current);
                //    FileName = "Pixelformat not implemented";
                //}
                Trb.SectFile.BaseStream.Seek(4, SeekOrigin.Current);
            }
            else
            {
                Flag = Trb.SectFile.ReadUInt32();
                FileNameOffset = offset + Trb.SectFile.ReadUInt32();
                FileName = Trb.SectFile.ReadStringFromOffset(FileNameOffset);
                DdsSize = Trb.SectFile.ReadUInt32();
                DdsOffset = offset + Trb.SectFile.ReadUInt32();
                RawImage = Trb.SectFile.ReadFromOffset(DdsSize, DdsOffset);
                Dds = new DDSImage(RawImage);
            }

        }
    }
}
