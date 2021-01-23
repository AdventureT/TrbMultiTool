using System;
using System.Collections.Generic;
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

namespace TrbMultiTool
{
    /// <summary>
    /// Interaction logic for TtlWindow.xaml
    /// </summary>
    public partial class TtlWindow : Window
    {
        public List<Ttl> Ttls { get; set; } = new();

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

        private void ReadTtl()
        {
            var lvi = new TreeViewItem
            {
                Header = Ttls.Last().TtlName
            };
            foreach (var textureInfo in Ttls.Last().TextureInfos)
            {
                var lvi2 = new TreeViewItem
                {
                    Header = textureInfo.FileName
                };
                lvi.Items.Add(lvi2);
            }
            treeView.Items.Add(lvi);
        }

        private void LoadTtl(TreeViewItem tvi)
        {
            var tviParent = (TreeViewItem)tvi.Parent;
            var ttl = Ttls.Where(x => x.TtlName == (string)tviParent.Header).First();
            var texInfos = ttl.TextureInfos;
            var dds = texInfos.Where(x => x.FileName.Contains((string)tvi.Header)).First();
            img.Source = Ttl.LoadBitmap(dds.Dds.BitmapImage);
            img.Width = dds.Dds.BitmapImage.Width;
            img.Height = dds.Dds.BitmapImage.Height;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sI = (TreeViewItem)treeView.SelectedItem;
            if (sI.Parent == null) return;
            LoadTtl(sI);
        }
    }
}
