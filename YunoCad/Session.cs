using System;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    /// <summary>
    /// セッション (実行中の MicroGDS のプロセス) を表すクラス。
    /// </summary>
    public class Session : IEquatable<Session>
    {
        public const int DefaultTimeoutMillisecond = 30 * 1000;

        public static Session Start(M.StartFileType fileType = M.StartFileType.MAN, int timeoutMillisecond = DefaultTimeoutMillisecond)
        {
            return new Session(MC.StartMicroGDS(fileType, timeoutMillisecond));
        }

        public int ID { get; }

        public Session(int sessionID)
        {
            ID = sessionID;
        }

        public bool Equals(Session other) => (object)other == null ? false : ID.Equals(other.ID);

        public override bool Equals(object obj) => Equals(obj as Session);

        public override int GetHashCode() => ID;

        public static bool operator ==(Session a, Session b) => ReferenceEquals(a, b) || (a?.Equals(b) ?? false);
        public static bool operator !=(Session a, Session b) => !(a == b);

        public void Exit(M.Save drawing = M.Save.Prompt, M.Save preference = M.Save.Prompt, int conversationTimeoutMillisecond = Conversation.DefaultTimeoutMillisecond)
        {
            using (var c = new M.Conversation())
            {
                c.Start(ID, conversationTimeoutMillisecond);
                MC.Exit(drawing, preference);
            }
        }

        public void DontSaveExit(int conversationTimeoutMillisecond = Conversation.DefaultTimeoutMillisecond) => Exit(M.Save.DoNotSave, M.Save.DoNotSave, conversationTimeoutMillisecond);
    }
}
