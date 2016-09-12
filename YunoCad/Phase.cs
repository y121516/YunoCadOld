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
                var value = "";
                MC.GetCurPhase("CHARSTYLE", out value);
                return value;
            }
            set { MC.CurPhase("CHARSTYLE", value); }
        }

        public string Colour
        {
            get
            {
                var value = "";
                MC.GetCurPhase("COLOUR", out value);
                return value;
            }
            set { MC.CurPhase("COLOUR", value); }
        }

        public string InclusionList
        {
            get
            {
                var value = "";
                MC.GetCurPhase("INCLUSIONLIST", out value);
                return value;
            }
            set { MC.CurPhase("INCLUSIONLIST", value); }
        }

        public string Label
        {
            get
            {
                var value = "";
                MC.GetCurPhase("LABEL", out value);
                return value;
            }
            set { MC.CurPhase("LABEL", value); }
        }

        public string LineStyle
        {
            get
            {
                var value = "";
                MC.GetCurPhase("LINESTYLE", out value);
                return value;
            }
            set { MC.CurPhase("LINESTYLE", value); }
        }

        public string Material
        {
            get
            {
                var value = "";
                MC.GetCurPhase("MATERIAL", out value);
                return value;
            }
            set { MC.CurPhase("MATERIAL", value); }
        }

        public string Pen
        {
            get
            {
                var value = "";
                MC.GetCurPhase("PEN", out value);
                return value;
            }
            set { MC.CurPhase("PEN", value); }
        }

        public string RespectStyleColours
        {
            get
            {
                var value = "";
                MC.GetCurPhase("RESPECTSTYLECOLOURS", out value);
                return value;
            }
            set { MC.CurPhase("RESPECTSTYLECOLOURS", value); }
        }

        public string RespectPrimitiveColours
        {
            get
            {
                var value = "";
                MC.GetCurPhase("RESPECTPRIMITIVECOLOURS ", out value);
                return value;
            }
            set { MC.CurPhase("RESPECTPRIMITIVECOLOURS ", value); }
        }

        public string Fading
        {
            get
            {
                var value = "";
                MC.GetCurPhase("FADING", out value);
                return value;
            }
            set { MC.CurPhase("FADING", value); }
        }

        public string ScreenOnly
        {
            get
            {
                var value = "";
                MC.GetCurPhase("SCREENONLY", out value);
                return value;
            }
            set { MC.CurPhase("SCREENONLY", value); }
        }

        public string State
        {
            get
            {
                var value = "";
                MC.GetCurPhase("STATE", out value);
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
                var name = "";
                MC.GetCurPhaseName(out name);
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
