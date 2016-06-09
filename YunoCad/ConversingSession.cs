namespace YunoCad
{
    public class ConversingSession
    {
        internal ConversingSession(Informatix.MGDS.Conversation conversation, Session session)
        {
            Conversation = conversation;
            Session = session;
        }

        public Informatix.MGDS.Conversation Conversation { get; }
        public Session Session { get; }
        public Documents Documents { get; } = Documents.Instance;
        public Menu Menu { get; } = Menu.Instance;
    }
}
