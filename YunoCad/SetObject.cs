using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class SetObject
    {
        public static SetObject Instance { get; } = new SetObject();

        SetObject() { }

        void MoveTo(MC.Vector pos)
        {
            MC.MoveTo(pos);
        }

        Selection LineTo(MC.Vector pos)
        {
            MC.LineTo(pos);
            return Selection.Instance;
        }

        Selection ArcTo(MC.Vector viaPos, MC.Vector endPos)
        {
            MC.ArcTo(viaPos, endPos);
            return Selection.Instance;
        }
    }
}
