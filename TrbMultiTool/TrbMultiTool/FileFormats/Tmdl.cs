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
    class Tmdl
    {
        record FileHeader(string signature, uint u, uint u2, uint u3); //TODO
        record SkeletonHeader(string fileName); //TODO
        record Skeleton(uint boneCount); //TODO
        record Materials(uint uk, uint uk2, uint uk3, uint size); //TODO
        record Collision(uint uk); //TODO
        record Header(uint uk); //TODO 36 Bytes

        record LOD_MeshInfo(uint unknown, uint vertexCount, uint faceCount, uint indicesCount, uint indicesOffset, uint vertexOffset, uint faceOffset, uint zero, uint hash, float f1, float f2, float f3, float f4);

        static TmdlWindow tmdlWindow = new();

        public Tmdl(List<Symb.NameEntry> nameEntry, int index, long hdrx)
        {
            //var meshEntry = nameEntry[index + 6];
            //var splittedNameEntry = meshEntry.Name.Split('_');
            //if (splittedNameEntry.First() != "LOD0") throw new Exception("Only LOD0 is implemented currently");
            //if (splittedNameEntry[1] != "Mesh") throw new Exception("No meshes?");
            //tmdlWindow.myViewport.Children.Clear();
            //CreateMesh(hdrx, meshEntry);
            tmdlWindow.modelName.Content = $"Opened Model: {Trb._safeFileName}";
            var copy = nameEntry;
            copy.RemoveRange(0, index);
            tmdlWindow.myViewport.Children.Clear();

            var meshEntries = copy.FindAll(x => x.Name.Contains("LOD0"));
            foreach (var item in meshEntries)
            {
                var meshEntry = item;
                var splittedNameEntry = meshEntry.Name.Split('_');
                if (splittedNameEntry.First() != "LOD0") throw new Exception("Only LOD0 is implemented currently");
                if (splittedNameEntry[1] != "Mesh") throw new Exception("No meshes?");

                CreateMesh(hdrx, meshEntry);
            }

            tmdlWindow.Show();
        }

        private static ModelVisual3D CreateMesh(long hdrx, Symb.NameEntry meshEntry)
        {
            Trb._f.BaseStream.Seek(meshEntry.DataOffset + (uint)hdrx, System.IO.SeekOrigin.Begin);
            uint lod_meshInfoCount = Trb._f.ReadUInt32(); //??
            uint faceCount = Trb._f.ReadUInt32();
            uint vertexCount = Trb._f.ReadUInt32();
            string meshName = ReadHelper.ReadStringFromOffset(Trb._f.ReadUInt32() + (uint)hdrx);
            tmdlWindow.meshName.Content += $"{meshName}, ";
            tmdlWindow.vertices.Content += $"{vertexCount}, ";
            tmdlWindow.faces.Content += $"{faceCount}, ";
            uint lodSubMeshInfoOffset = Trb._f.ReadUInt32();
            Trb._f.BaseStream.Seek((uint)hdrx + lodSubMeshInfoOffset, System.IO.SeekOrigin.Begin);
            var meshInfos = new List<LOD_MeshInfo>();

            for (int i = 0; i < lod_meshInfoCount; i++)
            {
                meshInfos.Add(new LOD_MeshInfo(Trb._f.ReadUInt32(), Trb._f.ReadUInt32(), Trb._f.ReadUInt32(), Trb._f.ReadUInt32(), Trb._f.ReadUInt32(),
                    Trb._f.ReadUInt32(), Trb._f.ReadUInt32(), Trb._f.ReadUInt32(), Trb._f.ReadUInt32(),
                    Trb._f.ReadSingle(), Trb._f.ReadSingle(), Trb._f.ReadSingle(), Trb._f.ReadSingle()));
            }

            var modelGroup = new Model3DGroup();
            for (int i = 0; i < lod_meshInfoCount; i++)
            {
                var gm = new GeometryModel3D();
                var mesh = new MeshGeometry3D();
                Trb._f.BaseStream.Seek((uint)hdrx + meshInfos[0].vertexOffset, System.IO.SeekOrigin.Begin);
                for (int j = 0; j < meshInfos[i].vertexCount; j++)
                {
                    mesh.Positions.Add(new Point3D(Trb._f.ReadSingle(), Trb._f.ReadSingle(), Trb._f.ReadSingle()));
                    mesh.Normals.Add(new Vector3D(Trb._f.ReadSingle(), Trb._f.ReadSingle(), Trb._f.ReadSingle()));
                    Trb._f.BaseStream.Seek(8, System.IO.SeekOrigin.Current);
                    mesh.TextureCoordinates.Add(new Point(Trb._f.ReadSingle(), -Trb._f.ReadSingle()));
                }
                Trb._f.BaseStream.Seek((uint)hdrx + meshInfos[i].faceOffset, System.IO.SeekOrigin.Begin);
                int startDirection = -1;
                uint indexCounter = 2;
                int faceDirection = startDirection;

                var faceA = Trb._f.ReadUInt16() + 1;
                var faceB = Trb._f.ReadUInt16() + 1;
                do
                {
                    var faceC = Trb._f.ReadUInt16();
                    indexCounter++;
                    if (faceC == 0xFFFF)
                    {
                        faceA = Trb._f.ReadUInt16() + 1;
                        faceB = Trb._f.ReadUInt16() + 1;
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
                } while ((uint)Trb._f.BaseStream.Position < (meshInfos[i].faceCount * 2 + meshInfos[i].faceOffset + (uint)hdrx));

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
            tmdlWindow.myViewport.Children.Add(modelVisual);

            return modelVisual;
        }
    }
}
