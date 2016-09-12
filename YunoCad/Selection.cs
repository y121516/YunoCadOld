using System;
using System.Collections.Generic;
using System.Linq;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class Selection
    {
        public static Selection Instance { get; } = new Selection();

        Selection() { }

        public M.SelectionMode Mode
        {
            get { return MC.GetSelectMode(); }
            set
            {
                switch (value)
                {
                    case M.SelectionMode.Obj:
                        MC.SelectObject();
                        break;
                    case M.SelectionMode.Prim:
                        MC.SelectPrim();
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
            if (nObjs < 0) throw new Exception();

            var objArray = new MC.ObjPair[nObjs];
            if (nObjs > 0) MC.GetObjSelections(nObjs, objArray);
            return objArray.Select(obj => new Object(obj));
        }

        public IEnumerable<Object> Objects(int atMostObjects)
            => ObjectsImpl(Math.Min(atMostObjects, MC.GetNumSelObj()));

        public IEnumerable<Object> Objects()
            => ObjectsImpl(MC.GetNumSelObj());

        MC.PriTriple[] PrimitivesImpl(int nPrims)
        {
            if (nPrims < 0) throw new Exception();

            var primArray = new MC.PriTriple[nPrims];
            if (nPrims > 0) MC.GetPriSelections(nPrims, primArray);
            return primArray;
        }

        public MC.PriTriple[] Primitives(int atMostPrimitives)
            => PrimitivesImpl(Math.Min(atMostPrimitives, MC.GetNumSelPrim()));

        public MC.PriTriple[] Primitives()
            => PrimitivesImpl(MC.GetNumSelPrim());

        public int ObjectCount => MC.GetNumSelObj();
        public int PrimitiveCount => MC.GetNumSelPrim();

        void AddImpl(IEnumerable<object> collection)
        {
            foreach (var item in collection)
            {
                try
                {
                    MC.SelectAdd();
                }
                catch (M.ApiException ex)
                {
                    if (ex.ErrorOccurred(M.AppErrorType.MGDS, M.AppError.AlreadySelected)) return;
                    throw;
                }
            }
        }

        public void Add(IEnumerable<CurrentObject> currentObjects)
        {
            if (MC.GetSelectMode() != M.SelectionMode.Obj) MC.SelectObject();
            AddImpl(currentObjects);
        }

        public void Add(IEnumerable<CurrentPrimitive> currentPrimitives)
        {
            if (MC.GetSelectMode() != M.SelectionMode.Prim) MC.SelectPrim();
            AddImpl(currentPrimitives);
        }

        public void Remove()
        {
            MC.SelectRemove();
        }

        public Selection Align()
        {
            MC.AlignSelection();
            return Instance;
        }

        // clipboard
        public void CopyToClipboard()
        {
            MC.CopySelection();
        }

        public void CutToClipboard()
        {
            MC.CutSelection();
        }

        public void Delete()
        {
            MC.DeleteSelection();
        }

        public void DeselectAll()
        {
            MC.DeselectAll();
        }
    }
}
