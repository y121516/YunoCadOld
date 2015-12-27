using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YunoCad;

namespace YunoCadTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void SessionStartExit()
        {
            var session = Session.Start();
            session.Exit(Informatix.MGDS.Save.DoNotSave, Informatix.MGDS.Save.DoNotSave);
        }

        [TestMethod]
        public void SessionConverseActionTest()
        {
            var session = Session.Start();

            int result = 0;
            Conversation.Start(session, 1000, (() => { result = 1; })).Wait();
            Assert.AreEqual(1, result);
            Conversation.Start(session, (() => { result = 2; ; })).Wait();
            Assert.AreEqual(2, result);
            Conversation.Start(1000, (() => { result = 3; })).Wait();
            Assert.AreEqual(3, result);
            Conversation.Start((() => { result = 4; })).Wait();
            Assert.AreEqual(4, result);

            session.Exit(Informatix.MGDS.Save.DoNotSave, Informatix.MGDS.Save.DoNotSave);
        }

        [TestMethod]
        public void SessionConverseFuncTest()
        {
            var session = Session.Start();

            int result;
            result = Conversation.Start(session, 1000, () => 1).Result;
            Assert.AreEqual(1, result);
            result = Conversation.Start(session, () => 2).Result;
            Assert.AreEqual(2, result);
            result = Conversation.Start(1000, () => 3).Result;
            Assert.AreEqual(3, result);
            result = Conversation.Start(() => 4).Result;
            Assert.AreEqual(4, result);

            session.Exit(Informatix.MGDS.Save.DoNotSave, Informatix.MGDS.Save.DoNotSave);
        }
        [TestMethod]
        public void DocumentForEachTest()
        {
            var session = Session.Start();

            Document.ForEach((docID) =>
            {
                var docName = "";
                Informatix.MGDS.Cad.DocActivate(out docName);
            });


            session.Exit(Informatix.MGDS.Save.DoNotSave, Informatix.MGDS.Save.DoNotSave);
        }
    }
}