using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class Menu
    {
        internal static Menu Instance { get; } = new Menu();

        Menu() { }

        public int GetFreeEvent(int eventID) => MC.GetFreeMenuEvent(eventID);

        public string PrefCfg
        {
            get
            {
                var fileName = "";
                MC.GetPrefMenuCfg(out fileName);
                return fileName;
            }
            set { MC.PrefMenuCfg(value); }
        }

        public void Load(string fileName, string section) => MC.LoadMenu(fileName, section);

        public void RestoreState() => MC.RestoreMenuState();

        public MenuItemState this[MenuItem menuItem] => new MenuItemState(menuItem.Name);

        public void Add(CommandMenuItem commandMenuItem)
            => MC.AddMenuCommand(commandMenuItem.Name, commandMenuItem.CommandLine);

        public CommandMenuItem Insert(MenuItem insertBeforeMenuItem, CommandMenuItem commandMenuItem)
        {
            MC.InsertMenuCommand(insertBeforeMenuItem.Name, commandMenuItem.Name, commandMenuItem.CommandLine);
            var absoluteName = System.Text.RegularExpressions.Regex.Replace(
                insertBeforeMenuItem.Name,
                @"^?([^\\]+\\)(?:.*)",
                @"$1" + commandMenuItem.Name);
            return new CommandMenuItem(absoluteName, commandMenuItem.CommandLine);
        }

        public void Remove(MenuItem menuItem) => MC.RemoveMenuCommand(menuItem.Name);
    }

    public class MenuItem
    {
        public string Name { get; }

        public MenuItem(string name)
        {
            Name = name;
        }
    }

    public class CommandMenuItem : MenuItem
    {
        public string CommandLine { get; } = "";

        public CommandMenuItem(string name, string commandLine) : base(name)
        {
            CommandLine = commandLine;
        }
    }

    public class MenuItemState
    {
        public string Name { get; }

        internal MenuItemState(string name)
        {
            Name = name;
        }

        public bool Enabled
        {
            set { MC.EnableMenuCommand(Name, value); }
        }

        public bool Checked
        {
            get
            {
                bool isChecked;
                MC.GetCheckMenuCommand(out isChecked, Name);
                return isChecked;
            }
            set { MC.SetCheckMenuCommand(Name, value); }
        }
    }
}
