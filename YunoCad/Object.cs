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
                double area;
                MC.GetCurObjArea(out area);
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
                MC.Vector orient;
                MC.GetCurObjAxes(out orient);
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
                MC.Axes axes;
                MC.GetObjectAxes(out axes);
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
                double xlo, ylo, xhi, yhi;
                MC.GetCurObjExtent(out xlo, out ylo, out xhi, out yhi);
                return Tuple.Create(xlo, ylo, xhi, yhi);
            }
        }

        public Tuple<MC.Vector, MC.Vector> Extent3D
        {
            get
            {
                MC.Vector pt1, pt2;
                MC.GetCurObj3DExtent(out pt1, out pt2);
                return Tuple.Create(pt1, pt2);
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

        public double Length
        {
            get
            {
                double length;
                MC.GetCurObjLen(out length);
                return length;
            }
        }

        public string Light
        {
            get
            {
                string light;
                MC.GetCurObjLight(out light);
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
                string name;
                MC.GetCurObjName(out name);
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
                double scale;
                MC.GetCurObjScale(out scale);
                return scale;
            }
        }

        public double SurfArea
        {
            get
            {
                double area;
                MC.GetCurObjSurfArea(out area);
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
                string type;
                MC.GetCurObjType(out type);
                return type;
            }
        }

        public double Volume
        {
            get
            {
                double volume;
                MC.GetCurObjVolume(out volume);
                return volume;
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
