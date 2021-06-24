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

        public TtlWindow(List<Ttl> ttls, List<Ttex> ttexes)
        {
            InitializeComponent();
            foreach (var item in ttls)
            {
                AddTtl(item);
            }
            foreach (var item in ttexes)
            {
                AddTtex(item);
            }
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

        private void ExtractFile(string path, string[] wholeName, byte[] rawImage)
        {
            string dirName = "";
            string fileName;

            if (wholeName.Length > 1) 
            {
                //Array.Resize(ref wholeName, dirName.Length - 1);
                dirName = string.Join('\\', wholeName.Take(wholeName.Length - 1));

                Directory.CreateDirectory(path + "\\" + dirName);
            }

            fileName = wholeName.Last().Remove(wholeName.Last().Length - 4) + ".dds";

            // Write the dds file
            using BinaryWriter writer = new(File.Open($"{path}\\{dirName}\\{fileName}", FileMode.Create));
            writer.Write(rawImage);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (treeView.SelectedItem == null) return;
            var sI = (TreeViewItem)treeView.SelectedItem;

            var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            string path = fbd.SelectedPath;

            if (sI.Tag is Ttl)
            {
                Ttl ttl = (Ttl)sI.Tag;

                for (int i = 0; i < ttl.TextureInfoCount; i++)
                {
                    TextureInfo tInfo = ttl.TextureInfos[i];
                    ExtractFile(path, ttl.TextureInfos[i].FileName.Split('\\'), tInfo.RawImage);
                }

                return;
            };

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                if (sI.Tag is Ttex) ExtractFile(path, ((Ttex)sI.Tag).TextureName.Split("\\"), ((Ttex)sI.Tag).RawImage);
                else ExtractFile(path, ((TextureInfo)sI.Tag).FileName.Split("\\"), ((TextureInfo)sI.Tag).RawImage);
            }
        }
    }
}
