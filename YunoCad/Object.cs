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

        public double Area
        {
            get
            {
                MC.GetCurObjArea(out double area);
                return area;
            }
        }

        /// <summary>
        /// オブジェクトの固有の座標軸を基準とした、オブジェクトの回転の向き。
        /// </summary>
        public MC.Vector Orient
        {
            get
            {
                MC.GetCurObjAxes(out MC.Vector orient);
                return orient;
            }
            set
            {
                MC.CurObjAxes(value);
            }
        }

        public MC.Axes Axes
        {
            get
            {
                MC.GetObjectAxes(out MC.Axes axes);
                return axes;
            }
            set
            {
                MC.ObjectAxes(ref value);
            }
        }

        public Tuple<double, double, double, double> Extent
        {
            get
            {
                MC.GetCurObjExtent(out double xlo, out double ylo, out double xhi, out double yhi);
                return Tuple.Create(xlo, ylo, xhi, yhi);
            }
        }

        public Tuple<MC.Vector, MC.Vector> Extent3D
        {
            get
            {
                MC.GetCurObj3DExtent(out MC.Vector pt1, out MC.Vector pt2);
                return Tuple.Create(pt1, pt2);
            }
        }

        public void Flash() => MC.CurObjFlash();

        public MC.Vector Hook
        {
            get
            {
                MC.GetCurObjHook(out MC.Vector hook);
                return hook;
            }
            set
            {
                MC.CurObjHook(value);
            }
        }

        public bool IsVirtual() => MC.CurObjIsVirtual();

        public double Length
        {
            get
            {
                MC.GetCurObjLen(out double length);
                return length;
            }
        }

        public string Light
        {
            get
            {
                MC.GetCurObjLight(out string light);
                return light;
            }
            set
            {
                MC.CurObjLight(value);
            }
        }

        public void Move(MC.Vector from, MC.Vector moveTo, bool copy = false, double byScale = 1, double radianRotation = 0)
            => MC.CurObjMove(copy, from, moveTo, byScale, radianRotation);

        public string Name
        {
            get
            {
                MC.GetCurObjName(out string name);
                return name;
            }
            set
            {
                MC.CurObjName(value);
            }
        }

        public void Rotate(MC.Vector orient) => MC.CurObjRotate(orient);

        public double Scale
        {
            get
            {
                MC.GetCurObjScale(out double scale);
                return scale;
            }
        }

        public double SurfArea
        {
            get
            {
                MC.GetCurObjSurfArea(out double area);
                return area;
            }
        }

        /// <summary>
        /// "Plain"（通常のオブジェクト）,
        /// "Instance"（インスタンスオブジェクト）,
        /// "Assembly"（アセンブリオブジェクト）,
        /// "Instance Assembly"（インスタンスアセンブリオブジェクト）のいずれか。
        /// </summary>
        public string Type
        {
            get
            {
                MC.GetCurObjType(out string type);
                return type;
            }
        }

        public double Volume
        {
            get
            {
                MC.GetCurObjVolume(out double volume);
                return volume;
            }
        }

        public Primitives Primitives { get; } = Primitives.Instance;

        public void ResetObject()
        {
            MC.ResetObject();
        }
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
                MC.GetExtentSize(out double extent);
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
