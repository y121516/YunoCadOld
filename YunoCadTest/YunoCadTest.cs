using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MGDS = Informatix.MGDS;
using YunoCad;

namespace YunoCadTest
{
    [TestClass]
    public class SessionTest
    {
        IEnumerable<Session> prevSessions;

        [TestInitialize]
        public void Initialize()
        {
            prevSessions = Session.Sessions;
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var session in Session.Sessions.Except(prevSessions)) session.DontSaveExit();
        }

        [TestMethod]
        public void IDTest()
        {
            Assert.AreEqual(Session.Any.ID, MGDS.Conversation.AnySession);
            Assert.AreEqual(new Session(1).ID, 1);
            Assert.AreEqual(new Session(2).ID, 2);
        }

        public void EqualsSessionTest(Session x, Session y, Session z)
        {
            Assert.IsTrue(x.Equals(x));
            Assert.AreEqual(x.Equals(y), y.Equals(x));
            if (x.Equals(y) && y.Equals(z)) Assert.IsTrue(x.Equals(z));
            var a = x.Equals(y);
            var b = x.Equals(y);
            Assert.AreEqual(a, b);
            Assert.IsFalse(x.Equals(null));
        }

        public void EqualsObjectTest(object x, object y, object z)
        {
            Assert.IsTrue(x.Equals(x));
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
        public void EqualsTest()
        {
            var x = new Session(1);
            {
                var y = x;
                {
                    var z = x;
                    EqualsTest(x, y, z);
                }
                {
                    var z = new Session(1);
                    EqualsTest(x, y, z);
                }
                {
                    var z = new Session(2);
                    EqualsTest(x, y, z);
                }
            }
            {
                var y = new Session(1);
                {
                    var z = x;
                    EqualsTest(x, y, z);
                }
                {
                    var z = y;
                    EqualsTest(x, y, z);
                }
                {
                    var z = new Session(1);
                    EqualsTest(x, y, z);
                }
                {
                    var z = new Session(2);
                    EqualsTest(x, y, z);
                }
            }
            {
                var y = new Session(2);
                {
                    var z = x;
                    EqualsTest(x, y, z);
                }
                {
                    var z = y;
                    EqualsTest(x, y, z);
                }
                {
                    var z = new Session(1);
                    EqualsTest(x, y, z);
                }
                {
                    var z = new Session(2);
                    EqualsTest(x, y, z);
                }
            }
        }

        [TestMethod]
        public void OperatorEqualTest()
        {
            {
                var a = (Session)null;
                {
                    var b = (Session)null;
                    Assert.IsTrue(a == b);
                    Assert.IsFalse(a != b);
                }
                {
                    var b = new Session(1);
                    Assert.IsFalse(a == b);
                    Assert.IsTrue(a != b);
                }
            }
            {
                var a = new Session(1);
                {
                    var b = (Session)null;
                    Assert.IsFalse(a == b);
                    Assert.IsTrue(a != b);
                }
                {
                    var b = new Session(1);
                    Assert.IsTrue(a == b);
                    Assert.IsFalse(a != b);
                }
                {
                    var b = new Session(2);
                    Assert.IsFalse(a == b);
                    Assert.IsTrue(a != b);
                }
            }
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var a = new Session(1);
            var b = a;
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            b = new Session(1);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            b = new Session(2);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
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
            CollectionAssert.IsSubsetOf(sessions, Session.Sessions.ToArray());
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
            Session.Any.Converse(cs =>
            {
                var menuName = @"{/pa}&ParentMenuName\&ChildMenuName";
                var command = new Command(menuName, @"cmd.exe");

                cs.Menu.Add(command);
                Assert.AreEqual(@"{/pa}&ParentMenuName\&ChildMenuName", cs.Menu[menuName].MenuName);

                Assert.AreEqual(false, cs.Menu[menuName].Checked);
                cs.Menu[menuName].Checked = true;
                Assert.AreEqual(true, cs.Menu[menuName].Checked);

                cs.Menu[menuName].Enabled = true;
                cs.Menu[menuName].Enabled = false;
            });
        }
    }

    [TestClass]
    public class AliasTest
    {
        [TestMethod]
        public void Test()
        {
            Session.Any.Converse(cs =>
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
}
