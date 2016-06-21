using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;
using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class CurrentLayer
    {
        const string defaultWildcardAll = "**";
        const string defaultScanEH = "E";

        public static CurrentLayer Instance { get; } = new CurrentLayer();

        CurrentLayer() { }

        /// <summary>
        /// 新規レイヤが設定レイヤかつカレントのレイヤになる
        /// </summary>
        /// <param name="nameOfAlias"></param>
        /// <param name="isTemporaryLayer"></param>
        /// <returns></returns>
        CurrentLayer Clone(string nameOfAlias, bool isTemporaryLayer = false)
        {
            CloneCurLayer(nameOfAlias, isTemporaryLayer);
            return Instance; // TODO: return CurrentLayer and SetLayer
        }

        public string Label
        {
            get
            {
                var label = "";
                GetCurLayLabel(out label);
                return label;
            }
            set { CurLayLabel(value); }
        }

        public int Link => GetCurLayLink();

        public string Name
        {
            get
            {
                var name = "";
                GetCurLayName(out name);
                return name;
            }
            set { CurLayName(value); }
        }

        /// <summary>
        /// "Layer", "Assembly", "Instance Assembly" のいずれか
        /// </summary>
        public string Type
        {
            get
            {
                var type = "";
                GetCurLayType(out type);
                return type;
            }
        }

        public int[] ObjectLinks => GetObjectLinks();


        public int GetObjectCount(string wildcard = defaultWildcardAll)
            => Cad.GetObjectCount(wildcard);

        int[] GetObjectLinksImpl(int count, string wildcard)
        {
            var objectLinks = new int[count];
            Cad.GetObjectLinks(objectLinks, wildcard, count);
            return objectLinks;
        }

        public int[] GetObjectLinks(int atMostCount, string wildcard = defaultWildcardAll)
        {
            var count = Math.Min(atMostCount, GetObjectCount(wildcard));
            return GetObjectLinksImpl(count, wildcard);
        }

        public int[] GetObjectLinks(string wildcard = defaultWildcardAll)
        {
            var count = GetObjectCount(wildcard);
            return GetObjectLinksImpl(count, wildcard);
        }


        public int GetObjectCountArea(double left, double top, double right, double bottom, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => Cad.GetObjectCountArea(scanEH, wildcard, left, top, right, bottom);

        int[] GetObjectLinksAreaImpl(int count, double left, double top, double right, double bottom, string scanEH, string wildcard)
        {
            var objectLinks = new int[count];
            Cad.GetObjectLinksArea(ObjectLinks, scanEH, wildcard, count, left, top, right, bottom);
            return ObjectLinks;
        }

        public int[] GetObjectLinksArea(int atMostCount, double left, double top, double right, double bottom, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = Math.Min(atMostCount, GetObjectCountArea(left, top, right, bottom, scanEH, wildcard));
            return GetObjectLinksAreaImpl(count, left, top, right, bottom, scanEH, wildcard);
        }

        public int[] GetObjectLinksArea(double left, double top, double right, double bottom, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = GetObjectCountArea(left, top, right, bottom, scanEH, wildcard);
            return GetObjectLinksAreaImpl(count, left, top, right, bottom, scanEH, wildcard);
        }


        public int GetObjectCountPoly(ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => Cad.GetObjectCountPoly(scanPoly, scanEH, wildcard);

        int[] GetObjectLinksPolyImpl(int count, ScanPoly scanPoly, string scanEH, string wildcard)
        {
            var objectLinks = new int[count];
            Cad.GetObjectLinksPoly(ObjectLinks, scanPoly, scanEH, wildcard, count);
            return ObjectLinks;
        }

        public int[] GetObjectLinksPoly(int atMostCount, ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = Math.Min(atMostCount, GetObjectCountPoly(scanPoly, scanEH, wildcard));
            return GetObjectLinksPolyImpl(count, scanPoly, scanEH, wildcard);
        }

        public int[] GetObjectLinksPoly(ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = GetObjectCountPoly(scanPoly, scanEH, wildcard);
            return GetObjectLinksPolyImpl(count, scanPoly, scanEH, wildcard);
        }


        public int GetObjectCountPrimPoly(int layerLink, int objectLink, int primitiveLink, ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => Cad.GetObjectCountPrimPoly(scanPoly, scanEH, wildcard, layerLink, objectLink, primitiveLink);

        public int GetObjectCountPrimPoly(PriTriple priTriple, ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => GetObjectCountPrimPoly(priTriple.llink, priTriple.vlink, priTriple.plink, scanPoly, scanEH, wildcard);

        int[] GetObjectLinksPrimPolyImpl(int count, int layerLink, int objectLink, int primitiveLink, ScanPoly scanPoly, string scanEH, string wildcard)
        {
            var objectLinks = new int[count];
            Cad.GetObjectLinksPrimPoly(ObjectLinks, scanPoly, scanEH, wildcard, count, layerLink, objectLink, primitiveLink);
            return ObjectLinks;
        }

        public int[] GetObjectLinksPrimPoly(int atMostCount, int layerLink, int objectLink, int primitiveLink, ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = Math.Min(atMostCount, GetObjectCountPrimPoly(layerLink, objectLink, primitiveLink, scanPoly, scanEH, wildcard));
            return GetObjectLinksPrimPolyImpl(count, layerLink, objectLink, primitiveLink, scanPoly, scanEH, wildcard);
        }

        public int[] GetObjectLinksPrimPoly(int atMostCount, PriTriple priTriple, ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => GetObjectLinksPrimPoly(atMostCount, priTriple.llink, priTriple.vlink, priTriple.plink, scanPoly, scanEH, wildcard);

        public int[] GetObjectLinksPrimPoly(int layerLink, int objectLink, int primitiveLink, ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = GetObjectCountPrimPoly(layerLink, objectLink, primitiveLink, scanPoly, scanEH, wildcard);
            return GetObjectLinksPrimPolyImpl(count, layerLink, objectLink, primitiveLink, scanPoly, scanEH, wildcard);
        }

        public int[] GetObjectLinksPrimPoly(PriTriple priTriple, ScanPoly scanPoly = ScanPoly.In, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => GetObjectLinksPrimPoly(priTriple.llink, priTriple.vlink, priTriple.plink, scanPoly, scanEH, wildcard);


        public int GetObjectCountVolume(Vector pt1, Vector pt2, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => Cad.GetObjectCountVolume(scanEH, wildcard, pt1, pt2);

        int[] GetObjectLinksVolumeImpl(int count, Vector pt1, Vector pt2, string scanEH, string wildcard)
        {
            var objectLinks = new int[count];
            Cad.GetObjectLinksVolume(ObjectLinks, scanEH, wildcard, count, pt1, pt2);
            return ObjectLinks;
        }

        public int[] GetObjectLinksVolume(int atMostCount, Vector pt1, Vector pt2, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = Math.Min(atMostCount, GetObjectCountVolume(pt1, pt2, scanEH, wildcard));
            return GetObjectLinksVolumeImpl(count, pt1, pt2, scanEH, wildcard);
        }

        public int[] GetObjectLinksVolume(Vector pt1, Vector pt2, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = GetObjectCountVolume(pt1, pt2, scanEH, wildcard);
            return GetObjectLinksVolumeImpl(count, pt1, pt2, scanEH, wildcard);
        }
    }

    public class SetLayer
    {
        int Link => GetSetLayLink();

        public SetObject CreateObject(string name, Cad.Vector pos)
        {
            Cad.CreateObject(name, pos);
            return SetObject.Instance;
        }
    }

    public class Layers
    {
        const Save defaultDeleteSave = Save.RequestSave; // Don't be Save.Prompt
        const Save defaultDisownSave = Save.RequestSave; // Don't be Save.Prompt & Save.DoNotDisown

        public static Layers Instance { get; } = new Layers();

        Layers() { }

        /* SetLayer */
        void Create(string layerName, string nameOfAlias)
        {
            CreateLayer(layerName, nameOfAlias);
            return; // TODO: return SetLayer.Instance;
        }

        CurrentLayer CreateTemporary(string layerName)
        {
            CreateTempLayer(layerName);
            return CurrentLayer.Instance; // TODO: return CurrentLayer and SetLayer
        }

        void Delete(int layerLink, Save save = defaultDeleteSave)
            => DeleteLayer(layerLink, save);

        void Disown(int layerLink, Save save = defaultDisownSave)
            => DisownLayer(layerLink, save);

        int GetLayerOfChild(int layerLink, int objectLink) => Cad.GetLayerOfChild(layerLink, objectLink);

        Tuple<int, int> GetLayerOfParent(int layerLink)
        {
            int objectLink;
            var parentLayer = Cad.GetLayerOfParent(layerLink, out objectLink);
            return Tuple.Create(parentLayer, objectLink);
        }

        /// <summary>
        /// カレントのレイヤ、オブジェクト、プリミティブは変更されません。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="scanWild"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        static IEnumerable<TResult> ForEach<TResult>(string scanWild, Func<string, int, TResult> func)
        {
            var layerName = "";
            int layerLink;
            if (Cad.LayerScanStart(scanWild, out layerName, out layerLink))
            {
                do
                {
                    yield return func(layerName, layerLink);
                } while (Cad.LayerNext(out layerName, out layerLink));
            }
        }

        static IEnumerable<TResult> ForEach<TResult>(Func<string, int, TResult> func)
        {
            var scanWild = "*";
            var layerName = "";
            int layerLink;
            if (Cad.LayerScanStart(scanWild, out layerName, out layerLink))
            {
                do
                {
                    yield return func(layerName, layerLink);
                } while (Cad.LayerNext(out layerName, out layerLink));
            }
        }
    }
}
