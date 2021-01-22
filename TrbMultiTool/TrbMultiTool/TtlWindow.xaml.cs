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
        public Ttl Ttl { get; set; }
        public TtlWindow(Ttl ttl)
        {
            InitializeComponent();
            Ttl = ttl;
            ReadTtl();
        }

        private void ReadTtl()
        {
            foreach (var textureInfo in Ttl.TextureInfos)
            {
                listView.Items.Add(textureInfo.FileName);
            }
        }

        private void LoadTtl(int index)
        {
            img.Source = Ttl.LoadBitmap(Ttl.TextureInfos[index].Dds.BitmapImage);
            img.Width = Ttl.TextureInfos[index].Dds.BitmapImage.Width;
            img.Height = Ttl.TextureInfos[index].Dds.BitmapImage.Height;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTtl(listView.SelectedIndex);
        }
    }
}
