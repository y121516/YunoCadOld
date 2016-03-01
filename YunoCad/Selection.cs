using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;

namespace YunoCad
{
    public class Selection
    {
        public static Selection Instance { get; } = new Selection();

        Selection() { }

        public SelectionMode Mode
        {
            get { return Cad.GetSelectMode(); }
            set
            {
                switch (value)
                {
                    case SelectionMode.Obj:
                        Cad.SelectObject();
                        break;
                    case SelectionMode.Prim:
                        Cad.SelectPrim();
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        void AddHelper(IEnumerable<object> collection)
        {
            foreach (var item in collection)
            {
                try
                {
                    Cad.SelectAdd();
                }
                catch (ApiException ex)
                {
                    if (ex.ErrorOccurred(AppErrorType.MGDS, AppError.AlreadySelected)) return;
                    throw;
                }
            }
        }

        public void Add(IEnumerable<CurrentObject> currentObjects)
        {
            Cad.SelectObject();
            AddHelper(currentObjects);
        }

        public void Add(IEnumerable<CurrentPrimitive> currentPrimitives)
        {
            Cad.SelectPrim();
            AddHelper(currentPrimitives);
        }

        public void Align()
        {
            Cad.AlignSelection();
        }

        // clipboard
        public void CopyToClipboard()
        {
            Cad.CopySelection();
        }

        public void CutToClipboard()
        {
            Cad.CutSelection();
        }

        public void Delete()
        {
            Cad.DeleteSelection();
        }

        public void Deselect()
        {
            Cad.DeselectAll();
        }
    }
}
