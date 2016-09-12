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

        public void Echo(string echoStr) => MC.Echo(echoStr);
        public void Prompt(string promptStr) => MC.Prompt(promptStr);
    }
}
