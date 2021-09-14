using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace TrbMultiTool.FileFormats
{
    public class TerrainVIS
    {
        private TerrainVISWindow window;

        List<TerrainPartInfo> tParts = new();

        record UsedFiles(string tklFile, string ttlFile, string skelFile);
        
        public class TerrainPartInfo {
            public uint nameOffset;
            public string name = "";

            public uint L0ModFilePointer;
            public string L0ModFile = "";

            public uint L1ModFilePointer;
            public string L1ModFile = "";

            public uint countOfL0Mods;
            public uint countOfL1Mods;

            public uint unkOffset;
            public short unk;

            public uint unk2Offset;
            public short unk2;

            public uint collisionFileOffset;
            public string collisionFile;

            public uint unk3;
            public uint unk4;
            public uint unk5;

            public MatLibInfo L0Mat;
            public MatLibInfo L1Mat;

            public uint unk6;
            public uint unk7;
            public ushort unk8;
            public ushort unk9;
            public uint unk10;
            public uint unk11;
            public uint unk12;
            public uint unk13;
            public uint unk14;

            public List<string> L0Models = new();
            public List<string> L1Models = new();

            // 0x64 - real size of this struct
        };

        public class MatLibInfo
        {
            public uint fileOffset;
            public uint unk1;
            public uint unk2;
            public string file;

            public MatLibInfo(EndiannessAwareBinaryReader sectFile)
            {
                fileOffset = sectFile.ReadUInt32();
                unk1 = sectFile.ReadUInt32();
                unk2 = sectFile.ReadUInt32();

                file = sectFile.ReadStringFromOffset(fileOffset);
            }
        }

        public TerrainPartInfo ReadTerrainPartInfo()
        {
            var sectFile = Trb.SectFile;

            TerrainPartInfo tpInfo = new();
            tpInfo.nameOffset = sectFile.ReadUInt32();
            tpInfo.L0ModFilePointer = sectFile.ReadUInt32();
            tpInfo.L1ModFilePointer = sectFile.ReadUInt32();
            tpInfo.countOfL0Mods = sectFile.ReadUInt32();
            tpInfo.countOfL1Mods = sectFile.ReadUInt32();
            tpInfo.unkOffset = sectFile.ReadUInt32();
            tpInfo.unk2Offset = sectFile.ReadUInt32();
            tpInfo.collisionFileOffset = sectFile.ReadUInt32();
            tpInfo.unk3 = sectFile.ReadUInt32();
            tpInfo.unk4 = sectFile.ReadUInt32();
            tpInfo.unk5 = sectFile.ReadUInt32();
            tpInfo.L0Mat = new(sectFile);
            tpInfo.L1Mat = new(sectFile);
            tpInfo.unk6 = sectFile.ReadUInt32();
            tpInfo.unk7 = sectFile.ReadUInt32();
            tpInfo.unk8 = sectFile.ReadUInt16();
            tpInfo.unk9 = sectFile.ReadUInt16();
            tpInfo.unk10 = sectFile.ReadUInt32();
            tpInfo.unk11 = sectFile.ReadUInt32();
            tpInfo.unk12 = sectFile.ReadUInt32();
            tpInfo.unk13 = sectFile.ReadUInt32();
            tpInfo.unk14 = sectFile.ReadUInt32();

            long structEnd = sectFile.BaseStream.Position;

            sectFile.BaseStream.Seek(tpInfo.L0ModFilePointer, SeekOrigin.Begin);
            for (int i = 0; i < tpInfo.countOfL0Mods; i++)
            {
                string model = sectFile.ReadStringFromOffset(sectFile.ReadUInt32());
                tpInfo.L0Models.Add(model);
            }

            sectFile.BaseStream.Seek(tpInfo.L1ModFilePointer, SeekOrigin.Begin);
            for (int i = 0; i < tpInfo.countOfL1Mods; i++)
            {
                string model = sectFile.ReadStringFromOffset(sectFile.ReadUInt32());
                tpInfo.L1Models.Add(model);
            }

            tpInfo.name = sectFile.ReadStringFromOffset(tpInfo.nameOffset);
            tpInfo.collisionFile = sectFile.ReadStringFromOffset(tpInfo.collisionFileOffset);
            sectFile.BaseStream.Seek(structEnd, SeekOrigin.Begin);

            return tpInfo;
        }

        public TerrainVIS()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var sectFile = Trb.SectFile;

                UsedFiles files = new UsedFiles(
                    sectFile.ReadStringFromOffset(sectFile.ReadUInt32()),
                    sectFile.ReadStringFromOffset(sectFile.ReadUInt32()),
                    sectFile.ReadStringFromOffset(sectFile.ReadUInt32())
                );

                var unk1 = sectFile.ReadUInt32(); // it's zero in envmain.trb
                var unk2 = sectFile.ReadUInt32(); // it's zero in envmain.trb
                var unk3 = sectFile.ReadUInt32(); // it's zero in envmain.trb
                var unk4 = sectFile.ReadUInt32(); // it's zero in envmain.trb

                var countOffset = (int)sectFile.BaseStream.Position;
                var countOfTerrainParts = sectFile.ReadUInt32();

                var firstItemPointer = sectFile.ReadUInt32();

                var blockSizeOffset = (int)sectFile.BaseStream.Position;
                var blockSize = sectFile.ReadUInt32(); // memory allocated to level

                sectFile.BaseStream.Seek(firstItemPointer, SeekOrigin.Begin);

                for (int i = 0; i < countOfTerrainParts; i++)
                {
                    tParts.Add(ReadTerrainPartInfo());
                }

                window = new TerrainVISWindow();

                window.AddMainInfo(countOfTerrainParts, countOffset, blockSize, blockSizeOffset);
                window.AddTerrainParts(tParts);

                window.Show();
            });
        }
    }
}
