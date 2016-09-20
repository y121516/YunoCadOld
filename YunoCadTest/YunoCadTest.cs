using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MGDS = Informatix.MGDS;
using Yuno.Cad;

namespace YunoCadTest
{
    [TestClass]
    public class SessionTest
    {
        IEnumerable<Session> prevSessions;

        [TestInitialize]
        public void Initialize()
        {
            prevSessions = YunoCad.Sessions;
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var session in YunoCad.Sessions.Except(prevSessions)) session.DontSaveExit();
        }

        [TestMethod]
        public void IDTest()
        {
            Assert.AreEqual(Sessions.Any.ID, MGDS.Conversation.AnySession);
        }

        public void EqualsSessionTest(Session x, Session y, Session z)
        {
#pragma warning disable RECS0088 // Comparing equal expression for equality is usually useless
            Assert.IsTrue(x.Equals(x));
#pragma warning restore RECS0088 // Comparing equal expression for equality is usually useless
            Assert.AreEqual(x.Equals(y), y.Equals(x));
            if (x.Equals(y) && y.Equals(z)) Assert.IsTrue(x.Equals(z));
            var a = x.Equals(y);
            var b = x.Equals(y);
            Assert.AreEqual(a, b);
            Assert.IsFalse(x.Equals(null));
        }

        public void EqualsObjectTest(object x, object y, object z)
        {
#pragma warning disable RECS0088 // Comparing equal expression for equality is usually useless
            Assert.IsTrue(x.Equals(x));
#pragma warning restore RECS0088 // Comparing equal expression for equality is usually useless
            Assert.AreEqual(x.Equals(y), y.Equals(x));
            if (x.Equals(y) && y.Equals(z)) Assert.IsTrue(x.Equals(z));
            var a = x.Equals(y);
            var b = x.Equals(y);
            Assert.AreEqual(a, b);
            Assert.IsFalse(x.Equals(null));
        }

        public void EqualsTest(Session x, Session y, Session z)
        {
            EqualsSessionTest(x, y, z);
            EqualsObjectTest(x, y, z);
        }

        [TestMethod]
        [ExpectedException(typeof(MGDS.Cad.CadException))]
        public void StartFailTest()
        {
            var session = Session.Start(timeoutMillisecond: 0);
            session.DontSaveExit();
        }

        [TestMethod]
        [ExpectedException(typeof(MGDS.Cad.CadException))]
        public void SessionStartExitTest()
        {
            var session = Session.Start();
            session.DontSaveExit();
            try
            {
                session.DontSaveExit();
            }
            catch (MGDS.ApiException ex)
            {
                var eo = ex.ErrorOccurred(MGDS.AppErrorType.MGDS, MGDS.AppError.CommSetupFail);
                Assert.IsTrue(eo);
                throw;
            }
        }

