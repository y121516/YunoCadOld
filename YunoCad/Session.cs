using System;
using System.Collections.Generic;
using System.Linq;
using Informatix.MGDS;

namespace YunoCad
{
    /// <summary>
    /// セッション (実行中の MicroGDS のプロセス) を表すクラス。
    /// </summary>
    public class Session : IEquatable<Session>
    {
        public const int DefaultTimeoutMillisecond = 30 * 1000;

        public static Session Start(StartFileType fileType = StartFileType.MAN, int timeoutMillisecond = DefaultTimeoutMillisecond)
        {
            return new Session(Cad.StartMicroGDS(fileType, timeoutMillisecond));
        }

        public static Session Any { get; } = new Session(Informatix.MGDS.Conversation.AnySession);

        public static int Count
        {
            get
            {
                return Cad.GetSessionCount();
            }
        }

        public static IEnumerable<Session> Sessions
        {
            get
            {
                var maxSessionIDs = Count;
                var sessionIDArray = new int[maxSessionIDs];
                Cad.GetSessionIDs(sessionIDArray, maxSessionIDs);
                return sessionIDArray.Select(session => new Session(session));
            }
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

        public void Exit(Save drawing = Save.Prompt, Save preference = Save.Prompt, int conversationTimeoutMillisecond = Conversation.DefaultTimeoutMillisecond)
        {
            using (var c = new Informatix.MGDS.Conversation())
            {
                c.Start(ID, conversationTimeoutMillisecond);
                Cad.Exit(drawing, preference);
            }
        }

        public void DontSaveExit(int conversationTimeoutMillisecond = Conversation.DefaultTimeoutMillisecond) => Exit(Save.DoNotSave, Save.DoNotSave, conversationTimeoutMillisecond);
    }
}
