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
        const double speed = 7.5;
        const double rotationSpeed = speed * 6;
        public List<Tmdl> Tmdls { get; set; } = new();

        public TmdlWindow()
        {
            InitializeComponent();
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
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
            foreach (var mv in tmdl.MVs)
            {
                myViewport.Children.Add(mv);
            }
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
    }
}
