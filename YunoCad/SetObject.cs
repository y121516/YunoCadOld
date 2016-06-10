using Informatix.MGDS;

namespace YunoCad
{
    class SetObject
    {
        void MoveTo(Cad.Vector pos)
        {
            Cad.MoveTo(pos);
        }

        Selection LineTo(Cad.Vector pos)
        {
            Cad.LineTo(pos);
            return Selection.Instance;
        }

        Selection ArcTo(Cad.Vector viaPos, Cad.Vector endPos)
        {
            Cad.ArcTo(viaPos, endPos);
            return Selection.Instance;
        }
    }
}
