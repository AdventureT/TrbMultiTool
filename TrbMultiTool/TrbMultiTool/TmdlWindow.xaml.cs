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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using TrbMultiTool.FileFormats;


namespace TrbMultiTool
{
    /// <summary>
    /// Interaction logic for TmdlWindow.xaml
    /// </summary>
    public partial class TmdlWindow : Window
    {
        const double speed = 12;
        const double rotationSpeed = speed * 6;
        public List<Tmdl> Tmdls { get; set; } = new();

        public List<Ttex> Ttexes { get; set; } = new();

        public List<Tmat> Tmats { get; set; } = new();

        public Assimp.AssimpContext Context { get; set; } = new Assimp.AssimpContext();

        public ObservableCollection<Assimp.ExportFormatDescription> ExportFormats = new(new Assimp.AssimpContext().GetSupportedExportFormats());

        public TmdlWindow()
        {
            InitializeComponent();
        }

        public TmdlWindow(List<Tmdl> tmdls)
        {
            InitializeComponent();
            DataContext = this;
            cb.ItemsSource = ExportFormats;
            foreach (var item in tmdls)
            {
                AddTmdl(item);
            }
        }

        public TmdlWindow(List<Tmdl> tmdls, List<Ttex> ttexes, List<Tmat> tmats)
        {
            InitializeComponent();
            Ttexes = ttexes;
            Tmats = tmats;
            DataContext = this;
            cb.ItemsSource = ExportFormats;

            foreach (var item in tmdls)
            {
                AddTmdl(item);
            }
        }

        public void Move(double d)
        {
            double u = 0.05;
            PerspectiveCamera camera = (PerspectiveCamera)myViewport.Camera;
            Vector3D lookDirection = camera.LookDirection;
            Point3D position = camera.Position;

            lookDirection.Normalize();
            position = position + u * lookDirection * d;

            camera.Position = position;
        }

        public void Rotate(double d)
        {
            double u = 0.05;
            double angleD = u * d;
            PerspectiveCamera camera = (PerspectiveCamera)myViewport.Camera;

            var m = new Matrix3D();
            m.Rotate(new Quaternion(camera.UpDirection, -angleD));
            camera.LookDirection = m.Transform(camera.LookDirection);
        }

        public void RotateVertical(double d)
        {
            double u = 0.05;
            double angleD = u * d;
            PerspectiveCamera camera = (PerspectiveCamera)myViewport.Camera;
            Vector3D lookDirection = camera.LookDirection;

            var cp = Vector3D.CrossProduct(camera.UpDirection, lookDirection);
            cp.Normalize();

            var m = new Matrix3D();
            m.Rotate(new Quaternion(cp, -angleD));
            camera.LookDirection = m.Transform(camera.LookDirection);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D:
                    Rotate(rotationSpeed);
                    break;
                case Key.A:
                    Rotate(-rotationSpeed);
                    break;
                case Key.W:
                    Move(speed);
                    break;
                case Key.S:
                    Move(-speed);
                    break;
                case Key.E:
                    RotateVertical(rotationSpeed);
                    break;
                case Key.Q:
                    RotateVertical(-rotationSpeed);
                    break;
            }
        }

        public void AddTmdl(Tmdl tmdl)
        {
            Tmdls.Add(tmdl);
            ReadTmdl();
        }

        private void ReadTmdl()
        {
            var lvi = new TreeViewItem
            {
                Header = Tmdls.Last().TmdlName
            };
            lvi.Tag = Tmdls.Last();
            treeView.Items.Add(lvi);
        }

        private void LoadTmdl(TreeViewItem tvi)
        {
            var tmdl = tvi.Tag as Tmdl;
            myViewport.Children.Clear();
            modelName.Content = $"Opened Model: {Trb._safeFileName}";
            var modelGroup = new Model3DGroup();
            foreach (var item in tmdl.Scene.Meshes)
            {
                var mesh = new MeshGeometry3D();
                foreach (var vertex in item.Vertices)
                {
                    mesh.Positions.Add(new(vertex.X, vertex.Y, vertex.Z));
                }
                foreach (var normal in item.Normals)
                {
                    mesh.Normals.Add(new(normal.X, normal.Y, normal.Z));
                }
                foreach (var uv in item.TextureCoordinateChannels.First())
                {
                    mesh.TextureCoordinates.Add(new(uv.X, uv.Y));
                }
                foreach (var face in item.Faces)
                {
                    mesh.TriangleIndices.Add(face.Indices[0]);
                    mesh.TriangleIndices.Add(face.Indices[1]);
                    mesh.TriangleIndices.Add(face.Indices[2]);
                }
                var gm = new GeometryModel3D();

                gm.Geometry = mesh;
                var diffuse = new DiffuseMaterial
                {
                    Brush = new SolidColorBrush(Color.FromRgb(166, 166, 166))
                };
                gm.Material = diffuse;

                modelGroup.Children.Add(gm);
            }
            var directionalLight = new DirectionalLight
            {
                Color = Color.FromRgb(255, 255, 255),
                Direction = new Vector3D(-1, -1, -1)
            };
            var directionalLight2 = new DirectionalLight
            {
                Color = Color.FromRgb(255, 255, 255),
                Direction = new Vector3D(5, 5, 5)
            };
            modelGroup.Children.Add(directionalLight);
            modelGroup.Children.Add(directionalLight2);
            var modelVisual = new ModelVisual3D
            {
                Content = modelGroup
            };
            myViewport.Children.Add(modelVisual);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sI = (TreeViewItem)treeView.SelectedItem;
            LoadTmdl(sI);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var exportFormat = cb.SelectedItem as Assimp.ExportFormatDescription;
            var tmdl = ((TreeViewItem)treeView.SelectedItem).Tag as Tmdl;
            //var test = tmdl.Scene.Meshes.Where(x => x.Name.ToLower() == Tmats.Where(y => y.MeshName.ToLower() == x.Name.ToLower()).Select(y => y.MeshName.ToLower()).First());
            //int x = 0;
            //foreach (var item in test)
            //{
            //    var tmatTexture = Ttexes.Where(x => x.TextureName == Tmats.Where(y => y.MeshName.ToLower() == item.Name.ToLower()).Select(y => y.TextureName.ToLower()).First()).First();
            //    tmdl.Scene.Textures.Add(new("dds\0", tmatTexture.RawImage));
            //    var mat = new Assimp.Material();
            //    mat.AddProperty(new(tmatTexture.TextureName, "1", Assimp.TextureType.Diffuse, 0));
            //    tmdl.Scene.Materials.Add(mat); //You need a default material!!!!! Cost me 5 hours of figguring out on how to debug native dlls aaaggh
            //    item.MaterialIndex = x++;
            //}
            var openFileDialog = new SaveFileDialog
            {
                Filter = $"{exportFormat.Description} (*{exportFormat.FileExtension})|*{exportFormat.FileExtension}",
                DefaultExt = $"{exportFormat.FileExtension}",
                Title = $"{exportFormat.Description}",
                FileName = $"{System.IO.Path.GetFileNameWithoutExtension(tmdl.TmdlName)}"
            };
            
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Context.ExportFile(tmdl.Scene, openFileDialog.FileName, exportFormat.FormatId);
            }
        }
    }
}
