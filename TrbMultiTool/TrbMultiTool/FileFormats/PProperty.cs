using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TrbMultiTool.PPropertyTools;

namespace TrbMultiTool.FileFormats
{
    public class PProperty
    {
        public int Zero { get; set; }
        public uint Offset { get; set; }
        public uint Count { get; set; }
        public record Info(uint Offset1, uint Offset2);
        record SubInfo(PPropertyItemType Type, uint Value, uint ValuePointer);
        record SubSubInfo(uint Offset1, uint Offset2, uint Offset2Count);
        record PlayerInfo(uint SubInfoOffset, uint SubInfoOffsetCount);

        public class TypeContent
        {
            public PPropertyItemType Type;
            public string Value;
            public long Offset;
            public long PointerPos;
            public bool InArray;
            public int index;

            public TypeContent(PPropertyItemType type, string value, long offset, long pointerPos = 0, bool inArray = false)
            {
                Type = type;
                Value = value;
                Offset = offset;
                PointerPos = pointerPos;
                InArray = inArray;
            }
        }

        public List<TypeContent> TypeContents { get; set; } = new();

        public PPropertyWindow QuestWindow { get; set; }

        public PProperty()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                QuestWindow = new();
                Zero = Trb.SectFile.ReadInt32();
                Offset = Trb.SectFile.ReadUInt32();
                Count = Trb.SectFile.ReadUInt32();
                Trb.SectFile.BaseStream.Seek(Offset, System.IO.SeekOrigin.Begin);
                var tvi = new TreeViewItem
                {
                    Header = "Properties"
                };
                Item(Count, tvi);
                QuestWindow.treeView.Items.Add(tvi);
                QuestWindow.TypeContentss = TypeContents;
                QuestWindow.Show();
            });
        }

        private void Item(uint count, TreeViewItem prev)
        {
            var infos = new List<Info>();
            for (int i = 0; i < count; i++)
            {
                infos.Add(new Info(Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32()));
            }

            int index = 0;
            foreach (var info in infos)
            {
                Trb.SectFile.BaseStream.Seek(info.Offset1, System.IO.SeekOrigin.Begin);
                string name = ReadHelper.ReadStringFromOffset(Trb.SectFile, Trb.SectFile.ReadUInt32());

                var tvi = new TreeViewItem
                {
                    Header = name
                };

                Trb.SectFile.BaseStream.Seek(info.Offset2, System.IO.SeekOrigin.Begin);
                var subInfo = new SubInfo((PPropertyItemType)Trb.SectFile.ReadUInt32(), 0, 0);

                uint textOffset = 0;
                if (subInfo.Type == PPropertyItemType.String) textOffset = Trb.SectFile.ReadUInt32();
                tvi.Tag = subInfo.Type switch
                {
                    //Number?
                    PPropertyItemType.Int => new TypeContent(PPropertyItemType.Int, Trb.SectFile.ReadInt32().ToString(), Trb.SectFile.BaseStream.Position - 4),
                    //float
                    PPropertyItemType.Float => new TypeContent(PPropertyItemType.Float, $"{Trb.SectFile.ReadSingle().ToString():N2}", Trb.SectFile.BaseStream.Position - 4),
                    //bool
                    PPropertyItemType.Bool => new TypeContent(PPropertyItemType.Bool, (Trb.SectFile.ReadUInt32() == 1).ToString(), Trb.SectFile.BaseStream.Position - 4),
                    //Another array? Pointing to the same info?
                    PPropertyItemType.SubItem => SubItem(tvi),
                    //Player
                    PPropertyItemType.Array => Array(ref tvi),
                    //string
                    PPropertyItemType.String => new TypeContent(PPropertyItemType.String, ReadHelper.ReadStringFromOffset(Trb.SectFile, textOffset), textOffset, Trb.SectFile.BaseStream.Position - 4),
                    //Uint
                    PPropertyItemType.UInt => new TypeContent(PPropertyItemType.UInt, Trb.SectFile.ReadUInt32().ToString(), Trb.SectFile.BaseStream.Position - 4),
                    _ => $"Type {subInfo.Type} hasn't been implemented yet",
                };

                (tvi.Tag as TypeContent).index = index++;

                TypeContents.Add((TypeContent)tvi.Tag);
                prev.Items.Add(tvi);
            }
        }

        private TypeContent SubItem(TreeViewItem prev)
        {
            Trb.SectFile.BaseStream.Seek(Trb.SectFile.ReadUInt32(), System.IO.SeekOrigin.Begin);
            var subsubInfo2 = new SubSubInfo(Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32());
            //Parent?? It's always 0
            //Trb._f.BaseStream.Seek(subsubInfo2.Offset1 + Trb.Tsfl.Sect.Offset, System.IO.SeekOrigin.Begin);
            //var subsubInfo3 = new SubSubInfo(Trb._f.ReadUInt32(), Trb._f.ReadUInt32(), Trb._f.ReadUInt32());
            Trb.SectFile.BaseStream.Seek(subsubInfo2.Offset2, System.IO.SeekOrigin.Begin);
            Item(subsubInfo2.Offset2Count, prev);
            return new TypeContent(PPropertyItemType.SubItem, "SubProperty", 0);
        }

        private TypeContent Array(ref TreeViewItem prev)
        {
            Trb.SectFile.BaseStream.Seek(Trb.SectFile.ReadUInt32(), System.IO.SeekOrigin.Begin);
            var playerInfo = new PlayerInfo(Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32());
            Trb.SectFile.BaseStream.Seek(playerInfo.SubInfoOffset, System.IO.SeekOrigin.Begin);
            var playerSubInfos = new List<SubInfo>();
            for (int i = 0; i < playerInfo.SubInfoOffsetCount; i++)
            {
                playerSubInfos.Add(new SubInfo((PPropertyItemType)Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), (uint)Trb.SectFile.BaseStream.Position - 4));
            }

            long lastPosition = Trb.SectFile.BaseStream.Position;

            foreach (var playerSubInfo in playerSubInfos)
            {
                var tvi = new TreeViewItem
                {
                    Header = playerSubInfo.Type switch
                    {
                        //string
                        PPropertyItemType.String => ReadHelper.ReadStringFromOffset(Trb.SectFile, playerSubInfo.Value),
                        _ => playerSubInfo.Value,
                    },
                    Tag = playerSubInfo.Type switch
                    {
                        //string
                        PPropertyItemType.String => new TypeContent(PPropertyItemType.String, ReadHelper.ReadStringFromOffset(Trb.SectFile, playerSubInfo.Value), playerSubInfo.Value, playerSubInfo.ValuePointer, true),
                        _ => new TypeContent(playerSubInfo.Type, playerSubInfo.Value.ToString(), playerSubInfo.ValuePointer, 0, true),
                    }
                };

                TypeContents.Add((TypeContent)tvi.Tag);
                prev.Items.Add(tvi);
            }

            Trb.SectFile.BaseStream.Seek(lastPosition, System.IO.SeekOrigin.Begin);

            return new TypeContent(PPropertyItemType.Array, "Array", 0);
        }
    }
}
