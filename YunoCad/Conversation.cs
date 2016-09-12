using System;
using System.Threading.Tasks;
using M = Informatix.MGDS;

namespace Yuno.Cad
{
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

        public static void Converse(this Session session, int timeoutMillisecond, Action<ConversingSession> action)
        {
            using (var c = new M.Conversation())
            {
                c.Start(session.ID, timeoutMillisecond);
                action(new ConversingSession(c, session));
            }
        }

        public static void Converse(this Session session, Action<ConversingSession> action)
            => Converse(session, DefaultTimeoutMillisecond, action);

        public static void Converse(int timeoutMillisecond, Action<ConversingSession> action)
            => Converse(Sessions.Any, timeoutMillisecond, action);

        public static void Converse(Action<ConversingSession> action)
            => Converse(Sessions.Any, action);


        public static TResult Converse<TResult>(this Session session, int timeoutMillisecond, Func<ConversingSession, TResult> func)
        {
            using (var c = new M.Conversation())
            {
                c.Start(session.ID, timeoutMillisecond);
                return func(new ConversingSession(c, session));
            }
        }

        public static TResult Converse<TResult>(this Session session, Func<ConversingSession, TResult> func)
            => Converse(session, DefaultTimeoutMillisecond, func);

        public static TResult Converse<TResult>(int timeoutMillisecond, Func<ConversingSession, TResult> func)
            => Converse(Sessions.Any, timeoutMillisecond, func);

        public static TResult Converse<TResult>(Func<ConversingSession, TResult> func)
            => Converse(Sessions.Any, func);


        public static Task ConverseAsync(this Session session, int timeoutMillisecond, Action<ConversingSession> action)
            => Task.Run(() => Converse(session, timeoutMillisecond, action));

        public static Task ConverseAsync(this Session session, Action<ConversingSession> action)
            => ConverseAsync(session, DefaultTimeoutMillisecond, action);

        public static Task ConverseAsync(int timeoutMillisecond, Action<ConversingSession> action)
            => ConverseAsync(Sessions.Any, timeoutMillisecond, action);

        public static Task ConverseAsync(Action<ConversingSession> action)
            => ConverseAsync(Sessions.Any, action);


        public static Task<TResult> ConverseAsync<TResult>(this Session session, int timeoutMillisecond, Func<ConversingSession, TResult> func)
            => Task.Run(() => Converse(session, timeoutMillisecond, func));

        public static Task<TResult> ConverseAsync<TResult>(this Session session, Func<ConversingSession, TResult> func)
            => ConverseAsync(session, DefaultTimeoutMillisecond, func);

        public static Task<TResult> ConverseAsync<TResult>(int timeoutMillisecond, Func<ConversingSession, TResult> func)
            => ConverseAsync(Sessions.Any, timeoutMillisecond, func);

        public static Task<TResult> ConverseAsync<TResult>(Func<ConversingSession, TResult> func)
            => ConverseAsync(Sessions.Any, func);
    }
}
