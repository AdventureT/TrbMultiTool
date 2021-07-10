using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static TrbMultiTool.FileFormats.TerrainVIS;

namespace TrbMultiTool
{
    /// <summary>
    /// Interaction logic for TerrainVISWindow.xaml
    /// </summary>
    public partial class TerrainVISWindow : Window
    {
        public TerrainVISWindow()
        {
            InitializeComponent();
        }

        private void AddListToItem(TreeViewItem tI, List<String> items)
        {
            foreach (var item in items)
            {
                tI.Items.Add(item);
            }
        }

        private void AddLODInfo(TreeViewItem tI, int index, List<String> models, uint count, string matlib)
        {
            if (count > 0)
            {
                TreeViewItem L0Item = new()
                {
                    Header = $"L{index}"
                };

                TreeViewItem ModelsItem = new()
                {
                    Header = "Models"
                };

                TreeViewItem MatlibsItem = new()
                {
                    Header = "MatLibs"
                };

                L0Item.Items.Add(ModelsItem);
                L0Item.Items.Add(MatlibsItem);

                AddListToItem(ModelsItem, models);
                MatlibsItem.Items.Add(matlib);

                tI.Items.Add(L0Item);
            }
        }

        enum ItemTypes
        {
            Int32,
            UInt32
        };

        class ItemWithValue
        {
            public string name;
            public string value;
            public int offset;
            public uint pointer;
            public uint type;
            public ItemTypes valueType;
        }

        public void AddMainInfo(uint countOfParts, int countOffset, uint blockSize, int blockSizeOffset)
        {
            TreeViewItem mainItem = new()
            {
                Header = "General Info"
            };

            ItemWithValue partsTag = new();
            partsTag.name = "Count of Parts";
            partsTag.value = countOfParts.ToString();
            partsTag.valueType = ItemTypes.UInt32;
            partsTag.offset = countOffset;

            TreeViewItem partsItem = new()
            {
                Header = partsTag.name,
                Tag = partsTag
            };

            ItemWithValue memoryLimitTag = new();
            memoryLimitTag.name = "Memory Limit";
            memoryLimitTag.value = blockSize.ToString();
            memoryLimitTag.valueType = ItemTypes.UInt32;
            memoryLimitTag.offset = blockSizeOffset;

            TreeViewItem memoryLimitItem = new()
            {
                Header = memoryLimitTag.name,
                Tag = memoryLimitTag
            };

            mainItem.Items.Add(partsItem);
            mainItem.Items.Add(memoryLimitItem);

            tree.Items.Add(mainItem);
        }

        public void AddTerrainParts(List<TerrainPartInfo> terrainPartInfos)
        {
            TreeViewItem mainItem = new()
            {
                Header = "Terrain Parts"
            };

            foreach (var tpInfo in terrainPartInfos)
            {
                TreeViewItem tI = new()
                {
                    Header = tpInfo.name
                };

                // Collision
                TreeViewItem collisionItem = new()
                {
                    Header = "Collision"
                };

                collisionItem.Items.Add(tpInfo.collisionFile);

                // Models
                TreeViewItem modelsItem = new()
                {
                    Header = "LOD List"
                };

                AddLODInfo(modelsItem, 0, tpInfo.L0Models, tpInfo.countOfL0Mods, tpInfo.L0Mat.file);
                AddLODInfo(modelsItem, 1, tpInfo.L1Models, tpInfo.countOfL1Mods, tpInfo.L1Mat.file);

                tI.Items.Add(collisionItem);
                tI.Items.Add(modelsItem);
                mainItem.Items.Add(tI);
            }

            tree.Items.Add(mainItem);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var sI = tree.SelectedItem as TreeViewItem;
            if (sI == null) return;

            if (sI.Tag is ItemWithValue)
            {
                var tag = sI.Tag as ItemWithValue;
                using BinaryWriter writer = new BinaryWriter(File.Open(Trb._fileName, FileMode.Open, FileAccess.ReadWrite));
                writer.BaseStream.Seek(Trb.Tsfl.Sect.Offset + tag.offset, SeekOrigin.Begin);

                switch (tag.valueType)
                {
                    case ItemTypes.Int32:
                        writer.Write(Convert.ToInt32(valueField.Text));
                        break;
                    case ItemTypes.UInt32:
                        writer.Write(Convert.ToUInt32(valueField.Text));
                        break;
                };

                writer.Close();
            }
        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sI = tree.SelectedItem as TreeViewItem;
            if (sI == null) return;

            if (sI.Tag is ItemWithValue)
            {
                var tag = sI.Tag as ItemWithValue;
                valueField.Text = tag.value;
            }
        }
    }
}
