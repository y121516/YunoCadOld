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

        public static CurrentPrimitive makeCurrent(this PriTriple priTriple)
        {
            CurPrimitive(priTriple.llink, priTriple.vlink, priTriple.plink);
            return GetCurrentPrimitive();
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
                var lay = Cad.GetCurLayLink();
                var obj = Cad.GetCurObjLink();
                var pri = Cad.GetCurPriLink();
                return new PriTriple(lay, obj, pri);
            }
        }

        public int LayerLink => Cad.GetCurLayLink();
        public int ObjectLink => Cad.GetCurObjLink();
        public int PrimitiveLink => Cad.GetCurPriLink();

        // 閉じたプリミティブ
        public double Area
        {
            get
            {
                double area;
                Cad.GetCurPriArea(out area);
                return area;
            }
        }

        public string LineStyle
        {
            get
            {
                var lineStyle = "";
                Cad.GetCurPriLinestyle(out lineStyle);
                return lineStyle;
            }
        }

        public string Type
        {
            get
            {
                var type = "";
                Cad.GetCurPriType(out type);
                return type;
            }
        }

        public void ResetPrimitive()
        {
            Cad.ResetPrim();
        }
    }

    public class CurrentClumpMeshPrimitive : CurrentPrimitive
    {
        internal new static CurrentClumpMeshPrimitive Instance { get; } = new CurrentClumpMeshPrimitive();

        CurrentClumpMeshPrimitive() { }
    }

    public class CurrentClumpSolidPrimitive : CurrentPrimitive
    {
        internal new static CurrentClumpSolidPrimitive Instance { get; } = new CurrentClumpSolidPrimitive();

        CurrentClumpSolidPrimitive() { }
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
                Cad.GetCurPriText(out text);
                return text;
            }
            set
            {
                Cad.CurPriText(value);
            }
        }
    }

    public class CurrentLinePrimitive : CurrentPrimitive
    {
        internal new static CurrentLinePrimitive Instance { get; } = new CurrentLinePrimitive();

        CurrentLinePrimitive() { }

        public void Break(Cad.Vector atPos)
        {
            Cad.CurPriBreak(atPos);
        }

        public void Glue(PriTriple otherPrimitive)
        {
            Cad.CurPriGlue(otherPrimitive.llink, otherPrimitive.vlink, otherPrimitive.plink);
        }

        public double Length
        {
            get
            {
                double length;
                Cad.GetCurPriLen(out length);
                return length;
            }
        }

        public int NumberOfPoints => Cad.GetCurPriNP();

        public Cad.Vector[] Pts()
        {
            var np = NumberOfPoints;
            var points = new Cad.Vector[np];
            Cad.GetCurPriPts(np, points);
            return points;
        }
    }

    public class CurrentOlePrimitive : CurrentPrimitive
    {
        internal new static CurrentOlePrimitive Instance { get; } = new CurrentOlePrimitive();

        CurrentOlePrimitive() { }
    }

    public class CurrentPhotoPrimitive : CurrentPrimitive
    {
        internal new static CurrentPhotoPrimitive Instance { get; } = new CurrentPhotoPrimitive();

        CurrentPhotoPrimitive() { }

        public int NumberOfPoints => Cad.GetCurPriNP();
    }

    public class CurrentRasterPrimitive : CurrentPrimitive
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
