using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrbMultiTool
{
    public class TSFLFileDialog
    {
        private OpenFileDialog dialog;
        public string fileName;

        public TSFLFileDialog()
        {
            dialog = new OpenFileDialog
            {
                Filter = "Toshi Files (*.trb, *.ttl, *.trz)|*.trb;*.ttl;*.trz|TRB Files (*.trb)|*.trb|TTL files (*.ttl)|*.ttl|TRZ files (*.trz)|*.trz",
                Title = "Select a file"
            };
        }

        public bool Open()
        {
            bool status = dialog.ShowDialog() == DialogResult.OK;
            fileName = dialog.FileName;

            return status;
        }
    }
}
