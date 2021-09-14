using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
    class Btec
    {
        public string Magic { get; set; } = new(Trb._f.ReadChars(4));
        public uint Version { get; set; } = Trb._f.ReadUInt32();
        public uint UncompressedSize { get; set; } = Trb._f.ReadUInt32();
        public uint DecompressedSize { get; set; } = Trb._f.ReadUInt32();

        public List<byte> DecompressedData { get; set; } = new();

        public void Decompress()
        {
            long file_pos = Trb._f.BaseStream.Position;

            bool read_dst = false;
            int size = 0, offset = 0;
            int read_count;
            int remaining = (int)UncompressedSize;
            int dst_pos;
            while (remaining > 0)
            {
                read_count = GetReadCount(file_pos, ref read_dst, ref size, ref offset);
                remaining -= read_count;
                file_pos += read_count;

                if (read_dst)
                {
                    if (size > 0)
                    {
                        if (size < offset)
                        {
                            DecompressedData.AddRange(DecompressedData.GetRange(DecompressedData.Count - offset, size));
                        }
                        else
                        {
                            dst_pos = DecompressedData.Count - offset;
                            for (int i = 0; i < size; i++)
                            {
                                DecompressedData.Add(DecompressedData[dst_pos + i]);
                            }
                        }
                    }
                }
                else
                {
                    Trb._f.BaseStream.Seek(file_pos, SeekOrigin.Begin);
                    DecompressedData.AddRange(Trb._f.ReadBytes(size));

                    remaining -= size;
                    file_pos += size;
                }

            }
        }

        private static int GetReadCount(long file_pos, ref bool read_dst, ref int size, ref int offset)
        {
            int read_count = 0;
            Trb._f.BaseStream.Seek(file_pos + read_count++, SeekOrigin.Begin);
            size = Trb._f.ReadByte();
            read_dst = (size & 0x80) == 0;
            if ((size & 0x40) == 0)
            {
                size &= 0x3F;
            }
            else
            {
                Trb._f.BaseStream.Seek(file_pos + read_count++, SeekOrigin.Begin);
                size = ((size & 0x3F) << 0x08) + Trb._f.ReadByte();
            }
            size += 1;
            offset = 0;
            if (read_dst)
            {
                Trb._f.BaseStream.Seek(file_pos + read_count++, SeekOrigin.Begin);
                offset = Trb._f.ReadByte();

                if ((offset & 0x80) == 0)
                {
                    offset &= 0x7F;
                }
                else
                {
                    Trb._f.BaseStream.Seek(file_pos + read_count++, SeekOrigin.Begin);
                    offset = ((offset & 0x7F) << 0x08) + Trb._f.ReadByte();
                }
                offset += 1;
            }
            return read_count;
        }

    }
}
