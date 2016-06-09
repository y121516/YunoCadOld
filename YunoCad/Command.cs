using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class Command
    {
        string MenuName = "";

        internal Command(string menuName) { MenuName = menuName; }

        public void Add(string commandLine) => AddMenuCommand(MenuName, commandLine);

        public void Enable(bool enable) => EnableMenuCommand(MenuName, enable);

        public bool Checked
        {
            get
            {
                bool isChecked;
                GetCheckMenuCommand(out isChecked, MenuName);
                return isChecked;
            }
            set { SetCheckMenuCommand(MenuName, value); }
        }

        void Insert(string insertBeforeMenuName, string commandLine)
            => InsertMenuCommand(insertBeforeMenuName, MenuName, commandLine);

        public void Remove() => RemoveMenuCommand(MenuName);
    }
}
