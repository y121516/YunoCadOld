using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;
using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class SetWnds
    {
        const string defaultWildcardName = "*";

        public static SetWnds Instance { get; } = new SetWnds();

        SetWnds() { }

        public IEnumerable<string> ScanName(string wildcardName = defaultWildcardName)
        {
            var name = "";
            if (WndScanStart(wildcardName, out name))
            {
                do
                {
                    yield return name;
                } while (WndNext(out name));
            }
        }

        public IEnumerable<SetWnd> Scan(string wildcardName = defaultWildcardName)
        {
            var name = "";
            if (WndScanStart(wildcardName, out name))
            {
                do
                {
                    OpenWnd(name, false); // MANファイルで実行されるので isReadOnly: false は無視される
                    yield return SetWnd.Instance;
                } while (WndNext(out name));
            }
        }
    }

    public class SetWnd
    {
        const Save defaultSave = Save.RequestSave; // Don't be Save.Prompt & Save.DoNotDisown

        public static SetWnd Instance { get; } = new SetWnd();

        public Phases Phases { get; } = Phases.Instance;

        public void DisownSetWnd(Save save = defaultSave)
            => DisownSetWndLayers(save);

        /// <summary>
        /// mm（ミリメートル）,cm（センチメートル）,Inches（インチ）,m（メートル）,km（キロメートル）,
        /// Feet（フィート）,Feet+Inch（フィート+インチ）,Imperial（インチ系単位）Miles（マイル）,
        /// Ken（間）,Shaku（尺）,Sun（寸）,Bu（分）,NauticalMiles（海里）
        /// </summary>
        public Tuple<string, int> Units
        {
            get
            {
                var units = "";
                var decimalPlace = GetSetUnits(out units);
                return Tuple.Create(units, decimalPlace);
            }
            set { SetUnits(value.Item1, value.Item2); }
        }
    }
}
