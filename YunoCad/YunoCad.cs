using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    /// <summary>
    /// MicroGDS との通信を表すクラス。
    /// 同期通信と非同期通信の両方をサポートします。
    /// ただし MicroGDS .NET Custom Application Support DLL (MGDSNet.dll) の制限に注意する必要があります。
    /// 非同期通信を行う場合は、通信終了前に呼び出し元スレッドが終了してしまわないようにします。
    /// 呼び出し元スレッドで Task.Wait や Task.Result を呼び出すことで、呼び出し元スレッドで通信終了を待てます。
    /// また、同時に複数の通信をすることはできないので Task.ContinueWith で継続タスクを作成します。
    /// </summary>
    public static class Conversation
    {
        public const int DefaultTimeoutMillisecond = 5 * 1000;

        public static void Converse(this Session session, int timeoutMillisecond, Action<Documents> action)
        {
            using (var c = new Informatix.MGDS.Conversation())
            {
                c.Start(session.ID, timeoutMillisecond);
                action(Documents.Instance);
            }
        }

        public static void Converse(this Session session, Action<Documents> action)
            => Converse(session, DefaultTimeoutMillisecond, action);

        public static void Converse(int timeoutMillisecond, Action<Documents> action)
            => Converse(Session.Any, timeoutMillisecond, action);

        public static void Converse(Action<Documents> action)
            => Converse(Session.Any, action);


        public static TResult Converse<TResult>(this Session session, int timeoutMillisecond, Func<Documents, TResult> func)
        {
            using (var c = new Informatix.MGDS.Conversation())
            {
                c.Start(session.ID, timeoutMillisecond);
                return func(Documents.Instance);
            }
        }

        public static TResult Converse<TResult>(this Session session, Func<Documents, TResult> func)
            => Converse(session, DefaultTimeoutMillisecond, func);

        public static TResult Converse<TResult>(int timeoutMillisecond, Func<Documents, TResult> func)
            => Converse(Session.Any, timeoutMillisecond, func);

        public static TResult Converse<TResult>(Func<Documents, TResult> func)
            => Converse(Session.Any, func);


        public static Task ConverseAsync(this Session session, int timeoutMillisecond, Action<Documents> action)
            => Task.Run(() => Converse(session, timeoutMillisecond, action));

        public static Task ConverseAsync(this Session session, Action<Documents> action)
            => ConverseAsync(session, DefaultTimeoutMillisecond, action);

        public static Task ConverseAsync(int timeoutMillisecond, Action<Documents> action)
            => ConverseAsync(Session.Any, timeoutMillisecond, action);

        public static Task ConverseAsync(Action<Documents> action)
            => ConverseAsync(Session.Any, action);


        public static Task<TResult> ConverseAsync<TResult>(this Session session, int timeoutMillisecond, Func<Documents, TResult> func)
            => Task.Run(() => Converse(session, timeoutMillisecond, func));

        public static Task<TResult> ConverseAsync<TResult>(this Session session, Func<Documents, TResult> func)
            => ConverseAsync(session, DefaultTimeoutMillisecond, func);

        public static Task<TResult> ConverseAsync<TResult>(int timeoutMillisecond, Func<Documents, TResult> func)
            => ConverseAsync(Session.Any, timeoutMillisecond, func);

        public static Task<TResult> ConverseAsync<TResult>(Func<Documents, TResult> func)
            => ConverseAsync(Session.Any, func);
    }
}
