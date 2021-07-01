using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for XuiWindow.xaml
    /// </summary>
    public partial class XuiWindow : Window
    {
        public ObservableCollection<string> Strings { get; set; } = new();

        public ListViewItem SelectedFile { get; set; }

        public XuiWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public XuiWindow(List<XUI> xuis)
        {
            InitializeComponent();
            DataContext = this;
            foreach (var item in xuis)
            {
                XuiListView2.Items.Add(new ListViewItem() { Content = item.FileName, Tag = item.Strings });
            }
            
        }

        private void XuiListView2_Selected(object sender, RoutedEventArgs e)
        {
            Strings.Clear();
            foreach (var item in SelectedFile.Tag as List<string>)
            {
                Strings.Add(item);
            }
        }
    }
}
