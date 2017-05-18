using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace YunoCad
{
    class Attribute
    {
        string Name { get; }

        Attribute(string name)
        {
            Name = name;
        }

        public string Value
        {
            get
            {
                MC.GetAttVal(Name, out string attr);
                return attr;
            }
            set => MC.AttVal(Name, value);
        }

        public string Style
        {
            get
            {
                MC.GetXMLAttrstyle(out string defn, Name);
                return defn;
            }
            set => MC.SetXMLAttrstyle(value);
        }
    }
}
