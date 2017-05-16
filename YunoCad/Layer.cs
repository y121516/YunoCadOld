using System;
using System.Collections.Generic;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class CurrentLayer
    {
        const string defaultWildcardAll = "**";
        const string defaultScanEH = "E";
        const M.ScanPoly defaultScanPoly = M.ScanPoly.In;

        public static CurrentLayer Instance { get; } = new CurrentLayer();

        CurrentLayer() { }

        /// <summary>
        /// 新規レイヤが設定レイヤかつカレントのレイヤになる
        /// </summary>
        /// <param name="nameOfAlias"></param>
        /// <param name="isTemporaryLayer"></param>
        /// <returns></returns>
        public CurrentLayer Clone(string nameOfAlias, bool isTemporaryLayer = false)
        {
            MC.CloneCurLayer(nameOfAlias, isTemporaryLayer);
            return Instance; // TODO: return CurrentLayer and SetLayer
        }

        public string Label
        {
            get
            {
                MC.GetCurLayLabel(out string label);
                return label;
            }
            set { MC.CurLayLabel(value); }
        }

        public int Link => MC.GetCurLayLink();

        public string Name
        {
            get
            {
                MC.GetCurLayName(out string name);
                return name;
            }
            set { MC.CurLayName(value); }
        }

        /// <summary>
        /// "Layer", "Assembly", "Instance Assembly" のいずれか
        /// </summary>
        public string Type
        {
            get
            {
                MC.GetCurLayType(out string type);
                return type;
            }
        }

        public int[] ObjectLinks => GetObjectLinks();


        public int GetObjectCount(string wildcard = defaultWildcardAll)
            => MC.GetObjectCount(wildcard);

        int[] GetObjectLinksImpl(int count, string wildcard)
        {
            var objectLinks = new int[count];
            MC.GetObjectLinks(objectLinks, wildcard, count);
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
            => MC.GetObjectCountArea(scanEH, wildcard, left, top, right, bottom);

        int[] GetObjectLinksAreaImpl(int count, double left, double top, double right, double bottom, string scanEH, string wildcard)
        {
            var objectLinks = new int[count];
            MC.GetObjectLinksArea(ObjectLinks, scanEH, wildcard, count, left, top, right, bottom);
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


        public int GetObjectCountPoly(M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => MC.GetObjectCountPoly(scanPoly, scanEH, wildcard);

        int[] GetObjectLinksPolyImpl(int count, M.ScanPoly scanPoly, string scanEH, string wildcard)
        {
            var objectLinks = new int[count];
            MC.GetObjectLinksPoly(ObjectLinks, scanPoly, scanEH, wildcard, count);
            return ObjectLinks;
        }

        public int[] GetObjectLinksPoly(int atMostCount, M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = Math.Min(atMostCount, GetObjectCountPoly(scanPoly, scanEH, wildcard));
            return GetObjectLinksPolyImpl(count, scanPoly, scanEH, wildcard);
        }

        public int[] GetObjectLinksPoly(M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = GetObjectCountPoly(scanPoly, scanEH, wildcard);
            return GetObjectLinksPolyImpl(count, scanPoly, scanEH, wildcard);
        }


        public int GetObjectCountPrimPoly(int layerLink, int objectLink, int primitiveLink, M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => MC.GetObjectCountPrimPoly(scanPoly, scanEH, wildcard, layerLink, objectLink, primitiveLink);

        public int GetObjectCountPrimPoly(MC.PriTriple priTriple, M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => GetObjectCountPrimPoly(priTriple.llink, priTriple.vlink, priTriple.plink, scanPoly, scanEH, wildcard);

        int[] GetObjectLinksPrimPolyImpl(int count, int layerLink, int objectLink, int primitiveLink, M.ScanPoly scanPoly, string scanEH, string wildcard)
        {
            var objectLinks = new int[count];
            MC.GetObjectLinksPrimPoly(ObjectLinks, scanPoly, scanEH, wildcard, count, layerLink, objectLink, primitiveLink);
            return ObjectLinks;
        }

        public int[] GetObjectLinksPrimPoly(int atMostCount, int layerLink, int objectLink, int primitiveLink, M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = Math.Min(atMostCount, GetObjectCountPrimPoly(layerLink, objectLink, primitiveLink, scanPoly, scanEH, wildcard));
            return GetObjectLinksPrimPolyImpl(count, layerLink, objectLink, primitiveLink, scanPoly, scanEH, wildcard);
        }

        public int[] GetObjectLinksPrimPoly(int atMostCount, MC.PriTriple priTriple, M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => GetObjectLinksPrimPoly(atMostCount, priTriple.llink, priTriple.vlink, priTriple.plink, scanPoly, scanEH, wildcard);

        public int[] GetObjectLinksPrimPoly(int layerLink, int objectLink, int primitiveLink, M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = GetObjectCountPrimPoly(layerLink, objectLink, primitiveLink, scanPoly, scanEH, wildcard);
            return GetObjectLinksPrimPolyImpl(count, layerLink, objectLink, primitiveLink, scanPoly, scanEH, wildcard);
        }

        public int[] GetObjectLinksPrimPoly(MC.PriTriple priTriple, M.ScanPoly scanPoly = defaultScanPoly, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => GetObjectLinksPrimPoly(priTriple.llink, priTriple.vlink, priTriple.plink, scanPoly, scanEH, wildcard);


        public int GetObjectCountVolume(MC.Vector pt1, MC.Vector pt2, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
            => MC.GetObjectCountVolume(scanEH, wildcard, pt1, pt2);

        int[] GetObjectLinksVolumeImpl(int count, MC.Vector pt1, MC.Vector pt2, string scanEH, string wildcard)
        {
            var objectLinks = new int[count];
            MC.GetObjectLinksVolume(ObjectLinks, scanEH, wildcard, count, pt1, pt2);
            return ObjectLinks;
        }

        public int[] GetObjectLinksVolume(int atMostCount, MC.Vector pt1, MC.Vector pt2, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = Math.Min(atMostCount, GetObjectCountVolume(pt1, pt2, scanEH, wildcard));
            return GetObjectLinksVolumeImpl(count, pt1, pt2, scanEH, wildcard);
        }

        public int[] GetObjectLinksVolume(MC.Vector pt1, MC.Vector pt2, string scanEH = defaultScanEH, string wildcard = defaultWildcardAll)
        {
            var count = GetObjectCountVolume(pt1, pt2, scanEH, wildcard);
            return GetObjectLinksVolumeImpl(count, pt1, pt2, scanEH, wildcard);
        }

        public bool IsOwned => MC.IsCurLayOwned();
        public bool IsTemporary => MC.IsCurLayTemp();
    }

    public class SetLayer
    {
        public static SetLayer Instance { get; } = new SetLayer();

        SetLayer() { }

        public int Link => MC.GetSetLayLink();

        public SetObject CreateObject(string name, MC.Vector pos)
        {
            MC.CreateObject(name, pos);
            return SetObject.Instance;
        }
    }

    public class Layers
    {
        const M.Save defaultDeleteSave = M.Save.RequestSave; // Don't be Save.Prompt
        const M.Save defaultDisownSave = M.Save.RequestSave; // Don't be Save.Prompt or Save.DoNotDisown
        const string defaultWildcard = "*";
        const string defaultObjectWildcard = "**";
        const string defaultScanEH = "E";

        public static Layers Instance { get; } = new Layers();

        Layers() { }

        public SetLayer Create(string layerName, string nameOfAlias)
        {
            MC.CreateLayer(layerName, nameOfAlias);
            return SetLayer.Instance;
        }

        public CurrentLayer CreateTemporary(string layerName)
        {
            MC.CreateTempLayer(layerName);
            return CurrentLayer.Instance; // TODO: return CurrentLayer and SetLayer
        }

        public void Delete(int layerLink, M.Save save = defaultDeleteSave)
            => MC.DeleteLayer(layerLink, save);

        public void Disown(int layerLink, M.Save save = defaultDisownSave)
            => MC.DisownLayer(layerLink, save);

        public int GetLayerOfChild(int layerLink, int objectLink) => MC.GetLayerOfChild(layerLink, objectLink);

        public Tuple<int, int> GetLayerOfParent(int layerLink)
        {
            var parentLayer = MC.GetLayerOfParent(layerLink, out int objectLink);
            return Tuple.Create(parentLayer, objectLink);
        }

        public int GetLinkFromPath(string layerPath) => MC.LayerLinkFromPath(layerPath);

        public string GetPathFromLink(int layerLink)
        {
            MC.LayerPathFromLink(layerLink, out string path);
            return path;
        }

        /// <summary>
        /// カレントのレイヤ、オブジェクト、プリミティブは変更されません。
        /// </summary>
        public IEnumerable<Tuple<string, int>> Scan(string scanWild = defaultWildcard)
        {
            if (MC.LayerScanStart(scanWild, out string layerName, out int layerLink))
            {
                do
                {
                    yield return Tuple.Create(layerName, layerLink);
                } while (MC.LayerNext(out layerName, out layerLink));
            }
        }

        public IEnumerable<CurrentObject> ObjectScan(int layerLink, M.ScanMode extentType, MC.Vector lo, MC.Vector hi,
            string scanEH = defaultScanEH, string wildcard = defaultObjectWildcard)
        {
            if (MC.ObjectScanLayer(layerLink, scanEH, wildcard, extentType, lo, hi))
            {
                do
                {
                    yield return CurrentObject.Instance;
                } while (MC.ObjectNext());
            }
        }

        public void Own(int layerLink) => MC.OwnLayer(layerLink);

        public void Reset() => MC.ResetLayer();

        public void SaveLayer(int layerLink) => MC.SaveLayer(layerLink);

        public void SaveSetWndLayers() => MC.SaveSetWndLayers();

        public SetLayer Set(int layerLink)
        {
            MC.SetLayer(layerLink);
            return SetLayer.Instance;
        }
    }
}
