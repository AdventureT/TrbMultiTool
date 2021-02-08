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
using TrbMultiTool.FileFormats;
using static TrbMultiTool.FileFormats.Quest;

namespace TrbMultiTool
{
    /// <summary>
    /// Interaction logic for QuestWindow.xaml
    /// </summary>
    public partial class QuestWindow : Window
    {
        public List<Quest.TypeContent> TypeContentss { get; set; } = new();

        public List<Quest.TypeContent> TypeContentsWithString { get; set; } = new();

        public QuestWindow()
        {
            InitializeComponent();
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tvi = (TreeViewItem)treeView.SelectedItem;
            if (tvi.Tag != null)
            {
                var tag = tvi.Tag as TypeContent;
                label.Content = tag.Type;
                textBox.Text = tag.Value;
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)treeView.SelectedItem;
            var tag = tvi.Tag as TypeContent;
            using BinaryWriter writer = new BinaryWriter(File.Open(Trb._fileName, FileMode.Open, FileAccess.ReadWrite));
            writer.BaseStream.Seek(tag.Offset, SeekOrigin.Begin);
            switch (tag.Type)
            {
                case FileFormats.Quest.Type.Int:
                    writer.Write(Convert.ToUInt32(textBox.Text));
                    break;
                case FileFormats.Quest.Type.Unknown:
                    break;
                case FileFormats.Quest.Type.Float:
                    writer.Write(Convert.ToSingle(textBox.Text));
                    break;
                case FileFormats.Quest.Type.Bool:
                    writer.Write(Convert.ToBoolean(textBox.Text) ? 1 : 0);
                    break;
                case FileFormats.Quest.Type.SubItem:
                    break;
                case FileFormats.Quest.Type.Unknown2:
                    break;
                case FileFormats.Quest.Type.Player:
                    break;
                case FileFormats.Quest.Type.String:
                    if (textBox.Text.Length > tag.Value.Length)
                    {
                        MessageBox.Show("Your inputted string is too large!");
                    }
                    else
                    {
                        //var allText = FindVisualChildren<TreeViewItem>(treeView);
                        var remainingLength = tag.Value.Length - textBox.Text.Length;
                        var charArr = textBox.Text.ToCharArray();
                        for (int i = 0; i < remainingLength; i++)
                        {
                            charArr = charArr.Append('\0').ToArray();
                        }
                        writer.Write(charArr);
                    }
                    break;
                case FileFormats.Quest.Type.UInt:
                    writer.Write(Convert.ToUInt32(textBox.Text));
                    break;
                default:
                    break;
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void swapButton_Click(object sender, RoutedEventArgs e)
        {
            var typeContentCb = TypeContentsWithString[comboBox.SelectedIndex];
            var tvi = (TreeViewItem)treeView.SelectedItem;
            var tag = tvi.Tag as TypeContent;
            using BinaryWriter writer = new BinaryWriter(File.Open(Trb._fileName, FileMode.Open, FileAccess.ReadWrite));
            writer.BaseStream.Seek(tag.PointerPos, SeekOrigin.Begin);
            writer.Write((uint)typeContentCb.Offset - (uint)Trb.Tsfl.Sect.Offset);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TypeContentsWithString = TypeContentss.Where(x => x.Type == Quest.Type.String).GroupBy(p => p.Value).Select(grp => grp.FirstOrDefault()).ToList();
            var tCWS2 = TypeContentsWithString.Select(x => x.Value);
            comboBox.Items.Clear();
            foreach (var item in tCWS2)
            {
                comboBox.Items.Add(item);
            }
        }

        private void comboBox_DropDownOpened(object sender, EventArgs e)
        {
            //TypeContentsWithString = TypeContentss.Where(x => x.Type == Quest.Type.String).Distinct().ToList();
            //var tCWS2 = TypeContentsWithString.Select(x => x.Value);
            //comboBox.Items.Clear();
            //foreach (var item in tCWS2)
            //{
            //    comboBox.Items.Add(item);
            //}
        }

        //IEnumerable<TreeViewItem> Collect(TreeView nodes)
        //{
        //    foreach (TreeViewItem node in nodes)
        //    {
        //        yield return node;

        //        foreach (var child in Collect(node.Items))
        //            yield return child;
        //    }
        //}
    }
}
