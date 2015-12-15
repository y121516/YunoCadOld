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
        public class Conversation
        {
            public const int DefaultTimeoutMs = 5 * 1000;
        }

        public const int DefaultTimeoutMs = 30 * 1000;

        public static Session Start(StartFileType fileType = StartFileType.MAN, int timeoutMs = DefaultTimeoutMs)
        {
            return new Session(Cad.StartMicroGDS(fileType, timeoutMs));
        }


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
}
