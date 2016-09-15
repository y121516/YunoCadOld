using System;
using System.Collections.Generic;
using System.Linq;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public static class Primitive
    {
        public static MC.PriTriple Create(int layerLink, int objectLink, int primitiveLink) => new MC.PriTriple(layerLink, objectLink, primitiveLink);
        public static MC.PriTriple Create(MC.ObjPair objPair, int primitiveLink) => new MC.PriTriple(objPair.llink, objPair.vlink, primitiveLink);

        public static MC.ObjPair ParentObject(this MC.PriTriple priTriple) => new MC.ObjPair(priTriple.llink, priTriple.vlink);

        public static void Delete(this MC.PriTriple priTriple) => MC.DeletePrimitive(priTriple.llink, priTriple.vlink, priTriple.plink);

        public static CurrentPrimitive GetCurrentPrimitive()
        {
            var priType = "";
            MC.GetCurPriType(out priType);
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

        public static TResult MakeCurrent<TResult>(this MC.PriTriple priTriple, Func<CurrentPrimitive, TResult> func)
        {
            MC.CurPrimitive(priTriple.llink, priTriple.vlink, priTriple.plink);
            return func(GetCurrentPrimitive());
        }

        public static void MakeCurrent(this MC.PriTriple priTriple, Action<CurrentPrimitive> action)
        {
            MC.CurPrimitive(priTriple.llink, priTriple.vlink, priTriple.plink);
            action(GetCurrentPrimitive());
        }

        public static CurrentPrimitive MakeCurrentPrimitive(this MC.PriTriple priTriple)
        {
            MC.CurPriLink(priTriple.plink);
            return GetCurrentPrimitive();
        }

        public static void Set(this MC.PriTriple priTriple)
        {
            MC.SetPrimitive(priTriple.llink, priTriple.vlink, priTriple.plink);
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
            if (MC.PrimScan(scanEH))
            {
                do
                {
                    yield return Primitive.GetCurrentPrimitive();
                } while (MC.PrimNext());
            }
        }

        public int Count => MC.GetPriCount();
    }

    public class CurrentPrimitive
    {
        internal static CurrentPrimitive Instance { get; } = new CurrentPrimitive();

        internal CurrentPrimitive() { }

        public MC.PriTriple Link
        {
            get
            {
                var lay = MC.GetCurLayLink();
                var obj = MC.GetCurObjLink();
                var pri = MC.GetCurPriLink();
                return new MC.PriTriple(lay, obj, pri);
            }
        }

        public int LayerLink => MC.GetCurLayLink();
        public int ObjectLink => MC.GetCurObjLink();
        public int PrimitiveLink => MC.GetCurPriLink();

        public Tuple<MC.Vector, MC.Vector> Extent3D
        {
            get
            {
                MC.Vector pt1, pt2;
                MC.GetCurPri3DExtent(out pt1, out pt2);
                return Tuple.Create(pt1, pt2);
            }
        }

        /// <summary>
        /// 面積を返します。カレントのプリミティブが閉じたプリミティブでない場合は、値は0です。
        /// ３次元の線の面積は現在のXY平面に投影させて測定されます。
        /// </summary>
        public double Area
        {
            get
            {
                double area;
                MC.GetCurPriArea(out area);
                return area;
            }
        }

        public string LineStyle
        {
            get
            {
                var lineStyle = "";
                MC.GetCurPriLinestyle(out lineStyle);
                return lineStyle;
            }
            set { MC.CurPriLinestyle(value); }
        }

        // TODO: 呼び出すAPIの仕様の確認が必要
        public string Style
        {
            // プリミティブの種類（線、テキスト、クランプ）に応じて、カレントのプリミティブの線種、文字種、マテリアルのいずれかを設定します。
            get
            {
                string style;
                MC.GetCurPriStyle(out style);
                return style;
            }
            // カレントのプリミティブに設定されている、線種、文字種、マテリアルのいずれかを返します。
            set { MC.CurPriStyle(value); }
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
                MC.GetCurPriType(out type);
                return type;
            }
        }

        public void Move(MC.Vector from, MC.Vector moveTo, bool copy = false, double byScale = 1, double radianRotation = 0)
            => MC.CurPriMove(copy, from, moveTo, byScale, radianRotation);

        const string DefaultPhasesStatusEHVI = "";

        public int PhasesWith(int[] phases, int maxPhases, string statusEHVI = DefaultPhasesStatusEHVI)
        {
            return MC.GetCurPriPhases(statusEHVI, maxPhases, phases);
        }

        public int PhasesWith(int[] phases, string statusEHVI = DefaultPhasesStatusEHVI)
        {
            return PhasesWith(phases, phases.Length, statusEHVI);
        }

        public void Rotate(MC.Vector radianOrient) => MC.CurPriRotate(radianOrient);

        public CurrentDocument Reset()
        {
            MC.ResetPrim();
            return CurrentDocument.Instance;
        }

        public bool IsMirroed => MC.IsCurPriMirrored();
        public bool IsReadOnlyInstance => MC.IsCurPrimReadonly() == M.Primitive.ReadOnly;
        public bool IsSelected => MC.IsCurPriSelected();

        public void Transform(ref MC.Axes from, ref MC.Axes moveTo, bool copy = false)
            => MC.TransformCurPrimitive(copy, ref from, ref moveTo);

        public void SelectRemove()
            => MC.SelectRemove();
    }

    // 線、フォト、ラスター、OLE
    public class CurrentLineOrPhotePrimitive : CurrentPrimitive
    {
        public void Polyline(int nPoints, MC.Vector[] pointArray)
            => MC.CurPriPolyline(nPoints, pointArray);

        public void Polyline(int nPoints, MC.Vector[] pointArray, double[] bulgeArray, MC.Vector[] axisArray)
            => MC.CurPriPolyline(nPoints, pointArray, bulgeArray, axisArray);

        /// <param name="options">"method=vertices" または "method=midpoints"</param>
        public void SmoothLine(bool copy = false, string options = "method=vertices")
            => MC.CurPriSmoothLine(copy, options);

        public void Trim(MC.Vector fromPos, MC.Vector toPos)
            => MC.CurPriTrim(fromPos, toPos);

        public Tuple<double, MC.Vector> GetBulge(int i)
        {
            MC.Vector axis;
            double bulge;
            MC.GetCurPriBulge(i, out axis, out bulge);
            return Tuple.Create(bulge, axis);
        }

        public double Length
        {
            get
            {
                double length;
                MC.GetCurPriLen(out length);
                return length;
            }
        }

        public int NumberOfPoints => MC.GetCurPriNP();

        public MC.Vector Pt(int i)
        {
            MC.Vector vec;
            MC.GetCurPriPt(i, out vec);
            return vec;
        }

        public MC.Vector[] Pts()
        {
            var np = NumberOfPoints;
            var points = new MC.Vector[np];
            MC.GetCurPriPts(np, points);
            return points;
        }

        public Tuple<MC.Vector[], double[], MC.Vector[]> PolylinePts(int start, int nPoints, MC.Vector[] pointArray, double[] bulgeArray, MC.Vector[] axisArray)
        {
            MC.GetPolylinePts(start, nPoints, pointArray, bulgeArray, axisArray);
            return Tuple.Create(pointArray, bulgeArray, axisArray);
        }

        /// <param name="whereAdd">頂点を追加する線分（1～）、あるいはVertexAt.Start、VertexAt.Endのいずれか</param>
        public void VertexAdd(int whereAdd, MC.Vector pos, bool redraw = true)
            => MC.VertexAdd(whereAdd, pos, redraw);

        public void VertexDelete(int vertex, bool redraw = true)
            => MC.VertexDelete(vertex, redraw);

        public void VertexMove(int vertex, MC.Vector pos, bool redraw = true)
            => MC.VertexMove(vertex, pos, redraw);
    }

    public class CurrentClumpPrimitive : CurrentPrimitive
    {
        protected CurrentClumpPrimitive() { }

        public double Smooth
        {
            get
            {
                double smooth;
                MC.GetCurPriSmooth(out smooth);
                return smooth;
            }
            set { MC.CurPriSmooth(value); }
        }

        public double SurfArea
        {
            get
            {
                double area;
                MC.GetCurPriSurfArea(out area);
                return area;
            }
        }
    }

    public class CurrentClumpMeshPrimitive : CurrentClumpPrimitive
    {
        internal new static CurrentClumpMeshPrimitive Instance { get; } = new CurrentClumpMeshPrimitive();

        CurrentClumpMeshPrimitive() { }
    }

    public class CurrentClumpSolidPrimitive : CurrentClumpPrimitive
    {
        internal new static CurrentClumpSolidPrimitive Instance { get; } = new CurrentClumpSolidPrimitive();

        CurrentClumpSolidPrimitive() { }

        public double Volume
        {
            get
            {
                double volume;
                MC.GetCurPriVolume(out volume);
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
                MC.GetCurPriExpandedText(out text);
                return text;
            }
        }

        public string FormattedText(string options = DefaultGetFormattedTextOptions)
        {
            var text = "";
            MC.GetCurPriFormattedText(out text, options);
            return text;
        }

        public void FormattedText(string text, string options = DefaultSetFormattedTextOptions)
        {
            MC.CurPriFormattedText(text, options);
        }

        public string Text
        {
            get
            {
                var text = "";
                MC.GetCurPriText(out text);
                return text;
            }
            set { MC.CurPriText(value); }
        }

        public Tuple<MC.Axes, double> Axes
        {
            get
            {
                MC.Axes axes;
                double yFactor;
                MC.GetCurPriTextAxes(out axes, out yFactor);
                return Tuple.Create(axes, yFactor);
            }
            set { MC.CurPriTextAxes(value.Item1, value.Item2); }
        }

        public TextProperty TextProperty { get; } = TextProperty.Instance;
        

        /// <summary>
        /// カレントのテキストプリミティブの1行のサイズ。サイズが設定されていない場合は、0。
        /// </summary>
        public double Wrap
        {
            get
            {
                double wrap;
                MC.GetCurPriWrap(out wrap);
                return wrap;
            }
            set { MC.CurPriWrap(value); }
        }
    }

    public class TextProperty
    {
        internal static TextProperty Instance { get; } = new TextProperty();
        private TextProperty() { }

        public string this[string index]
        {
            get
            {
                string value;
                MC.GetCurPriTextProperty(index, out value);
                return value;
            }
            set { MC.CurPriTextProperty($"{index}={value}"); }
        }

        /// <summary>
        /// 次のいずれか。
        /// "Horizontal" (横書き), "VerticalRightToLeft" (縦書き・右から左へ), "VerticalLeftToRight" (縦書き・左から右へ)
        /// </summary>
        public string Direction
        {
            get
            {
                return this["DIRECTION"];
            }
            set
            {
                this["DIRECTION"] = value;
            }
        }

        /// <summary>
        /// テキストの囲み線の線種。空白の文字列を指定すると、線種は設定されません。
        /// </summary>
        public string LineStyle
        {
            get
            {
                return this["LINESTYLE"];
            }
            set
            {
                this["LINESTYLE"] = value;
            }
        }

        /// <summary>
        /// 調整点のみをスナップできるようにするかどうかの設定。YesあるいはNo
        /// </summary>
        public string Point
        {
            get
            {
                return this["POINT"];
            }
            set
            {
                this["POINT"] = value;
            }
        }

        /// <summary>
        /// 調整点は、垂直方向の調整点と水平方向の調整点の位置を示す２文字で表されます。
        /// 最初の文字 T（Top）、C（Centre）、B（Bottom）のいずれか
        /// ２番目の文字 L（Left）、C（Centre）、R（Right）のいずれか
        /// </summary>
        public string Justification
        {
            get
            {
                return this["JUSTIFICATION"];
            }
            set
            {
                this["JUSTIFICATION"] = value;
            }
        }
    }

    public class CurrentLinePrimitive : CurrentLineOrPhotePrimitive
    {
        internal new static CurrentLinePrimitive Instance { get; } = new CurrentLinePrimitive();

        CurrentLinePrimitive() { }

        public void Break(MC.Vector atPos) => MC.CurPriBreak(atPos);

        public void Glue(MC.PriTriple otherPrimitive)
            => MC.CurPriGlue(otherPrimitive.llink, otherPrimitive.vlink, otherPrimitive.plink);

        public void Intersect(MC.Vector fromPos, MC.PriTriple otherPrimitive, MC.Vector toPos)
            => MC.CurPriIntersect(fromPos, otherPrimitive.llink, otherPrimitive.vlink, otherPrimitive.plink, toPos);

        public void Join(MC.Vector fromPos, MC.PriTriple otherPrimitive, MC.Vector toPos)
            => MC.CurPriJoin(fromPos, otherPrimitive.llink, otherPrimitive.vlink, otherPrimitive.plink, toPos);
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
