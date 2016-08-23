using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Tests
{
    [TestClass]
    public class EventsTests
    {

        [TestMethod]
        public void EventsEquality()
        {
            EntityEvent e1 = new EntityEvent();
            e1.OrderKey = "111222";

            EntityEvent e2 = new EntityEvent();
            e2.OrderKey = "111222";

            var result = e2.CompareTo(e1);
            Assert.IsTrue(result == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EventsEqualityException()
        {
            EntityEvent e1 = new EntityEvent();
            e1.OrderKey = "111222";

            Exception ex = new Exception();
            e1.CompareTo(ex);
        }

        [TestMethod]
        public void EventsGreater()
        {
            EntityEvent e1 = new EntityEvent();
            e1.OrderKey = "111223";

            EntityEvent e2 = new EntityEvent();
            e2.OrderKey = "111222";

            var result = e2.CompareTo(e1);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void EventsLess()
        {
            EntityEvent e1 = new EntityEvent();
            e1.OrderKey = "111221";

            EntityEvent e2 = new EntityEvent();
            e2.OrderKey = "111222";

            var result = e2.CompareTo(e1);
            Assert.IsTrue(result < 0);
        }
    }
}
