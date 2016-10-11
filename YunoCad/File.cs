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

        public void Close(M.Save drawing = M.Save.Prompt)
        {
            MC.CloseFile(drawing);
        }

        public void Create()
        {
            MC.CreateFile();
        }

        public void CreateMan()
        {
            MC.CreateMANFile();
        }

        public void Import(string fileName, M.ConfirmFileImport prompt)
        {
            MC.ImportFile(fileName, prompt);
        }

        public void Open(string fileName)
        {
            MC.OpenFile(fileName);
        }

        public void OpenMan(string manFile, bool isReadOnly = false)
        {
            MC.OpenMANFile(manFile, isReadOnly);
        }
    }
}
