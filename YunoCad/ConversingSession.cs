using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class ConversingSession
    {
        internal ConversingSession(M.Conversation conversation, Session session)
        {
            Conversation = conversation;
            Session = session;
        }

        public M.Conversation Conversation { get; }
        public Session Session { get; }
        public Documents Documents { get; } = Documents.Instance;
        public Menu Menu { get; } = Menu.Instance;
        public InfoBar InfoBar { get; } = InfoBar.Instance;
        public Preference Preference { get; } = Preference.Instance;
        public File File { get; } = File.Instance;

        public void Echo(string echoStr) => MC.Echo(echoStr);
        public void Prompt(string promptStr) => MC.Prompt(promptStr);

        public string Title
        {
            get
            {
                string title;
                MC.GetTitle(out title);
                return title;
            }
        }

        public M.ScreenUpdate ScreenUpdateMode
        {
            set { MC.ScreenUpdateMode(value); }
        }

        public SystemVersion Version
        {
            get
            {
                SystemVersion v;
                v.SystemType = MC.GetSystemType(out v.Major, out v.Minor);
                return v;
            }
        }

        void KillInteractiveCommand() => MC.KillInteractiveCmd();
    }

    public struct SystemVersion
    {
        public int Major;
        public int Minor;
        public M.Sys SystemType;
    }
}
