using AliasType = Informatix.MGDS.AliasName;
using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class Alias
    {
        AliasType Type { get; }
        string Name { get; } = "";

        Alias(AliasType type, string name)
        {
            Type = type;
            Name = name;
        }

        void Define(string path, bool expandable)
            => AliasDefinition(Type, Name, path, expandable);
        void Delete() => DeleteAliasDefinition(Type, Name);

    }
}
