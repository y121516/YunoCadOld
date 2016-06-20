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

        public string Type
        {
            get
            {
                var type = "";
                GetCurLayType(out type);
                return type;
            }
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
