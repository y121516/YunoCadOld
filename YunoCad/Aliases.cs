using MC = Informatix.MGDS.Cad;
using AliasType = Informatix.MGDS.AliasName;

namespace Yuno.Cad
{
    public class Aliases
    {
        internal static Aliases Instance { get; } = new Aliases();

        Aliases() { }

        public AliasInfo this[Alias alias]
        {
            get
            {
                MC.GetAliasDefinition(alias.Type, alias.Name, out string path, out bool expandable);
                return new AliasInfo(path, expandable);
            }
            set
            {
                Add(alias, value);
            }
        }

        public void Add(Alias alias, AliasInfo info)
            => MC.AliasDefinition(alias.Type, alias.Name, info.Path, info.Expandable);

        public void Delete(Alias alias) => MC.DeleteAliasDefinition(alias.Type, alias.Name);

        public DefaultAlias DefaultAlias(AliasType type) => new DefaultAlias(type);
    }

    public class DefaultAlias
    {
        public AliasType Type { get; }

        internal DefaultAlias(AliasType type)
        {
            Type = type;
        }

        public string Name
        {
            get
            {
                MC.GetSetAlias(Type, out string name);
                return name;
            }
            set { MC.SetAlias(Type, value); }
        }

        public void Reset() => MC.ResetAlias(Type);
    }

    public class Alias
    {
        public AliasType Type { get; }
        public string Name { get; } = "";

        public Alias(AliasType type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public class AliasInfo
    {
        public string Path { get; } = "";
        public bool Expandable { get; }

        public AliasInfo(string path, bool expandable = false)
        {
            Path = path;
            Expandable = expandable;
        }
    }
}
