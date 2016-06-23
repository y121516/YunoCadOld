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


        IEnumerable<Object> ObjectsImpl(int nObjs)
        {
            // GetObjSelectionsのnObjsに0以下の値を渡すと
            // 「[1000] 許容範囲外の引数が関数に指定されました。」例外を投げる。
            // 選択されていない状態でGetObjSelectionsを呼び出すと
            // 「[1053] この関数を実行するには、何らかの要素を選択することが必要です。」例外を投げる。
            // 例外を投げうる状況の場合はGetObjSelectionsを呼び出さないようにする。
            if (nObjs <= 0) return new Object[0];

            var objArray = new Cad.ObjPair[nObjs];
            Cad.GetObjSelections(nObjs, objArray);
            return objArray.Select(obj => new Object(obj));
        }

        public IEnumerable<Object> Objects(int atMostObjects)
            => ObjectsImpl(Math.Min(atMostObjects, Cad.GetNumSelObj()));

        public IEnumerable<Object> Objects()
            => ObjectsImpl(Cad.GetNumSelObj());

        Cad.PriTriple[] PrimitivesImpl(int nPrims)
        {
            var primArray = new Cad.PriTriple[nPrims];
            Cad.GetPriSelections(nPrims, primArray);
            return primArray;
        }

        public Cad.PriTriple[] Primitives(int atMostPrimitives)
            => PrimitivesImpl(Math.Min(atMostPrimitives, Cad.GetNumSelPrim()));

        public Cad.PriTriple[] Primitives()
            => PrimitivesImpl(Cad.GetNumSelPrim());


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
