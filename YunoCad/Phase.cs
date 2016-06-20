using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;
using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class CurrentPhase
    {
        public static CurrentPhase Instance { get; } = new CurrentPhase();

        CurrentPhase() { }

        public string CharStyle
        {
            get
            {
                var value = "";
                GetCurPhase("CHARSTYLE", out value);
                return value;
            }
            set { CurPhase("CHARSTYLE", value); }
        }

        public string Colour
        {
            get
            {
                var value = "";
                GetCurPhase("COLOUR", out value);
                return value;
            }
            set { CurPhase("COLOUR", value); }
        }

        public string InclusionList
        {
            get
            {
                var value = "";
                GetCurPhase("INCLUSIONLIST", out value);
                return value;
            }
            set { CurPhase("INCLUSIONLIST", value); }
        }

        public string Label
        {
            get
            {
                var value = "";
                GetCurPhase("LABEL", out value);
                return value;
            }
            set { CurPhase("LABEL", value); }
        }

        public string LineStyle
        {
            get
            {
                var value = "";
                GetCurPhase("LINESTYLE", out value);
                return value;
            }
            set { CurPhase("LINESTYLE", value); }
        }

        public string Material
        {
            get
            {
                var value = "";
                GetCurPhase("MATERIAL", out value);
                return value;
            }
            set { CurPhase("MATERIAL", value); }
        }

        public string Pen
        {
            get
            {
                var value = "";
                GetCurPhase("PEN", out value);
                return value;
            }
            set { CurPhase("PEN", value); }
        }

        public string RespectStyleColours
        {
            get
            {
                var value = "";
                GetCurPhase("RESPECTSTYLECOLOURS", out value);
                return value;
            }
            set { CurPhase("RESPECTSTYLECOLOURS", value); }
        }

        public string RespectPrimitiveColours
        {
            get
            {
                var value = "";
                GetCurPhase("RESPECTPRIMITIVECOLOURS ", out value);
                return value;
            }
            set { CurPhase("RESPECTPRIMITIVECOLOURS ", value); }
        }

        public string Fading
        {
            get
            {
                var value = "";
                GetCurPhase("FADING", out value);
                return value;
            }
            set { CurPhase("FADING", value); }
        }

        public string ScreenOnly
        {
            get
            {
                var value = "";
                GetCurPhase("SCREENONLY", out value);
                return value;
            }
            set { CurPhase("SCREENONLY", value); }
        }

        public string State
        {
            get
            {
                var value = "";
                GetCurPhase("STATE", out value);
                return value;
            }
            set { CurPhase("STATE", value); }
        }

        public int LayerLink => GetCurPhaseLink();

        public void ChangeLayerLink(int layerLink, Save save = Save.RequestSave)
            => CurPhaseLink(layerLink, save);

        public string LayerName
        {
            get
            {
                var name = "";
                GetCurPhaseName(out name);
                return name;
            }
        }

        /// <summary>
        /// フェーズ番号、または PhaseNum.FastDraw
        /// </summary>
        public Phase Number => new Phase(GetCurPhaseNum());

        public void RenumberAfter(Phase phase) => CurPhaseReNum(phase.Number);
        public void RenumberFirst() => CurPhaseReNum(0);
        public void RenumberLast() => CurPhaseReNum(-1);
    }

    public struct Phase
    {
        public int Number;

        public Phase(int number)
        {
            Number = number;
        }

        public static Phase FastDraw { get; } = new Phase(PhaseNum.FastDraw);

        public CurrentPhase MakeCurrent()
        {
            if (!CurPhaseNum(Number)) throw new Exception();
            return CurrentPhase.Instance;
        }
    }

    public class Phases
    {
        const int DefaultColour = Col.Col1;
        const PhStat DefaultPhaseStatus = PhStat.Hittable; // Don't be PhStat.Editable
        const string TemporaryLayerName = "Temp";
        const Save DefaultSave = Save.RequestSave; // Don't be Save.Prompt

        public static Phases Instance { get; } = new Phases();

        Phases() { }

        void CreateFastDrawPhase(int layerLink, int colour = DefaultColour, PhStat status = DefaultPhaseStatus)
            => Cad.CreateFastDrawPhase(layerLink, colour, status);

        void CreateFastDrawPhase(string tempLayerName = TemporaryLayerName, int colour = DefaultColour, PhStat status = DefaultPhaseStatus)
        {
            // TODO: 例外処理
            var layerLink = CreateTempLayer(tempLayerName);
            Cad.CreateFastDrawPhase(layerLink, colour, status);
        }

        void Create(int layerLink, int colour = DefaultColour)
        {
            CreatePhase(layerLink, colour);
            return; // TODO: CurrentLayer or CurrentPhase
        }

        void Delete(Phase phase, Save save = DefaultSave)
            => DeletePhase(phase.Number, save);

        IEnumerable<CurrentPhase> Scan(bool includingFastDrawPhase = true)
        {
            if (includingFastDrawPhase && CurPhaseNum(PhaseNum.FastDraw))
            {
                yield return CurrentPhase.Instance;
            }
            for (var n = 1; CurPhaseNum(n); ++n)
            {
                yield return CurrentPhase.Instance;
            }
        }

        IEnumerable<Tuple<int, CurrentPhase>> ScanWithNumber(bool includingFastDrawPhase = true)
        {
            if (includingFastDrawPhase && CurPhaseNum(PhaseNum.FastDraw))
            {
                yield return Tuple.Create(PhaseNum.FastDraw, CurrentPhase.Instance);
            }
            for (var n = 1; CurPhaseNum(n); ++n)
            {
                yield return Tuple.Create(n, CurrentPhase.Instance);
            }
        }
    }
}
