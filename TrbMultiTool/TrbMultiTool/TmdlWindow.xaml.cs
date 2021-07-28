using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using SEViewer.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Windows.Shapes;
using System.Windows.Threading;
using TrbMultiTool.FileFormats;


namespace TrbMultiTool
{
    /// <summary>
    /// Interaction logic for TmdlWindow.xaml
    /// </summary>
    public partial class TmdlWindow : Window
    {

        public List<Tmdl> Tmdls { get; set; } = new();

        public List<Ttex> Ttexes { get; set; } = new();

        public List<Tmat> Tmats { get; set; } = new();

        public Assimp.AssimpContext Context { get; set; } = new Assimp.AssimpContext();

        public ObservableCollection<Assimp.ExportFormatDescription> ExportFormats = new(new Assimp.AssimpContext().GetSupportedExportFormats());

        GLControl gLControl;

        private Stopwatch watch;

        private float deltaTime;

        private static Vector2 lastPosition;

        private static bool firstMove = true;

        private static bool mouseDown;

        private Matrix4 model;

        Color4 color = Color4.FromHsv(new Vector4(1, 0.75f, 0.75f, 1));

        Shader shader;
        Core.Camera camera;
        private List<VBO<Vector3>> _verticesVBO = new();
        private List<VBO<uint>> _indicesVBO = new();
        private List<int> _vertexArrayObject = new();
        bool selected;

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
            gLControl = new GLControl
            {
                Dock = DockStyle.Fill
            };
            gLControl.Paint += GLControl_Paint;
            gLControl.Load += GLControl_Load;
            gLControl.Resize += GLControl_Resize;
            gLControl.MouseDown += GLControl_MouseDown;
            gLControl.MouseUp += GLControl_MouseUp;
            Host.Child = gLControl;

            camera = new Core.Camera(Vector3.UnitZ * 3, (float)gLControl.Width / (float)gLControl.Height);

            ComponentDispatcher.ThreadIdle += new EventHandler(ComponentDispatcher_ThreadIdle);
        }

