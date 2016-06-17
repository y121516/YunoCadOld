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

        public Primitives Primitives { get; } = Primitives.Instance;

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

        public override string ToString() => _ObjPair.ToString();

        public CurrentObject ToCurrent()
        {
            Cad.CurObject(_ObjPair.llink, _ObjPair.vlink);
            return CurrentObject.Instance;
        }

        public class Objects
        {
            const string DefaultScanEH = "E";
            const string DefaultWildcard = "**";

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

            public IEnumerable<CurrentObject> ScanWildcard(string wildcard, string scanEH = DefaultScanEH)
            {
                double extent;
                Cad.GetExtentSize(out extent);
                if (Cad.ObjectScanArea(scanEH, wildcard, -extent, extent, extent, -extent))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (Cad.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanArea(double left, double top, double right, double bottom,
                string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (Cad.ObjectScanArea(DefaultScanEH, DefaultWildcard, left, top, right, bottom))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (Cad.ObjectNext());
                }
            }

            // TODO: レイヤオブジェクトで呼び出すようにする
            public IEnumerable<CurrentObject> ScanLayer(int layerLink, ScanMode extentType, Cad.Vector lo, Cad.Vector hi,
                string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (Cad.ObjectScanLayer(layerLink, scanEH, wildcard, extentType, lo, hi))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (Cad.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanPoly(ScanPoly scanPoly, string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (Cad.ObjectScanPoly(scanPoly, scanEH, wildcard))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (Cad.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanPrimPoly(ScanPoly scanPoly, Cad.PriTriple priTriple,
                string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (Cad.ObjectScanPrimPoly(scanPoly, scanEH, wildcard, priTriple.llink, priTriple.vlink, priTriple.plink))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (Cad.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanVolume(Cad.Vector pt1, Cad.Vector pt2,
                string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (Cad.ObjectScanVolume(scanEH, wildcard, pt1, pt2))
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
