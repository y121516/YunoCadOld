using System;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class Snap
    {
        public static Snap Instance { get; } = new Snap();

        Snap() { }

        public Tuple<M.Snapped, MC.Vector, string> GetArg(string prompt, M.Snap snapType)
        {
            var snap = "";
            var snapped = MC.GetArg(out MC.Vector argPos, ref snap, prompt, snapType);
            return Tuple.Create(snapped, argPos, snap);
        }

        public enum SetEdit
        {
            NotUse = M.Snap.AllowList,
            Use = M.Snap.SEAllowList
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
        public R GetArg<R>(string prompt, string allowSnaps, SetEdit setEdit, Func<M.Snapped, R> byKeyboard, Func<M.Snapped, MC.Vector, string, CurrentPrimitive, R> byMouse)
        {
            var snapped = MC.GetArg(out MC.Vector argPos, ref allowSnaps, prompt, (M.Snap)setEdit);
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
                var snapped = MC.GetArg(out MC.Vector argPos, ref AllowSnaps, Prompt, (M.Snap)SetEdit);
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
        public static bool IsByKey(this M.Snapped snapped)
        {
            switch (snapped)
            {
                case M.Snapped.Enter:
                case M.Snapped.Escape:
                case M.Snapped.Backspace:
                case M.Snapped.CtrlEnter:
                case M.Snapped.ShiftEnter:
                case M.Snapped.CtrlShiftEnter:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsByMouse(this M.Snapped snapped)
        {
            switch (snapped)
            {
                case M.Snapped.Mouse:
                case M.Snapped.CtrlMouse:
                case M.Snapped.ShiftMouse:
                case M.Snapped.CtrlShiftMouse:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasCtrl(this M.Snapped snapped)
        {
            switch (snapped)
            {
                case M.Snapped.CtrlMouse:
                case M.Snapped.CtrlShiftMouse:
                case M.Snapped.CtrlEnter:
                case M.Snapped.CtrlShiftEnter:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasShift(this M.Snapped snapped)
        {
            switch (snapped)
            {
                case M.Snapped.ShiftMouse:
                case M.Snapped.CtrlShiftMouse:
                case M.Snapped.ShiftEnter:
                case M.Snapped.CtrlShiftEnter:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasModifiers(this M.Snapped snapped)
        {
            switch (snapped)
            {
                case M.Snapped.CtrlMouse:
                case M.Snapped.ShiftMouse:
                case M.Snapped.CtrlShiftMouse:
                case M.Snapped.CtrlEnter:
                case M.Snapped.ShiftEnter:
                case M.Snapped.CtrlShiftEnter:
                    return true;
                default:
                    return false;
            }
        }
    }
}
