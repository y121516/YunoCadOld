using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;

namespace YunoCad
{
    public class Session
    {
        public const int DefaultTimeoutMs = 30 * 1000;

        public static Session Start(StartFileType fileType = StartFileType.MAN, int timeoutMs = DefaultTimeoutMs)
        {
            return new Session(Cad.StartMicroGDS(fileType, timeoutMs));
        }

        public static Session Any { get; } = new Session(Informatix.MGDS.Conversation.AnySession);

        public int SessionID { get; }

        protected Session(int sessionID)
        {
            SessionID = sessionID;
        }

        public void Exit(Save drawing = Save.Prompt, Save preference = Save.Prompt)
        {
            using (var c = new Informatix.MGDS.Conversation())
            {
                c.Start(SessionID, Conversation.DefaultTimeoutMs);
                Cad.Exit(drawing, preference);
            }
        }
    }

    public class Conversation
    {
        public const int DefaultTimeoutMs = 5 * 1000;

        public static Task Start(Session session, int timeoutMs, Action action)
        {
            return Task.Run(() =>
            {
                using (var c = new Informatix.MGDS.Conversation())
                {
                    c.Start(session.SessionID, timeoutMs);
                    action();
                }
            });
        }

        public static Task Start(Session session, Action action)
        {
            return Start(session, DefaultTimeoutMs, action);
        }

        public static Task Start(int timeoutMs, Action action)
        {
            return Start(Session.Any, timeoutMs, action);
        }

        public static Task Start(Action action)
        {
            return Start(Session.Any, action);
        }

        public static Task<TResult> Start<TResult>(Session session, int timeoutMs, Func<TResult> func)
        {
            return Task.Run(() =>
            {
                using (var c = new Informatix.MGDS.Conversation())
                {
                    c.Start(session.SessionID, timeoutMs);
                    return func();
                }
            });
        }

        public static Task<TResult> Start<TResult>(Session session, Func<TResult> func)
        {
            return Start(session, DefaultTimeoutMs, func);
        }

        public static Task<TResult> Start<TResult>(int timeoutMs, Func<TResult> func)
        {
            return Start(Session.Any, timeoutMs, func);
        }

        public static Task<TResult> Start<TResult>(Func<TResult> func)
        {
            return Start(Session.Any, func);
        }
    }
}
