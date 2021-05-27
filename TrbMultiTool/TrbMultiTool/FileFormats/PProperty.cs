using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TrbMultiTool.FileFormats
{
    public class PProperty
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
            SubItem, //TP8String
            Unknown2, //String16
            Player,//String16
            String,
            UInt
        };

        public record TypeContent(Type Type, string Value, long Offset, long PointerPos = 0);

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
            foreach (var info in infos)
            {
                Trb.SectFile.BaseStream.Seek(info.Offset1, System.IO.SeekOrigin.Begin);
                string name = ReadHelper.ReadStringFromOffset(Trb.SectFile, Trb.SectFile.ReadUInt32());

                var tvi = new TreeViewItem
                {
                    Header = name
                };

                Trb.SectFile.BaseStream.Seek(info.Offset2, System.IO.SeekOrigin.Begin);
                var subInfo = new SubInfo((Type)Trb.SectFile.ReadUInt32(), 0);

                uint textOffset = 0;
                if (subInfo.Type == Type.String) textOffset = Trb.SectFile.ReadUInt32();
                tvi.Tag = subInfo.Type switch
                {
                    //Number?
                    Type.Int => new TypeContent(Type.Int, Trb.SectFile.ReadInt32().ToString(), Trb.SectFile.BaseStream.Position - 4),
                    //float
                    Type.Float => new TypeContent(Type.Float, $"{Trb.SectFile.ReadSingle().ToString():N2}", Trb.SectFile.BaseStream.Position - 4),
                    //bool
                    Type.Bool => new TypeContent(Type.Bool, (Trb.SectFile.ReadUInt32() == 1).ToString(), Trb.SectFile.BaseStream.Position - 4),
                    //Another array? Pointing to the same info?
                    Type.SubItem => SubItem(tvi),
                    //Player
                    Type.Player => Player(ref tvi),
                    //string
                    Type.String => new TypeContent(Type.String, ReadHelper.ReadStringFromOffset(Trb.SectFile, textOffset), textOffset, Trb.SectFile.BaseStream.Position - 4),
                    //Uint
                    Type.UInt => new TypeContent(Type.UInt, Trb.SectFile.ReadUInt32().ToString(), Trb.SectFile.BaseStream.Position - 4),
                    _ => $"Type {subInfo.Type} hasn't been implemented yet",
                };
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
            return new TypeContent(Type.String, "SubProperty", 0);
        }

        //This method can be replaced and it is not player only...
        private TypeContent Player(ref TreeViewItem prev)
        {
            Trb.SectFile.BaseStream.Seek(Trb.SectFile.ReadUInt32(), System.IO.SeekOrigin.Begin);
            var playerInfo = new PlayerInfo(Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32());
            Trb.SectFile.BaseStream.Seek(playerInfo.SubInfoOffset, System.IO.SeekOrigin.Begin);
            var playerSubInfos = new List<SubInfo>();
            for (int i = 0; i < playerInfo.SubInfoOffsetCount; i++)
            {
                playerSubInfos.Add(new SubInfo((Type)Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32()));
            }
            foreach (var playerSubInfo in playerSubInfos)
            {
                var tvi = new TreeViewItem
                {
                    Header = playerSubInfo.Type switch
                    {
                        //string
                        Type.String => ReadHelper.ReadStringFromOffset(Trb.SectFile, playerSubInfo.Value),
                        Type.Int => playerSubInfo.Value,
                        _ => $"Type {playerSubInfo.Type} hasn't been implemented yet",
                    },
                    Tag = playerSubInfo.Type switch
                    {
                        //string
                        Type.String => new TypeContent(Type.String, ReadHelper.ReadStringFromOffset(Trb.SectFile, playerSubInfo.Value), playerSubInfo.Value, Trb.SectFile.BaseStream.Position - 4),
                        Type.Int => new TypeContent(Type.Int, playerSubInfo.Value.ToString(), Trb.SectFile.BaseStream.Position - 4),
                        _ => new TypeContent(Type.String, $"Type {playerSubInfo.Type} hasn't been implemented yet", 0, 0) ,
                    }
                };
                TypeContents.Add((TypeContent)tvi.Tag);
                prev.Items.Add(tvi);
            }
            return new TypeContent(Type.String, "SubProperty", 0);
        }
    }
}
