using System;
using System.Collections.Generic;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class SetWnds
    {
        const string defaultWildcardName = "*";

        public static SetWnds Instance { get; } = new SetWnds();

        SetWnds() { }

        public IEnumerable<string> ScanName(string wildcardName = defaultWildcardName)
        {
            if (MC.WndScanStart(wildcardName, out string name))
            {
                do
                {
                    yield return name;
                } while (MC.WndNext(out name));
            }
        }

        public IEnumerable<SetWnd> Scan(string wildcardName = defaultWildcardName)
        {
            if (MC.WndScanStart(wildcardName, out string name))
            {
                do
                {
                    MC.OpenWnd(name, false); // MANファイルで実行されるので isReadOnly: false は無視される
                    yield return SetWnd.Instance;
                } while (MC.WndNext(out name));
            }
        }
    }

    public class SetWnd
    {
        const M.Save defaultSave = M.Save.RequestSave; // Don't be Save.Prompt or Save.DoNotDisown

        public static SetWnd Instance { get; } = new SetWnd();

        public Phases Phases { get; } = Phases.Instance;

        public void DisownSetWnd(M.Save save = defaultSave)
            => MC.DisownSetWndLayers(save);

        /// <summary>
        /// mm（ミリメートル）,cm（センチメートル）,Inches（インチ）,m（メートル）,km（キロメートル）,
        /// Feet（フィート）,Feet+Inch（フィート+インチ）,Imperial（インチ系単位）Miles（マイル）,
        /// Ken（間）,Shaku（尺）,Sun（寸）,Bu（分）,NauticalMiles（海里）
        /// </summary>
        public Tuple<string, int> Units
        {
            get
            {
                var decimalPlace = MC.GetSetUnits(out string units);
                return Tuple.Create(units, decimalPlace);
            }
            set { MC.SetUnits(value.Item1, value.Item2); }
        }
    }
}
