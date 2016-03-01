using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;

namespace YunoCad
{
    public class CurrentPhase
    {
        static IEnumerable<TResult> ForEach<TResult>(Func<int, TResult> func)
        {
            for (var i = 1; Cad.CurPhaseNum(i); ++i)
            {
                yield return func(i);
            }
        }
    }

}
