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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TrbMultiTool.FileFormats;
using TrbMultiTool.FileFormats.TTL;

namespace TrbMultiTool
{
    /// <summary>
    /// Interaction logic for TtlWindow.xaml
    /// </summary>
    public partial class TtlWindow : Window
    {
        public List<Ttl> Ttls { get; set; } = new();

        public List<Ttex> Ttex { get; set; } = new();

        //public List<TreeViewItem> Lvis { get; set; } = new();

        public TtlWindow(Ttl ttl)
        {
            InitializeComponent();
            AddTtl(ttl);
        }

        public TtlWindow()
        {
            InitializeComponent();
        }

        public void AddTtl(Ttl ttl)
        {
            Ttls.Add(ttl);
            ReadTtl();
        }

        public void AddTtex(Ttex ttl)
        {
            Ttex.Add(ttl);
            ReadTtex();
        }

        private void ReadTtex()
        {
            var lvi = new TreeViewItem
            {
                Header = Ttex.Last().TextureName
            };
            lvi.Tag = Ttex.Last();
            treeView.Items.Add(lvi);
        }

        private void ReadTtl()
        {
            var lvi = new TreeViewItem
            {
                Header = Ttls.Last().TtlName
            };
            lvi.Tag = Ttls.Last();
            foreach (var textureInfo in Ttls.Last().TextureInfos)
            {
                var lvi2 = new TreeViewItem
                {
                    Header = textureInfo.FileName
                };
                lvi2.Tag = textureInfo;
                lvi.Items.Add(lvi2);
            }
            treeView.Items.Add(lvi);
        }

        private void LoadTtl(TreeViewItem tvi)
        {
            if (tvi.Tag is Ttex) { LoadTtex(tvi); return; }
            var dds = ((TextureInfo)tvi.Tag).Dds;
            img.Source = Ttl.LoadBitmap(dds.BitmapImage);
            img.Width = dds.BitmapImage.Width;
            img.Height = dds.BitmapImage.Height;
        }

        private void LoadTtex(TreeViewItem tvi)
        {
            var dds = ((Ttex)tvi.Tag).DDS;
            img.Source = Ttl.LoadBitmap(dds.BitmapImage);
            img.Width = dds.BitmapImage.Width;
            img.Height = dds.BitmapImage.Height;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sI = (TreeViewItem)treeView.SelectedItem;
            if (sI.Tag is Ttl) return;
            LoadTtl(sI);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var sI = (TreeViewItem)treeView.SelectedItem;
            if (sI.Tag is Ttl) return; //TODO extract whole TTL

            using var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                var wholeName = sI.Tag is Ttex ttex ? ttex.TextureName.Split('\\') : ((TextureInfo)sI.Tag).FileName.Split('\\');
                var dirName = "";
                var fileName = "";
                if (wholeName.Length > 1) // Create Directory
                {
                    dirName = wholeName.First();
                    if (!Directory.Exists(fbd.SelectedPath + "\\" + dirName))
                    {
                        Directory.CreateDirectory(fbd.SelectedPath + "\\" + dirName);
                    }
                    fileName = wholeName[1].Remove(wholeName[1].Length - 4) + ".dds";
                }
                else // No Directory
                {
                    fileName = wholeName.First().Remove(wholeName.First().Length - 4) + ".dds";
                }

                // Write the dds file
                using BinaryWriter writer = new(File.Open($"{fbd.SelectedPath}\\{dirName}\\{fileName}", FileMode.Create));
                if (sI.Tag is Ttex) writer.Write(((Ttex)sI.Tag).RawImage);
                else writer.Write(((TextureInfo)sI.Tag).RawImage);
            }
        }
    }
}
