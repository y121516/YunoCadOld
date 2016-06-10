using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;

namespace YunoCad
{
    public class CurrentLayer
    {
        public int Link { get { return Cad.GetCurLayLink(); } }

        public string Name
        {
            get
            {
                var name = "";
                Cad.GetCurLayName(out name);
                return name;
            }
            set
            {
                Cad.CurLayName(value);
            }
        }
    }

    public class Layer
    {
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
