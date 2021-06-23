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

namespace TrbMultiTool
{
    /// <summary>
    /// Логика взаимодействия для LocaleStringsWindow.xaml
    /// </summary>
    public partial class LocaleStringsWindow : Window
    {
        public LocaleStringsWindow()
        {
            InitializeComponent();
        }

        void Update(LocaleString lS)
        {
            textField.MaxLength = lS.text.Length;
            textField.Text = lS.text;
            idField.Text = lS.id.ToString();
        }

        public void SearchForItem()
        {
            int targetID = Convert.ToInt32(idSearchField.Text);

            foreach (ListViewItem item in ListView.Items)
            {
                LocaleString lS = (LocaleString)item.Tag;

                if (lS.id == targetID)
                {
                    ListView.SelectedItem = item;
                    return;
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem sI = (ListViewItem)ListView.SelectedItem;
            if (sI == null) return;

            LocaleString lS = (LocaleString)sI.Tag;
            Update(lS);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem sI = (ListViewItem)ListView.SelectedItem;
            if (sI == null) return;

            LocaleString lS = (LocaleString)sI.Tag;
            Trb.SectFile.BaseStream.Seek(lS.offset, SeekOrigin.Begin);
            using BinaryWriter writer = new BinaryWriter(File.Open(Trb._fileName, FileMode.Open, FileAccess.ReadWrite));
            writer.BaseStream.Seek(Trb.Tsfl.Sect.Offset + lS.offset, SeekOrigin.Begin);
            writer.Write(Encoding.Unicode.GetBytes(textField.Text));

            writer.Close();
            sI.Content = textField.Text;
            lS.text = textField.Text;
            Update(lS);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ListViewItem sI = (ListViewItem)ListView.SelectedItem;
            if (sI == null) return;

            LocaleString lS = (LocaleString)sI.Tag;
            Clipboard.SetText(lS.text);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SearchForItem();
        }

        private void idSearchField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchForItem();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "LocaleStrings.txt";
            dlg.Filter = $"Text File|*.txt";

            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.FileName))
            {
                using (var writer = new StreamWriter(File.Open(dlg.FileName, FileMode.Create, FileAccess.Write)))
                {
                    foreach (ListViewItem item in ListView.Items)
                    {
                        LocaleString lS = (LocaleString)item.Tag;
                        writer.WriteLine(lS.text);
                    }

                    writer.Close();
                }
            }
        }
    }
}
