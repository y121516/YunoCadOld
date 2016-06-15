using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class MenuItem
    {
        public string Name { get; } = "";

        internal MenuItem(string menuName) { Name = menuName; }

        public bool Enabled
        {
            set { EnableMenuCommand(Name, value); }
        }

        public bool Checked
        {
            get
            {
                bool isChecked;
                GetCheckMenuCommand(out isChecked, Name);
                return isChecked;
            }
            set { SetCheckMenuCommand(Name, value); }
        }
    }
}
