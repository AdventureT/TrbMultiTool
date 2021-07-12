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

        public List<Ttex> Ttexes { get; set; } = new();

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
            Ttexes.Add(ttl);
            ReadTtex();
        }

        private void ReadTtex()
        {
            var lvi = new TreeViewItem
            {
                Header = Ttexes.Last().TextureName
            };
            lvi.Tag = Ttexes.Last();
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

        private void Extract_Everything_Button_Clicked(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            string path = fbd.SelectedPath;
            if (Ttexes.Count > 0)
            {
                foreach (var ttex in Ttexes)
                {
                    ExtractFile(path, new[] { ttex.TextureName }, ttex.RawImage);
                }
            }
            else
            {
                foreach (var ttl in Ttls)
                {
                    for (int i = 0; i < ttl.TextureInfoCount; i++)
                    {
                        TextureInfo tInfo = ttl.TextureInfos[i];
                        ExtractFile(path, ttl.TextureInfos[i].FileName.Split('\\'), tInfo.RawImage);
                    }
                }
            }

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (treeView.SelectedItem == null) return;
            var sI = (TreeViewItem)treeView.SelectedItem;

            if (sI.Tag is Ttl) return;

            var fd = new Microsoft.Win32.OpenFileDialog();
            fd.Filter = $"Image File (*.png, *.jpg, *.dds)|*.png;*.jpg;*.dds";

            if (fd.ShowDialog() == true)
            {
                var imgStream = new MemoryStream();

                if (!fd.FileName.EndsWith(".dds"))
                {
                    byte[] dds = PrimeWPF.DDSConverter.FromFile(fd.FileName);
                    await imgStream.WriteAsync(dds);
                }
                else
                {
                    var stream = fd.OpenFile();
                    await stream.CopyToAsync(imgStream);
                }

                var sect = new MemoryStream();
                var fileSizes = new List<uint>();
                var offsets = new List<List<uint>>();
                var names = new List<string>();
                var idx = new List<short>();

                if (Ttexes.Count > 0)
                {
                    var ttex = sI.Tag as Ttex;


                    foreach (var tex in Ttexes)
                    {
                        MemoryStream currentFile;
                        if (ttex.TextureName == tex.TextureName)
                        {
                            currentFile = tex.Repack(imgStream);
                            sect.Write(currentFile.ToArray());
                        }
                        else
                        {
                            currentFile = tex.Repack();
                            sect.Write(currentFile.ToArray());
                        }

                        names.Add("ttex\0");
                        offsets.Add(tex.Offsets);
                        fileSizes.Add((uint)currentFile.Length);
                        idx.Add(tex.Idx);

                        currentFile.Close();
                    }

                    Trb.GenerateFile(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\new.trb", sect, fileSizes, offsets, names, idx);
                }
                else if (Ttls.Count > 0)
                {
                    if (sI.Tag is TextureInfo)
                    {
                        foreach (var ttl in Ttls)
                        {
                            var tInfo = sI.Tag as TextureInfo;

                            var newSect = ttl.RepackSECT(tInfo.FileName, imgStream);
                            sect.Write(newSect.ToArray());

                            names.Add($"{ttl.TtlName}\0");
                            fileSizes.Add((uint)newSect.Length);
                            offsets.Add(ttl.Offsets);
                            idx.Add(ttl.Idx);

                            newSect.Close();
                        }

                        Trb.GenerateFile(Trb._fileName, sect, fileSizes, offsets, names, idx);
                    }
                }

                imgStream.Close();
                sect.Close();

                //var f = new BinaryWriter(File.Open("C:\\Users\\nepel\\Desktop\\new.trb", FileMode.Create));
                //f.Write(sect.ToArray());
                //f.Close();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (treeView.SelectedItem == null) return;
            var sI = (TreeViewItem)treeView.SelectedItem;

            if (sI.Tag is Ttl) return;

            var fd = new Microsoft.Win32.OpenFileDialog();
            fd.Filter = $"DDS File (*.dds)|*.dds";

            if (fd.ShowDialog() == true)
            {
                var ms = Ttex.FromFile(fd.FileName);

                Trb.AppendFile(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\new.trb", ms, (uint)ms.Length, new() { 0x4, 0x8, 0x10 }, "ttex\0");
            }
        }
    }
}
