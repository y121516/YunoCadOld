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
                MC.GetAliasDefinition(alias.AliasType, alias.Name, out string path, out bool expandable);
                return new AliasInfo(path, expandable);
            }
            set
            {
                Add(alias, value);
            }
        }

        public void Add(Alias alias, AliasInfo info)
            => MC.AliasDefinition(alias.AliasType, alias.Name, info.Path, info.Expandable);

        public void Delete(Alias alias) => MC.DeleteAliasDefinition(alias.AliasType, alias.Name);

        public DefaultAlias DefaultAlias(AliasType type) => new DefaultAlias(type);
    }

    public class DefaultAlias
    {
        public AliasType AliasType { get; }

        internal DefaultAlias(AliasType type)
        {
            AliasType = type;
        }

        public string Name
        {
            get
            {
                MC.GetSetAlias(AliasType, out string name);
                return name;
            }
            set { MC.SetAlias(AliasType, value); }
        }

        public void Reset() => MC.ResetAlias(AliasType);
    }

    public class Alias
    {
        public AliasType AliasType { get; }
        public string Name { get; } = "";

        public Alias(AliasType type, string name)
        {
            AliasType = type;
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
