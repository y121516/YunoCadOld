using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class MenuItem
    {
        public string MenuName { get; } = "";

        public MenuItem(string menuName) { MenuName = MenuName; }

        public bool Enabled
        {
            set
            {
                EnableMenuCommand(MenuName, value);
            }
        }

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
    }
}
