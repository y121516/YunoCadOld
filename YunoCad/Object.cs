using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;

namespace YunoCad
{
    // カレントレイヤは、カレントオブジェクトの直接の上位レイヤ（レイヤあるいはアセンブリオブジェクトを含む）に設定されます。この関数を呼び出すと、カレントのプリミティブがなくなります。
    public class CurrentObject
    {
        internal static CurrentObject Instance { get; } = new CurrentObject();

        CurrentObject() { }


        public static int Link { get { return Cad.GetCurObjLink(); } }

        public static string Name
        {
            get
            {
                var name = "";
                Cad.GetCurObjName(out name);
                return name;
            }
        }

        public Primitive.Primitives Primitives { get; } = Primitive.Primitives.Instance;

        public void ResetObject()
        {
            Cad.ResetObject();
        }
    }

    // todo: ObjectScanArea, ObjectScanLayer, ObjectScanPoly, ObjectScanPrimPoly, ObjectScanVolume などのオーバーロードを追加する。
    public struct Object
    {
        Cad.ObjPair _ObjPair;

        public Object(Cad.ObjPair objPair)
        {
            _ObjPair = objPair;
        }

        public Object(int layerLink, int objectLink)
        {
            _ObjPair.llink = layerLink;
            _ObjPair.vlink = objectLink;
        }

        static Cad.ObjPair[] SelectedImpl(int nObjs)
        {
            var objArray = new Cad.ObjPair[nObjs];
            Cad.GetObjSelections(nObjs, objArray);
            return objArray;
        }

        // 多くても atMostObjects のオブジェクトを列挙
        public static Cad.ObjPair[] Selected(int atMostObjects)
        {
            return SelectedImpl(Math.Min(atMostObjects, Cad.GetNumSelObj()));
        }

        public static Cad.ObjPair[] Selected()
        {
            return SelectedImpl(Cad.GetNumSelObj());
        }

        public class Objects
        {
            const string DefaultScanEH = "E";

            // カレントのウィンドウ定義内でのみよびだせるとよい
            public IEnumerable<CurrentObject> Scan(string scanEH = DefaultScanEH)
            {
                if (Cad.ObjectScan(scanEH))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (Cad.ObjectNext());
                }
            }
        }
    }
}
