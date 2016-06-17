﻿using System;
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

        void AddImpl(IEnumerable<object> collection)
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
            if (Cad.GetSelectMode() != SelectionMode.Obj) Cad.SelectObject();
            AddImpl(currentObjects);
        }

        public void Add(IEnumerable<CurrentPrimitive> currentPrimitives)
        {
            if (Cad.GetSelectMode() != SelectionMode.Prim) Cad.SelectPrim();
            AddImpl(currentPrimitives);
        }

        public Selection Align()
        {
            Cad.AlignSelection();
            return Instance;
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

        public void DeselectAll()
        {
            Cad.DeselectAll();
        }
    }
}
