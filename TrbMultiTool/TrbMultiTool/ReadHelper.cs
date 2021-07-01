using Aspose.ThreeD.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool
{
    class ReadHelper
    {
        public static long _lastPos = -1;
        public static string ReadString(BinaryReader f)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var newByte = f.ReadByte();
                if (newByte == 0) break;
                sb.Append((char)newByte);
            }
            return sb.ToString();
        }

        public static string ReadUnicodeString(BinaryReader f)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var newByte = f.ReadByte();
                var newByte2 = f.ReadByte();
                if (newByte == 0 && newByte2 == 0) break;
                var convertedChar = Encoding.Unicode.GetString(new byte[] { newByte, newByte2 });
                sb.Append(convertedChar);
            }
            return sb.ToString();
        }

        public static string ReadUnicodeString(BinaryReader f, uint size)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                var newByte = f.ReadByte();
                var newByte2 = f.ReadByte();
                if (newByte == 0 && newByte2 == 0) break;
                var convertedChar = Encoding.Unicode.GetString(new byte[] { newByte, newByte2 });
                sb.Append(convertedChar);
            }
            return sb.ToString();
        }

        public static string ReadUnicodeStringB(BinaryReader f)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var newByte = f.ReadByte();
                var newByte2 = f.ReadByte();
                if (newByte == 0 && newByte2 == 0) break;
                var convertedChar = Encoding.Unicode.GetString(new byte[] { newByte, newByte2 }.Reverse().ToArray());
                sb.Append(convertedChar);
            }
            return sb.ToString();
        }

        public static string ReadUnicodeStringB(BinaryReader f, uint size)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                var newByte = f.ReadByte();
                var newByte2 = f.ReadByte();
                if (newByte == 0 && newByte2 == 0) break;
                var convertedChar = Encoding.Unicode.GetString(new byte[] { newByte, newByte2 }.Reverse().ToArray());
                sb.Append(convertedChar);
            }
            return sb.ToString();
        }

        public static string ReadStringFromOffset(BinaryReader f, uint offset)
        {
            var pos = f.BaseStream.Position;
            f.BaseStream.Seek(offset, SeekOrigin.Begin);
            string str = ReadString(f);
            //var intCheck = Trb._f.ReadInt16();
            //if (intCheck <= 0xFF)
            //{
            //    Trb._f.BaseStream.Seek(-2, SeekOrigin.Current);
            //    str = ReadUnicodeString();
            //}
            //else
            //{
            //    Trb._f.BaseStream.Seek(-2, SeekOrigin.Current);
            //    str = ReadString();
            //}

            f.BaseStream.Seek(pos, SeekOrigin.Begin);
            return str;
        }

        public static byte[] ReadFromOffset(BinaryReader f, uint bytesToRead, uint offset)
        {
            var pos = f.BaseStream.Position;
            f.BaseStream.Seek(offset, SeekOrigin.Begin);
            var buffer = new byte[bytesToRead];
            var bytesRead = f.Read(buffer);
            f.BaseStream.Seek(pos, SeekOrigin.Begin);
            return buffer;
        }

        public static long SeekToOffset(BinaryReader f, uint offset)
        {
            _lastPos = f.BaseStream.Position;
            f.BaseStream.Seek(offset, SeekOrigin.Begin);
            return _lastPos;
        }

        public static void ReturnToOrginalPosition(BinaryReader f)
        {
            if (_lastPos != -1) f.BaseStream.Seek(_lastPos, SeekOrigin.Begin);
        }

        public static Vector4 ReadVector4(BinaryReader f) => new Vector4(f.ReadSingle(), f.ReadSingle(), f.ReadSingle(), f.ReadSingle());
        public static Vector3 ReadVector3(BinaryReader f) => new Vector3(f.ReadSingle(), f.ReadSingle(), f.ReadSingle());

        public static Vector4 ReadVector4FromOffset(BinaryReader f, uint offset)
        {
            var pos = f.BaseStream.Position;
            f.BaseStream.Seek(offset, SeekOrigin.Begin);
            var result = ReadVector4(f);
            f.BaseStream.Seek(pos, SeekOrigin.Begin);
            return result;
        }

        public static Vector3 ReadVector3FromOffset(BinaryReader f, uint offset)
        {
            var pos = f.BaseStream.Position;
            f.BaseStream.Seek(offset, SeekOrigin.Begin);
            var result = ReadVector3(f);
            f.BaseStream.Seek(pos, SeekOrigin.Begin);
            return result;
        }

        public static uint ReadUInt32B(BinaryReader f)
        {
            var data = f.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public static ushort ReadUInt16B(BinaryReader f)
        {
            var data = f.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public static float ReadFloatB(BinaryReader f)
        {
            var data = f.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }
    }
}
