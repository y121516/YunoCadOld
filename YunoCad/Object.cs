using System;
using System.Collections.Generic;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
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
                MC.GetCurObjLink();
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
                var lay = MC.GetCurLayLink();
                var obj = MC.GetCurObjLink();
                return new Object(lay, obj);
            }
        }

        public int LayerLink => MC.GetCurLayLink();
        public int ObjectLink => MC.GetCurObjLink();

        /// <summary>
        /// オブジェクトの固有の座標軸を基準とした、オブジェクトの回転の向き。
        /// </summary>
        public MC.Vector Orient
        {
            get
            {
                MC.Vector orient;
                MC.GetCurObjAxes(out orient);
                return orient;
            }
            set
            {
                MC.CurObjAxes(value);
            }
        }

        public void Flash() => MC.CurObjFlash();

        public MC.Vector Hook
        {
            get
            {
                MC.Vector hook;
                MC.GetCurObjHook(out hook);
                return hook;
            }
            set
            {
                MC.CurObjHook(value);
            }
        }

        public bool IsVirtual() => MC.CurObjIsVirtual();

        public void Move(MC.Vector from, MC.Vector moveTo, bool copy = false, double byScale = 1, double radianRotation = 0)
            => MC.CurObjMove(copy, from, moveTo, byScale, radianRotation);

        public string Name
        {
            get
            {
                var name = "";
                MC.GetCurObjName(out name);
                return name;
            }
        }

        public Primitives Primitives { get; } = Primitives.Instance;

        public void ResetObject()
        {
            MC.ResetObject();
        }

        public CurrentObjectAttribute Attribute { get; } = CurrentObjectAttribute.Instance;
    }

    public struct Object
    {
        MC.ObjPair _ObjPair;

        public Object(MC.ObjPair objPair)
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
            MC.CurObject(_ObjPair.llink, _ObjPair.vlink);
            return CurrentObject.Instance;
        }

        public class Objects
        {
            const string DefaultScanEH = "E";
            const string DefaultWildcard = "**";

            public IEnumerable<CurrentObject> Scan(string scanEH = DefaultScanEH)
            {
                if (MC.ObjectScan(scanEH))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (MC.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanWildcard(string wildcard, string scanEH = DefaultScanEH)
            {
                double extent;
                MC.GetExtentSize(out extent);
                if (MC.ObjectScanArea(scanEH, wildcard, -extent, extent, extent, -extent))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (MC.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanArea(double left, double top, double right, double bottom,
                string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (MC.ObjectScanArea(scanEH, wildcard, left, top, right, bottom))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (MC.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanPoly(M.ScanPoly scanPoly, string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (MC.ObjectScanPoly(scanPoly, scanEH, wildcard))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (MC.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanPrimPoly(M.ScanPoly scanPoly, MC.PriTriple priTriple,
                string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (MC.ObjectScanPrimPoly(scanPoly, scanEH, wildcard, priTriple.llink, priTriple.vlink, priTriple.plink))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (MC.ObjectNext());
                }
            }

            public IEnumerable<CurrentObject> ScanVolume(MC.Vector pt1, MC.Vector pt2,
                string scanEH = DefaultScanEH, string wildcard = DefaultWildcard)
            {
                if (MC.ObjectScanVolume(scanEH, wildcard, pt1, pt2))
                {
                    do
                    {
                        yield return CurrentObject.Instance;
                    } while (MC.ObjectNext());
                }
            }

        }
    }
}
