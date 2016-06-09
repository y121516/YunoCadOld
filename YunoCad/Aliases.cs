using AliasType = Informatix.MGDS.AliasName;
using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class Aliases
    {
        internal static Aliases Instance { get; } = new Aliases();

        Aliases() { }

        public AliasInfo this[Alias alias]
        {
            get
            {
                var path = "";
                bool expandable;
                GetAliasDefinition(alias.Type, alias.Name, out path, out expandable);
                return new AliasInfo(path, expandable);
            }
            set
            {
                Add(alias, value);
            }
        }

        public void Add(Alias alias, AliasInfo info)
            => AliasDefinition(alias.Type, alias.Name, info.Path, info.Expandable);

        public void Delete(Alias alias) => DeleteAliasDefinition(alias.Type, alias.Name);

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
                var name = "";
                GetSetAlias(Type, out name);
                return name;
            }
            set { SetAlias(Type, value); }
        }

        public void Reset() => ResetAlias(Type);
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
