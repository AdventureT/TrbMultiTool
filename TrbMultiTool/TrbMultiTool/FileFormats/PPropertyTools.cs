using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TrbMultiTool.PPropertyTools
{
    #region Optimization

    class StringInfo
    {
        public string Value;
        public uint Offset;

        public StringInfo(string value)
        {
            Value = value;
        }
    }

    class StringsLibrary
    {
        public List<StringInfo> strings = new();

        public StringInfo Add(string str)
        {
            StringInfo strInfo = default(StringInfo);

            if (!strings.Exists(s => s.Value == str))
            {
                strInfo = new StringInfo(str);
                strings.Add(strInfo);
            }
            else strInfo = strings.Find(s => s.Value == str);

            return strInfo;
        }

        public StringInfo Find(string str)
        {
            return strings.Find(s => s.Value == str);
        }

        public int Count
        {
            get => strings.Count;
        }
    }

    #endregion

    #region PProperty Types

    public enum PPropertyItemType
    {
        UInt,
        Unknown,
        Float,
        Bool,
        SubItem,
        Unknown2,
        Array,
        String,
        Int
    }

    class PPropertyItem
    {
        public int Value;
        public StringInfo Name;
        public PPropertyItemType Type;
        public uint NamePointer;
        public uint TypePointer;

        public PPropertyItem(StringInfo name, PPropertyItemType type)
        {
            Name = name;
            Type = type;
        }
    }

    class PPropertyString : PPropertyItem
    {
        public new StringInfo Value;

        public PPropertyString(StringInfo name, StringInfo value) : base(name, PPropertyItemType.String)
        {
            Value = value;
        }

        public PPropertyString(StringInfo value) : base(new StringInfo("Unnamed"), PPropertyItemType.String)
        {
            Value = value;
        }
    }

    class PPropertyInt : PPropertyItem
    {
        public new int Value;

        public PPropertyInt(StringInfo name, int value) : base(name, PPropertyItemType.Int)
        {
            Value = value;
        }

        public PPropertyInt(int value) : base(new StringInfo("Unnamed"), PPropertyItemType.Int)
        {
            Value = value;
        }
    }

    class PPropertyUInt : PPropertyItem
    {
        public new uint Value;

        public PPropertyUInt(StringInfo name, uint value) : base(name, PPropertyItemType.UInt)
        {
            Value = value;
        }

        public PPropertyUInt(uint value) : base(new StringInfo("Unnamed"), PPropertyItemType.UInt)
        {
            Value = value;
        }
    }

    class PPropertyFloat : PPropertyItem
    {
        public new float Value;

        public PPropertyFloat(StringInfo name, float value) : base(name, PPropertyItemType.Float)
        {
            Value = value;
        }

        public PPropertyFloat(float value) : base(new StringInfo("Unnamed"), PPropertyItemType.Float)
        {
            Value = value;
        }
    }

    class PPropertyBool : PPropertyItem
    {
        public new bool Value;

        public PPropertyBool(StringInfo name, bool value) : base(name, PPropertyItemType.Bool)
        {
            Value = value;
        }

        public PPropertyBool(bool value) : base(new StringInfo("Unnamed"), PPropertyItemType.Bool)
        {
            Value = value;
        }
    }

    class PPropertyArray : PPropertyItem
    {
        public new List<PPropertyItem> Value = new List<PPropertyItem>();

        public PPropertyArray(StringInfo name) : base(name, PPropertyItemType.Array) { }

        public void Add(PPropertyItem propertyItem)
        {
            if (propertyItem is not PPropertyArray && propertyItem is not PPropertySubItem)
                Value.Add(propertyItem);
        }

        public int GenerateData(BinaryWriter SECT, ref List<uint> ppOffsets)
        {
            var start = SECT.BaseStream.Position;

            foreach (var property in Value)
            {
                SECT.Write((int)property.Type);

                switch (property)
                {
                    case PPropertyString:
                        ppOffsets.Add((uint)SECT.BaseStream.Position); // For RELC (value pointer)
                        SECT.Write((property as PPropertyString).Value.Offset);
                        break;
                    case PPropertyInt:
                        SECT.Write((property as PPropertyInt).Value);
                        break;
                    case PPropertyUInt:
                        SECT.Write((property as PPropertyUInt).Value);
                        break;
                    case PPropertyFloat:
                        SECT.Write((property as PPropertyFloat).Value);
                        break;
                    case PPropertyBool:
                        SECT.Write((property as PPropertyBool).Value ? 1 : 0);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return (int)(SECT.BaseStream.Position - start);
        }
    }

    class PPropertySubItem : PPropertyItem
    {
        public new List<PPropertyItem> Value = new List<PPropertyItem>();

        public PPropertySubItem(StringInfo name) : base(name, PPropertyItemType.SubItem) { }

        public void Add(PPropertyItem propertyItem)
        {
            Value.Add(propertyItem);
        }
    }

    #endregion

    #region PProperty
    class Maker
    {
        static public int WriteProperties(List<PPropertyItem> properties, BinaryWriter SECT, int sizeOfMainProperties, ref int additionalDataSize, ref List<uint> ppOffsets, bool isMain = true, long lastParent = 0)
        {
            int size = 0;

            foreach (PPropertyItem baseProperty in properties)
            {
                ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC (name offset)
                baseProperty.NamePointer = (uint)SECT.BaseStream.Position;
                SECT.Write(baseProperty.Name.Offset);  // writing offset to the name
                baseProperty.TypePointer = (uint)SECT.BaseStream.Position;
                SECT.Write((int)baseProperty.Type);    // writing type of the property

                if (!isMain) additionalDataSize += 12;

                switch (baseProperty)
                {
                    case PPropertyString:
                        ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC (value pointer)
                        SECT.Write((baseProperty as PPropertyString).Value.Offset);
                        break;
                    case PPropertyInt:
                        SECT.Write((baseProperty as PPropertyInt).Value);
                        break;
                    case PPropertyUInt:
                        SECT.Write((baseProperty as PPropertyUInt).Value);
                        break;
                    case PPropertyFloat:
                        SECT.Write((baseProperty as PPropertyFloat).Value);
                        break;
                    case PPropertyBool:
                        SECT.Write((baseProperty as PPropertyBool).Value ? 1 : 0);
                        break;
                    case PPropertyArray:
                        var array = baseProperty as PPropertyArray;
                        var offsetToArrayInfo = (uint)(sizeOfMainProperties + additionalDataSize);
                        ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC (value pointer)
                        SECT.Write(offsetToArrayInfo); // offset to the info about array

                        var originalPos = SECT.BaseStream.Position;
                        SECT.BaseStream.Seek(offsetToArrayInfo, SeekOrigin.Begin);

                        additionalDataSize += 8; // offset of array, count of items
                        ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC
                        SECT.Write(sizeOfMainProperties + additionalDataSize); // offset of array
                        SECT.Write(array.Value.Count); // count of items

                        additionalDataSize += array.GenerateData(SECT, ref ppOffsets);

                        if (isMain)
                            SECT.BaseStream.Seek(originalPos, SeekOrigin.Begin);
                        break;
                    case PPropertySubItem:
                        var subItem = baseProperty as PPropertySubItem;
                        var offsetToSubItemInfo = (uint)(sizeOfMainProperties + additionalDataSize);
                        ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC (value pointer)
                        SECT.Write(offsetToSubItemInfo); // offset to the info about array
                        originalPos = SECT.BaseStream.Position;

                        SECT.BaseStream.Seek(offsetToSubItemInfo, SeekOrigin.Begin);

                        additionalDataSize += 12; // offset to parent SubItem, offset to array, count of items

                        var subItemInfoOffset = SECT.BaseStream.Position;
                        ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC
                        SECT.Write((uint)lastParent); // offset to parent SubItem
                        ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC
                        SECT.Write(sizeOfMainProperties + additionalDataSize); // offset to array
                        SECT.Write(subItem.Value.Count); // count of items

                        var arrayOffset = SECT.BaseStream.Position;
                        SECT.BaseStream.Seek(subItem.Value.Count * 8, SeekOrigin.Current);

                        additionalDataSize += subItem.Value.Count * 8;
                        WriteProperties(subItem.Value, SECT, sizeOfMainProperties, ref additionalDataSize, ref ppOffsets, false, subItemInfoOffset);
                        var lastPropertyPos = SECT.BaseStream.Position;

                        SECT.BaseStream.Seek(arrayOffset, SeekOrigin.Begin);
                        foreach (PPropertyItem property in subItem.Value)
                        {
                            ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC
                            SECT.Write(property.NamePointer);
                            ppOffsets.Add((uint)SECT.BaseStream.Position); // for RELC
                            SECT.Write(property.TypePointer);
                        }

                        if (isMain)
                            SECT.BaseStream.Seek(originalPos, SeekOrigin.Begin);
                        else
                            SECT.BaseStream.Seek(lastPropertyPos, SeekOrigin.Begin);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return size;
        }

        static byte[] GetStringBytes(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        static public void MakeFile(string path, StringsLibrary stringsLib, List<PPropertyItem> properties)
        {
            MemoryStream ms = new();

            List<uint> filesSizes = new();
            List<List<uint>> offsets = new();
            List<string> names = new();
            var SECT = new BinaryWriter(ms);

            List<uint> ppOffsets = new();
            names.Add("Main\0");

            // Building the file
            SECT.Write(0); // zero
            ppOffsets.Add((uint)SECT.BaseStream.Position);
            SECT.Write(12); // offset to the array
            SECT.Write(properties.Count); // count of properties

            // leaving some space for the info about properties
            var mainArrayOffset = SECT.BaseStream.Position;
            SECT.BaseStream.Seek(properties.Count * 8, SeekOrigin.Current);

            // writing our library of strings
            var stringsStart = SECT.BaseStream.Position;
            foreach (StringInfo strInfo in stringsLib.strings)
            {
                strInfo.Offset = (uint)SECT.BaseStream.Position;
                SECT.Write(GetStringBytes(strInfo.Value));
                SECT.BaseStream.Seek(1, SeekOrigin.Current);
            }

            // VERY IMPORTANT MARGIN, I HATE IT
            SECT.BaseStream.Seek(-1, SeekOrigin.Current);
            SECT.BaseStream.Seek(4 - (SECT.BaseStream.Position - stringsStart) % 4, SeekOrigin.Current); // margin from the strings

            // writing our properties
            int sizeOfMainProperties = properties.Count * 12;
            int additionalDataSize = (int)SECT.BaseStream.Position;

            WriteProperties(properties, SECT, sizeOfMainProperties, ref additionalDataSize, ref ppOffsets);

            // Writing offsets of the properties values
            SECT.BaseStream.Seek(mainArrayOffset, SeekOrigin.Begin);
            foreach (PPropertyItem baseProperty in properties)
            {
                ppOffsets.Add((uint)SECT.BaseStream.Position);
                SECT.Write(baseProperty.NamePointer);
                ppOffsets.Add((uint)SECT.BaseStream.Position);
                SECT.Write(baseProperty.TypePointer);
            }

            filesSizes.Add((uint)ms.ToArray().Length);
            offsets.Add(ppOffsets);

            Trb.GenerateFile(path, ms, filesSizes, offsets, names, new List<short>());
        }
    }

    #endregion
}
