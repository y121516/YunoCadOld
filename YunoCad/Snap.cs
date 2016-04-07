using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGDS = Informatix.MGDS;

namespace YunoCad
{
    public class Snap
    {
        public static Snap Instance { get; } = new Snap();

        Snap() { }

        public Tuple<MGDS.Snapped, MGDS.Cad.Vector, string> GetArg(string prompt, MGDS.Snap snapType)
        {
            MGDS.Cad.Vector argPos;
            var snap = "";
            var snapped = MGDS.Cad.GetArg(out argPos, ref snap, prompt, snapType);
            return Tuple.Create(snapped, argPos, snap);
        }

        public enum SetEdit
        {
            NotUse = MGDS.Snap.AllowList,
            Use = MGDS.Snap.SEAllowList
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="allowSnaps">
        /// スナップコード A, B, C, D, E, F, G, I, L, M, N, O, P, T, V の組み合わせ
        /// </param>
        /// <param name="setEdit"></param>
        /// <returns></returns>
        public R GetArg<R>(string prompt, string allowSnaps, SetEdit setEdit, Func<MGDS.Snapped, R> byKeyboard, Func<MGDS.Snapped, MGDS.Cad.Vector, string, CurrentPrimitive, R> byMouse)
        {
            MGDS.Cad.Vector argPos;
            var snapped = MGDS.Cad.GetArg(out argPos, ref allowSnaps, prompt, (MGDS.Snap)setEdit);
            return snapped.IsByKey() ? byKeyboard(snapped)
                : byMouse(snapped, argPos, allowSnaps, CurrentPrimitive.Instance);
        }


        class SnapArg
        {
            string Prompt = "";
            string AllowSnaps = "";
            SetEdit SetEdit;

            SnapArg(string prompt, string allowSnaps = "", SetEdit setEdit = SetEdit.NotUse)
            {
                Prompt = prompt;
                AllowSnaps = allowSnaps;
                SetEdit = setEdit;
            }

            void Get()
            {
                MGDS.Cad.Vector argPos;
                var snapped = MGDS.Cad.GetArg(out argPos, ref AllowSnaps, Prompt, (MGDS.Snap)SetEdit);
            }
        }
    }


    public class KeySnap
    {

    }
    public class MouseSnap
    {

    }

    public static class SnappedExtension
    {
        public static bool IsByKey(this MGDS.Snapped snapped)
        {
            switch (snapped)
            {
                case MGDS.Snapped.Enter:
                case MGDS.Snapped.Escape:
                case MGDS.Snapped.Backspace:
                case MGDS.Snapped.CtrlEnter:
                case MGDS.Snapped.ShiftEnter:
                case MGDS.Snapped.CtrlShiftEnter:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsByMouse(this MGDS.Snapped snapped)
        {
            switch (snapped)
            {
                case MGDS.Snapped.Mouse:
                case MGDS.Snapped.CtrlMouse:
                case MGDS.Snapped.ShiftMouse:
                case MGDS.Snapped.CtrlShiftMouse:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasCtrl(this MGDS.Snapped snapped)
        {
            switch (snapped)
            {
                case MGDS.Snapped.CtrlMouse:
                case MGDS.Snapped.CtrlShiftMouse:
                case MGDS.Snapped.CtrlEnter:
                case MGDS.Snapped.CtrlShiftEnter:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasShift(this MGDS.Snapped snapped)
        {
            switch (snapped)
            {
                case MGDS.Snapped.ShiftMouse:
                case MGDS.Snapped.CtrlShiftMouse:
                case MGDS.Snapped.ShiftEnter:
                case MGDS.Snapped.CtrlShiftEnter:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasModifiers(this MGDS.Snapped snapped)
        {
            switch (snapped)
            {
                case MGDS.Snapped.CtrlMouse:
                case MGDS.Snapped.ShiftMouse:
                case MGDS.Snapped.CtrlShiftMouse:
                case MGDS.Snapped.CtrlEnter:
                case MGDS.Snapped.ShiftEnter:
                case MGDS.Snapped.CtrlShiftEnter:
                    return true;
                default:
                    return false;
            }
        }
    }

}
