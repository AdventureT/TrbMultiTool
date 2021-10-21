using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrbMultiTool.FileFormats
{
    public class Entities
    {
        public enum PropertyType
        {
            INT, // Correct
            FLOAT, // Correct
            BOOL, // Correct
            String, // Correct
            VECTOR4, // Correct
            INSTANCELIST, // Correct
            NAVIGATIONSETLIST, // Correct
            COLLISTIONSHAPE, // Guess
            Unknown4,
            OFFSET
        }

        record EntityInstanceInfo(uint Offset, uint Count);

        record EntityInstance(uint InstanceId);

        public List<EntityInfo> EntityInfos { get; set; } = new();

        public class EntityInfo
        {
            public string EntityName { get; set; }
            public ushort PropertiesCount { get; set; }
            public ushort Flag { get; set; }
            public uint PropertiesOffset { get; set; }
            public uint MatrixOffset { get; set; }
            public uint PositionOffset { get; set; }
            public uint Unk2 { get; set; }
            public uint Unk3 { get; set; }
            public uint Unk4 { get; set; }
            public uint Unk5 { get; set; }
            public uint ValuesOffset { get; set; }
            public List<EntityProperty> Properties { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Scale { get; set; }
            public Vector4 Rotation { get; set; }
            public uint PositionPos { get; set; }
            public EntityInfo(string entityName, ushort propertiesCount, ushort flag, uint propertiesOffset, uint matrixOffset, uint positionOffset, uint unk2, uint unk3,uint unk4, uint unk5, uint valuesOffset)
            {
                EntityName = entityName;
                PropertiesCount = propertiesCount;
                Flag = flag;
                PropertiesOffset = propertiesOffset;
                MatrixOffset = matrixOffset;
                PositionOffset = positionOffset;
                Unk2 = unk2;
                Unk3 = unk3;
                Unk4 = unk4;
                Unk5 = unk5;
                ValuesOffset = valuesOffset;
                Properties = new();
            }
        };

        public class EntityProperty
        {
            public string Name { get; set; }
            public PropertyType Type { get; set; }
            public object Value { get; set; }
            public uint Position { get; set; }
            public EntityProperty(string name, PropertyType type, object value)
            {
                Name = name;
                Type = type;
                Value = value;
                Position = value is bool ? (uint)Trb.SectFile.BaseStream.Position - 9 : (uint)Trb.SectFile.BaseStream.Position - 12;
            }
            public override string ToString()
            {
                return $"Name: {Name}, Type: {Type}, Value: {Value}";
            }
        }
        public Entities()
        {
            var entityInfoOffset = Trb.SectFile.ReadUInt32();
            var entityInfoCount = Trb.SectFile.ReadUInt32();
            Trb._f.BaseStream.Seek(entityInfoOffset, SeekOrigin.Begin);
            for (int i = 0; i < entityInfoCount; i++)
            {
                EntityInfos.Add(new EntityInfo(Trb.SectFile.ReadStringFromOffset(Trb.SectFile.ReadUInt32()), Trb.SectFile.ReadUInt16(),
                    Trb.SectFile.ReadUInt16(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(),
                    Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32()));
            }

            for (int i = 0; i < EntityInfos.Count; i++)
            {
                Trb.SectFile.BaseStream.Seek(EntityInfos[i].PropertiesOffset, SeekOrigin.Begin);
                for (int j = 0; j < EntityInfos[i].PropertiesCount; j++)
                {
                    var entName = Trb.SectFile.ReadStringFromOffset(Trb.SectFile.ReadUInt32());
                    var propType = (PropertyType)Trb.SectFile.ReadUInt32();
                    switch (propType)
                    {
                        case PropertyType.INT:
                            EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, Trb.SectFile.ReadInt32()));
                            break;
                        case PropertyType.String:
                            var offset = Trb.SectFile.ReadUInt32();
                            if (offset == 0) EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, "N/A"));
                            else EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, Trb.SectFile.ReadStringFromOffset(offset)));
                            break;
                        case PropertyType.FLOAT:
                            EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, Trb.SectFile.ReadSingle()));
                            break;
                        case PropertyType.BOOL:
                            EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, Trb.SectFile.ReadBoolean()));
                            Trb.SectFile.BaseStream.Seek(3, SeekOrigin.Current);
                            break;
                        case PropertyType.INSTANCELIST:
                            offset = Trb.SectFile.ReadUInt32();
                            Trb.SectFile.BaseStream.Seek(offset, SeekOrigin.Begin);
                            var entityInstanceSet = new EntityInstanceInfo(Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32());
                            Trb.SectFile.BaseStream.Seek(entityInstanceSet.Offset, SeekOrigin.Begin);
                            var entityInstances = new List<EntityInstance>();
                            for (int x = 0; x < entityInstanceSet.Count; x++)
                            {
                                entityInstances.Add(new EntityInstance(Trb.SectFile.ReadUInt32()));
                            }
                            //if (offset == 0) EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, "N/A"));
                            //else EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, Trb.SectFile.ReadStringFromOffset(offset)));
                            break;
                        case PropertyType.VECTOR4:
                            EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, Trb.SectFile.ReadVector4FromOffset(Trb.SectFile.ReadUInt32())));
                            break;
                        case PropertyType.NAVIGATIONSETLIST:
                            break;
                        case PropertyType.COLLISTIONSHAPE:
                            break;
                        case PropertyType.Unknown4:
                            break;
                        case PropertyType.OFFSET:
                            EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, Trb.SectFile.ReadInt32()));
                            break;
                        default:
                            EntityInfos[i].Properties.Add(new EntityProperty(entName, propType, Trb.SectFile.ReadInt32()));
                            break;
                    }
                }
                Trb.SectFile.BaseStream.Seek(EntityInfos[i].MatrixOffset, SeekOrigin.Begin);
                EntityInfos[i].PositionPos = (uint)Trb.SectFile.BaseStream.Position;
                Matrix4 mat = new(Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(),
                    Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(),
                    Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(),
                    Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle());

                //mat.Decompose(out Vector3 pos, out Vector3 scale, out Quaternion rot);
                EntityInfos[i].Position = mat.ExtractTranslation();
                EntityInfos[i].Scale = mat.ExtractScale();
                EntityInfos[i].Rotation = mat.ExtractRotation().ToAxisAngle();
            }
        }
    }

}
