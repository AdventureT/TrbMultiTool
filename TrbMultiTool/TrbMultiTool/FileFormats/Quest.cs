using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TrbMultiTool.FileFormats
{
    class Quest
    {
        public int Zero { get; set; }
        public uint Offset { get; set; }
        public uint Count { get; set; }
        public record Info(uint Offset1, uint Offset2);
        record SubInfo(Type Type, uint Value);
        record SubSubInfo(uint Offset1, uint Offset2, uint Offset2Count);

        record PlayerInfo(uint SubInfoOffset, uint SubInfoOffsetCount);

        public enum Type
        {
            Int,
            Unknown,
            Float,
            Bool,
            SubItem,
            Unknown2,
            Player,
            String,
            UInt
        };

        public record TypeContent(Type Type, string Value, long Offset, long PointerPos = 0);

        public QuestWindow QuestWindow { get; set; } = new();

        public Quest()
        {
            Zero = Trb._f.ReadInt32();
            Offset = Trb._f.ReadUInt32();
            Count = Trb._f.ReadUInt32();
            Trb._f.BaseStream.Seek(Offset + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
            var tvi = new TreeViewItem
            {
                Header = "Quests"
            };
            Item(Count, tvi);
            QuestWindow.treeView.Items.Add(tvi);
            QuestWindow.Show();
        }

        private void Item(uint count, TreeViewItem prev)
        {
            var infos = new List<Info>();
            for (int i = 0; i < count; i++)
            {
                infos.Add(new Info(Trb._f.ReadUInt32(), Trb._f.ReadUInt32()));
            }
            foreach (var info in infos)
            {
                Trb._f.BaseStream.Seek(info.Offset1 + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
                string name = ReadHelper.ReadStringFromOffset(Trb._f.ReadUInt32() + (uint)Trb.Tsfl.Sect.Offset); // Usually "quest"

                var tvi = new TreeViewItem
                {
                    Header = name
                };

                Trb._f.BaseStream.Seek(info.Offset2 + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
                var subInfo = new SubInfo((Type)Trb._f.ReadUInt32(), 0);

                uint textOffset = 0;
                if (subInfo.Type == Type.String) textOffset = Trb._f.ReadUInt32() + (uint)Trb.Tsfl.Sect.Offset;
                tvi.Tag = subInfo.Type switch
                {
                    //Number?
                    Type.Int => new TypeContent(Type.Int, Trb._f.ReadInt32().ToString(), Trb._f.BaseStream.Position - 4),
                    //float
                    Type.Float => new TypeContent(Type.Float, $"{Trb._f.ReadSingle().ToString():N2}", Trb._f.BaseStream.Position - 4),
                    //bool
                    Type.Bool => new TypeContent(Type.Bool, (Trb._f.ReadUInt32() == 1).ToString(), Trb._f.BaseStream.Position - 4),
                    //Another array? Pointing to the same info?
                    Type.SubItem => SubItem(tvi),
                    //Player
                    Type.Player => Player(ref tvi),
                    //string
                    Type.String => new TypeContent(Type.String, ReadHelper.ReadStringFromOffset(textOffset), textOffset, Trb._f.BaseStream.Position - 4),
                    //Uint
                    Type.UInt => new TypeContent(Type.UInt, Trb._f.ReadUInt32().ToString(), Trb._f.BaseStream.Position - 4),
                    _ => throw new NotImplementedException($"Type {subInfo.Type} hasn't been implemented yet"),
                };
                prev.Items.Add(tvi);
            }
        }

        private TypeContent SubItem(TreeViewItem prev)
        {
            Trb._f.BaseStream.Seek(Trb._f.ReadUInt32() + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
            var subsubInfo2 = new SubSubInfo(Trb._f.ReadUInt32(), Trb._f.ReadUInt32(), Trb._f.ReadUInt32());
            //Parent?? It's always 0
            //Trb._f.BaseStream.Seek(subsubInfo2.Offset1 + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
            //var subsubInfo3 = new SubSubInfo(Trb._f.ReadUInt32(), Trb._f.ReadUInt32(), Trb._f.ReadUInt32());
            Trb._f.BaseStream.Seek(subsubInfo2.Offset2 + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
            Item(subsubInfo2.Offset2Count, prev);
            return new TypeContent(Type.String, "SubInfo", 0);
        }

        private static TypeContent Player(ref TreeViewItem prev)
        {
            Trb._f.BaseStream.Seek(Trb._f.ReadUInt32() + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
            var playerInfo = new PlayerInfo(Trb._f.ReadUInt32(), Trb._f.ReadUInt32());
            Trb._f.BaseStream.Seek(playerInfo.SubInfoOffset + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
            var playerSubInfos = new List<SubInfo>();
            for (int i = 0; i < playerInfo.SubInfoOffsetCount; i++)
            {
                playerSubInfos.Add(new SubInfo((Type)Trb._f.ReadUInt32(), Trb._f.ReadUInt32()));
            }
            foreach (var playerSubInfo in playerSubInfos)
            {
                var tvi = new TreeViewItem
                {
                    Header = playerSubInfo.Type switch
                    {
                        //string
                        Type.String => ReadHelper.ReadStringFromOffset(playerSubInfo.Value + (uint)Trb.Tsfl.Sect.Offset),
                        _ => throw new NotImplementedException($"Type {playerSubInfo.Type} hasn't been implemented yet"),
                    },
                    Tag = playerSubInfo.Type switch
                    {
                        //string
                        Type.String => new TypeContent(Type.String, ReadHelper.ReadStringFromOffset(playerSubInfo.Value + (uint)Trb.Tsfl.Sect.Offset), playerSubInfo.Value + (uint)Trb.Tsfl.Sect.Offset, Trb._f.BaseStream.Position - 4),
                        _ => throw new NotImplementedException($"Type {playerSubInfo.Type} hasn't been implemented yet"),
                    }
                };
                prev.Items.Add(tvi);
            }
            return new TypeContent(Type.String, "Players", 0);
        }
    }
}
