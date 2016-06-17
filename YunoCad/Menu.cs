using Informatix.MGDS;

namespace YunoCad
{
    public class Menu
    {
        internal static Menu Instance { get; } = new Menu();

        Menu() { }

        public int GetFreeEvent(int eventID) => Cad.GetFreeMenuEvent(eventID);

        public string PrefCfg
        {
            get
            {
                var fileName = "";
                Cad.GetPrefMenuCfg(out fileName);
                return fileName;
            }
            set { Cad.PrefMenuCfg(value); }
        }

        public void Load(string fileName, string section) => Cad.LoadMenu(fileName, section);

        public void RestoreState() => Cad.RestoreMenuState();

        public MenuItemState this[MenuItem menuItem] => new MenuItemState(menuItem.Name);

        public void Add(CommandMenuItem commandMenuItem)
            => Cad.AddMenuCommand(commandMenuItem.Name, commandMenuItem.CommandLine);

        public CommandMenuItem Insert(MenuItem insertBeforeMenuItem, CommandMenuItem commandMenuItem)
        {
            Cad.InsertMenuCommand(insertBeforeMenuItem.Name, commandMenuItem.Name, commandMenuItem.CommandLine);
            var absoluteName = System.Text.RegularExpressions.Regex.Replace(
                insertBeforeMenuItem.Name,
                @"^?([^\\]+\\)(?:.*)",
                @"$1" + commandMenuItem.Name);
            return new CommandMenuItem(absoluteName, commandMenuItem.CommandLine);
        }

        public void Remove(MenuItem menuItem) => Cad.RemoveMenuCommand(menuItem.Name);
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
            set { Cad.EnableMenuCommand(Name, value); }
        }

        public bool Checked
        {
            get
            {
                bool isChecked;
                Cad.GetCheckMenuCommand(out isChecked, Name);
                return isChecked;
            }
            set { Cad.SetCheckMenuCommand(Name, value); }
        }
    }
}
