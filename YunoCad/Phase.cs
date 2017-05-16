using System;
using System.Collections.Generic;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class CurrentPhase
    {
        public static CurrentPhase Instance { get; } = new CurrentPhase();

        CurrentPhase() { }

        public string CharStyle
        {
            get
            {
                MC.GetCurPhase("CHARSTYLE", out string value);
                return value;
            }
            set { MC.CurPhase("CHARSTYLE", value); }
        }

        public string Colour
        {
            get
            {
                MC.GetCurPhase("COLOUR", out string value);
                return value;
            }
            set { MC.CurPhase("COLOUR", value); }
        }

        public string InclusionList
        {
            get
            {
                MC.GetCurPhase("INCLUSIONLIST", out string value);
                return value;
            }
            set { MC.CurPhase("INCLUSIONLIST", value); }
        }

        public string Label
        {
            get
            {
                MC.GetCurPhase("LABEL", out string value);
                return value;
            }
            set { MC.CurPhase("LABEL", value); }
        }

        public string LineStyle
        {
            get
            {
                MC.GetCurPhase("LINESTYLE", out string value);
                return value;
            }
            set { MC.CurPhase("LINESTYLE", value); }
        }

        public string Material
        {
            get
            {
                MC.GetCurPhase("MATERIAL", out string value);
                return value;
            }
            set { MC.CurPhase("MATERIAL", value); }
        }

        public string Pen
        {
            get
            {
                MC.GetCurPhase("PEN", out string value);
                return value;
            }
            set { MC.CurPhase("PEN", value); }
        }

        public string RespectStyleColours
        {
            get
            {
                MC.GetCurPhase("RESPECTSTYLECOLOURS", out string value);
                return value;
            }
            set { MC.CurPhase("RESPECTSTYLECOLOURS", value); }
        }

        public string RespectPrimitiveColours
        {
            get
            {
                MC.GetCurPhase("RESPECTPRIMITIVECOLOURS ", out string value);
                return value;
            }
            set { MC.CurPhase("RESPECTPRIMITIVECOLOURS ", value); }
        }

        public string Fading
        {
            get
            {
                MC.GetCurPhase("FADING", out string value);
                return value;
            }
            set { MC.CurPhase("FADING", value); }
        }

        public string ScreenOnly
        {
            get
            {
                MC.GetCurPhase("SCREENONLY", out string value);
                return value;
            }
            set { MC.CurPhase("SCREENONLY", value); }
        }

        public string State
        {
            get
            {
                MC.GetCurPhase("STATE", out string value);
                return value;
            }
            set { MC.CurPhase("STATE", value); }
        }

        public int LayerLink => MC.GetCurPhaseLink();

        public void ChangeLayerLink(int layerLink, M.Save save = M.Save.RequestSave)
            => MC.CurPhaseLink(layerLink, save);

        public string LayerName
        {
            get
            {
                MC.GetCurPhaseName(out string name);
                return name;
            }
        }

        /// <summary>
        /// フェーズ番号、または PhaseNum.FastDraw
        /// </summary>
        public Phase Number => new Phase(MC.GetCurPhaseNum());

        public void RenumberAfter(Phase phase) => MC.CurPhaseReNum(phase.Number);
        public void RenumberFirst() => MC.CurPhaseReNum(0);
        public void RenumberLast() => MC.CurPhaseReNum(-1);
    }

    public struct Phase
    {
        public int Number;

        public Phase(int number)
        {
            Number = number;
        }

        public static Phase FastDraw { get; } = new Phase(M.PhaseNum.FastDraw);

        public CurrentPhase MakeCurrent()
        {
            if (!MC.CurPhaseNum(Number)) throw new Exception();
            return CurrentPhase.Instance;
        }
    }

    public class Phases
    {
        const int DefaultColour = M.Col.Col1;
        const M.PhStat DefaultPhaseStatus = M.PhStat.Hittable; // Don't be PhStat.Editable
        const string TemporaryLayerName = "Temp";
        const M.Save DefaultSave = M.Save.RequestSave; // Don't be Save.Prompt

        public static Phases Instance { get; } = new Phases();

        Phases() { }

        void CreateFastDrawPhase(int layerLink, int colour = DefaultColour, M.PhStat status = DefaultPhaseStatus)
            => MC.CreateFastDrawPhase(layerLink, colour, status);

        void CreateFastDrawPhase(string tempLayerName = TemporaryLayerName, int colour = DefaultColour, M.PhStat status = DefaultPhaseStatus)
        {
            // TODO: 例外処理
            var layerLink = MC.CreateTempLayer(tempLayerName);
            MC.CreateFastDrawPhase(layerLink, colour, status);
        }

        void Create(int layerLink, int colour = DefaultColour)
        {
            MC.CreatePhase(layerLink, colour);
            return; // TODO: CurrentLayer or CurrentPhase
        }

        void Delete(Phase phase, M.Save save = DefaultSave)
            => MC.DeletePhase(phase.Number, save);

        IEnumerable<CurrentPhase> Scan(bool includingFastDrawPhase = true)
        {
            if (includingFastDrawPhase && MC.CurPhaseNum(M.PhaseNum.FastDraw))
            {
                yield return CurrentPhase.Instance;
            }
            for (var n = 1; MC.CurPhaseNum(n); ++n)
            {
                yield return CurrentPhase.Instance;
            }
        }

        IEnumerable<Tuple<int, CurrentPhase>> ScanWithNumber(bool includingFastDrawPhase = true)
        {
            if (includingFastDrawPhase && MC.CurPhaseNum(M.PhaseNum.FastDraw))
            {
                yield return Tuple.Create(M.PhaseNum.FastDraw, CurrentPhase.Instance);
            }
            for (var n = 1; MC.CurPhaseNum(n); ++n)
            {
                yield return Tuple.Create(n, CurrentPhase.Instance);
            }
        }
    }
}
