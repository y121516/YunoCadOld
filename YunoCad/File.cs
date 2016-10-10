using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class File
    {
        internal static File Instance { get; } = new File();
        private File() { }

        public void OpenMan(string manFile, bool isReadOnly = false)
        {
            MC.OpenMANFile(manFile, isReadOnly);
        }

        public void Close(M.Save drawing = M.Save.Prompt)
        {
            MC.CloseFile(drawing);
        }
    }
}
