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
        public Printer Printer { get; } = Printer.Instance;

        public void Echo(string echoStr) => MC.Echo(echoStr);
        public void Prompt(string promptStr) => MC.Prompt(promptStr);

        public string Title
        {
            get
            {
                MC.GetTitle(out string title);
                return title;
            }
        }

        public void ScreenUpdateMode(M.ScreenUpdate updateMode) => MC.ScreenUpdateMode(updateMode);
        
        public SystemVersion Version
        {
            get
            {
                var systemType = MC.GetSystemType(out int majVer, out int minVer);
                return new SystemVersion(majVer, minVer, systemType);
            }
        }

        public void KillInteractiveCommand() => MC.KillInteractiveCmd();
    }

    public struct SystemVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public M.Sys SystemType { get; set; }

        public SystemVersion(int major, int minor, M.Sys systemType)
        {
            Major = major;
            Minor = minor;
            SystemType = systemType;
        }
    }
}
