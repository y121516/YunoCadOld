using System.Collections;
using System.Collections.Generic;
using System.Linq;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class Sessions : IEnumerable<Session>
    {
        internal static Sessions Instance { get; } = new Sessions();
        public static Session Any { get; } = new Session(M.Conversation.AnySession);

        Sessions() { }

        public int Count => MC.GetSessionCount();

        public IEnumerator<Session> GetEnumerator()
        {
            var maxSessionIDs = Count;
            var sessionIDArray = new int[maxSessionIDs];
            MC.GetSessionIDs(sessionIDArray, maxSessionIDs);
            return sessionIDArray.Select(session => new Session(session)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
