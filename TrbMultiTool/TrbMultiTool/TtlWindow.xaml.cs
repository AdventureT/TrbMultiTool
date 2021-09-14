using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SEViewer.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
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

        private GLControl _glControl;

        private Core.Camera _camera;

        private Shader _shader;

        private Shader _shader2;

        private Stopwatch _watch;

        private bool _selected;

        private float _deltaTime;

        private TextureInfo _currentTTL;

        private bool isPalette = true;

        //public List<TreeViewItem> Lvis { get; set; } = new();

        public TtlWindow(Ttl ttl)
        {
            InitializeComponent();

            AddTtl(ttl);
        }

        // set up vertex data (and buffer(s)) and configure vertex attributes
        // ------------------------------------------------------------------
        float[] vertices = {
        // positions          // colors           // texture coords
         0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 0.0f, // top right
         0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 1.0f, // bottom right
        -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 1.0f, // bottom left
        -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 0.0f  // top left 
    };
        uint[] indices = {
        0, 1, 3, // first triangle
        1, 2, 3  // second triangle
    };
        int VBO, VAO, EBO;
        int myColorTableID, myIndexTable;

        private void Render()
        {
            //Movement();
            _glControl.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color4.FromHsv(new Vector4(1, 0.75f, 0.75f, 1)));

            if (_selected)
            {
                
                _deltaTime = (float)_watch.ElapsedTicks / Stopwatch.Frequency;

                if (isPalette)
                {
                    _shader.Use();
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, myIndexTable);
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, myColorTableID);
                }
                else
                {
                    _shader2.Use();
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, myIndexTable);
                }
                

                GL.BindVertexArray(VAO);
                //GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
                GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                //GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, 1);
                

                //Deblob has a 270° rotation on the X-axis for some reason
                //if (Trb._game == Game.DeBlob) model = Matrix4.Identity * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(270)) * Matrix4.CreateRotationZ(deltaTime);
                //else model = Matrix4.Identity * Matrix4.CreateRotationZ(deltaTime);

                //shader.SetMatrix("model", model);
                //shader.SetMatrix("view", camera.GetViewMatrix());
                //shader.SetMatrix("projection", camera.GetProjectionMatrix());

                //for (int i = 0; i < _vertexArrayObject.Count; i++)
                //{
                //    GL.BindVertexArray(_vertexArrayObject[i]);
                //    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesVBO[i].Handle);
                //    GL.DrawElements(PrimitiveType.Triangles, _indicesVBO[i].Count, DrawElementsType.UnsignedInt, 0);
                //    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                //    GL.BindVertexArray(0);
                //}


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
            _glControl.SwapBuffers();    // Display the result.
            _glControl.Invalidate();
        }

        private void ComponentDispatcher_ThreadIdle(object sender, EventArgs e)
        {
            Render();
        }

        private void GLControl_Resize(object sender, EventArgs e)
        {
            _glControl.MakeCurrent();
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
            _camera.Position = new Vector3(0, -3, 1);
            _camera.Pitch = 90;
            _camera.AspectRatio = (float)_glControl.Width / (float)_glControl.Height;
            _glControl.Invalidate();
        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            _glControl.MakeCurrent();
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color4.FromHsv(new Vector4(1, 0.75f, 0.75f, 1)));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader = Shader.Create("test", "Shaders/texture.vert", "Shaders/texture.frag");
            _shader2 = Shader.Create("test2", "Shaders/texture.vert", "Shaders/texture2.frag");
            _watch = Stopwatch.StartNew();
            _glControl.Invalidate();
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        public TtlWindow(List<Ttl> ttls, List<Ttex> ttexes)
        {
            InitializeComponent();
            _glControl = new GLControl
            {
                Dock = DockStyle.Fill
            };
            _glControl.Paint += GLControl_Paint;
            _glControl.Load += GLControl_Load;
            _glControl.Resize += GLControl_Resize;
            Host.Child = _glControl;
            _camera = new Core.Camera(Vector3.UnitZ * 3, (float)_glControl.Width / (float)_glControl.Height);

            ComponentDispatcher.ThreadIdle += new EventHandler(ComponentDispatcher_ThreadIdle);
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

        private void LoadTtl()
        {
            _glControl.MakeCurrent();
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 32 * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, 24, indices, BufferUsageHint.StaticDraw);

            // position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), new IntPtr());
            GL.EnableVertexAttribArray(0);
            // color attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), new IntPtr(3 * sizeof(float)));
            GL.EnableVertexAttribArray(1);
            // texture coord attribute
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), new IntPtr(6 * sizeof(float)));
            GL.EnableVertexAttribArray(2);

            myColorTableID = GL.GenTexture();
            myIndexTable = GL.GenTexture();
            var pf = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
            var test = new ushort[_currentTTL.RawImage.Length / 2];
            var fourBpp = new byte[_currentTTL.Width * _currentTTL.Height];
            switch (_currentTTL.textureFormat)
            {
                case TextureInfo.TextureFormat.FORMAT_RGB565:
                    break;
                case TextureInfo.TextureFormat.FORMAT_RGB5A3:
                    break;
                case TextureInfo.TextureFormat.FORMAT_Unknown:
                    break;
                case TextureInfo.TextureFormat.FORMAT_Z8:
                    break;
                case TextureInfo.TextureFormat.FORMAT_CMPR:
                    break;
                case TextureInfo.TextureFormat.FORMAT_Z16:
                    break;
                case TextureInfo.TextureFormat.FORMAT_Z24X8:
                    break;
                case TextureInfo.TextureFormat.FORMAT_INDEX4:
                    break;
                case TextureInfo.TextureFormat.FORMAT_INDEX14X2:
                    break;
                case TextureInfo.TextureFormat.FORMAT_INTENSITY4:
                    break;
                case TextureInfo.TextureFormat.FORMAT_INTENSITY4A:
                    break;
                case TextureInfo.TextureFormat.FORMAT_INTENSITY8:
                    break;
                case TextureInfo.TextureFormat.FORMAT_INTENSITY8A:
                    break;
                case TextureInfo.TextureFormat.FORMAT_UNKNOWN:
                    break;
                case TextureInfo.TextureFormat.FORMAT_RGBA8:
                    //isPalette = false;
                    break;
                case TextureInfo.TextureFormat.FORMAT_UNKNOWN3:
                    //4bpp

                    //for (int i = 0; i < _currentTTL.RawImage.Length; i += 2)
                    //{
                    //    test[i] = BitConverter.ToUInt16(new byte[] { _currentTTL.RawImage[i], _currentTTL.RawImage[i + 1] });
                    //}
                    
                    //pf = OpenTK.Graphics.OpenGL.
                    break;
                case TextureInfo.TextureFormat.FORMAT_INDEX8:

                    //for (int i = 0; i < _currentTTL.RawImage.Length; i++)
                    //{
                    //    fourBpp[i] = Convert.ToByte(_currentTTL.RawImage[i] & 0x0F);
                    //    fourBpp[i + 1] = Convert.ToByte(_currentTTL.RawImage[i] >> 4);
                    //}
                    for (int i = 0; i < _currentTTL.Pallete.Length; i += 2)
                    {
                        test[i] = BitConverter.ToUInt16(new byte[] { _currentTTL.Pallete[i], _currentTTL.Pallete[i + 1] });
                    }

                    //_currentTTL.RawImage = ;
                    break;
                default:
                    break;
            }

            if (isPalette)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, myIndexTable);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, (int)_currentTTL.Width / 2, (int)_currentTTL.Height / 2, 0, OpenTK.Graphics.OpenGL.PixelFormat.Red, PixelType.UnsignedByte, _currentTTL.RawImage.ToArray());
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, myColorTableID);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, (int)_currentTTL.PBPP * 8, 1, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, _currentTTL.Pallete.ToArray());
                _shader.Use();
                _shader.SetInt("MyIndexTexture", 0);
                _shader.SetInt("ColorTable", 1);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, myIndexTable);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)_currentTTL.Width, (int)_currentTTL.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, _currentTTL.RawImage.ToArray());
                _shader2.Use();
                _shader.SetInt("texture", 0);
                isPalette = !isPalette;
            }


            

            //if (tvi.Tag is Ttex) { LoadTtex(tvi); return; }
            //if (Trb._game == Game.NicktoonsUnite)
            //{
            //    var bitmap = ((TextureInfo)tvi.Tag).Bitmap;
            //    img.Source = Ttl.LoadBitmap(bitmap);
            //    img.Width = bitmap.Width;
            //    img.Height = bitmap.Height;
            //}
            //else
            //{
            //    var dds = ((TextureInfo)tvi.Tag).Dds;
            //    img.Source = Ttl.LoadBitmap(dds.BitmapImage);
            //    img.Width = dds.BitmapImage.Width;
            //    img.Height = dds.BitmapImage.Height;
            //}
            _selected = true;
        }

        private void LoadTtex(TreeViewItem tvi)
        {
            //var dds = ((Ttex)tvi.Tag).DDS;
            //img.Source = Ttl.LoadBitmap(dds.BitmapImage);
            //img.Width = dds.BitmapImage.Width;
            //img.Height = dds.BitmapImage.Height;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sI = (TreeViewItem)treeView.SelectedItem;
            if (sI.Tag is Ttl) return;
            _currentTTL = sI.Tag as TextureInfo;
            
            LoadTtl();
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
            if (treeView.SelectedItem == null)
            {
                System.Windows.Forms.MessageBox.Show("Choose your Texture to replace", "Choose a Texture", MessageBoxButtons.OK);
                return;
            }
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
            var fd = new Microsoft.Win32.OpenFileDialog();
            fd.Filter = $"DDS File (*.dds)|*.dds";

            if (fd.ShowDialog() == true)
            {
                var fd2 = new Microsoft.Win32.SaveFileDialog();
                fd2.Filter = $"Trb File (*.trb)|*.trb";
                
                if (fd2.ShowDialog() == true)
                {
                    var ms = Ttex.FromFile(fd.FileName);
                    Trb.AppendFile(fd2.FileName, ms, (uint)ms.Length, new() { 0x4, 0x8, 0x10 }, "ttex\0");
                    System.Windows.Forms.MessageBox.Show("Your Trb file has been repacked", "Repacked", MessageBoxButtons.OK);
                }
            }
        }
    }
}