        [TestMethod]
        public void SessionsTest()
        {
            var sessions = new Session[] {
                Session.Start(),
                Session.Start(MGDS.StartFileType.DXF),
                Session.Start(timeoutMillisecond: 50 * 1000),
                Session.Start(MGDS.StartFileType.DWG, 50 * 1000)
            };
            CollectionAssert.IsSubsetOf(sessions, YunoCad.Sessions.ToArray());
        }
    }

    [TestClass]
    public class ConversationTest
    {
        Session session;

        [TestInitialize]
        public void Initialize()
        {
            session = Session.Start();
        }

        [TestCleanup]
        public void Cleanup()
        {
            session.DontSaveExit();
        }

        [TestMethod]
        public void ConverseTest()
        {
            int result = 0;

            Conversation.Converse(session, 1000, _ => result = 1);
            Assert.AreEqual(1, result);

            Conversation.Converse(session, _ => result = 2);
            Assert.AreEqual(2, result);

            Conversation.Converse(1000, _ => result = 3);
            Assert.AreEqual(3, result);

            Conversation.Converse(_ => result = 4);
            Assert.AreEqual(4, result);

            session.Converse(1000, _ => result = 5);
            Assert.AreEqual(5, result);

            session.Converse(_ => result = 6);
            Assert.AreEqual(6, result);


            result = Conversation.Converse(session, 1000, _ => 1);
            Assert.AreEqual(1, result);

            result = Conversation.Converse(session, _ => 2);
            Assert.AreEqual(2, result);

            result = Conversation.Converse(1000, _ => 3);
            Assert.AreEqual(3, result);

            result = Conversation.Converse(_ => 4);
            Assert.AreEqual(4, result);

            result = session.Converse(1000, _ => 5);
            Assert.AreEqual(5, result);

            result = session.Converse(_ => result = 6);
            Assert.AreEqual(6, result);


            Conversation.ConverseAsync(session, 1000, _ => result = 1).Wait();
            Assert.AreEqual(1, result);

            Conversation.ConverseAsync(session, _ => result = 2).Wait();
            Assert.AreEqual(2, result);

            Conversation.ConverseAsync(1000, _ => result = 3).Wait();
            Assert.AreEqual(3, result);

            Conversation.ConverseAsync(_ => result = 4).Wait();
            Assert.AreEqual(4, result);

            session.ConverseAsync(1000, _ => result = 5).Wait();
            Assert.AreEqual(5, result);

            session.ConverseAsync(_ => result = 6).Wait();
            Assert.AreEqual(6, result);


            result = Conversation.ConverseAsync(session, 1000, _ => 1).Result;
            Assert.AreEqual(1, result);

            result = Conversation.ConverseAsync(session, _ => 2).Result;
            Assert.AreEqual(2, result);

            result = Conversation.ConverseAsync(1000, _ => 3).Result;
            Assert.AreEqual(3, result);

            result = Conversation.ConverseAsync(_ => 4).Result;
            Assert.AreEqual(4, result);

            result = session.ConverseAsync(1000, _ => 5).Result;
            Assert.AreEqual(5, result);

            result = session.ConverseAsync(_ => result = 6).Result;
            Assert.AreEqual(6, result);
        }

    }

    [TestClass]
    public class MenuTest
    {
        [TestMethod]
        public void MenuItemTest()
        {
            Sessions.Any.Converse(cs =>
            {
                var addName = @"{/pa}&Parent\Child&Add";
                var addItem = new MenuItem(addName);
                var addCommandItem = new CommandMenuItem(addName, @"cmd.exe /c");
                cs.Menu.Add(addCommandItem);
                Assert.AreEqual(@"{/pa}&Parent\Child&Add", cs.Menu[addItem].Name);
                Assert.AreEqual(@"{/pa}&Parent\Child&Add", cs.Menu[addCommandItem].Name);

                cs.Menu[addItem].Checked = false;
                Assert.AreEqual(false, cs.Menu[addItem].Checked);
                cs.Menu[addCommandItem].Checked = true;
                Assert.AreEqual(true, cs.Menu[addCommandItem].Checked);

                cs.Menu[addItem].Enabled = false;
                cs.Menu[addCommandItem].Enabled = true;

                var insertName = @"Child&Insert"; // 相対
                var insertCommandItem = new CommandMenuItem(insertName, @"cmd.exe /c");
                insertCommandItem = cs.Menu.Insert(addCommandItem, insertCommandItem);
                Assert.AreEqual(@"{/pa}&Parent\Child&Insert", cs.Menu[insertCommandItem].Name);
                Assert.AreEqual(@"{/pa}&Parent\Child&Insert", cs.Menu[insertCommandItem].Name);

                var insertItem = new MenuItem(@"&Parent\Child&Insert"); // 絶対
                cs.Menu[insertCommandItem].Checked = false;
                Assert.AreEqual(false, cs.Menu[insertItem].Checked);
                cs.Menu[insertCommandItem].Checked = true;
                Assert.AreEqual(true, cs.Menu[insertItem].Checked);

                cs.Menu[insertItem].Enabled = true;
                cs.Menu[insertItem].Enabled = false;
            });
        }
    }

    [TestClass]
    public class AliasTest
    {
        [TestMethod]
        public void Test()
        {
            Sessions.Any.Converse(cs =>
            {
                foreach (var doc in cs.Documents.Scan().Take(1))
                {
                    var aliases = doc.Activate().Resynch().Aliases;
                    aliases.DefaultAlias(MGDS.AliasName.Layer).Name = "";
                    var name = aliases.DefaultAlias(MGDS.AliasName.Layer).Name;

                    var alias = new Alias(MGDS.AliasName.Layer, name);
                    aliases.Add(alias, new AliasInfo(@"C:\Windows\Temp"));
                    var info = aliases[alias];
                    aliases[alias] = info;
                }
            });
        }
    }

    [TestClass]
    public class SetObjectTest
    {
        [TestMethod]
        public void Test()
        {
            Sessions.Any.Converse(cs =>
            {
                foreach (var d in cs.Documents.Scan().Take(1).Select(d => d.Activate().Resynch()))
                {
                    //var se = d.SetEdit;
                    //var ls1 = se.LineStyle.All;
                    //var ls2 = se.LineStyle.Not.All;
                    //ls1 = se.LineStyle("hoge");
                    //ls2 = se.LineStyleNot("hoge");
                    //se.LineStyle = false;
                }
                {
                    var ib = cs.InfoBar;
                    { var a = ib.HoverHighlight; ib.HoverHighlight = !a; }
                    { var a = ib.SnapGuides; ib.SnapGuides = !a; }
                    { var a = ib.ZLock; ib.ZLock = !a; }
                }
            });
        }
    }

    [TestClass]
    public class SyntaxText
    {
        [TestMethod]
        public void Test()
        {
            Sessions.Any.Converse(cs =>
            {
                foreach(var nameAndLink in cs.Documents.Scan().Select(d => d.Activate().Resynch())
                    .SelectMany(d => d.Layers.Scan()))
                {
                    var name = nameAndLink.Item1;
                    var link = nameAndLink.Item2;
                    Informatix.MGDS.Cad.CurLayLink(link);
                }
            });
        }
    }
}
