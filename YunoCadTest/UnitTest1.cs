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
    }
}