        private void GLControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            //{
            //    System.Windows.Forms.Cursor.Position = gLControl.Parent.PointToScreen(new System.Drawing.Point((gLControl.Left + gLControl.Right) / 2, (gLControl.Top + gLControl.Bottom) / 2));
            //    lastPosition = new Vector2(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            //    System.Windows.Forms.Cursor.Hide();
            //    mouseDown = true;
            //}
        }

        private void GLControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            //{
            //    lastPosition = new Vector2(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            //    System.Windows.Forms.Cursor.Position = gLControl.Parent.PointToScreen(new System.Drawing.Point((gLControl.Left + gLControl.Right) / 2, (gLControl.Top + gLControl.Bottom) / 2));
            //    System.Windows.Forms.Cursor.Show();
            //    mouseDown = false;
            //}
        }

        void ComponentDispatcher_ThreadIdle(object sender, EventArgs e)
        {
            Render();
        }

        private void Movement()
        {
            if (!gLControl.Focused)
            {
                return;
            }

            KeyboardState input = OpenTK.Input.Keyboard.GetState();

            const float sensitivity = 0.2f;

            float cameraSpeed = 3; // cameraMovementSpeed here is 3

            if (input.IsKeyDown(Key.ShiftLeft))
            {
                cameraSpeed *= 3;
            }

            if (input.IsKeyDown(Key.W))
            {
                camera.Position += camera.Front * cameraSpeed * deltaTime; // Forward
            }
            if (input.IsKeyDown(Key.S))
            {
                camera.Position -= camera.Front * cameraSpeed * deltaTime; // Backwards
            }

            if (input.IsKeyDown(Key.A))
            {
                camera.Position -= camera.Right * cameraSpeed * deltaTime; // Left
            }
            if (input.IsKeyDown(Key.D))
            {
                camera.Position += camera.Right * cameraSpeed * deltaTime; // Right
            }

            if (input.IsKeyDown(Key.Q))
            {
                camera.Position += camera.Up * cameraSpeed * deltaTime; // Up
            }
            if (input.IsKeyDown(Key.E))
            {
                camera.Position -= camera.Up * cameraSpeed * deltaTime; // Down
            }

            if (mouseDown)
            {
                var mouse = System.Windows.Forms.Cursor.Position;

                if (firstMove)
                {
                    lastPosition = new Vector2(mouse.X, mouse.Y);
                    firstMove = false;
                }
                else
                {
                    var deltaX = mouse.X - lastPosition.X;
                    var deltaY = mouse.Y - lastPosition.Y;
                    lastPosition = new Vector2(mouse.X, mouse.Y);

                    camera.Yaw += deltaX * sensitivity;
                    camera.Pitch -= deltaY * sensitivity;
                }

                if ((lastPosition.X <= 0 || lastPosition.X >= gLControl.Width) || (lastPosition.Y <= 0 || lastPosition.Y >= gLControl.Height))
                {
                    System.Windows.Forms.Cursor.Position = gLControl.Parent.PointToScreen(new System.Drawing.Point((gLControl.Left + gLControl.Right) / 2, (gLControl.Top + gLControl.Bottom) / 2));
                    mouse = System.Windows.Forms.Cursor.Position;
                    lastPosition = new Vector2(mouse.X, mouse.Y);
                }
            }
        }

        private void GLControl_Resize(object sender, EventArgs e)
        {
            gLControl.MakeCurrent();
            GL.Viewport(0, 0, gLControl.Width, gLControl.Height);
            if (Trb._game == Game.DeBlob)
            {
                // Get the fuck back, de blob models are big as hell
                camera.Position = new Vector3(0, -20, 1);
            }
            else
            {
                camera.Position = new Vector3(0, -3, 1);
            }
            camera.Pitch = 90;
            camera.AspectRatio = (float)gLControl.Width / (float)gLControl.Height;
            gLControl.Invalidate();
        }

        public static Vector3 RotateAround(Vector3 point, Vector3 center, Quaternion rotation)
        {
            return rotation * (point - center) + center;
        }

        private void Render()
        {
            //Movement();
            gLControl.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(color);

            if (selected)
            {
                shader.Use();
                deltaTime = (float)watch.ElapsedTicks / Stopwatch.Frequency;

                //Deblob has a 270° rotation on the X-axis for some reason
                if (Trb._game == Game.DeBlob) model = Matrix4.Identity * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(270)) * Matrix4.CreateRotationZ(deltaTime);
                else model = Matrix4.Identity * Matrix4.CreateRotationZ(deltaTime);
                
                shader.SetMatrix("model", model);
                shader.SetMatrix("view", camera.GetViewMatrix());
                shader.SetMatrix("projection", camera.GetProjectionMatrix());

                for (int i = 0; i < _vertexArrayObject.Count; i++)
                {
                    GL.BindVertexArray(_vertexArrayObject[i]);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesVBO[i].Handle);
                    GL.DrawElements(PrimitiveType.Triangles, _indicesVBO[i].Count, DrawElementsType.UnsignedInt, 0);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                    GL.BindVertexArray(0);
                }


            }



            //const float sensitivity = 0.2f;
            //var point = PointToScreen(Mouse.GetPosition(this));

            //if (firstMove) // This bool variable is initially set to true.
            //{
            //    lastPos = new Vector2((float)point.X, (float)point.Y);
            //    firstMove = false;
            //}
            //else
            //{
            //    // Calculate the offset of the mouse position
            //    var deltaX = (float)point.X - lastPos.X;
            //    var deltaY = (float)point.Y - lastPos.Y;
            //    lastPos = new Vector2((float)point.X, (float)point.Y);

            //    // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            //    if (camera != null)
            //    {
            //        camera.Yaw += deltaX * sensitivity;
            //        camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            //    }

            //}
            gLControl.SwapBuffers();    // Display the result.
            gLControl.Invalidate();
        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            gLControl.MakeCurrent();
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(color);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shader = Shader.Create("yeet", "Shaders/default.vert", "Shaders/default.frag");
            watch = Stopwatch.StartNew();
            gLControl.Invalidate();
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            Render();
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
            selected = true;
            var tmdl = tvi.Tag as Tmdl;
            modelName.Content = $"Opened Model: {Trb._safeFileName}";
            _verticesVBO.Clear();
            _indicesVBO.Clear();
            _vertexArrayObject.Clear();

            foreach (var item in tmdl.Scene.Meshes)
            {
                List<Vector3> vertices = new();
                List<Vector3> vertexNormals = new();
                List<uint> faces = new();

                for (int i = 0; i < item.VertexCount; i++)
                {
                    vertices.Add(new(item.Vertices[i].X, item.Vertices[i].Y, item.Vertices[i].Z));
                    vertexNormals.Add(new(item.Normals[i].X, item.Normals[i].Y, item.Normals[i].Z));
                }

                foreach (var face in item.Faces)
                {
                    faces.Add((uint)face.Indices[0]);
                    faces.Add((uint)face.Indices[1]);
                    faces.Add((uint)face.Indices[2]);
                }

                var VAO = GL.GenVertexArray();

                _vertexArrayObject.Add(VAO);
                GL.BindVertexArray(VAO);

                var VBO = new VBO<Vector3>(vertices.ToArray());
                var VBON = new VBO<Vector3>(vertexNormals.ToArray());

                VBO.BindToShaderAttribute(shader, "Position");
                VBON.BindToShaderAttribute(shader, "aNormal");
                _verticesVBO.Add(VBO);

                _indicesVBO.Add(new VBO<uint>(faces.ToArray(), BufferTarget.ElementArrayBuffer));
                GL.BindVertexArray(0);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

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
