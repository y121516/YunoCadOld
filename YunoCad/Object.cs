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

        public static CurrentObject Get()
        {
            try
            {
                Cad.GetCurObjLink();
            }
            catch (Exception)
            {
                return null;
            }
            return Instance;
        }

        public Object Link
        {
            get
            {
                var lay = Cad.GetCurLayLink();
                var obj = Cad.GetCurObjLink();
                return new Object(lay, obj);
            }
        }

        public int LayerLink => Cad.GetCurLayLink();
        public int ObjectLink => Cad.GetCurObjLink();

        public string Name
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

        public CurrentObjectAttribute Attribute { get; } = CurrentObjectAttribute.Instance;
    }

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

        public int LayerLink => _ObjPair.llink;
        public int ObjectLink => _ObjPair.vlink;

        public override string ToString()=> _ObjPair.ToString();

        public CurrentObject ToCurrent()
        {
            Cad.CurObject(_ObjPair.llink, _ObjPair.vlink);
            return CurrentObject.Instance;
        }

        public class Objects
        {
            const string DefaultScanEH = "E";

            // todo: ObjectScanArea, ObjectScanLayer, ObjectScanPoly, ObjectScanPrimPoly, ObjectScanVolume などのオーバーロードを追加する。
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
