using Informatix.MGDS;

namespace YunoCad
{
    class SetLayer
    {
        public SetObject CreateObject(string name, Cad.Vector pos)
        {
            Cad.CreateObject(name, pos);
            return new SetObject();
        }
    }
}
