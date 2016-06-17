using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;
using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public static class Primitive
    {
        public static PriTriple Create(int layerLink, int objectLink, int primitiveLink) => new PriTriple(layerLink, objectLink, primitiveLink);
        public static PriTriple Create(ObjPair objPair, int primitiveLink) => new PriTriple(objPair.llink, objPair.vlink, primitiveLink);

        public static ObjPair ParentObject(this PriTriple priTriple) => new ObjPair(priTriple.llink, priTriple.vlink);

        public static void Delete(this PriTriple priTriple) => DeletePrimitive(priTriple.llink, priTriple.vlink, priTriple.plink);

        public static CurrentPrimitive GetCurrentPrimitive()
        {
            var priType = "";
            GetCurPriType(out priType);
            switch (priType)
            {
                case "CLUMP MESH": return CurrentClumpMeshPrimitive.Instance;
                case "CLUMP SOLID": return CurrentClumpSolidPrimitive.Instance;
                case "TEXT": return CurrentTextPrimitive.Instance;
                case "LINE": return CurrentLinePrimitive.Instance;
                case "OLE": return CurrentOlePrimitive.Instance;
                case "PHOTO": return CurrentPhotoPrimitive.Instance;
                case "RASTER": return CurrentRasterPrimitive.Instance;
                case "NONE": return CurrentNonePrimitive.Instance;
                case "UNKNOWN": return CurrentUnknownPrimitive.Instance;
                default: throw new Exception();
            }
        }

        public static CurrentPrimitive MakeCurrent(this PriTriple priTriple)
        {
            CurPrimitive(priTriple.llink, priTriple.vlink, priTriple.plink);
            return GetCurrentPrimitive();
        }

        public static void MakeCurrentPrimitive(this PriTriple priTriple)
        {
            CurPriLink(priTriple.plink);
        }

        public static void Set(this PriTriple priTriple)
        {
            SetPrimitive(priTriple.llink, priTriple.vlink, priTriple.plink);
            return; // TODO:
        }
    }



    public class Primitives
    {
        internal static Primitives Instance { get; } = new Primitives();

        Primitives() { }

        const string DefaultScanEH = "E";

        //カレントのレイヤ、オブジェクトは変更されません。カレントオブジェクトがアセンブリオブジェクトの場合は、プリミティブは返されません。
        public IEnumerable<CurrentPrimitive> Scan(string scanEH = DefaultScanEH)
        {
            if (Cad.PrimScan(scanEH))
            {
                do
                {
                    yield return Primitive.GetCurrentPrimitive();
                } while (Cad.PrimNext());
            }
        }
    }

    public class CurrentPrimitive
    {
        internal static CurrentPrimitive Instance { get; } = new CurrentPrimitive();

        internal CurrentPrimitive() { }

        public PriTriple Link
        {
            get
            {
                var lay = GetCurLayLink();
                var obj = GetCurObjLink();
                var pri = GetCurPriLink();
                return new PriTriple(lay, obj, pri);
            }
        }

        public int LayerLink => GetCurLayLink();
        public int ObjectLink => GetCurObjLink();
        public int PrimitiveLink => GetCurPriLink();

        // 閉じたプリミティブ
        public double Area
        {
            get
            {
                double area;
                GetCurPriArea(out area);
                return area;
            }
        }

        public string LineStyle
        {
            get
            {
                var lineStyle = "";
                GetCurPriLinestyle(out lineStyle);
                return lineStyle;
            }
            set { CurPriLinestyle(value); }
        }

        /// <summary>
        /// 次のいずれか。
        /// "CLUMP MESH", "CLUMP SOLID", "TEXT", "LINE", "OLE", "PHOTO", "RASTER", "NONE", "UNKNOWN"
        /// </summary>
        public string Type
        {
            get
            {
                var type = "";
                GetCurPriType(out type);
                return type;
            }
        }

        public void Move(Vector from, Vector moveTo, bool copy = false, double byScale = 1, double radianRotation = 0)
            => CurPriMove(copy, from, moveTo, byScale, radianRotation);

        public void Rotate(Vector radianOrient) => CurPriRotate(radianOrient);

        public CurrentDocument Reset()
        {
            Cad.ResetPrim();
            return CurrentDocument.Instance;
        }



        // todo: text primitive only?
        public double Wrap
        {
            get
            {
                double wrap;
                GetCurPriWrap(out wrap);
                return wrap;
            }
            set { CurPriWrap(value); }
        }

        public bool IsMirroed => IsCurPriMirrored();
        public bool IsReadOnlyInstance => IsCurPrimReadonly() == Informatix.MGDS.Primitive.ReadOnly;
        public bool IsSelected => IsCurPriSelected();

        public void Transform(ref Axes from, ref Axes moveTo, bool copy = false)
            => TransformCurPrimitive(copy, ref from, ref moveTo);
    }

    public class CurrentLineOrPhotePrimitive : CurrentPrimitive
    {
        public void Polyline(int nPoints, Vector[] pointArray)
            => CurPriPolyline(nPoints, pointArray);

        public void Polyline(int nPoints, Vector[] pointArray, double[] bulgeArray, Vector[] axisArray)
            => CurPriPolyline(nPoints, pointArray, bulgeArray, axisArray);

        /// <param name="options">"method=vertices" または "method=midpoints"</param>
        public void SmoothLine(bool copy = false, string options = "method=vertices")
            => CurPriSmoothLine(copy, options);

        public void Trim(Vector fromPos, Vector toPos)
            => CurPriTrim(fromPos, toPos);

        public Tuple<double, Vector> GetBulge(int i)
        {
            Vector axis;
            double bulge;
            GetCurPriBulge(i, out axis, out bulge);
            return Tuple.Create(bulge, axis);
        }

        public double Length
        {
            get
            {
                double length;
                GetCurPriLen(out length);
                return length;
            }
        }

        public int NumberOfPoints => GetCurPriNP();

        public Vector Pt(int i)
        {
            Vector vec;
            GetCurPriPt(i, out vec);
            return vec;
        }

        public Vector[] Pts()
        {
            var np = NumberOfPoints;
            var points = new Vector[np];
            GetCurPriPts(np, points);
            return points;
        }

        public Tuple<Vector[], double[], Vector[]> PolylinePts(int start, int nPoints, Vector[] pointArray, double[] bulgeArray, Vector[] axisArray)
        {
            GetPolylinePts(start, nPoints, pointArray, bulgeArray, axisArray);
            return Tuple.Create(pointArray, bulgeArray, axisArray);
        }

        /// <param name="whereAdd">頂点を追加する線分（1～）、あるいはVertexAt.Start、VertexAt.Endのいずれか</param>
        public void VertexAdd(int whereAdd, Vector pos, bool redraw = true)
            => Cad.VertexAdd(whereAdd, pos, redraw);

        public void VertexDelete(int vertex, bool redraw = true)
            => Cad.VertexDelete(vertex, redraw);

        public void VertexMove(int vertex, Vector pos, bool redraw = true)
            => Cad.VertexMove(vertex, pos, redraw);
    }

    public class CurrentClumpMeshPrimitive : CurrentPrimitive
    {
        internal new static CurrentClumpMeshPrimitive Instance { get; } = new CurrentClumpMeshPrimitive();

        CurrentClumpMeshPrimitive() { }

        public double Smooth
        {
            get
            {
                double smooth;
                GetCurPriSmooth(out smooth);
                return smooth;
            }
            set { CurPriSmooth(value); }
        }
    }

    public class CurrentClumpSolidPrimitive : CurrentPrimitive
    {
        internal new static CurrentClumpSolidPrimitive Instance { get; } = new CurrentClumpSolidPrimitive();

        CurrentClumpSolidPrimitive() { }

        public double Volume
        {
            get
            {
                double volume;
                GetCurPriVolume(out volume);
                return volume;
            }
        }
    }

    public class CurrentTextPrimitive : CurrentPrimitive
    {
        internal new static CurrentTextPrimitive Instance { get; } = new CurrentTextPrimitive();

        CurrentTextPrimitive() { }

        const string DefaultGetFormattedTextOptions = "";
        const string DefaultSetFormattedTextOptions = "";

        public string ExpandedText
        {
            get
            {
                var text = "";
                Cad.GetCurPriExpandedText(out text);
                return text;
            }
        }

        public string FormattedText(string options = DefaultGetFormattedTextOptions)
        {
            var text = "";
            Cad.GetCurPriFormattedText(out text, options);
            return text;
        }

        public void FormattedText(string text, string options = DefaultSetFormattedTextOptions)
        {
            Cad.CurPriFormattedText(text, options);
        }

        public string Text
        {
            get
            {
                var text = "";
                GetCurPriText(out text);
                return text;
            }
            set { CurPriText(value); }
        }

        public Tuple<Axes, double> Axes
        {
            get
            {
                Axes axes;
                double yFactor;
                GetCurPriTextAxes(out axes, out yFactor);
                return Tuple.Create(axes, yFactor);
            }
            set { CurPriTextAxes(value.Item1, value.Item2); }
        }

        public string TextPropertyDirection()
        {
            var value = "";
            GetCurPriTextProperty("DIRECTION", out value);
            return value;
        }

        public string TextPropertyLinestyle()
        {
            var value = "";
            GetCurPriTextProperty("LINESTYLE", out value);
            return value;
        }

        public string TextPropertyPoint()
        {
            var value = "";
            GetCurPriTextProperty("POINT", out value);
            return value;
        }

        public string TextPropertyJustification()
        {
            var value = "";
            GetCurPriTextProperty("JUSTIFICATION", out value);
            return value;
        }

        public void TextProperty(string options)
        {
            CurPriTextProperty(options);
        }
    }

    public class CurrentLinePrimitive : CurrentLineOrPhotePrimitive
    {
        internal new static CurrentLinePrimitive Instance { get; } = new CurrentLinePrimitive();

        CurrentLinePrimitive() { }

        public void Break(Vector atPos) => CurPriBreak(atPos);

        public void Glue(PriTriple otherPrimitive)
            => CurPriGlue(otherPrimitive.llink, otherPrimitive.vlink, otherPrimitive.plink);

        public void Intersect(Vector fromPos, PriTriple otherPrimitive, Vector toPos)
            => CurPriIntersect(fromPos, otherPrimitive.llink, otherPrimitive.vlink, otherPrimitive.plink, toPos);

        public void Join(Vector fromPos, PriTriple otherPrimitive, Vector toPos)
            => CurPriJoin(fromPos, otherPrimitive.llink, otherPrimitive.vlink, otherPrimitive.plink, toPos);
    }

    public class CurrentOlePrimitive : CurrentLineOrPhotePrimitive
    {
        internal new static CurrentOlePrimitive Instance { get; } = new CurrentOlePrimitive();

        CurrentOlePrimitive() { }
    }

    public class CurrentPhotoPrimitive : CurrentLineOrPhotePrimitive
    {
        internal new static CurrentPhotoPrimitive Instance { get; } = new CurrentPhotoPrimitive();

        CurrentPhotoPrimitive() { }
    }

    public class CurrentRasterPrimitive : CurrentLineOrPhotePrimitive
    {
        internal new static CurrentRasterPrimitive Instance { get; } = new CurrentRasterPrimitive();

        CurrentRasterPrimitive() { }
    }

    public class CurrentNonePrimitive : CurrentPrimitive
    {
        internal new static CurrentNonePrimitive Instance { get; } = new CurrentNonePrimitive();

        CurrentNonePrimitive() { }
    }

    public class CurrentUnknownPrimitive : CurrentPrimitive
    {
        internal new static CurrentUnknownPrimitive Instance { get; } = new CurrentUnknownPrimitive();

        CurrentUnknownPrimitive() { }
    }

    public static class CurrentPrimitivesExtension
    {
        public static IEnumerable<CurrentClumpMeshPrimitive> OfClumpMesh(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentClumpMeshPrimitive>();
        public static IEnumerable<CurrentClumpSolidPrimitive> OfClumpSolid(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentClumpSolidPrimitive>();
        public static IEnumerable<CurrentTextPrimitive> OfText(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentTextPrimitive>();
        public static IEnumerable<CurrentLinePrimitive> OfLine(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentLinePrimitive>();
        public static IEnumerable<CurrentOlePrimitive> OfOle(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentOlePrimitive>();
        public static IEnumerable<CurrentPhotoPrimitive> OfPhoto(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentPhotoPrimitive>();
        public static IEnumerable<CurrentRasterPrimitive> OfRaster(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentRasterPrimitive>();
        public static IEnumerable<CurrentNonePrimitive> OfNone(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentNonePrimitive>();
        public static IEnumerable<CurrentUnknownPrimitive> OfUnknown(this IEnumerable<CurrentPrimitive> currentPrimitives)
            => currentPrimitives.OfType<CurrentUnknownPrimitive>();
    }
}
