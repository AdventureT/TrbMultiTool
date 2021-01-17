using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrbMultiTool.FileFormats;

namespace TrbMultiTool
{
    public class Trb
    {
        public static BinaryReader _f;
        private string _fileName;

        public Trb(string fileName)
        {
            _fileName = fileName;
            _f = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read));
            var tsfl = new Tsfl();
            _f.Close();
        }
    }
}
