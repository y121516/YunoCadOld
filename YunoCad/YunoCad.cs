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

        bool IEquatable<Session>.Equals(Session other)
        {
            if (other == null) return false;
            return ID.Equals(other.ID);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Session);
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public static bool operator ==(Session a, Session b) => a?.Equals(b) ?? false;
        public static bool operator !=(Session a, Session b) => !(a == b);

        public void Exit(Save drawing = Save.Prompt, Save preference = Save.Prompt, int conversationTimeoutMillisecond = Conversation.DefaultTimeoutMillisecond)
        {
            using (var c = new Informatix.MGDS.Conversation())
            {
                c.Start(ID, conversationTimeoutMillisecond);
                Cad.Exit(drawing, preference);
            }
        }
    }

    /// <summary>
    /// MicroGDS との通信を表すクラス。
    /// 同期通信と非同期通信の両方をサポートします。
    /// ただし非同期通信を行う場合は、通信終了前に呼び出し元スレッドが終了してしまわないようにする必要があります。
    /// これは MicroGDS .NET Custom Application Support DLL（mgdsnet.dll）の制限です。
    /// 呼び出し元スレッドで Task.Wait や Task.Result を呼び出すことで、呼び出し元スレッドで通信終了を待てます。
    /// </summary>
    public static class Conversation
    {
        public const int DefaultTimeoutMillisecond = 5 * 1000;

        public static void Start(this Session session, int timeoutMillisecond, Action action)
        {
            using (var c = new Informatix.MGDS.Conversation())
            {
                c.Start(session.ID, timeoutMillisecond);
                action();
            }
        }

        public static void Start(this Session session, Action action)
        {
            Start(session, DefaultTimeoutMillisecond, action);
        }

        public static void Start(int timeoutMillisecond, Action action)
        {
            Start(Session.Any, timeoutMillisecond, action);
        }

        public static void Start(Action action)
        {
            Start(Session.Any, action);
        }

        public static TResult Start<TResult>(this Session session, int timeoutMillisecond, Func<TResult> func)
        {
            using (var c = new Informatix.MGDS.Conversation())
            {
                c.Start(session.ID, timeoutMillisecond);
                return func();
            }
        }

        public static TResult Start<TResult>(this Session session, Func<TResult> func)
        {
            return Start(session, DefaultTimeoutMillisecond, func);
        }

        public static TResult Start<TResult>(int timeoutMillisecond, Func<TResult> func)
        {
            return Start(Session.Any, timeoutMillisecond, func);
        }

        public static TResult Start<TResult>(Func<TResult> func)
        {
            return Start(Session.Any, func);
        }

        public static Task StartAsync(Session session, int timeoutMillisecond, Action action)
        {
            return Task.Run(() => Start(session, timeoutMillisecond, action));
        }

        public static Task StartAsync(Session session, Action action)
        {
            return StartAsync(session, DefaultTimeoutMillisecond, action);
        }

        public static Task StartAsync(int timeoutMillisecond, Action action)
        {
            return StartAsync(Session.Any, timeoutMillisecond, action);
        }

        public static Task StartAsync(Action action)
        {
            return StartAsync(Session.Any, action);
        }

        public static Task<TResult> StartAsync<TResult>(Session session, int timeoutMillisecond, Func<TResult> func)
        {
            return Task.Run(() => Start(session, timeoutMillisecond, func));
        }

        public static Task<TResult> StartAsync<TResult>(Session session, Func<TResult> func)
        {
            return StartAsync(session, DefaultTimeoutMillisecond, func);
        }

        public static Task<TResult> StartAsync<TResult>(int timeoutMillisecond, Func<TResult> func)
        {
            return StartAsync(Session.Any, timeoutMillisecond, func);
        }

        public static Task<TResult> StartAsync<TResult>(Func<TResult> func)
        {
            return StartAsync(Session.Any, func);
        }
    }

    class CurrentDocument
    {

    }

    public class Document
    {
        static Document New()
        {
            Cad.CreateFile();
            return new Document(""); // 作成したファイル(ドキュメント)がカレントドキュメントとなる
        }

        public static IEnumerable<TResult> ForEach<TResult>(Func<string, TResult> func)
        {
            var docID = "";
            if (Cad.DocFirst(out docID))
            {
                do
                {
                    yield return func(docID);
                } while (Cad.DocNext(out docID));
            }
        }

        public static IEnumerable<TResult> ForEach<TResult>(Func<TResult> func)
        {
            var docID = "";
            if (Cad.DocFirst(out docID))
            {
                do
                {
                    yield return func();
                } while (Cad.DocNext(out docID));
            }
        }

        public static void ForEach(Action<string> action)
        {
            var docID = "";
            if (Cad.DocFirst(out docID))
            {
                do
                {
                    action(docID);
                } while (Cad.DocNext(out docID));
            }
        }

        public static void ForEach(Action action)
        {
            var docID = "";
            if (Cad.DocFirst(out docID))
            {
                do
                {
                    action();
                } while (Cad.DocNext(out docID));
            }
        }

        public static int Count
        {
            get
            {
                int n = 0;
                ForEach(() => ++n);
                return n;
            }
        }

        string ID { get; }

        Document(string docID)
        {
            ID = docID;
        }
    }
}
