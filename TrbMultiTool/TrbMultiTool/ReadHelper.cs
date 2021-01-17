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
        public static string ReadString()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var newByte = Trb._f.ReadByte();
                if (newByte == 0) break;
                sb.Append((char)newByte);
            }
            return sb.ToString();
        }

        public static string ReadUnicodeString()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var newByte = Trb._f.ReadByte();
                var newByte2 = Trb._f.ReadByte();
                if (newByte == 0 && newByte2 == 0) break;
                var convertedChar = Encoding.Unicode.GetString(new byte[] { newByte, newByte2 });
                sb.Append(convertedChar);
            }
            return sb.ToString();
        }

        public static string ReadStringFromOffset(uint offset)
        {
            var pos = Trb._f.BaseStream.Position;
            Trb._f.BaseStream.Seek(offset, SeekOrigin.Begin);
            string str = ReadString();
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

            Trb._f.BaseStream.Seek(pos, SeekOrigin.Begin);
            return str;
        }

        public static byte[] ReadFromOffset(uint bytesToRead, uint offset)
        {
            var pos = Trb._f.BaseStream.Position;
            Trb._f.BaseStream.Seek(offset, SeekOrigin.Begin);
            var buffer = new byte[bytesToRead];
            var bytesRead = Trb._f.Read(buffer);
            Trb._f.BaseStream.Seek(pos, SeekOrigin.Begin);
            return buffer;
        }

        public static long SeekToOffset(uint offset)
        {
            _lastPos = Trb._f.BaseStream.Position;
            Trb._f.BaseStream.Seek(offset, SeekOrigin.Begin);
            return _lastPos;
        }

        public static void ReturnToOrginalPosition()
        {
            if (_lastPos != -1) Trb._f.BaseStream.Seek(_lastPos, SeekOrigin.Begin);
        }

        public static Vector4 ReadVector4() => new Vector4(Trb._f.ReadSingle(), Trb._f.ReadSingle(), Trb._f.ReadSingle(), Trb._f.ReadSingle());
        public static Vector3 ReadVector3() => new Vector3(Trb._f.ReadSingle(), Trb._f.ReadSingle(), Trb._f.ReadSingle());

        public static Vector4 ReadVector4FromOffset(uint offset)
        {
            var pos = Trb._f.BaseStream.Position;
            Trb._f.BaseStream.Seek(offset, SeekOrigin.Begin);
            var result = ReadVector4();
            Trb._f.BaseStream.Seek(pos, SeekOrigin.Begin);
            return result;
        }

        public static Vector3 ReadVector3FromOffset(uint offset)
        {
            var pos = Trb._f.BaseStream.Position;
            Trb._f.BaseStream.Seek(offset, SeekOrigin.Begin);
            var result = ReadVector3();
            Trb._f.BaseStream.Seek(pos, SeekOrigin.Begin);
            return result;
        }
    }
}
