using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace TrbMultiTool.FileFormats
{
    public class Tmdl
    {
        record Database(uint Count, uint offset);

        record SubInfoOffset(uint offset);
        //record SubInfo()

        record FileHeader(string signature, uint u, uint u2, uint u3); //TODO
        record SkeletonHeader(string fileName); //TODO
        record Skeleton(uint boneCount); //TODO
        record Materials(uint uk, uint uk2, uint uk3, uint size); //TODO
        record Collision(uint count, uint offset); //TODO
        record Header(uint uk); //TODO 36 Bytes

        record LOD_MeshInfo(uint unknown, uint vertexCount, uint faceCount, uint indicesCount, uint indicesOffset, uint vertexOffset, uint faceOffset, uint zero, uint hash, float f1, float f2, float f3, float f4);

        //static TmdlWindow tmdlWindow = new();

        public string TmdlName { get; set; }

        public List<ModelVisual3D> MVs { get; set; } = new();

        public Tmdl(List<Symb.NameEntry> nameEntry, long hdrx)
        {
            //var meshEntry = nameEntry[index + 6];
            //var splittedNameEntry = meshEntry.Name.Split('_');
            //if (splittedNameEntry.First() != "LOD0") throw new Exception("Only LOD0 is implemented currently");
            //if (splittedNameEntry[1] != "Mesh") throw new Exception("No meshes?");
            //tmdlWindow.myViewport.Children.Clear();
            //CreateMesh(hdrx, meshEntry);
            //tmdlWindow.meshName.Content = "";
            //tmdlWindow.vertices.Content = "";
            //tmdlWindow.faces.Content = "";
            //tmdlWindow.modelName.Content = $"Opened Model: {Trb._safeFileName}";
            //tmdlWindow.myViewport.Children.Clear();

            var fileHeaderEntry = nameEntry.Find(x => x.Name.Contains("FileHeader"));
            TmdlName = fileHeaderEntry.Name.Split('_').FirstOrDefault();
            Trb.SectFile.BaseStream.Seek(fileHeaderEntry.DataOffset + (uint)hdrx, System.IO.SeekOrigin.Begin);
            var fileHeader = new FileHeader(new string(Trb.SectFile.ReadChars(4)), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32());
            if (fileHeader.signature != "TMDL") return;

            var meshEntries = nameEntry.FindAll(x => x.Name.Contains("LOD0"));
            foreach (var item in meshEntries)
            {
                var meshEntry = item;
                //var splittedNameEntry = meshEntry.Name.Split('_');
                //if (splittedNameEntry.First() != "LOD0") throw new Exception("Only LOD0 is implemented currently");
                //if (splittedNameEntry[1] != "Mesh") throw new Exception("No meshes?");

                MVs.Add(CreateMesh(hdrx, meshEntry));
            }

            //tmdlWindow.Show();
        }

        private static ModelVisual3D CreateMesh(long hdrx, Symb.NameEntry meshEntry)
        {
            Trb.SectFile.BaseStream.Seek(meshEntry.DataOffset + (uint)hdrx, System.IO.SeekOrigin.Begin);
            uint lod_meshInfoCount = Trb.SectFile.ReadUInt32(); //??
            uint faceCount = Trb.SectFile.ReadUInt32();
            uint vertexCount = Trb.SectFile.ReadUInt32();
            string meshName = ReadHelper.ReadStringFromOffset(Trb.SectFile, Trb.SectFile.ReadUInt32() + (uint)hdrx);
            //tmdlWindow.meshName.Content += $"{meshName}, ";
            //tmdlWindow.vertices.Content += $"{vertexCount}, ";
            //tmdlWindow.faces.Content += $"{faceCount}, ";
            uint lodSubMeshInfoOffset = Trb.SectFile.ReadUInt32();
            Trb.SectFile.BaseStream.Seek(lodSubMeshInfoOffset + (uint)hdrx, System.IO.SeekOrigin.Begin);
            var meshInfos = new List<LOD_MeshInfo>();

            for (int i = 0; i < lod_meshInfoCount; i++)
            {
                meshInfos.Add(new LOD_MeshInfo(Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(),
                    Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(), Trb.SectFile.ReadUInt32(),
                    Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle()));
            }

            var modelGroup = new Model3DGroup();
            for (int i = 0; i < lod_meshInfoCount; i++)
            {
                var gm = new GeometryModel3D();
                var mesh = new MeshGeometry3D();
                Trb.SectFile.BaseStream.Seek(meshInfos[0].vertexOffset + (uint)hdrx, System.IO.SeekOrigin.Begin);
                for (int j = 0; j < meshInfos[i].vertexCount; j++)
                {
                    mesh.Positions.Add(new Point3D(Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle()));
                    mesh.Normals.Add(new Vector3D(Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle()));
                    Trb.SectFile.BaseStream.Seek(8, System.IO.SeekOrigin.Current);
                    mesh.TextureCoordinates.Add(new Point(Trb.SectFile.ReadSingle(), Trb.SectFile.ReadSingle()));
                }
                Trb.SectFile.BaseStream.Seek(meshInfos[i].faceOffset + (uint)hdrx, System.IO.SeekOrigin.Begin);
                int startDirection = -1;
                uint indexCounter = 2;
                int faceDirection = startDirection;

                var faceA = Trb.SectFile.ReadUInt16() + 1;
                var faceB = Trb.SectFile.ReadUInt16() + 1;
                do
                {
                    var faceC = Trb.SectFile.ReadUInt16();
                    indexCounter++;
                    if (faceC == 0xFFFF)
                    {
                        faceA = Trb.SectFile.ReadUInt16() + 1;
                        faceB = Trb.SectFile.ReadUInt16() + 1;
                        indexCounter += 2;
                        faceDirection = startDirection;
                    }
                    else
                    {
                        faceC++;
                        faceDirection *= -1;
                        if (faceA != faceB && faceB != faceC && faceC != faceA)
                        {
                            if (faceDirection > 0)
                            {
                                mesh.TriangleIndices.Add(faceA - 1); mesh.TriangleIndices.Add(faceB - 1); mesh.TriangleIndices.Add(faceC - 1);
                            }
                            else
                            {
                                mesh.TriangleIndices.Add(faceA - 1); mesh.TriangleIndices.Add(faceC - 1); mesh.TriangleIndices.Add(faceB - 1);
                            }
                        }
                        faceA = faceB;
                        faceB = faceC;
                    }
                } while ((uint)Trb.SectFile.BaseStream.Position < (meshInfos[i].faceCount * 2 + meshInfos[i].faceOffset + (uint)hdrx));

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
            //tmdlWindow.myViewport.Children.Add(modelVisual);
            return modelVisual;
        }
    }
}
