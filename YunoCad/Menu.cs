using System;
using static Informatix.MGDS.Cad;

namespace YunoCad
{
    public class Menu
    {
        internal static Menu Instance { get; } = new Menu();

        Menu() { }

        public int GetFreeEvent(int eventID) => GetFreeMenuEvent(eventID);

        public string PrefCfg
        {
            get
            {
                var fileName = "";
                GetPrefMenuCfg(out fileName);
                return fileName;
            }
            set
            {
                PrefMenuCfg(value);
            }
        }

        public void Load(string fileName, string section) => LoadMenu(fileName, section);

        public void RestoreState() => RestoreMenuState();

        public Command this[string menuName]
        {
            get { return new Command(menuName); }
        }
    }
}
